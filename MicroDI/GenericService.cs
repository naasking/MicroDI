using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroDI
{
    /// <summary>
    /// A dependency resolver for a generic type.
    /// </summary>
    public abstract class GenericService
    {
        /// <summary>
        /// Return an instance of a generic type.
        /// </summary>
        /// <typeparam name="TService">The service being constructed.</typeparam>
        /// <typeparam name="T0">The first generic parameter.</typeparam>
        /// <param name="registry">The dependency registry.</param>
        public virtual void Register<T0>(ServiceRegistry registry)
        {
            throw new NotSupportedException("Incorrect constructor invoked.");
        }
        /// <summary>
        /// Return an instance of a generic type.
        /// </summary>
        /// <typeparam name="TService">The service being constructed.</typeparam>
        /// <typeparam name="T0">The first generic parameter.</typeparam>
        /// <typeparam name="T1">The second generic parameter.</typeparam>
        /// <param name="registry">The dependency registry.</param>
        public virtual void Register<T0, T1>(ServiceRegistry registry)
        {
            throw new NotSupportedException("Incorrect constructor invoked.");
        }
        /// <summary>
        /// Return an instance of a generic type.
        /// </summary>
        /// <typeparam name="TService">The service being constructed.</typeparam>
        /// <typeparam name="T0">The first generic parameter.</typeparam>
        /// <typeparam name="T1">The second generic parameter.</typeparam>
        /// <typeparam name="T2">The third generic parameter.</typeparam>
        /// <param name="registry">The dependency registry.</param>
        public virtual void Register<T0, T1, T2>(ServiceRegistry registry)
        {
            throw new NotSupportedException("Incorrect constructor invoked.");
        }
        /// <summary>
        /// Return an instance of a generic type.
        /// </summary>
        /// <typeparam name="TService">The service being constructed.</typeparam>
        /// <typeparam name="T0">The first generic parameter.</typeparam>
        /// <typeparam name="T1">The second generic parameter.</typeparam>
        /// <typeparam name="T2">The third generic parameter.</typeparam>
        /// <typeparam name="T3">The fourth generic parameter.</typeparam>
        /// <param name="registry">The dependency registry.</param>
        public virtual void Register<T0, T1, T2, T3>(ServiceRegistry registry)
        {
            throw new NotSupportedException("Incorrect constructor invoked.");
        }
        /// <summary>
        /// Return an instance of a generic type.
        /// </summary>
        /// <typeparam name="TService">The service being constructed.</typeparam>
        /// <typeparam name="T0">The first generic parameter.</typeparam>
        /// <typeparam name="T1">The second generic parameter.</typeparam>
        /// <typeparam name="T2">The third generic parameter.</typeparam>
        /// <typeparam name="T3">The fourth generic parameter.</typeparam>
        /// <typeparam name="T4">The fifth generic parameter.</typeparam>
        /// <param name="registry">The dependency registry.</param>
        public virtual void Register<T0, T1, T2, T3, T4>(ServiceRegistry registry)
        {
            throw new NotSupportedException("Incorrect constructor invoked.");
        }
        /// <summary>
        /// Return an instance of a generic type.
        /// </summary>
        /// <typeparam name="TService">The service being constructed.</typeparam>
        /// <typeparam name="T0">The first generic parameter.</typeparam>
        /// <typeparam name="T1">The second generic parameter.</typeparam>
        /// <typeparam name="T2">The third generic parameter.</typeparam>
        /// <typeparam name="T3">The fourth generic parameter.</typeparam>
        /// <typeparam name="T4">The fifth generic parameter.</typeparam>
        /// <typeparam name="T5">The sixth generic parameter.</typeparam>
        /// <param name="registry">The dependency registry.</param>
        public virtual void Register<T0, T1, T2, T3, T4, T5>(ServiceRegistry registry)
        {
            throw new NotSupportedException("Incorrect constructor invoked.");
        }
        /// <summary>
        /// Return an instance of a generic type.
        /// </summary>
        /// <typeparam name="TService">The service being constructed.</typeparam>
        /// <typeparam name="T0">The first generic parameter.</typeparam>
        /// <typeparam name="T1">The second generic parameter.</typeparam>
        /// <typeparam name="T2">The third generic parameter.</typeparam>
        /// <typeparam name="T3">The fourth generic parameter.</typeparam>
        /// <typeparam name="T4">The fifth generic parameter.</typeparam>
        /// <typeparam name="T5">The sixth generic parameter.</typeparam>
        /// <typeparam name="T6">The seventh generic parameter.</typeparam>
        /// <param name="registry">The dependency registry.</param>
        public virtual void Register<T0, T1, T2, T3, T4, T5, T6>(ServiceRegistry registry)
        {
            throw new NotSupportedException("Incorrect constructor invoked.");
        }
        /// <summary>
        /// Return an instance of a generic type.
        /// </summary>
        /// <typeparam name="TService">The service being constructed.</typeparam>
        /// <typeparam name="T0">The first generic parameter.</typeparam>
        /// <typeparam name="T1">The second generic parameter.</typeparam>
        /// <typeparam name="T2">The third generic parameter.</typeparam>
        /// <typeparam name="T3">The fourth generic parameter.</typeparam>
        /// <typeparam name="T4">The fifth generic parameter.</typeparam>
        /// <typeparam name="T5">The sixth generic parameter.</typeparam>
        /// <typeparam name="T6">The seventh generic parameter.</typeparam>
        /// <typeparam name="T7">The eighth generic parameter.</typeparam>
        /// <param name="registry">The dependency registry.</param>
        public virtual void Register<T0, T1, T2, T3, T4, T5, T6, T7>(ServiceRegistry registry)
        {
            throw new NotSupportedException("Incorrect constructor invoked.");
        }
    }
}
