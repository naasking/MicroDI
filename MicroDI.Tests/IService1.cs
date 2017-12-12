using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicroDI.Tests
{
    public interface IService1
    {
        IService1 Self { get; }
        IService2 Service2 { get; }
    }
}
