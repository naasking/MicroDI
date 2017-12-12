using System;
using System.Collections.Generic;
using System.Linq;

namespace Injector
{
    static class Service<T>
    {
        /// <summary>
        /// A cached delegate that efficiently resolves a service.
        /// </summary>
        public static Func<Dependency, T> Resolve;
    }
}
