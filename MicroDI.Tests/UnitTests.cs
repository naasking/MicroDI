using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MicroDI;

namespace MicroDI.Tests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void TestCircularScoped()
        {
            Dependency.Scoped<IService1, Instance1>();
            Dependency.Scoped<IService2, Instance2>();
            using (var deps = new Dependency())
            {
                var x = deps.Resolve<IService2>();
                Assert.IsNotNull(x);
                Assert.IsNotNull(x.Service);
                Assert.IsNotNull(x.Service.Self);
                Assert.IsNotNull(x.Service.Service2);
                Assert.AreEqual(x, x.Service.Service2);
                Assert.AreEqual(x.Service, x.Service.Self);
            }
            Dependency.Clear<IService1>();
            Dependency.Clear<IService2>();
        }

        [TestMethod]
        public void TestTransient()
        {
            Dependency.Scoped<IService1, Instance1>();
            Dependency.Transient<IService2, Instance2>();
            using (var deps = new Dependency())
            {
                var x = deps.Resolve<IService2>();
                Assert.IsNotNull(x);
                Assert.IsNotNull(x.Service);
                Assert.IsNotNull(x.Service.Self);
                Assert.IsNotNull(x.Service.Service2);
                Assert.AreNotEqual(x, x.Service.Service2);
                Assert.AreEqual(x.Service, x.Service.Self);
            }
            Dependency.Clear<IService1>();
            Dependency.Clear<IService2>();
        }
    }
}
