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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace AWhewell.Owin.Utility
{
    /// <summary>
    /// Wraps an OWIN environment dictionary to expose type-safe access to its content.
    /// </summary>
    public class OwinContext
    {
        private RequestHeadersDictionary _RequestHeadersDictionary;

        private QueryStringDictionary _RequestQueryStringDictionary;

        private ResponseHeadersDictionary _ResponseHeadersDictionary;

        /// <summary>
        /// Gets or sets a cancellation token indicating whether the request has been cancelled or aborted. Attempts
        /// to overwrite this when non-null will throw an exception.
        /// </summary>
        public CancellationToken? CallCancelled
        {
            get => Environment[EnvironmentKey.CallCancelled] as CancellationToken?;
            set {
                if(CallCancelled != null && CallCancelled != value) {
                    throw new InvalidOperationException("A value has already been set for CallCancelled");
                }
                Environment[EnvironmentKey.CallCancelled] = value;
            }
        }

        /// <summary>
        /// Gets the environment that this context is wrapping.
        /// </summary>
        public IDictionary<string, object> Environment { get; }

        /// <summary>
        /// Gets or sets the request body stream. Setter will throw if overwriting an existing non-null stream.
        /// </summary>
        public Stream RequestBody
        {
            get => Environment[EnvironmentKey.RequestBody] as Stream;
            set => SetIfInitialisingOrNoChange(RequestBody, value, EnvironmentKey.RequestBody, "There is already a stream set for RequestBody");
        }

        /// <summary>
        /// Gets or sets the raw request headers dictionary. Setter will throw if overwriting an existing dictionary.
        /// </summary>
        public IDictionary<string, string[]> RequestHeaders
        {
            get => Environment[EnvironmentKey.RequestHeaders] as IDictionary<string, string[]>;
            set => SetIfInitialisingOrNoChange(RequestHeaders, value, EnvironmentKey.RequestHeaders, "There is already a dictionary set for RequestHeaders");
        }

        /// <summary>
        /// Gets a <see cref="RequestHeadersDictionary"/> wrapper around <see cref="RequestHeaders"/>.
        /// </summary>
        public RequestHeadersDictionary RequestHeadersDictionary
        {
            get {
                return GetOrCreateWrapper(
                    RequestHeaders,
                    _RequestHeadersDictionary,
                    headers => new RequestHeadersDictionary(headers),
                    wrapper => _RequestHeadersDictionary = wrapper
                );
            }
        }

        /// <summary>
        /// Gets the <see cref="RequestMethod"/> parsed into an <see cref="HttpMethod"/>.
        /// </summary>
        public HttpMethod RequestHttpMethod => Parser.ParseHttpMethod(RequestMethod);

        /// <summary>
        /// Gets <see cref="RequestProtocol"/> parsed into an <see cref="HttpProtocol"/>.
        /// </summary>
        public HttpProtocol RequestHttpProtocol => Parser.ParseHttpProtocol(RequestProtocol);

        /// <summary>
        /// Gets the <see cref="RequestScheme"/> parsed into an <see cref="HttpScheme"/> enum value.
        /// </summary>
        public HttpScheme RequestHttpScheme => Parser.ParseHttpScheme(RequestScheme);

        /// <summary>
        /// Gets or sets the request method. Setter will throw if overwriting an existing method.
        /// </summary>
        public string RequestMethod
        {
            get => Environment[EnvironmentKey.RequestMethod] as string;
            set => SetIfInitialisingOrNoChange(RequestMethod, value, EnvironmentKey.RequestMethod, "There is already a value set for RequestMethod");
        }

        /// <summary>
        /// Gets or sets the request path after unescaping. Setter will throw if overwriting an existing path.
        /// </summary>
        public string RequestPath
        {
            get => Environment[EnvironmentKey.RequestPath] as string;
            set => SetIfInitialisingOrNoChange(RequestPath, value, EnvironmentKey.RequestPath, "There is already a value set for RequestPath");
        }

        /// <summary>
        /// Gets or sets the request path base after unescaping. Setter will throw if overwriting an existing path base.
        /// </summary>
        public string RequestPathBase
        {
            get => Environment[EnvironmentKey.RequestPathBase] as string;
            set => SetIfInitialisingOrNoChange(RequestPathBase, value, EnvironmentKey.RequestPathBase, "There is already a value set for RequestPathBase");
        }

        /// <summary>
        /// Gets the request path split into parts.
        /// </summary>
        public string[] RequestPathParts => OwinPath.RequestPathParts(Environment, createAndUseCachedResult: true);

        /// <summary>
        /// Gets or sets the request protocol.
        /// </summary>
        public string RequestProtocol
        {
            get => Environment[EnvironmentKey.RequestProtocol] as string;
            set => SetIfInitialisingOrNoChange(RequestProtocol, value, EnvironmentKey.RequestProtocol, "There is already a value set for RequestProtocol");
        }
        /// <summary>
        /// Gets or sets the query string without the initial question mark and without any unescaping.
        /// </summary>
        public string RequestQueryString
        {
            get => Environment[EnvironmentKey.RequestQueryString] as string;
            set => SetIfInitialisingOrNoChange(RequestQueryString, value, EnvironmentKey.RequestQueryString, "There is already a value set for RequestQueryString");
        }
        /// <summary>
        /// Gets or sets the request scheme. Attempts to overwrite an existing scheme with a new one will throw an exception.
        /// </summary>
        public string RequestScheme
        {
            get => Environment[EnvironmentKey.RequestScheme] as string;
            set => SetIfInitialisingOrNoChange(RequestScheme, value, EnvironmentKey.RequestScheme, "There is already a value set for RequestScheme");
        }

        /// <summary>
        /// Gets or sets the response stream. Attempts to overwrite a non-null / non-Stream.Null stream with a
        /// new stream will throw an exception.
        /// </summary>
        public Stream ResponseBody
        {
            get => Environment[EnvironmentKey.ResponseBody] as Stream;
            set => SetIfInitialisingOrNoChange(ResponseBody, value, EnvironmentKey.ResponseBody, "There is already a stream set for ResponseBody");
        }

        /// <summary>
        /// Gets or sets the raw response headers dictionary. Setter will throw if overwriting an existing dictionary.
        /// </summary>
        public IDictionary<string, string[]> ResponseHeaders
        {
            get => Environment[EnvironmentKey.ResponseHeaders] as IDictionary<string, string[]>;
            set => SetIfInitialisingOrNoChange(ResponseHeaders, value, EnvironmentKey.ResponseHeaders, "There is already a dictionary set for ResponseHeaders");
        }

        /// <summary>
        /// Gets a <see cref="ResponseHeadersDictionary"/> wrapper around the <see cref="ResponseHeaders"/> dictionary.
        /// </summary>
        public ResponseHeadersDictionary ResponseHeadersDictionary
        {
            get {
                return GetOrCreateWrapper(
                    ResponseHeaders,
                    _ResponseHeadersDictionary,
                    headers => new ResponseHeadersDictionary(headers),
                    wrapper => _ResponseHeadersDictionary = wrapper
                );
            }
        }

        /// <summary>
        /// Gets or sets the response status code as an <see cref="HttpStatusCode"/>.
        /// </summary>
        public HttpStatusCode? ResponseHttpStatusCode
        {
            get {
                var statusCode = ResponseStatusCode;
                return statusCode == null ? (HttpStatusCode?)null : (HttpStatusCode)statusCode.Value;
            }
            set => ResponseStatusCode = value == null ? (int?)null : (int)value.Value;
        }

        /// <summary>
        /// Gets or sets the response protocol. Overwriting an existing response protocol is permitted.
        /// </summary>
        public string ResponseProtocol
        {
            get => Environment[EnvironmentKey.ResponseProtocol] as string;
            set => Environment[EnvironmentKey.ResponseProtocol] = value;
        }

        /// <summary>
        /// Gets or sets the response reason phrase. Overwriting an existing reason phrase is permitted.
        /// </summary>
        public string ResponseReasonPhrase
        {
            get => Environment[EnvironmentKey.ResponseReasonPhrase] as string;
            set => Environment[EnvironmentKey.ResponseReasonPhrase] = value;
        }

        /// <summary>
        /// Gets or sets the response status code. Overwriting an existing status code is permitted.
        /// </summary>
        public int? ResponseStatusCode
        {
            get => Environment[EnvironmentKey.ResponseStatusCode] as int?;
            set => Environment[EnvironmentKey.ResponseStatusCode] = value;
        }

        /// <summary>
        /// Gets or sets a value indicating that the request was from a browser on the same machine as the
        /// server. Attempts to overwrite this if it is not null will throw an exception.
        /// </summary>
        public bool? ServerIsLocal
        {
            get => Environment[EnvironmentKey.ServerIsLocal] as bool?;
            set => SetIfInitialisingOrNoChange(ServerIsLocal, value, EnvironmentKey.ServerIsLocal, "A value has already been set for ServerIsLocal");
        }

        /// <summary>
        /// Gets or sets the IP address that the request was received on. Attempts to overwrite this when
        /// it is not a null value will throw an exception.
        /// </summary>
        public string ServerLocalIpAddress
        {
            get => Environment[EnvironmentKey.ServerLocalIpAddress] as string;
            set => SetIfInitialisingOrNoChange(ServerLocalIpAddress, value, EnvironmentKey.ServerLocalIpAddress, "There is already a value set for ServerLocalIpAddress");
        }

        /// <summary>
        /// Gets or sets the port number (as a string) that the request was received on. Attempts to overwrite this when
        /// it is not a null value will throw an exception.
        /// </summary>
        public string ServerLocalPort
        {
            get => Environment[EnvironmentKey.ServerLocalPort] as string;
            set => SetIfInitialisingOrNoChange(ServerLocalPort, value, EnvironmentKey.ServerLocalPort, "There is already a value set for ServerLocalPort");
        }

        /// <summary>
        /// Gets or sets the IP address that the request was received from. Attempts to overwrite this when
        /// it is not a null value will throw an exception.
        /// </summary>
        public string ServerRemoteIpAddress
        {
            get => Environment[EnvironmentKey.ServerRemoteIpAddress] as string;
            set => SetIfInitialisingOrNoChange(ServerRemoteIpAddress, value, EnvironmentKey.ServerRemoteIpAddress, "There is already a value set for ServerRemoteIpAddress");
        }

        /// <summary>
        /// Gets or sets the port number (as a string) that the request was received from. Attempts to overwrite this when
        /// it is not a null value will throw an exception.
        /// </summary>
        public string ServerRemotePort
        {
            get => Environment[EnvironmentKey.ServerRemotePort] as string;
            set => SetIfInitialisingOrNoChange(ServerRemotePort, value, EnvironmentKey.ServerRemotePort, "There is already a value set for ServerRemotePort");
        }

        /// <summary>
        /// Gets or sets the X509Certificate presented by the client during an encrypted request. Attempts to overwrite
        /// this when it is not a null value will throw an exception.
        /// </summary>
        public X509Certificate SslClientCertificate
        {
            get => Environment[EnvironmentKey.SslClientCertificate] as X509Certificate;
            set => SetIfInitialisingOrNoChange(SslClientCertificate, value, EnvironmentKey.SslClientCertificate, "There is already a value set for SslClientCertificate");
        }

        /// <summary>
        /// Gets or sets the OWIN version that the host complies with. Setting an established value will throw
        /// an exception.
        /// </summary>
        public string Version
        {
            get => Environment[EnvironmentKey.Version] as string;
            set => SetIfInitialisingOrNoChange(Version, value, EnvironmentKey.Version, "A value has already been set for Version");
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public OwinContext() : this(null)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="environment"></param>
        public OwinContext(IDictionary<string, object> environment)
        {
            Environment = environment ?? new OwinDictionary<object>();
        }

        /// <summary>
        /// Returns the existing context from the OWIN environment passed across or creates and caches a
        /// context within the environment for later use.
        /// </summary>
        /// <param name="environment">The environment to wrap or null to create a new empty environment.</param>
        /// <returns></returns>
        public static OwinContext Create(IDictionary<string, object> environment)
        {
            var result = environment == null ? null : environment[CustomEnvironmentKey.Context] as OwinContext;
            if(result == null) {
                result = new OwinContext(environment);
                result.Environment[CustomEnvironmentKey.Context] = result;
            }

            return result;
        }

        /// <summary>
        /// Returns a <see cref="QueryStringDictionary"/> wrapper around <see cref="RequestQueryString"/>.
        /// </summary>
        /// <param name="caseSensitiveKeys">True if the query string keys can be distinguished just by case.</param>
        /// <remarks></remarks>
        public QueryStringDictionary RequestQueryStringDictionary(bool caseSensitiveKeys)
        {
            var queryString = RequestQueryString ?? "";
            if(_RequestQueryStringDictionary == null
                || _RequestQueryStringDictionary.QueryString != queryString
                || _RequestQueryStringDictionary.CaseSensitiveKeys != caseSensitiveKeys
            ) {
                _RequestQueryStringDictionary = new QueryStringDictionary(queryString, caseSensitiveKeys);
            }
            return _RequestQueryStringDictionary;
        }

        /// <summary>
        /// Sets up the environment for a text response.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="encoding"></param>
        /// <param name="mimeType"></param>
        public void ReturnText(string text, Encoding encoding, string mimeType)
        {
            encoding = encoding ?? Encoding.UTF8;
            mimeType = String.IsNullOrEmpty(mimeType) ? "text/plain" : mimeType;
            var bytes = encoding.GetBytes(text ?? "");

            var headers = ResponseHeadersDictionary;
            if(headers != null) {
                headers.ContentTypeValue = new ContentTypeValue(mimeType, charset: encoding.WebName);
                headers.ContentLength =    bytes.Length;
            }

            var stream = ResponseBody;
            if(stream != null) {
                ResponseBody.Write(bytes, 0, bytes.Length);
            }
        }

        /// <summary>
        /// Gets or creates a wrapper around a headers dictionary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="envHeaders"></param>
        /// <param name="existingWrapper"></param>
        /// <param name="createWrapper"></param>
        /// <param name="assignWrapper"></param>
        /// <returns></returns>
        T GetOrCreateWrapper<T>(IDictionary<string, string[]> envHeaders, T existingWrapper, Func<IDictionary<string, string[]>, T> createWrapper, Action<T> assignWrapper)
            where T : HeadersDictionary
        {
            if(envHeaders == null) {
                if(existingWrapper != null) {
                    existingWrapper = null;
                    assignWrapper(existingWrapper);
                }
            } else if(existingWrapper == null
                || (
                          !Object.ReferenceEquals(envHeaders, existingWrapper)
                       && !Object.ReferenceEquals(existingWrapper.WrappedDictionary, envHeaders)
                   )
            ) {
                existingWrapper = envHeaders as T ?? createWrapper(envHeaders);
                assignWrapper(existingWrapper);
            }

            return existingWrapper;
        }

        /// <summary>
        /// Assigns a value to the environment but only if the existing value is either null or is the same as
        /// the value being set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="existingValue"></param>
        /// <param name="newValue"></param>
        /// <param name="environmentKey"></param>
        /// <param name="exceptionMessage"></param>
        private void SetIfInitialisingOrNoChange(object existingValue, object newValue, string environmentKey, string exceptionMessage)
        {
            if(existingValue != null && !Object.Equals(existingValue, newValue)) {
                throw new InvalidOperationException(exceptionMessage);
            }
            Environment[environmentKey] = newValue;
        }

        private void SetIfInitialisingOrNoChange(Stream existingValue, Stream newValue, string environmentKey, string exceptionMessage)
        {
            SetIfInitialisingOrNoChange(
                (object)(existingValue == Stream.Null ? null : existingValue),
                newValue,
                environmentKey,
                exceptionMessage
            );
        }
    }
}
