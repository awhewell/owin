# Owin
An OWIN server for Virtual Radar Server version 3. The server targets
.NET Framework 4.6.1, Mono and DotNET Core 3+.

## Packages
* **AWhewell.Owin**: An OWIN compliant server.
* **AWhewell.Owin.Host.HttpListener**: HTTP.sys hosting for OWIN server.
* **AWhewell.Owin.Utility**: Utility classes usable with any OWIN server.
* **AWhewell.Owin.WebApi**: Web API framework usable with any OWIN server.

## Interfaces

The library uses the same class factory that Virtual Radar Server uses to break everything
up into interfaces and then register and obtain implementations of those interfaces.

See https://github.com/awhewell/interface-factory for more details.

## Building the Pipeline

Virtual Radar Server has a plugin architecture. Plugins are loaded at runtime. They must
be able to add middlware freely to the OWIN pipeline.

To facilitate this the library has a two-step process to create a pipeline:

1. At program startup the main program and all plugins register callbacks with `IPipelineBuilder`.

   This interface is not a singleton and all parts of the program that want to add to the pipeline
   need to register themselves with the same builder, so it is up to the program to arrange the
   sharing of a single builder instance.

   Each callback registered with the builder is assigned a priority. It is up to the program and
   the plugins to decide on how priorities are assigned.
2. Whenever a pipeline needs to be built the program should call `CreatePipeline` on the builder,
   passing into it a new instance of an `IPipelineBuilderEnvironment`.

   The builder will call each callback in ascending order of priority. Each callback is passed the
   pipeline builder environment. The environment has methods on it to tell the pipeline to use a
   middleware function.

   The environment also has a `Properties` dictionary that contains information about the server
   environment, as per section 4 (Application Startup) of the OWIN 1.0.0 spec.

The result of calling `CreatePipeline` is an `IPipeline` object. The pipeline has a `ProcessRequest`
method that can be used to send a request (as described by a standard OWIN environment dictionary)
through the pipeline.

## Starting the Server

The **Owin** package only deals with building the pipeline and processing the pipeline. It declares an
interface for hosts (`IHost`) but it does not contain an implementation.

As of time of writing there is one implementation of `IHost`, an `HttpListener` based implementation
in **Owin.Host.HttpListener**.

The interface for the HttpListener host is `IHostHttpListener`.

Every host has some standard properties that need to be supplied (the port to listen on etc., see
`IHost`).

Once a host has been instantiated and its properties set up you need to initialise it once with
a call to `Initialise`. This method is passed a pipeline builder and an environment. The host
sets up the environment properties, calls the builder to create a pipeline and then uses the
resulting pipeline to process requests.

This means that pipelines are tied to a single instance of a host, they are not shared between
hosts.

`IHost` contains methods to start and stop the server.

## Web API

One of the main reasons for writing a proprietary OWIN server for Virtual Radar Server is because
Microsoft's Web API is not cross platform. VRS needs a web API that runs unchanged across all three
.NET platforms.

The **Owin.WebApi** package is similar in operation to Microsoft's Web API 2 but with some notable
differences:

1. Controllers do not have a base class. Rather they must implement `IApiController`. The interface
   only has one property - `OwinEnvironment`, which is an environment dictionary. This always gets
   filled before the route method is called.

2. Public methods on a controller are not automatically exposed as endpoints. You must expose every
   endpoint with the `Route` attribute. At time of writing the route must be fully pathed but you
   can have parameters embedded in the route.

3. You can tag static methods as routes. The restriction is that the method must accept an OWIN
   environment dictionary as a parameter.

4. You can tag void methods as routes. The route will have to set up the OWIN environment for itself
   if it wants to return anything, although if it doesn't assign a status code then the API will
   return 200 on its behalf.

### Adding the Web API to a Pipeline

You must register a callback with the `IPipelineBuilder` that the program is using. The callback
must instantiate `IWebApiMiddleware`. This interface has a method called `CreateMiddleware`, you
call that to obtain a standard OWIN **AppFunc** and then pass that to the builder environment
function `UseMiddleware`.

The `IWebApiMiddleware` has properties that let you configure the web API's behaviour.

### Use with other OWIN environments

The web API depends on **Owin.Utility** but neither the web API nor the utility package depend on
the **Owin** package. You should be able to use the web API with any OWIN environment. You just
need to arrange for a call to `IWebApiMiddleware.CreateMiddleware` and then add it to the OWIN
pipeline as you would any other bit of middleware.

## Utility

The **Owin.Utility** package is a set of enums and helper classes that are agnostic to the
OWIN library in use. You should be able to use them with any OWIN environment.

The library does not use the class factory. It has no dependencies on any other package.

Some of the more useful classes it contains are:

* **OwinContext**: wraps an OWIN environment dictionary and exposes values in a type-safe manner.
* **OwinDictionary**: the base for all dictionaries exposed by the library. Similar to a normal
  dictionary except (a) it always has string keys and (b) it returns null when you index with a
  non-existent key, rather than throwing an exception.
* **EnvironmentKey**: A collection of const strings representing all standard OWIN keys.
* **CustomEnvironmentKey**: Custom keys that `OwinContext` adds to the environment.
 