using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;

namespace MicroDI
{
    /// <summary>
    /// The dependency manager.
    /// </summary>
    public class Dependency : IDisposable
    {
        List<IDisposable> disposables = new List<IDisposable>();
        object[] scoped = new object[instances];

#if DEBUG
        /// <summary>
        /// A flag to control use of code generation.
        /// </summary>
        public static bool UseCodeGeneration;
#endif

        #region Dependency registrations
        internal static int instances = 0;

        /// <summary>
        /// The event that's fired when errors occur during disposal.
        /// </summary>
        public static event Action<IEnumerable<Exception>> OnError;
        
        /// <summary>
        /// Define a scoped service registration.
        /// </summary>
        /// <param name="create">The custom constructor to create new instances.</param>
        /// <typeparam name="TService">The service type.</typeparam>
        public static void Scoped<TService, TInstance>(Func<Dependency, TInstance> create)
            where TInstance : TService
        {
            //FIXME: I could add a circularity check here too now that this supports arbitrary constructors.
            //This could be detected at runtime via a test construction of all instances.
            lock (typeof(Service<TService>))
            {
                RequiresEmptyRegistration<TService>();
                var i =
#if DEBUG
                    Service<TService>.Index = //Service<TInstance>.Index =
#endif
                    Interlocked.Increment(ref instances) - 1;
                Service<TService>.Resolve =
                    deps => (TService)(deps.scoped[i] ?? deps.Init(create(deps), i));
            }
        }

        /// <summary>
        /// Declare a global instance.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="instance">The global instance.</param>
        public static void Singleton<TService>(TService instance)
        {
            lock (typeof(Service<TService>))
            {
                RequiresEmptyRegistration<TService>();
                Service<TService>.Resolve = deps => instance;
            }
        }
        
        /// <summary>
        /// Define a transient 
        /// </summary>
        /// <param name="create">The custom constructor to create new instances.</param>
        /// <param name="debug">A circular dependency check is performed if true, otherwise the check is skipped.</param>
        /// <typeparam name="TInstance">The type of the service instance.</typeparam>
        /// <typeparam name="TService">The service type.</typeparam>
        public static void Transient<TService, TInstance>(Func<Dependency, TInstance> create, bool debug = false)
            where TInstance : TService
        {
            lock (typeof(Service<TService>))
            {
                RequiresEmptyRegistration<TService>();
                // check for a circular dependency on TService in TInstance
                if (debug && IsCircular<TInstance, TService>(new HashSet<Type>(new[] { typeof(TInstance) })))
                    throw new ArgumentException("Type " + typeof(TInstance).Name + " has a circular dependency on " + typeof(TService).Name + " and so cannot have transient lifetime.");
                Service<TService>.Resolve = deps => deps.Init(create(deps));
            }
        }

        /// <summary>
        /// Clear any previous registrations.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        public static void Clear<TService>()
        {
            Service<TService>.Resolve = null;
        }

