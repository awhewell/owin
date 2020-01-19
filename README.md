# Owin
An OWIN server and Web API library for .NET Framework 4.6.1+, Mono and DotNET Core 3+.

## Why?

This project came about because Microsoft's OWIN library only targets .NET Framework, and I had
a need for an OWIN library and Web API framework that targets both .NET Framework and DotNET Core.
The .NET Framework build had to be capable of running under Mono.

I considered using Microsoft's OWIN under the .NET Framework / Mono and DotNet Core's web pages
under DotNET Core. The intention was to write an abstraction layer that would hide the implementation
details from the rest of the program and behind the scenes it would use the appropriate library for
the platform that was being built. However I think that would have got messy, especially considering
that both web API schemes involve base classes for the controllers. Those base classes would be hard
to hide from things that are using the abstraction.

I did consider using Nancy. However, Nancy does not support weak wildcards when configuring HTTP.SYS
profiles and there was no appetite to add support for them. I needed weak wildcard support.


## Packages
* **AWhewell.Owin**: An OWIN compliant server.
* **AWhewell.Owin.Host.HttpListener**: HTTP.sys hosting for OWIN server.
* **AWhewell.Owin.Host.Ram**: In-memory hosting for OWIN server.
* **AWhewell.Owin.Utility**: Utility classes usable with any OWIN server.
* **AWhewell.Owin.WebApi**: Web API framework usable with any OWIN server.

## Interfaces

With the exception of **Owin.Utility**, which is intended to be agnostic to the OWIN implementation
that it is used with, the packages make extensive use of interfaces. The program does not
instantiate classes directly, instead it uses a class factory to create instances of interfaces.

