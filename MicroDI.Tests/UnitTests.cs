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
            var r = new ServiceRegistry();
            r.Scoped<IService1, Instance1>(x => new Instance1());
            r.Scoped<IService2, Instance2>(x => new Instance2(x.Resolve<IService1>()));
            using (var deps = r.GetLocator())
            {
                var x = deps.Resolve<IService2>();
                Assert.IsNotNull(x);
                Assert.IsNotNull(x.Service);
                Assert.IsNotNull(x.Service.Self);
                Assert.IsNotNull(x.Service.Service2);
                Assert.AreEqual(x, x.Service.Service2);
                Assert.AreEqual(x.Service, x.Service.Self);
            }
        }

        [TestMethod]
        public void TestTransient()
        {
            var r = new ServiceRegistry();
            r.Scoped<IService1, Instance1>(x => new Instance1());
            r.Transient<IService2, Instance2>(x => new Instance2(x.Resolve<IService1>()));
            using (var deps = r.GetLocator())
            {
                var x = deps.Resolve<IService2>();
                Assert.IsNotNull(x);
                Assert.IsNotNull(x.Service);
                Assert.IsNotNull(x.Service.Self);
                Assert.IsNotNull(x.Service.Service2);
                Assert.AreNotEqual(x, x.Service.Service2);
                Assert.AreEqual(x.Service, x.Service.Self);
            }
        }
        [TestMethod]
        public void TestCircularScoped2()
        {
            var r = new ServiceRegistry();
            r.Scoped<IService1, Instance1>(x => new Instance1());
            r.Scoped<IService2, Instance2>(x => new Instance2(x.Resolve<IService1>()));
            using (var deps = r.GetLocator())
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
        }

        [TestMethod]
        public void TestTransient2()
        {
            var r = new ServiceRegistry();
            r.Scoped<IService1, Instance1>(x => new Instance1());
            r.Transient<IService2, Instance2>(x => new Instance2(x.Resolve<IService1>()));
            using (var deps = r.GetLocator())
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
        }

        [TestMethod]
        public void TestInvalidTransientCircular()
        {
            // ensure circular dependencies for transients fail
            var r = new ServiceRegistry();
            try
            {
                r.Transient<IService1, Instance1>(x => new Instance1(), true);
                Assert.Fail(nameof(Instance1) + " is circular with " + nameof(IService1));
            }
            catch (ArgumentException)
            {
                return;
            }
            Assert.Fail("Expected circular dependency error");
        }
        
        [TestMethod]
        public void TestInvalidTransient()
        {
            var r = new ServiceRegistry();
            r.Transient<ITransient1, Transient1>(x => new Transient1());
        }

        class TestGenericService : GenericService
        {
            public override void Register<T0, T1>(ServiceRegistry registry)
            {
                registry.Scoped<ITestGeneric<T0, T1>, TestGeneric<T1, T0>>(x => new TestGeneric<T1, T0>(x.Resolve<IService1>()));
            }
        }

        [TestMethod]
        public void TestGeneric1()
        {
            var r = new ServiceRegistry();
            r.Register(typeof(ITestGeneric<,>), new TestGenericService());
            r.Scoped<IService1, Instance1>(x => new Instance1());
            r.Scoped<IService2, Instance2>(x => new Instance2(x.Resolve<IService1>()));
            using (var deps = r.GetLocator())
            {
                var y = deps.Resolve<ITestGeneric<int, int>>();
                var x = deps.Resolve<ITestGeneric<string, int>>();
                Assert.IsNotNull(x);
                Assert.IsNotNull(x.Service);
                Assert.IsNotNull(y);
                Assert.IsNotNull(y.Service);
                Assert.AreEqual(x.Service, y.Service);
            }
        }
    }
}
