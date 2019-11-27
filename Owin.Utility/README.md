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

| Property                    | Type                            | Set? | Environment Key or Notes |
|-                            |-                                |:-:   |- |
| `CallCancelled`             | `CancellationToken?`            | 1    | `owin.CallCancelled` |
| `Environment`               | `IDictionary<string, object>`   | N    | The OWIN environment that is being wrapped. |
| `RequestBody`               | `Stream`                        | 1    | `owin.RequestBody` |
| `RequestHeaders`            | `IDictionary<string, string[]>` | 1    | `owin.RequestHeaders`  |
| `RequestHeadersDictionary`  | `RequestHeadersDictionary`      | N    | Wrapper around *RequestHeaders* |
| `RequestHttpMethod`         | `HttpMethod`                    | N    | *RequestMethod* parsed into an enum |
| `RequestHttpProtocol`       | `HttpProtocol`                  | N    | *RequestProtocol* parsed into an enum |
| `RequestHttpScheme`         | `HttpScheme`                    | N    | *RequestScheme* parsed into an enum |
| `RequestMethod`             | `string`                        | 1    | `owin.RequestMethod` |
| `RequestPath`               | `string`                        | 1    | `owin.RequestPath` |
| `RequestPathBase`           | `string`                        | 1    | `owin.RequestPathBase` |
| `RequestPathFlattened`      | `string`                        | N    | *RequestPathNormalised* with directory traversal path parts resolved |
| `RequestPathNormalised`     | `string`                        | N    | *RequestPath* with backslashes translated to forward-slashes and an empty path expressed as a single forward-slash |
| `RequestPathParts`          | `string[]`                      | N    | *RequestPath* split into chunks at the forward-slashes. Backslashes in chunks are preserved. |
| `RequestProtocol`           | `string`                        | 1    | `owin.RequestProtocol` |
| `RequestQueryString`        | `string`                        | 1    | `owin.RequestQueryString` |
| `RequestScheme`             | `string`                        | 1    | `owin.RequestScheme` |
| `ResponseBody`              | `Stream`                        | 1    | `owin.ResponseBody` |
| `ResponseHeaders`           | `IDictionary<string, string[]>` | 1    | `owin.ResponseHeaders` |
| `ResponseHeadersDictionary` | `ResponseHeadersDictionary`     | N    | Wrapper around *ResponseHeadersDictionary* |
| `ResponseHttpStatusCode`    | `System.Net.HttpStatusCode`     | Y    | *ResponseStatusCode* cast to an enum |
| `ResponseProtocol`          | `string`                        | Y    | `owin.ResponseProtocol` |
| `ResponseReasonPhrase`      | `string`                        | Y    | `owin.ResponseReasonPhrase` |
| `ResponseStatusCode`        | `int?`                          | Y    | `owin.ResponseStatusCode` |
| `ServerIsLocal`             | `bool?`                         | 1    | `server.IsLocal` |
| `ServerLocalIpAddress`      | `string`                        | 1    | `server.LocalIpAddress` |
| `ServerLocalPort`           | `string`                        | 1    | `server.LocalPort` |
| `ServerRemoteIpAddress`     | `string`                        | 1    | `server.RemoteIpAddress` |
| `ServerRemotePort`          | `string`                        | 1    | `server.RemotePort` |
| `SslClientCertificate`      | `X509Certificate`               | 1    | `ssl.ClientCertificate` |
| `Version`                   | `string`                        | 1    | `owin.Version` |
