using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicroDI.Tests
{
    class Instance2 : IService2
    {
        public Instance2(IService1 service)
        {
            Service = service;
        }

        public IService1 Service { get; private set; }

        public int Bar
        {
            get { return 3; }
        }

        public string Foo { get; set; }
    }
}
