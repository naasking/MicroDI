using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicroDI.Tests
{
    public interface IService2
    {
        IService1 Service { get; }
    }
}
