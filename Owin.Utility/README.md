# AWhewell.Owin.Utility

Utility classes that handle some of the repetitive jobs when dealing with
OWIN implementations.

The classes do not assume that the AWhewell.Owin library is being used. They
should be usable with any OWIN implementation.

All other AWhewell.Owin libraries are allowed to refer to this library but
this library is not allowed to refer to anything else.

## OwinContext
This class wraps an OWIN environment dictionary and provides type-safe access
to the mandatory and (some of) the recommended keys.

Some properties can only be set if they are either null or if you are not actually
changing their value. These are usually request properties. Note that this is just
a nicety, you can always set the appropriate keys in the underlying OWIN environment
if you need to - it just highlights accidental writes of values that middleware would
not normally be expected to change during processing.

### Construction
The default ctor creates a new environment dictionary and wraps that.

The ctor that takes an OWIN environment dictionary just wraps the dictionary that it's
been given.

#### Create
There is a static method called **Create** that take an environment and returns an
`OwinContext`. The method will check the environment for a key called `awowin.Context`,
if it exists then it's assumed to hold an existing OwinContext for the environment and
it returns that, otherwise it creates a new context, saves it under the custom key and
returns it.

The context caches some results that can potentially be expensive to keep calculating,
by using `Create` to store the context within the environment you can reuse those cached
results throughout the middleware chain.

### Properties
Setters marked as "1" can only be set if the current value is null, or if the new value
equals the current value.

| Property                    | Type                            | Get? | Set? | Key                       | Notes |
|-                            |-                                |:-:   |:-:   |-                          | - |
| `CallCancelled`             | `CancellationToken?`            | Y    | 1    | owin.CallCancelled        | See spec. |
| `Environment`               | `IDictionary<string, object>`   | Y    | N    | -                         | The OWIN environment that is being wrapped. |
| `RequestBody`               | `Stream`                        | Y    | 1    | owin.RequestBody          | See spec. |
| `RequestHeaders`            | `IDictionary<string, string[]>` | Y    | 1    | owin.RequestHeaders       | See spec. |
| `RequestHeadersDictionary`  | `RequestHeadersDictionary`      | Y    | N    | -                         | Wrapper around `RequestHeaders` |
| `RequestHttpMethod`         | `HttpMethod`                    | Y    | N    | -                         | `RequestMethod` parsed into an enum |
| `RequestHttpProtocol`       | `HttpProtocol`                  | Y    | N    | -                         | `RequestProtocol` parsed into an enum |
| `RequestHttpScheme`         | `HttpScheme`                    | Y    | N    | -                         | `RequestScheme` parsed into an enum |
| `RequestMethod`             | `string`                        | Y    | 1    | owin.RequestMethod        | See spec. |
| `RequestPath`               | `string`                        | Y    | 1    | owin.RequestPath          | See spec. |
| `RequestPathBase`           | `string`                        | Y    | 1    | owin.RequestPathBase      | See spec. |
| `RequestPathFlattened`      | `string`                        | Y    | N    | -                         | `RequestPathNormalised` with directory traversal path parts resolved |
| `RequestPathNormalised`     | `string`                        | Y    | N    | -                         | `RequestPath` with backslashes translated to forward-slashes and an empty path expressed as a single forward-slash |
| `RequestPathParts`          | `string[]`                      | Y    | N    | -                         | `RequestPath` split into chunks at the forward-slashes. Backslashes in chunks are preserved. |
| `RequestProtocol`           | `string`                        | Y    | 1    | owin.RequestProtocol      | See spec. |
| `RequestQueryString`        | `string`                        | Y    | 1    | owin.RequestQueryString   | See spec. |
| `RequestScheme`             | `string`                        | Y    | 1    | owin.RequestScheme        | See spec. |
| `ResponseBody`              | `Stream`                        | Y    | 1    | owin.ResponseBody         | See spec. |
| `ResponseHeaders`           | `IDictionary<string, string[]>` | Y    | 1    | owin.ResponseHeaders      | See spec. |
| `ResponseHeadersDictionary` | `ResponseHeadersDictionary`     | Y    | N    | -                         | Wrapper around `ResponseHeadersDictionary` |
| `ResponseHttpStatusCode`    | `System.Net.HttpStatusCode`     | Y    | Y    | -                         | `ResponseStatusCode` expressed as an enum |
| `ResponseProtocol`          | `string`                        | Y    | Y    | owin.ResponseProtocol     | See spec. |
| `ResponseReasonPhrase`      | `string`                        | Y    | Y    | owin.ResponseReasonPhrase | See spec. |
| `ResponseStatusCode`        | `int?`                          | Y    | Y    | owin.ResponseStatusCode   | See spec. |
| `ServerIsLocal`             | `bool?`                         | Y    | 1    | server.IsLocal            | See common keys in spec. |
| `ServerLocalIpAddress`      | `string`                        | Y    | 1    | server.LocalIpAddress     | See common keys in spec. |
| `ServerLocalPort`           | `string`                        | Y    | 1    | server.LocalPort          | See common keys in spec. |
| `ServerRemoteIpAddress`     | `string`                        | Y    | 1    | server.RemoteIpAddress    | See common keys in spec. |
| `ServerRemotePort`          | `string`                        | Y    | 1    | server.RemotePort         | See common keys in spec. |
| `SslClientCertificate`      | `X509Certificate`               | Y    | 1    | ssl.ClientCertificate     | See common keys in spec. |
| `Version`                   | `string`                        | Y    | 1    | owin.Version              | See spec. |
