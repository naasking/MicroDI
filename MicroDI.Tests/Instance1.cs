using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicroDI.Tests
{
    class Instance1 : IService1
    {
        public IService1 Self { get; protected set; }
        public IService2 Service2 { get; protected set; }
    }
}
