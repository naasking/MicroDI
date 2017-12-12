using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicroDI.Tests
{
    class Instance2 : IService2
    {
        public IService1 Service { get; private set; }
    }
}
