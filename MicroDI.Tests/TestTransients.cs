using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroDI.Tests
{
    public interface ITransient1
    {
        void DoSomething();
    }
    
    public class Transient1 : ITransient1
    {
        private static int counter;
        
        public Transient1()
        {
            System.Threading.Interlocked.Increment(ref counter);
        }

        public static int Instances
        {
            get { return counter; }
            set { counter = value; }
        }

        public void DoSomething()
        {
            Console.WriteLine("World");
        }
    }
}
