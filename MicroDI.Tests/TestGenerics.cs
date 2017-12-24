using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroDI.Tests
{
    interface ITestGeneric<T0, T1>
    {
        IService1 Service { get; }
    }

    public class TestGeneric<T0, T1> : ITestGeneric<T1, T0>
    {
        public TestGeneric(IService1 service)
        {
            Service = service;
        }

        public IService1 Service { get; set; }
    }
}
