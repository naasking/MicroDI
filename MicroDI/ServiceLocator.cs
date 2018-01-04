using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MicroDI
{
    /// <summary>
    /// The dependency manager.
    /// </summary>
    public class ServiceLocator : IDisposable
    {
        List<IDisposable> disposables = new List<IDisposable>();
        internal Dictionary<Type, object> scoped = new Dictionary<Type, object>();
        Dictionary<Type, Func<ServiceLocator, object>> monotypes;
        Dictionary<Type, GenericService> polytypes;
        ServiceRegistry registry;
        object[] dynamicRegister;
        
        internal ServiceLocator(ServiceRegistry registry)
            : base()
        {
            this.registry = registry;
            this.monotypes = registry.monotypes;
            this.polytypes = registry.polytypes;
            dynamicRegister = new object[] { registry };
        }

        #region Private methods
        /// <summary>
        /// Initialize a fresh instance of a type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal object Init<T>(T x, Type serviceType)
        {
            // check for scoped instance again due possible to circular dependencies
            object o;
            if (scoped.TryGetValue(serviceType, out o))
            {
                (x as IDisposable)?.Dispose();
                return o;
            }
            // scoped structs can't be saved until they are initialized
            if (x is ValueType)
                return scoped[serviceType] = Init(x);
            scoped[serviceType] = x;
            return Init(x);
        }

        /// <summary>
        /// Initialize a fresh instance of a type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal object Init<T>(T x)
        {
            Instance<T>.Init?.Invoke(this, x);
            var y = x as IDisposable;
            if (y != null)
                disposables.Add(y);
            return x;
        }
        #endregion

        /// <summary>
        /// Resolve a service instance.
        /// </summary>
        /// <typeparam name="TService">The service type to resolve.</typeparam>
        /// <returns>An instance of the service.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TService Resolve<TService>()
        {
            return (TService)Resolve(typeof(TService));
        }

        /// <summary>
        /// Resolve an instance of the given service type.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Resolve(Type service)
        {
            Func<ServiceLocator, object> resolve;
            if (monotypes.TryGetValue(service, out resolve))
                return resolve(this);
            GenericService info;
            if (service.IsConstructedGenericType && polytypes.TryGetValue(service.GetGenericTypeDefinition(), out info))
            {
                var targs = service.GenericTypeArguments;
                register[targs.Length - 1].MakeGenericMethod(targs).Invoke(info, dynamicRegister);
                return Resolve(service);
            }
            else
                throw new InvalidOperationException("Type " + service.Name + " has no registration.");
        }

        static readonly MethodInfo[] register = typeof(GenericService)
            .GetRuntimeMethods()
            .Where(x => x.Name == "Register")
            .OrderBy(x => x.GetGenericArguments().Length)
            .ToArray();
        
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
                    registry.RaiseErrors(errors);
            }
        }

        /// <summary>
        /// The dependency manager destructor.
        /// </summary>
        ~ServiceLocator()
        {
            Dispose();
        }
    }
}