The OWIN library was written for use with [Virtual Radar Server](https://github.com/vradarserver/vrs)
so it uses the same class factory that VRS uses. See the repository for more details:

https://github.com/awhewell/interface-factory

#### Initialisation

You need to initialise the Owin library on startup:

````
using InterfaceFactory;
...
AWhewell.Owin.Implementations.Register(Factory.Singleton);
````

Each package, except Owin.Utility, has its own ````Implementations```` class that needs to be
initialised with ````Factory.Singleton````.

#### Creating an instance of an interface

````
var pipelineBuilder = Factory.Resolve<IPipelineBuilder>();
pipelineBuilder.RegisterCallback(... etc. ...);
````



## Building the Pipeline

Virtual Radar Server has a plugin architecture. Plugins are loaded at runtime. They must
be able to add middleware freely to the OWIN pipeline.

One of the issues that VRS faced with the Microsoft OWIN pipeline is that it needed middleware
to be added in the same order that it appears in the pipeline. That is hard to arrange when some
of the middleware is spread across plugins that are not known about until runtime.

The intention with this library is to support the registration of pipeline middleware in any order.
This support involves associating a priority with each bit of middleware.

1. At program startup the main program and all plugins register callbacks with `IPipelineBuilder`.

   This interface is not a singleton and all parts of the program that want to add to the pipeline
   need to register themselves with the same builder, so it is up to the program to arrange the
   sharing of a single builder instance.

   Each callback registered with the builder is assigned a priority. It is up to the program and
   the plugins to decide how priorities are assigned.

2. Whenever a pipeline needs to be built the program should call `CreatePipeline` on the builder,
   passing into it a new instance of an `IPipelineBuilderEnvironment`.

   The builder will call each callback in ascending order of priority. Each callback is passed the
   pipeline builder environment. The environment has methods on it to register AppFunc builders in
   a similar fashion to Microsoft's OWIN library. 

   The environment also has a `Properties` dictionary that contains information about the server
   environment, as per section 4 (Application Startup) of the OWIN 1.0.0 spec.

The result of calling `CreatePipeline` is an `IPipeline` object. The pipeline has a `ProcessRequest`
method that can be used to send a request (as described by a standard OWIN environment dictionary)
through the pipeline.

### Middleware vs. Stream Manipulators

The Microsoft OWIN library breaks middleware into discrete functions that are chained together
to form the processing pipeline for a web request.

The **Owin** package adds the concept of stream manipulators. Virtual Radar Server has a requirement
for tasks that modify the response that middleware creates - for example, a task that might compress
the result if the request asked for compression. These tasks differ from normal middleware in that
they must not be skippable. Normal middleware will not be called if an earlier middleware function
decided not to call the next middleware in the chain. That can't happen with stream manipulators,
they are always called regardless of what happened in the middleware chain.


The `IPipelineBuilderEnvironment` object has a method called `UseStreamManipulatorBuilder` which
creates an AppFunc middleware function. Stream manipulators are identical to middleware in all but
three aspects:

1. They have their own set of priorities and are always called *after* the middleware chain has
   been called.

2. The builder is passed a "next" AppFunc, just like a middleware builder. However, this "next"
   AppFunc is a stub. It does not matter whether the stream manipulator AppFunc calls it or not,
   it will not influence whether the other stream manipulators get called.

3. If a stream manipulator is registered then the `IPipeline` implementation will replace the response
   stream with a memory stream. It will copy the content of the memory stream back to the original
   response stream once all of the stream manipulators have finished running.

   The switching out of the host's response stream with a memory stream lets stream manipulators work
   with hosts that use a forward-only response stream.

## Building and Running the Server

The **Owin** package only deals with building and processing the pipeline. It declares an
interface for hosts (`IHost`) but it does not contain an implementation.

As of time of writing there are two implementations of `IHost`:

* An `HttpListener` based implementation in **Owin.Host.HttpListener**. The interface is
  `IHostHttpListener`.
* An in-memory implementation in **Owin.Host.Ram**. The interface is `IHostRam`.

Every host has some standard properties that need to be supplied (the port to listen on etc., see
`IHost`).

Once a host has been instantiated and its properties set up you need to initialise it once with
a call to `Initialise`. This method is passed a pipeline builder and an environment. The host
sets up the environment properties, calls the builder to create a pipeline and then uses the
resulting pipeline to process requests.

This means that pipeline instances are tied to a single instance of a host, they are not shared
between hosts.

`IHost` contains methods to start and stop the server.

## Web API

One of the main reasons for writing a proprietary OWIN server for Virtual Radar Server is because
Microsoft's Web API is not cross platform. VRS needs a web API that runs unchanged across all three
.NET platforms.

The **Owin.WebApi** package is similar in operation to Microsoft's Web API 2 but with some notable
differences:

1. Controllers do not have a base class. Instead they must implement `IApiController`. The interface
   only has one property - `OwinEnvironment`, which is an environment dictionary. This always gets
   filled before the route method is called.

2. Public methods on a controller are not automatically exposed as endpoints. You must expose every
   endpoint with the `Route` attribute. This is more cumbersome than Microsoft's approach but it
   should make it harder to accidentally expose methods.

3. You can tag static methods as routes. The restriction is that the method must accept an OWIN
   environment dictionary as one of its parameters.

4. You can tag void methods as routes. The route will have to set up the OWIN environment for itself
   if it wants to return anything, although if it doesn't assign a status code then the API will
   return 200 on its behalf.

### Adding the Web API to a Pipeline

You must register a callback with the `IPipelineBuilder` that the program is using. The callback
must instantiate `IWebApiMiddleware`. This interface has a method called `AppFuncBuilder`, you
call that to obtain a standard OWIN **AppFunc** and then pass that to the builder environment
function `UseMiddlewareBuilder`.

The `IWebApiMiddleware` has properties that let you configure the web API's behaviour.

### Use with other OWIN environments

The web API depends on **Owin.Utility** but neither the web API nor the utility package depend on
the **Owin** package. You should be able to use the web API with any OWIN environment. You just
need to arrange for a call to `IWebApiMiddleware.AppFuncBuilder` and then add it to the OWIN
pipeline as you would any other bit of middleware.

## Utility

The **Owin.Utility** package is a set of enums and helper classes that are agnostic to the
OWIN library in use. You should be able to use them with any OWIN environment.

The library does not use the class factory. It has no dependencies on any other package.
