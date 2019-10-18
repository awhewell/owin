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
using Owin.Interface.Host.HttpListener.HttpListenerWrapper;

namespace Owin.Host.HttpListener.HttpListenerWrapper
{
    /// <summary>
    /// Wraps a request object.
    /// </summary>
    class HttpListenerRequestWrapper : IHttpListenerRequest
    {
        /// <summary>
        /// The wrapped request.
        /// </summary>
        private HttpListenerRequest _Wrapped;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsSecureConnection => _Wrapped.IsSecureConnection;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string UserHostAddress => _Wrapped.UserHostAddress;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string UserAgent => _Wrapped.UserAgent;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public Uri UrlReferrer => _Wrapped.UrlReferrer;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public Uri Url => _Wrapped.Url;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public TransportContext TransportContext => _Wrapped.TransportContext;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ServiceName => _Wrapped.ServiceName;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public Guid RequestTraceIdentifier => _Wrapped.RequestTraceIdentifier;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IPEndPoint RemoteEndPoint => _Wrapped.RemoteEndPoint;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string RawUrl => _Wrapped.RawUrl;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public NameValueCollection QueryString => _Wrapped.QueryString;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public Version ProtocolVersion => _Wrapped.ProtocolVersion;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IPEndPoint LocalEndPoint => _Wrapped.LocalEndPoint;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool KeepAlive => _Wrapped.KeepAlive;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsWebSocketRequest => _Wrapped.IsWebSocketRequest;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string UserHostName => _Wrapped.UserHostName;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string[] UserLanguages => _Wrapped.UserLanguages;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsAuthenticated => _Wrapped.IsAuthenticated;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public Stream InputStream => _Wrapped.InputStream;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string HttpMethod => _Wrapped.HttpMethod;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public WebHeaderCollection Headers => (WebHeaderCollection)_Wrapped.Headers;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool HasEntityBody => _Wrapped.HasEntityBody;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public CookieCollection Cookies => _Wrapped.Cookies;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ContentType => _Wrapped.ContentType;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long ContentLength64 => _Wrapped.ContentLength64;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public Encoding ContentEncoding => _Wrapped.ContentEncoding;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int ClientCertificateError => _Wrapped.ClientCertificateError;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string[] AcceptTypes => _Wrapped.AcceptTypes;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsLocal => _Wrapped.IsLocal;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="wrapped"></param>
        public HttpListenerRequestWrapper(HttpListenerRequest wrapped)
        {
            _Wrapped = wrapped;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="requestCallback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IAsyncResult BeginGetClientCertificate(AsyncCallback requestCallback, object state) => _Wrapped.BeginGetClientCertificate(requestCallback, state);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="asyncResult"></param>
        /// <returns></returns>
        public X509Certificate2 EndGetClientCertificate(IAsyncResult asyncResult) => _Wrapped.EndGetClientCertificate(asyncResult);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public X509Certificate2 GetClientCertificate() => _Wrapped.GetClientCertificate();

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public Task<X509Certificate2> GetClientCertificateAsync() => _Wrapped.GetClientCertificateAsync();
    }
}
