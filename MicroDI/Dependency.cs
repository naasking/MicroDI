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
        /// <typeparam name="TInstance">The type of the service instance.</typeparam>
        /// <typeparam name="TService">The service type.</typeparam>
        public static void Scoped<TService, TInstance>()
            where TInstance : TService, new()
        {
            lock (typeof(Service<TService>))
            {
                RequiresEmptyRegistration<TService>();
                var i = Service<TService>.Index = Service<TInstance>.Index =
                    Interlocked.Increment(ref instances) - 1;
                Service<TService>.Resolve =
                    deps => (TService)(deps.scoped[i] ?? deps.Init(new TInstance(), i));
            }
        }

        /// <summary>
        /// Define a scoped service registration.
        /// </summary>
        /// <param name="create">The custom constructor to create new instances.</param>
        /// <typeparam name="TInstance">The type of the service instance.</typeparam>
        /// <typeparam name="TService">The service type.</typeparam>
        public static void Scoped<TService, TInstance>(Func<TInstance> create)
            where TInstance : TService
        {
            lock (typeof(Service<TService>))
            {
                RequiresEmptyRegistration<TService>();
                var i = Service<TService>.Index = Service<TInstance>.Index =
                    Interlocked.Increment(ref instances);
                Service<TService>.Resolve =
                    deps => (TService)(deps.scoped[i] ?? deps.Init(create(), i));
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
        /// <typeparam name="TInstance">The type of the service instance.</typeparam>
        /// <typeparam name="TService">The service type.</typeparam>
        public static void Transient<TService, TInstance>(bool debug = false)
            where TInstance : TService, new()
        {
            lock (typeof(Service<TService>))
            {
                RequiresEmptyRegistration<TService>();
                // check for a circular dependency on TService in TInstance
                if (debug && IsCircular<TInstance, TService>(new HashSet<Type>()))
                    throw new ArgumentException("Type " + typeof(TInstance).Name + " has a circular dependency on " + typeof(TService).Name + " and so cannot have transient lifetime.");
                Service<TService>.Resolve = deps => deps.Init(new TInstance());
            }
        }

        /// <summary>
        /// Define a transient 
        /// </summary>
        /// <param name="create">The custom constructor to create new instances.</param>
        /// <typeparam name="TInstance">The type of the service instance.</typeparam>
        /// <typeparam name="TService">The service type.</typeparam>
        public static void Transient<TService, TInstance>(Func<TInstance> create, bool debug = false)
            where TInstance : TService
        {
            lock (typeof(Service<TService>))
            {
                RequiresEmptyRegistration<TService>();
                // check for a circular dependency on TService in TInstance
                if (debug && IsCircular<TInstance, TService>(new HashSet<Type>()))
                    throw new ArgumentException("Type " + typeof(TInstance).Name + " has a circular dependency on " + typeof(TService).Name + " and so cannot have transient lifetime.");
                Service<TService>.Resolve = deps => deps.Init(create());
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
            // register scoped instance before init in case of circular dependencies
            scoped[scopeIndex] = x;
            return Init(x);
        }
        /// <summary>
        /// Initialize a fresh instance of a type.
        /// </summary>
        T Init<T>(T x)
        {
            Instance<T>.Init(this, x);
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
            if (!visited.Add(type))
                return false;
            var stype = typeof(TService);
            foreach (var x in type.GetRuntimeProperties())
            {
                if (x.PropertyType == stype || x.PropertyType == type)
                    return true;
                else if (visited.Add(x.PropertyType) && IsCircular(x.PropertyType, stype, visited))
                    return true;
            }
            return false;
        }
        static bool IsCircular(Type instance, Type service, HashSet<Type> visited)
        {
            return (bool)isCircular.MakeGenericMethod(instance, service)
                                   .Invoke(null, new[] { visited });
        }
        #endregion

#if DEBUG
        /// <summary>
        /// Resolve a service instance.
        /// </summary>
        /// <typeparam name="TService">The service type to resolve.</typeparam>
        /// <returns>An instance of the service.</returns>
        public TService Scoped<TService>(TService instance)
        {
            //FIXME: manually inject a scoped instance?
            if (scoped[Service<TService>.Index] != null && (TService)scoped[Service<TService>.Index] != instance)
                throw new ArgumentException("An instance is already registered.");
            scoped[Service<TService>.Index] = instance;
            return instance;
        }
#endif

        /// <summary>
        /// Resolve a service instance.
        /// </summary>
        /// <typeparam name="TService">The service type to resolve.</typeparam>
        /// <returns>An instance of the service.</returns>
        public TService Scoped<TService>()
            where TService:new()
        {
            return Init(new TService());
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
