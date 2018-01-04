using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MicroDI
{
    /// <summary>
    /// A registry for service dependencies.
    /// </summary>
    public class ServiceRegistry
    {
        internal Dictionary<Type, Func<ServiceLocator, object>> monotypes = new Dictionary<Type, Func<ServiceLocator, object>>();
        internal Dictionary<Type, GenericService> polytypes = new Dictionary<Type, GenericService>();

        /// <summary>
        /// Generate a service locator.
        /// </summary>
        /// <returns></returns>
        public ServiceLocator GetLocator()
        {
            return new ServiceLocator(this);
        }

        /// <summary>
        /// The event that's fired when errors occur during disposal.
        /// </summary>
        public event Action<IEnumerable<Exception>> OnError;

        internal void RaiseErrors(IEnumerable<Exception> errors)
        {
            OnError?.Invoke(errors);
        }

        #region Monotype registrations
        /// <summary>
        /// Define a scoped service registration.
        /// </summary>
        /// <param name="create">The custom constructor to create new instances.</param>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TInstance">The instance type.</typeparam>
        public void Scoped<TService, TInstance>(Func<ServiceLocator, TInstance> create)
            where TInstance : TService
        {
            //FIXME: I could add a circularity check here on constructor parameters. However,
            //this isn't fully precise because we don't know which constructor will be invoked. Only
            //runtime tracing of invocation depth can handle both transient and scoped circularity checks.
            //Can probably do this with a customized ServiceLocator instance.
            var type = typeof(TService);
            monotypes.Add(type, locator =>
            {
                object o;
                return locator.scoped.TryGetValue(type, out o) ? o : locator.Init(create(locator), type);
            });
        }

        /// <summary>
        /// Declare a global instance.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="instance">The global instance.</param>
        public void Singleton<TService>(TService instance)
        {
            monotypes.Add(typeof(TService), locator => instance);
        }

        /// <summary>
        /// Define a transient 
        /// </summary>
        /// <param name="create">The custom constructor to create new instances.</param>
        /// <param name="debug">A circular dependency check is performed if true, otherwise the check is skipped.</param>
        /// <typeparam name="TInstance">The type of the service instance.</typeparam>
        /// <typeparam name="TService">The service type.</typeparam>
        public void Transient<TService, TInstance>(Func<ServiceLocator, TInstance> create, bool debug = false)
            where TInstance : TService
        {
            // check for a circular dependency on TService in TInstance
            if (debug && IsCircular<TInstance, TService>(new HashSet<Type>(new[] { typeof(TInstance) })))
                throw new ArgumentException("Type " + typeof(TInstance).Name + " has a circular dependency on " + typeof(TService).Name + " and so cannot have transient lifetime.");
            monotypes.Add(typeof(TService), locator => locator.Init(create(locator)));
        }
        #endregion

        #region Poltype registrations
        /// <summary>
        /// Register a generic service.
        /// </summary>
        /// <param name="typeDefinition"></param>
        /// <param name="register"></param>
        public void Register(Type typeDefinition, GenericService register)
        {
            if (typeDefinition.IsConstructedGenericType || !typeDefinition.GetTypeInfo().IsGenericTypeDefinition)
                throw new ArgumentException("Parameter must be a generic type definition.", "typeDefinition");
            polytypes.Add(typeDefinition, register);
        }
        #endregion

        #region Internal checks
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
            foreach (var x in type.GetRuntimeProperties().Where(x => x.GetCustomAttributes<InjectServiceAttribute>() != null))
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
    }
}
