using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MicroDI
{
    /// <summary>
    /// The class that initializes new instances.
    /// </summary>
    /// <typeparam name="T">The type to be initialized.</typeparam>
    static class Instance<T>
    {
        /// <summary>
        /// Initialize an instance via property setters.
        /// </summary>
        public static readonly Action<Dependency, T> Init;
        static Instance()
        {
            var type = typeof(T);
            var mkSetter = new Func<Action<T, int>, Action<Dependency, T>>(Set<int>)
                .GetMethodInfo()
                .GetGenericMethodDefinition();
            var init = new[] { type, type };
            var args = new object[] { null };
            var dispatch = new List<Action<Dependency, T>>();
            foreach (var x in type.GetRuntimeProperties())
            {
                if (x.SetMethod != null)
                {
                    init[1] = x.PropertyType;
                    args[0] = x.SetMethod.CreateDelegate(typeof(Action<,>).MakeGenericType(init));
                    dispatch.Add((Action<Dependency, T>)
                        mkSetter.MakeGenericMethod(x.PropertyType)
                                .Invoke(null, args));
                }
            }
            Init = (Action<Dependency, T>)Delegate.Combine(dispatch.ToArray());
        }

        static Action<Dependency, T> Set<TProperty>(Action<T, TProperty> setter)
        {
            return (deps, obj) => setter(obj, deps.Resolve<TProperty>());
        }
    }
}
