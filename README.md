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

If you're after ultimate performance then you should use the overloads
that accept a delegate to construct an empty instance, even if your type
has a parameterless constructor. This is because the CLR's performance
on calling parameterless constructors of generic type arguments is
noticeably slower than a delegate invocation.

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

Therefore I decided on property-based DI. MicroDI thus requires
no runtime code generation and consists of only 3 simple classes
with around 250 lines of heavily commented code that you can
understand in minutes.

MicroDI is also quite efficient despite the lack of code generation
because of the way it exploits the CLR's generics to cache precomputed
delegates for constructing and initializing types (forked from the
IoCPerformance repo):

|Container|Version|Singleton|Transient|Property|Prepare And Register|Prepare And Register And Simple Resolve|
|---------|-------|---------|---------|--------|--------------------|---------------------------------------|
|No|-|133|158|211|4|5|
|DryIoc|2.11.5|98|117|170|115|415|
|LightInject|5.0.3|85|110|173|219|1095|
|MicroDI|1.0.0-RC1|48|125|954|202|252|

MicroDI's property slowdown is because it doesn't used code
generation.

The limited feature set may or may not be suitable for your
application. Fortunately, MicroDI is so simple you can probably
add any missing features.