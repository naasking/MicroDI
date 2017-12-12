using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Injector
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
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TService"></typeparam>
        public static void Scoped<TService, TInstance>()
            where TInstance : TService, new()
        {
            RequiresEmptyRegistration<TService>();
            var i = instances++;
            Service<TService>.Resolve =
                deps => (TService)(deps.scoped[i] ?? (deps.scoped[i] = deps.Fresh<TInstance>()));
        }

        /// <summary>
        /// Declare a global instance.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="instance"></param>
        public static void Singleton<TService>(TService instance)
        {
            RequiresEmptyRegistration<TService>();
            Service<TService>.Resolve = deps => instance;
        }

        /// <summary>
        /// Define a transient 
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <typeparam name="TService"></typeparam>
        public static void Transient<TService, TInstance>()
            where TInstance : TService, new()
        {
            RequiresEmptyRegistration<TService>();
            Service<TService>.Resolve = deps => deps.Fresh<TInstance>();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Create a fresh instance of a type.
        /// </summary>
        T Fresh<T>()
            where T : new()
        {
            var x = new T();
            Instance<T>.Init(this, x);
            if (x is IDisposable)
                disposables.Add(x as IDisposable);
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
                var raise = OnError;
                if (errors.Count > 0 && raise != null)
                    raise(errors);
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
