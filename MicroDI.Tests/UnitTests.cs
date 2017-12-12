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
        [TestMethod]
        public void TestCircularScoped2()
        {
            Dependency.Scoped<IService1, Instance1>();
            Dependency.Scoped<IService2, Instance2>();
            using (var deps = new Dependency())
            {
                var x = deps.Resolve<IService2>();
                var y = deps.Resolve<IService1>();
                Assert.IsNotNull(x);
                Assert.IsNotNull(x.Service);
                Assert.IsNotNull(x.Service.Self);
                Assert.IsNotNull(x.Service.Service2);
                Assert.AreEqual(x, y.Service2);
                Assert.AreEqual(y, x.Service);
                Assert.AreEqual(x.Service, y.Self);
            }
            Dependency.Clear<IService1>();
            Dependency.Clear<IService2>();
        }

        [TestMethod]
        public void TestTransient2()
        {
            Dependency.Scoped<IService1, Instance1>();
            Dependency.Transient<IService2, Instance2>();
            using (var deps = new Dependency())
            {
                var y = deps.Resolve<IService1>();
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

        //FIXME: this fails because a transient Instance1 recursively references
        //itself, causing recursive initialization until stack overflow.
        [TestMethod]
        public void TestInvalidTransientCircular()
        {
            try
            {
                Dependency.Transient<IService1, Instance1>(true);
                Assert.Fail(nameof(Instance1) + " is circular with " + nameof(IService1));
            }
            catch (ArgumentException)
            {
            }
        }
    }
}
