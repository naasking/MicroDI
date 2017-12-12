using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

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
                var i = Interlocked.Increment(ref instances) - 1;
                Service<TService>.Resolve =
                    deps => (TService)(deps.scoped[i] ?? deps.Init(new TInstance(), i));
            }
        }

        /// <summary>
        /// Define a scoped service registration.
        /// </summary>
        /// <param name="create">The default constructor to use.</param>
        /// <typeparam name="TInstance">The type of the service instance.</typeparam>
        /// <typeparam name="TService">The service type.</typeparam>
        public static void Scoped<TService, TInstance>(Func<TInstance> create)
            where TInstance : TService
        {
            lock (typeof(Service<TService>))
            {
                RequiresEmptyRegistration<TService>();
                var i = Interlocked.Increment(ref instances);
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
        public static void Transient<TService, TInstance>()
            where TInstance : TService, new()
        {
            lock (typeof(Service<TService>))
            {
                RequiresEmptyRegistration<TService>();
                Service<TService>.Resolve = deps => deps.Init(new TInstance());
            }
        }

        /// <summary>
        /// Define a transient 
        /// </summary>
        /// <param name="create">The default constructor to use.</param>
        /// <typeparam name="TInstance">The type of the service instance.</typeparam>
        /// <typeparam name="TService">The service type.</typeparam>
        public static void Transient<TService, TInstance>(Func<TInstance> create)
            where TInstance : TService
        {
            lock (typeof(Service<TService>))
            {
                RequiresEmptyRegistration<TService>();
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
        #endregion

        /// <summary>
        /// Resolve a service instance.
        /// </summary>
        /// <typeparam name="TService">The service type to resolve.</typeparam>
        /// <returns>An instance of the service.</returns>
        public TService Resolve<TService>()
        {
            return Service<TService>.Resolve(this);
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
