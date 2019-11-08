// Copyright © 2019 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AWhewell.Owin.Interface.Host.HttpListener.HttpListenerWrapper
{
    /// <summary>
    /// The interface for objects that wrap an HttpListenerRequest.
    /// </summary>
    public interface IHttpListenerRequest
    {
        //
        // Summary:
        //     Gets a System.Boolean value that indicates whether the TCP connection used to
        //     send the request is using the Secure Sockets Layer (SSL) protocol.
        //
        // Returns:
        //     true if the TCP connection is using SSL; otherwise, false.
        bool IsSecureConnection { get; }

        //
        // Summary:
        //     Gets the server IP address and port number to which the request is directed.
        //
        // Returns:
        //     A System.String that contains the host address information.
        string UserHostAddress { get; }

        //
        // Summary:
        //     Gets the user agent presented by the client.
        //
        // Returns:
        //     A System.String object that contains the text of the request's User-Agent header.
        string UserAgent { get; }

        //
        // Summary:
        //     Gets the Uniform Resource Identifier (URI) of the resource that referred the
        //     client to the server.
        //
        // Returns:
        //     A System.Uri object that contains the text of the request's System.Net.HttpRequestHeader.Referer
        //     header, or null if the header was not included in the request.
        Uri UrlReferrer { get; }

        //
        // Summary:
        //     Gets the System.Uri object requested by the client.
        //
        // Returns:
        //     A System.Uri object that identifies the resource requested by the client.
        Uri Url { get; }

        //
        // Summary:
        //     Gets the System.Net.TransportContext for the client request.
        //
        // Returns:
        //     A System.Net.TransportContext object for the client request.
        TransportContext TransportContext { get; }

        //
        // Summary:
        //     Gets the Service Provider Name (SPN) that the client sent on the request.
        //
        // Returns:
        //     A System.String that contains the SPN the client sent on the request.
        string ServiceName { get; }

        //
        // Summary:
        //     Gets the request identifier of the incoming HTTP request.
        //
        // Returns:
        //     A System.Guid object that contains the identifier of the HTTP request.
        Guid RequestTraceIdentifier { get; }

        //
        // Summary:
        //     Gets the client IP address and port number from which the request originated.
        //
        // Returns:
        //     An System.Net.IPEndPoint that represents the IP address and port number from
        //     which the request originated.
        IPEndPoint RemoteEndPoint { get; }

        //
        // Summary:
        //     Gets the URL information (without the host and port) requested by the client.
        //
        // Returns:
        //     A System.String that contains the raw URL for this request.
        string RawUrl { get; }

        //
        // Summary:
        //     Gets the query string included in the request.
        //
        // Returns:
        //     A System.Collections.Specialized.NameValueCollection object that contains the
        //     query data included in the request System.Net.HttpListenerRequest.Url.
        NameValueCollection QueryString { get; }

        //
        // Summary:
        //     Gets the HTTP version used by the requesting client.
        //
        // Returns:
        //     A System.Version that identifies the client's version of HTTP.
        Version ProtocolVersion { get; }

        //
        // Summary:
        //     Get the server IP address and port number to which the request is directed.
        //
        // Returns:
        //     An System.Net.IPEndPoint that represents the IP address that the request is sent
        //     to.
        IPEndPoint LocalEndPoint { get; }

        //
        // Summary:
        //     Gets a System.Boolean value that indicates whether the client requests a persistent
        //     connection.
        //
        // Returns:
        //     true if the connection should be kept open; otherwise, false.
        bool KeepAlive { get; }

        //
        // Summary:
        //     Gets a System.Boolean value that indicates whether the TCP connection was a WebSocket
        //     request.
        //
        // Returns:
        //     Returns System.Boolean. true if the TCP connection is a WebSocket request; otherwise,
        //     false.
        bool IsWebSocketRequest { get; }

        //
        // Summary:
        //     Gets the DNS name and, if provided, the port number specified by the client.
        //
        // Returns:
        //     A System.String value that contains the text of the request's Host header.
        string UserHostName { get; }

        //
        // Summary:
        //     Gets the natural languages that are preferred for the response.
        //
        // Returns:
        //     A System.String array that contains the languages specified in the request's
        //     System.Net.HttpRequestHeader.AcceptLanguage header or null if the client request
        //     did not include an System.Net.HttpRequestHeader.AcceptLanguage header.
        string[] UserLanguages { get; }

        //
        // Summary:
        //     Gets a System.Boolean value that indicates whether the client sending this request
        //     is authenticated.
        //
        // Returns:
        //     true if the client was authenticated; otherwise, false.
        bool IsAuthenticated { get; }

        //
        // Summary:
        //     Gets a stream that contains the body data sent by the client.
        //
        // Returns:
        //     A readable System.IO.Stream object that contains the bytes sent by the client
        //     in the body of the request. This property returns System.IO.Stream.Null if no
        //     data is sent with the request.
        Stream InputStream { get; }

        //
        // Summary:
        //     Gets the HTTP method specified by the client.
        //
        // Returns:
        //     A System.String that contains the method used in the request.
        string HttpMethod { get; }


        /// <summary>
        /// Gets the collection of header name/value pairs sent in the request.
        /// </summary>
        /// <returns>
        /// A System.Net.WebHeaderCollection that contains the HTTP headers included in the request.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Note that HttpListenerRequest declares this property as a NameValueCollection, which is a base
        /// class of WebHeaderCollection. However, the documentation says it returns a WebHeaderCollection,
        /// every version of .NET Framework since at least 3.5 and the DotNet Core implementations all return a
        /// WebHeaderCollection. The response Headers declares the response as a WebHeaderCollection.
        /// </para>
        /// <para>
        /// There are two major differences between NameValueCollection and WebHeaderCollection. WHC will throw
        /// an exception if you try to set restricted headers and it will coalesce null header values into an
        /// empty string. NVC will not do either of those, so testing using NVCs is not realistic. You need to
        /// use a WHC instead.
        /// </para>
        /// <para>
        /// Long story short - in the implementation you need to cast the NameValueCollection into a
        /// WebHeaderCollection.
        /// </para>
        /// </remarks>
        WebHeaderCollection Headers { get; }

        //
        // Summary:
        //     Gets a System.Boolean value that indicates whether the request has associated
        //     body data.
        //
        // Returns:
        //     true if the request has associated body data; otherwise, false.
        bool HasEntityBody { get; }

        //
        // Summary:
        //     Gets the cookies sent with the request.
        //
        // Returns:
        //     A System.Net.CookieCollection that contains cookies that accompany the request.
        //     This property returns an empty collection if the request does not contain cookies.
        CookieCollection Cookies { get; }

        //
        // Summary:
        //     Gets the MIME type of the body data included in the request.
        //
        // Returns:
        //     A System.String that contains the text of the request's Content-Type header.
        string ContentType { get; }

        //
        // Summary:
        //     Gets the length of the body data included in the request.
        //
        // Returns:
        //     The value from the request's Content-Length header. This value is -1 if the content
        //     length is not known.
        long ContentLength64 { get; }

        //
        // Summary:
        //     Gets the content encoding that can be used with data sent with the request
        //
        // Returns:
        //     An System.Text.Encoding object suitable for use with the data in the System.Net.HttpListenerRequest.InputStream
        //     property.
        Encoding ContentEncoding { get; }

        //
        // Summary:
        //     Gets an error code that identifies a problem with the System.Security.Cryptography.X509Certificates.X509Certificate
        //     provided by the client.
        //
        // Returns:
        //     An System.Int32 value that contains a Windows error code.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The client certificate has not been initialized yet by a call to the System.Net.HttpListenerRequest.BeginGetClientCertificate(System.AsyncCallback,System.Object)
        //     or System.Net.HttpListenerRequest.GetClientCertificate methods -or - The operation
        //     is still in progress.
        int ClientCertificateError { get; }

        //
        // Summary:
        //     Gets the MIME types accepted by the client.
        //
        // Returns:
        //     A System.String array that contains the type names specified in the request's
        //     Accept header or null if the client request did not include an Accept header.
        string[] AcceptTypes { get; }

        //
        // Summary:
        //     Gets a System.Boolean value that indicates whether the request is sent from the
        //     local computer.
        //
        // Returns:
        //     true if the request originated on the same computer as the System.Net.HttpListener
        //     object that provided the request; otherwise, false.
        bool IsLocal { get; }

        //
        // Summary:
        //     Begins an asynchronous request for the client's X.509 v.3 certificate.
        //
        // Parameters:
        //   requestCallback:
        //     An System.AsyncCallback delegate that references the method to invoke when the
        //     operation is complete.
        //
        //   state:
        //     A user-defined object that contains information about the operation. This object
        //     is passed to the callback delegate when the operation completes.
        //
        // Returns:
        //     An System.IAsyncResult that indicates the status of the operation.
        IAsyncResult BeginGetClientCertificate(AsyncCallback requestCallback, object state);

        //
        // Summary:
        //     Ends an asynchronous request for the client's X.509 v.3 certificate.
        //
        // Parameters:
        //   asyncResult:
        //     The pending request for the certificate.
        //
        // Returns:
        //     The System.IAsyncResult object that is returned when the operation started.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     asyncResult is null.
        //
        //   T:System.ArgumentException:
        //     asyncResult was not obtained by calling System.Net.HttpListenerRequest.BeginGetClientCertificate(System.AsyncCallback,System.Object)e.
        //
        //   T:System.InvalidOperationException:
        //     This method was already called for the operation identified by asyncResult.
        X509Certificate2 EndGetClientCertificate(IAsyncResult asyncResult);

        //
        // Summary:
        //     Retrieves the client's X.509 v.3 certificate.
        //
        // Returns:
        //     A System.Security.Cryptography.X509Certificates object that contains the client's
        //     X.509 v.3 certificate.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     A call to this method to retrieve the client's X.509 v.3 certificate is in progress
        //     and therefore another call to this method cannot be made.
        X509Certificate2 GetClientCertificate();

        //
        // Summary:
        //     Retrieves the client's X.509 v.3 certificate as an asynchronous operation.
        //
        // Returns:
        //     Returns System.Threading.Tasks.Task`1. The task object representing the asynchronous
        //     operation. The System.Threading.Tasks.Task`1.Result property on the task object
        //     returns a System.Security.Cryptography.X509Certificates object that contains
        //     the client's X.509 v.3 certificate.
        Task<X509Certificate2> GetClientCertificateAsync();
    }
}
