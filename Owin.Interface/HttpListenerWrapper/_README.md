# Owin.Interface.HttpListenerWrapper

These interfaces exist so that the **HttpListener** can be
unit tested.

The **IHttpListener** interface's default implementation is
a pass-through wrapper around HttpListener. Unit tests can
replace it with mocks and verify that the HttpListener host
is using HttpListener correctly.

The other interfaces around **HttpListenerContext**,
**HttpListenerRequest** and **HttpListenerResponse** do not
have implementations. They are interfaces around very thin
wrappers that are instantiated by the implementation of
**IHttpListener**. Unit tests can have their mock IHttpListener
return a mock context, request and response object.