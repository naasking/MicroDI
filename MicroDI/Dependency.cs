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
            //FIXME: add a circular dependency check on constructor parameters, similar to transient property check
            lock (typeof(Service<TService>))
            {
                RequiresEmptyRegistration<TService>();
                var i = Interlocked.Increment(ref instances) - 1;
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
        #endregion

        #region Private methods
        /// <summary>
        /// Initialize a fresh instance of a type.
        /// </summary>
        T Init<T>(T x, int scopeIndex)
        {
            // check for scoped instance again due possible to circular dependencies
            if (scoped[scopeIndex] != null)
                return (T)scoped[scopeIndex];
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
            foreach (var x in type.GetRuntimeProperties().Where(x => x.GetCustomAttributes<InjectDependencyAttribute>() != null))
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
            if (resolve == null)
                throw new InvalidOperationException("Type " + typeof(TService).Name + " has no registration.");
            return resolve(this);
        }
        
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
