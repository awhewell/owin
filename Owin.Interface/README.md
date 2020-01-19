# Owin

The core interfaces for the Owin library.

The documentation is not exhaustive. Consult the XML comments on the interfaces for
full documentation.

## IPipelineBuilder

#### ````RegisterCallback````

This is used to register callbacks and the order in which they will be called.

A monolithic program where all of the middleware is known at compile time only needs to
register a single callback.

A plugin architecture program can use the priorities to ensure that the callbacks are
arranged into the order in which middleware should appear in the pipeline.

#### ````CreatePipeline````

This takes an ````IPipelineBuilderEnvironment```` and passes it to every registered
callback in ascending order of priority.

The callback is expected to make calls on the environment object to register functions
that will create OWIN ````AppFunc```` Tasks. These are typically methods on a middleware
object that accept the next AppFunc in the chain and call it if the request should be
sent along the pipeline (or don't call it if the request needs no further processing).

### Examples

At runtime this will register a callback that will be called every time a new pipeline
is created.

The callback creates an instance of a ````Middleware```` class and passes a reference to
the ````AppFuncBuilder```` function on that class to the environment. Note that it is just
telling the environment about the builder function. It is not calling the builder function.

Later on when all of the callbacks have been called (in this case there is only one callback,
but in more complicated examples there could be dozens) the **Owin** library will call all
of the functions passed to ````UseMiddlewareBuilder```` to produce a chain of tasks. This
chain will become the OWIN specification's *Application Delegate* and will be called with
each request that comes in.

```csharp
public static class Program
{
    public static void Main()
    {
        ...
        var pipelineBuilder = Factory.Resolve<IPipelineBuilder>();
        pipelineBuilder.RegisterCallback(AddPipelineSection, 0);

        // Use the HttpListener host - you need to use the Host.HttpListener package
        // for this and reference the AWhewell.Owin.Interface.Host namespace.
        using(var host = Factory.Resolve<IHostHttpListener>()) {
            var pipelineBuilderEnvironment = Factory.Resolve<IPipelineBuilderEnvironment>();
            host.Initialise(pipeline, pipelineBuilderEnvironment);

            // Add host configuration here...

            host.Start();

            // Add code here to wait for an event signalling program shutdown...

            host.Stop();
        }
    }

    static void AddPipelineSection(IPipelineBuilderEnvironment environment)
    {
        var middleware = new Middleware();
        environment.UseMiddlewareBuilder(middleware.AppFuncBuilder);
    }
}

class Middleware
{
    public Func<IDictionary<string, object>, Task> AppFuncBuilder(Func<IDictionary<string, object>, Task> next)
    {
        return async(IDictionary<string, object> env) =>
        {
            if((string)env["owin.RequestPath"] != "/blocked") {
                await next(env);
            }
        };
    }
}
````

## IPipelineBuilderEnvironment

Contains methods and configuration properties that applications and hosts can use
to configure the pipeline and influence its behaviour. Every callback registered with
````IPipelineBuilder```` is passed one of these.

#### ````Properties````

A dictionary of values added by the host. The dictionary always starts with these keys and
callbacks are free to add their own if required:

| Key             | Type   | Meaning |
| -               | -      | -       |
| owin.Version    | string | The version of the OWIN spec that the library complies with |
| server.HostType | string | The implementation of ````IHost```` that is building the pipeline |

There are some keys that are only intended for use by hosts:

| Key                      | Type   | Meaning |
| -                        | -      | -       |
| server.HostFinalCallback | Action | An action to call after every callback has been called |

The ````HostFinalCallback```` is intended for use by hosts that need to ensure that certain environment
values are configured a particular way. For example, if a host wants to do its own exception handling
then it might register a callback to force the exception handling flags after all of the callbacks
have run.

#### ````UseMiddlewareBuilder````

Accepts a reference to function that has the signature:

````Func<IDictionary<string, object>, Task> FUNCTION (Func<IDictionary<string, object>, Task> next)````

Usually ````Func<IDictionary<string, object>, Task>```` is assigned the name "**AppFunc**":

````using AppFunc = Func<IDictionary<string, object>, Task>;````

in which case the signature becomes:

````AppFunc FUNCTION (AppFunc next)````

The **AppFunc** is the OWIN application delegate as described in the OWIN specification. It is
the middleware task. The builder function creates and returns a new Task that represents this
instance of middleware. The Task that it returns should chain to the *next* task that was passed
to the builder if it wants the request to continue processing, or it should return without calling
*next* if processing is to stop.

#### ````UseStreamManipulatorBuilder````

As per ````UseMiddlewareBuilder```` but it registers a function that will create stream
manipulator middleware. This is optional OWIN compliant middleware that ````IPipeline```` guarantees
will always be called for all requests after all of the middleware has finished running.

Note that stream manipulators are NOT a part of the OWIN specification. They are there to
make it easier to implement middleware like stream compressors that have to work with all responses.
If you want to make it easy to switch to a different OWIN implementation in the future then do not
use stream manipulators, stick to standard middleware instead.

#### ````UseExceptionLogger````

Registers an object that will be called when the pipeline would like to log an exception. If
no object is registered then exceptions will not be caught or logged.

The exception logger is not a part of the OWIN specification. Do not use exception loggers if
you want to write middleware that can be easily ported to other OWIN libraries.

## IPipeline

Applications don't usually get to see the pipeline - they are typically created and called by
````IHost```` implementations.

They hold the OWIN specification *Application Delegate* that requests can be sent through via
the **ProcessRequest** method.

## IHost

Applications should not instantiate ````IHost```` interfaces directly. It is an interface common to all
hosts. Applications should instantiate a host implementation instead (e.g. Owin.Interface.Host.IHostRam
for an in-memory implementation of IHost, or Owin.Interface.Host.IHostHttpListener for an implementation
that uses ````HttpListener```` / HTTP.sys.)
