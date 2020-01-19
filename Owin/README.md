# Owin

The implementation of interfaces in **Owin.Interface**.

The only public class in here is ````Implementations````. You should call this early in
the startup of the program and pass in ````Factory.Singleton```` from the interface
factory:

````
using InterfaceFactory;
...
AWhewell.Owin.Implementations.Register(Factory.Singleton);
````

Each package, except Owin.Utility, has its own Implementations class that needs to be initialised with
````Factory.Singleton````.

## Dependencies
* ````Owin.Interface````
* ````Owin.Utility````
