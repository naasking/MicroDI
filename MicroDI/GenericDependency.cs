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
    public abstract class GenericDependency
    {
        /// <summary>
        /// Return a constructor for a generic type.
        /// </summary>
        /// <typeparam name="TService">The service being constructed.</typeparam>
        /// <typeparam name="T0">The first generic parameter.</typeparam>
        /// <param name="container">The dependency container.</param>
        /// <returns>An instance of <typeparamref name="TService"/>.</returns>
        public virtual TService Constructor<TService, T0>(Dependency container)
        {
            throw new NotSupportedException("Incorrect constructor invoked.");
        }
        /// <summary>
        /// Return a constructor for a generic type.
        /// </summary>
        /// <typeparam name="TService">The service being constructed.</typeparam>
        /// <typeparam name="T0">The first generic parameter.</typeparam>
        /// <typeparam name="T1">The second generic parameter.</typeparam>
        /// <param name="container">The dependency container.</param>
        /// <returns>An instance of <typeparamref name="TService"/>.</returns>
        public virtual TService Constructor<TService, T0, T1>(Dependency container)
        {
            throw new NotSupportedException("Incorrect constructor invoked.");
        }
        /// <summary>
        /// Return a constructor for a generic type.
        /// </summary>
        /// <typeparam name="TService">The service being constructed.</typeparam>
        /// <typeparam name="T0">The first generic parameter.</typeparam>
        /// <typeparam name="T1">The second generic parameter.</typeparam>
        /// <typeparam name="T2">The third generic parameter.</typeparam>
        /// <param name="container">The dependency container.</param>
        /// <returns>An instance of <typeparamref name="TService"/>.</returns>
        public virtual TService Constructor<TService, T0, T1, T2>(Dependency container)
        {
            throw new NotSupportedException("Incorrect constructor invoked.");
        }
        /// <summary>
        /// Return a constructor for a generic type.
        /// </summary>
        /// <typeparam name="TService">The service being constructed.</typeparam>
        /// <typeparam name="T0">The first generic parameter.</typeparam>
        /// <typeparam name="T1">The second generic parameter.</typeparam>
        /// <typeparam name="T2">The third generic parameter.</typeparam>
        /// <typeparam name="T3">The fourth generic parameter.</typeparam>
        /// <param name="container">The dependency container.</param>
        /// <returns>An instance of <typeparamref name="TService"/>.</returns>
        public virtual TService Constructor<TService, T0, T1, T2, T3>(Dependency container)
        {
            throw new NotSupportedException("Incorrect constructor invoked.");
        }
        /// <summary>
        /// Return a constructor for a generic type.
        /// </summary>
        /// <typeparam name="TService">The service being constructed.</typeparam>
        /// <typeparam name="T0">The first generic parameter.</typeparam>
        /// <typeparam name="T1">The second generic parameter.</typeparam>
        /// <typeparam name="T2">The third generic parameter.</typeparam>
        /// <typeparam name="T3">The fourth generic parameter.</typeparam>
        /// <typeparam name="T4">The fifth generic parameter.</typeparam>
        /// <param name="container">The dependency container.</param>
        /// <returns>An instance of <typeparamref name="TService"/>.</returns>
        public virtual TService Constructor<TService, T0, T1, T2, T3, T4>(Dependency container)
        {
            throw new NotSupportedException("Incorrect constructor invoked.");
        }
        /// <summary>
        /// Return a constructor for a generic type.
        /// </summary>
        /// <typeparam name="TService">The service being constructed.</typeparam>
        /// <typeparam name="T0">The first generic parameter.</typeparam>
        /// <typeparam name="T1">The second generic parameter.</typeparam>
        /// <typeparam name="T2">The third generic parameter.</typeparam>
        /// <typeparam name="T3">The fourth generic parameter.</typeparam>
        /// <typeparam name="T4">The fifth generic parameter.</typeparam>
        /// <typeparam name="T5">The sixth generic parameter.</typeparam>
        /// <param name="container">The dependency container.</param>
        /// <returns>An instance of <typeparamref name="TService"/>.</returns>
        public virtual TService Constructor<TService, T0, T1, T2, T3, T4, T5>(Dependency container)
        {
            throw new NotSupportedException("Incorrect constructor invoked.");
        }
        /// <summary>
        /// Return a constructor for a generic type.
        /// </summary>
        /// <typeparam name="TService">The service being constructed.</typeparam>
        /// <typeparam name="T0">The first generic parameter.</typeparam>
        /// <typeparam name="T1">The second generic parameter.</typeparam>
        /// <typeparam name="T2">The third generic parameter.</typeparam>
        /// <typeparam name="T3">The fourth generic parameter.</typeparam>
        /// <typeparam name="T4">The fifth generic parameter.</typeparam>
        /// <typeparam name="T5">The sixth generic parameter.</typeparam>
        /// <typeparam name="T6">The seventh generic parameter.</typeparam>
        /// <param name="container">The dependency container.</param>
        /// <returns>An instance of <typeparamref name="TService"/>.</returns>
        public virtual TService Constructor<TService, T0, T1, T2, T3, T4, T5, T6>(Dependency container)
        {
            throw new NotSupportedException("Incorrect constructor invoked.");
        }
        /// <summary>
        /// Return a constructor for a generic type.
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
        /// <param name="container">The dependency container.</param>
        /// <returns>An instance of <typeparamref name="TService"/>.</returns>
        public virtual TService Constructor<TService, T0, T1, T2, T3, T4, T5, T6, T7>(Dependency container)
        {
            throw new NotSupportedException("Incorrect constructor invoked.");
        }
    }
}
