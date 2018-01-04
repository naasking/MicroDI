using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroDI
{
    /// <summary>
    /// Designates a property as requiring dependency resolution.
    /// </summary>
    public sealed class InjectServiceAttribute : Attribute
    {
    }
}
