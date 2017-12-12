# MicroDI

This is a tiny but very efficient property-based dependency injector
for .NET standard 1.0 and up. It has no dependencies and performs
no runtime code generation, thus making it suitable for environments
where runtime codegen is difficult, expensive or impossible.

# Usage

The typical lifetimes are supported:

    Dependency.Single<IService, ServiceInstance>(instance);
    Dependency.Transient<IService, ServiceInstance>();
    Dependency.Scoped<IService, ServiceInstance>();

The transient and scoped calls require that ServiceInstance have
a parameterless constructor. You can optionally also provide a
constructor delegate if this isn't possible:

    Dependency.Transient<IService, ServiceInstance>(() => new ServiceInstance(...));
    Dependency.Scoped<IService, ServiceInstance>(() => new ServiceInstance(...));

Then you can create an instance of the dependency manager:

    using(var deps = new Dependency())
	{
		var service1 = deps.Resolve<IService1>();
		var service2 = deps.Resolve<IService2>();
		...
	}

The dependency manager will dispose of any IDisposable transient
or scoped instances when it's disposed.

# Circular Dependencies

MicroDI supports arbitary circular dependencies with one
exception: a *transient* instance cannot circularly depend on
the service it satisfies. Consider:

    public class Foo : IService
	{
		public IService Service { get; private set; }
	}
	...
	Dependency.Transient<IService, Foo>();

When resolving IService, MicroDI creates an instance of Foo
and tries to initialize it. While initializing the Foo.Service
property, it tries to resolve IService. Since IService is
registered as a transient, MicroDI again creates an
instance of Foo and initializes it, and it will continue this
recursively until a StackOverflowException is thrown.

Dependency.Transient can thus optionally perform a circular
dependency check at registration time:

    // throws ArgumentException if ServiceInstance transitively
	// depends on IService
    Dependency.Transient<IService, ServiceInstance>(debug:true);

# Why?

This started as an exploratory project to see how simple I could
make a performant DI. I was a little dissatisfied with the overly
complicated DI frameworks and just wanted something simple and
extensible. Typical DI frameworks perform plenty of runtime code
generation in order to efficiently call a type's constructor.
This is unfortunately necessary given the design of .NET.

I decided on property-based DI, and so MicroDI requires
no runtime code generation and consists of only 3 simple classes
with around 250 lines of heavily commented code that you can
understand in minutes.

MicroDI is also extremely efficient because of the way it exploits
the CLR's generics to cache precomputed delegates for constructing
and initializing types.

The limited feature set may or may not be suitable for your
application. Fortunately, MicroDI is so simple you can probably
add any missing features.