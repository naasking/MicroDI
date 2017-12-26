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

        #region Dependency registrations
        static int instances = 0;
        static Dictionary<Type, DependencyInfo> ctors = new Dictionary<Type, DependencyInfo>();

        struct DependencyInfo
        {
            public bool Scoped;
            public GenericDependency Dependency;
        }

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

        public static void Scoped(Type service, MicroDI.GenericDependency create)
        {
            lock (ctors)
            {
                ctors.Add(service, new DependencyInfo { Scoped = true, Dependency = create });
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
            var service = typeof(TService);
            DependencyInfo info;
            if (service.IsConstructedGenericType && ctors.TryGetValue(service.GetGenericTypeDefinition(), out info))
            {
                var targs = service.GenericTypeArguments;
                var ctor = Map<TService>(service, info.Dependency, ref targs);
                Service<TService>.Resolve = resolve = (Func<Dependency, TService>)ctor.GetMethodInfo()
                    .GetGenericMethodDefinition()
                    .MakeGenericMethod(targs)
                    .CreateDelegate(typeof(Func<Dependency, TService>), info.Dependency);
                return resolve(this);
            }
            else
                throw new InvalidOperationException("Type " + typeof(TService).Name + " has no registration.");
        }

        static Func<Dependency, TService> Map<TService>(Type service, GenericDependency info, ref Type[] typeArgs)
        {
            switch (typeArgs.Length)
            {
                case 1:
                    typeArgs = new[] { service, typeArgs[0] };
                    return new Func<Dependency, TService>(info.Constructor<TService, int>);
                case 2:
                    typeArgs = new[] { service, typeArgs[0], typeArgs[1] };
                    return new Func<Dependency, TService>(info.Constructor<TService, int, int>);
                case 3:
                    typeArgs = new[] { service, typeArgs[0], typeArgs[1], typeArgs[2] };
                    return new Func<Dependency, TService>(info.Constructor<TService, int, int, int>);
                case 4:
                    typeArgs = new[] { service, typeArgs[0], typeArgs[1], typeArgs[2], typeArgs[3] };
                    return new Func<Dependency, TService>(info.Constructor<TService, int, int, int, int>);
                case 5:
                    typeArgs = new[] { service, typeArgs[0], typeArgs[1], typeArgs[2], typeArgs[3], typeArgs[4] };
                    return new Func<Dependency, TService>(info.Constructor<TService, int, int, int, int, int>);
                case 6:
                    typeArgs = new[] { service, typeArgs[0], typeArgs[1], typeArgs[2], typeArgs[3], typeArgs[4], typeArgs[5] };
                    return new Func<Dependency, TService>(info.Constructor<TService, int, int, int, int, int, int>);
                case 7:
                    typeArgs = new[] { service, typeArgs[0], typeArgs[1], typeArgs[2], typeArgs[3], typeArgs[4], typeArgs[5], typeArgs[6] };
                    return new Func<Dependency, TService>(info.Constructor<TService, int, int, int, int, int, int, int>);
                case 8:
                    typeArgs = new[] { service, typeArgs[0], typeArgs[1], typeArgs[2], typeArgs[3], typeArgs[4], typeArgs[5], typeArgs[6], typeArgs[7] };
                    return new Func<Dependency, TService>(info.Constructor<TService, int, int, int, int, int, int, int, int>);
                default:
                    throw new NotSupportedException("Unrecognized constructor: " + typeArgs.Length + " type parameters.");
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