        /// <summary>
        /// Clear any previous registrations.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        public static void Clear(Type typeDefinition)
        {
            if (typeDefinition.IsConstructedGenericType || !typeDefinition.GetTypeInfo().IsGenericTypeDefinition)
                throw new ArgumentException("Parameter must be a generic type definition.");
            ctors.Remove(typeDefinition);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Initialize a fresh instance of a type.
        /// </summary>
        T Init<T>(T x, int scopeIndex)
        {
            // register scoped instance before init in case of circular dependencies
            scoped[scopeIndex] = x;
            return Init(x);
        }
        /// <summary>
        /// Initialize a fresh instance of a type.
        /// </summary>
        T Init<T>(T x)
        {
            Instance<T>.Init?.Invoke(this, x);
            var y = x as IDisposable;
            if (y != null)
                disposables.Add(y);
            return x;
        }
        /// <summary>
        /// Ensure no registration for a service exists.
        /// </summary>
        static void RequiresEmptyRegistration<TReturn>()
        {
            if (Service<TReturn>.Resolve != null)
                throw new InvalidOperationException("Type " + typeof(TReturn).Name + " is already registered.");
        }

        static MethodInfo isCircular = new Func<HashSet<Type>, bool>(IsCircular<int, int>)
            .GetMethodInfo()
            .GetGenericMethodDefinition();
        
        /// <summary>
        /// Check whether any properties recursively reference T.
        /// </summary>
        static bool IsCircular<TInstance, TService>(HashSet<Type> visited)
        {
            var type = typeof(TInstance);
            var stype = typeof(TService);
            foreach (var x in type.GetRuntimeProperties())
            {
                if (x.PropertyType == stype || x.PropertyType == type)
                    return true;
                else if (visited.Add(x.PropertyType))
                    return IsCircular(x.PropertyType, stype, visited);
            }
            return false;
        }
        static bool IsCircular(Type instance, Type service, HashSet<Type> visited)
        {
            return (bool)isCircular.MakeGenericMethod(instance, service)
                                   .Invoke(null, new[] { visited });
        }
        #endregion

        #region Generic constructor overloads
        struct TypeApply
        {
            public Type Definition;
            public short CtorIndex;
            public short[] TypeArgs;
        }
        static ConstructorInfo Apply(TypeInfo definition, Type instance, int ctorIndex)
        {
            return definition.MakeGenericType(instance.GenericTypeArguments)
                             .GetTypeInfo()
                             .DeclaredConstructors
                             .ElementAt(ctorIndex);
        }
        static Func<Dependency, T> Const<T>(T value)
        {
            return x => value;
        }
        static Func<Dependency, T> New<T, T0>(ConstructorInfo ctor, Func<Dependency, T0> arg0)
        {
            return x => (T)ctor.Invoke(new object[] { arg0(x) });
        }
        static Func<Dependency, T> New<T, T0, T1>(ConstructorInfo ctor, Func<Dependency, T0> arg0, Func<Dependency, T1> arg1)
        {
            return x => (T)ctor.Invoke(new object[] { arg0(x), arg1(x) });
        }
        static Func<Dependency, T> New<T, T0, T1, T2>(ConstructorInfo ctor, Func<Dependency, T0> arg0, Func<Dependency, T1> arg1, Func<Dependency, T2> arg2)
        {
            return x => (T)ctor.Invoke(new object[] { arg0(x), arg1(x), arg2(x) });
        }
        static Func<Dependency, T> New<T, T0, T1, T2, T3>(ConstructorInfo ctor, Func<Dependency, T0> arg0, Func<Dependency, T1> arg1, Func<Dependency, T2> arg2, Func<Dependency, T3> arg3)
        {
            return x => (T)ctor.Invoke(new object[] { arg0(x), arg1(x), arg2(x), arg3(x) });
        }

        static Func<Type, Dependency, object> GenericCtor(TypeInfo definition, int ctorIndex)
        {
            return (type, deps) => Apply(definition, type, ctorIndex).Invoke(null);
        }
        static Func<Type, Dependency, object> GenericCtor<T0>(TypeInfo definition, int ctorIndex, Func<Dependency, T0> arg0)
        {
            return (type, deps) => Apply(definition, type, ctorIndex).Invoke(new object[] { arg0(deps) });
        }
        static Func<Type, Dependency, object> GenericCtor<T0, T1>(TypeInfo definition, int ctorIndex, Func<Dependency, T0> arg0, Func<Dependency, T1> arg1)
        {
            return (type, deps) => Apply(definition, type, ctorIndex).Invoke(new object[] { arg0(deps), arg1(deps) });
        }
        static Func<Type, Dependency, object> GenericCtor<T0, T1, T2>(TypeInfo definition, int ctorIndex, Func<Dependency, T0> arg0, Func<Dependency, T1> arg1, Func<Dependency, T2> arg2)
        {
            return (type, deps) => Apply(definition, type, ctorIndex).Invoke(new object[] { arg0(deps), arg1(deps), arg2(deps) });
        }
        static Func<Type, Dependency, object> GenericCtor<T0, T1, T2, T3>(TypeInfo definition, int ctorIndex, Func<Dependency, T0> arg0, Func<Dependency, T1> arg1, Func<Dependency, T2> arg2, Func<Dependency, T3> arg3)
        {
            return (type, deps) => Apply(definition, type, ctorIndex).Invoke(new object[] { arg0(deps), arg1(deps), arg2(deps), arg3(deps) });
        }
        static Func<Type, Dependency, object> GenericCtor<T0, T1, T2, T3, T4>(TypeInfo definition, int ctorIndex, Func<Dependency, T0> arg0, Func<Dependency, T1> arg1, Func<Dependency, T2> arg2, Func<Dependency, T3> arg3, Func<Dependency, T4> arg4)
        {
            return (type, deps) => Apply(definition, type, ctorIndex).Invoke(new object[] { arg0(deps), arg1(deps), arg2(deps), arg3(deps), arg4(deps) });
        }
        static Func<Type, Dependency, object> GenericCtor<T0, T1, T2, T3, T4, T5>(TypeInfo definition, int ctorIndex, Func<Dependency, T0> arg0, Func<Dependency, T1> arg1, Func<Dependency, T2> arg2, Func<Dependency, T3> arg3, Func<Dependency, T4> arg4, Func<Dependency, T5> arg5)
        {
            return (type, deps) => Apply(definition, type, ctorIndex).Invoke(new object[] { arg0(deps), arg1(deps), arg2(deps), arg3(deps), arg4(deps), arg5(deps) });
        }
        #endregion

        static Dictionary<Type, Func<Type, Dependency, object>> ctors = new Dictionary<Type, Func<Type, Dependency, object>>();

        /// <summary>
        /// Resolve a service instance.
        /// </summary>
        /// <typeparam name="TService">The service type to resolve.</typeparam>
        /// <returns>An instance of the service.</returns>
        public TService Resolve<TService>()
        {
            var resolve = Service<TService>.Resolve;
            if (resolve != null)
                return resolve(this);
#if DEBUG
            var service = typeof(TService);
            Func<Type, Dependency, object> create;
            if (service.IsConstructedGenericType && ctors.TryGetValue(service.GetGenericTypeDefinition(), out create))
                return (TService)create(service, this);
#endif
            else
                throw new InvalidOperationException("Type " + typeof(TService).Name + " has no registration.");
        }

        public static void GenericScoped<TService, TInstance>(System.Linq.Expressions.Expression<Func<Dependency, TInstance>> create)
            where TInstance : TService
        {

        }

        static readonly MethodInfo resolve = typeof(Dependency).GetRuntimeMethod("Resolve", new Type[0]);

        public static void Scoped(Type service, Type instance)
        {
            if (!service.GetTypeInfo().IsGenericTypeDefinition || !instance.GetTypeInfo().IsGenericTypeDefinition)
                throw new ArgumentException("This overload can only be used to register services with generic type parameters.");
            var ctorIndex = instance.GetTypeInfo()
                                    .DeclaredConstructors
                                    .Select((x, i) => new { i = i, ctor = x })
                                    .OrderByDescending(x => x.ctor.GetParameters().Length)
                                    .First()
                                    .i;
            var impl = instance.GetTypeInfo().ImplementedInterfaces.Single(x => service == x.GetGenericTypeDefinition());
            var map = Map(impl.GenericTypeArguments.Select(x => x.GenericParameterPosition).ToArray());
            ctors.Add(service, (type, deps) =>
            {
                var typeArgs = type.GenericTypeArguments;
                var ctor = instance.MakeGenericType(map?.Invoke(typeArgs))
                                   .GetTypeInfo()
                                   .DeclaredConstructors
                                   .ElementAt(ctorIndex);
                var param = ctor.GetParameters();
                var args = new object[param.Length];
                for (int i = 0; i < args.Length; ++i)
                    args[i] = resolve.MakeGenericMethod(param[i].ParameterType).Invoke(deps, null);
                return ctor.Invoke(args);
            });
        }

        static Func<Type[], Type[]> Map(int[] map)
        {
            switch (map.Length)
            {
                case 1: return args => new[] { args[map[0]] };
                case 2: return args => new[] { args[map[0]], args[map[1]] };
                case 3: return args => new[] { args[map[0]], args[map[1]], args[map[2]] };
                case 4: return args => new[] { args[map[0]], args[map[1]], args[map[2]], args[map[3]] };
                case 5: return args => new[] { args[map[0]], args[map[1]], args[map[2]], args[map[3]], args[map[4]] };
                case 6: return args => new[] { args[map[0]], args[map[1]], args[map[2]], args[map[3]], args[map[4]], args[map[5]] };
                case 7: return args => new[] { args[map[0]], args[map[1]], args[map[2]], args[map[3]], args[map[4]], args[map[5]], args[map[6]] };
                case 8: return args => new[] { args[map[0]], args[map[1]], args[map[2]], args[map[3]], args[map[4]], args[map[5]], args[map[6]], args[map[7]] };
                default:
                    throw new NotSupportedException("Type mapping with " + map.Length + " type parameters is not supported.");
            }
        }

#if DEBUG
        /// <summary>
        /// Resolve a service instance.
        /// </summary>
        /// <typeparam name="TService">The service type to resolve.</typeparam>
        /// <returns>An instance of the service.</returns>
        public TService Scoped<TService>(TService instance)
        {
            //FIXME: manually inject a scoped instance?
            if (scoped[Service<TService>.Index] != null && scoped[Service<TService>.Index] != (object)instance)
                throw new ArgumentException("An instance is already registered.");
            scoped[Service<TService>.Index] = instance;
            return instance;
        }
#endif

        /// <summary>
        /// Dispose of this dependency manager and any <seealso cref="IDisposable"/> instances it created.
        /// </summary>
        public void Dispose()
        {
            var x = Interlocked.Exchange(ref disposables, null);
            if (x != null)
            {
                var errors = new List<Exception>();
                foreach (var y in x)
                {
                    try
                    {
                        y.Dispose();
                    }
                    catch (Exception e)
                    {
                        errors.Add(e);
                    }
                }
                if (errors.Count > 0)
                    OnError?.Invoke(errors);
            }
        }

        /// <summary>
        /// The dependency manager destructor.
        /// </summary>
        ~Dependency()
        {
            Dispose();
        }
    }
}
