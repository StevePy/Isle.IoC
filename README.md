# Isle.IoC
Container wrapper for Autofac

The ContainerRegistry is a registry class to wrap the Autofac IOC Container. This provides basic AutoFac support without requiring Autofac 
references within the application and simplifies registering and retrieving contained objects. The typical use-case for this class is
introducing dependency injection into legacy applications where changes to support constructor-based dependencies will be prohibitively 
time consuming. This class is more accurately a service locator implementation for the Autofac IOC container, however it is *not* intended 
to be used loosely as a service locator. To use this registry, structure your application to Register all classes and Singletons at the start, 
then use Resolve to pull those registered instances at any time afterwards.

The pattern this registry is intended for is lazy property injection. The registry should be the sole dependency injected via constructor 
or accessed via the Singleton Instance with all other dependencies set up using the lazy property signature below:

private readonly IContainerRegistry _containerRegistry = null; // Initialized in constructor.
private TDependency _dependency = null;
public TDependency Dependency
{
    get { return _dependency ?? ( _dependency = _containerRegistry.Resolve<TDependency>()); }
    set { _dependency = value; }
}

This class serves as a Singleton within the application. When injected as a dependency itself it should register itself within itself under 
the contract interface IContainerRegistry to inject itself into constructed classes. Worst case in legacy applications where adding 
constructor parameters or non-IoC initialized classes you can opt to use the Singleton Instance directly, though aim to only ever use it 
in Lazy Properties.

Service Locators in general are an anti-pattern. Accessing ContainerRegistry.Resolve willy-nilly throughout code is a bad practice, and as 
un-test-able as simply new-ing up concrete instances everywhere. By limiting its use to the lazy property structure above, it is easy to 
monitor everywhere the Resolve method is used, and any instances other than lazy properties should flag a discussion with the offending 
developer to outline the merits of using the pattern properly to keep code easily test-able.

The lazy property injection pattern easily facilitates testing by ensuring that unit tests can use the dependency property setter to provide
a mock of the dependency. Whereby constructor injection will require *all* dependencies to be mocked (save for allowing null references in 
the constructor, which I prefer to enforce all parameters to be required and asserted) the lazy property pattern means that tests only need 
to set dependencies that are relevant to the test at hand. The container can be set up to throw an exception if accessed via a test. (I don't
believe this version accommodates this as I normally pass the container as a single constructor parameter, but I'll look to revise this 
shortly to expose an alternate interceptor.)