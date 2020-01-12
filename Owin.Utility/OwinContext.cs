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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
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
        /// Gets the IP address of the machine that made the request. If the request came from a
        /// proxy server on the LAN then it is the address of the machine that last called the
        /// proxy server, otherwise it is <see cref="ServerRemoteIpAddress"/>.
        /// </summary>
        public string ClientIpAddress
        {
            get {
                DetermineClientAndProxyAddresses();
                return Environment[CustomEnvironmentKey.ClientIpAddress] as string;
            }
        }

        /// <summary>
        /// Gets the IP address of the proxy that the request came through, if applicable.
        /// </summary>
        public string ProxyIpAddress
        {
            get {
                DetermineClientAndProxyAddresses();
                return Environment[CustomEnvironmentKey.ProxyIpAddress] as string;
            }
        }

        /// <summary>
        /// Gets <see cref="ClientIpAddress"/> parsed into an <see cref="IPAddress"/>.
        /// </summary>
        public IPAddress ClientIpAddressParsed
        {
            get {
                DetermineClientAndProxyAddresses();
                return Environment[CustomEnvironmentKey.ClientIpAddressParsed] as IPAddress;
            }
        }

        /// <summary>
        /// Gets a value indicating that the request originated from the local machine or the LAN.
        /// </summary>
        public bool IsLocalOrLan => IPAddressHelper.IsLocalOrLanAddress(ClientIpAddressParsed);

        /// <summary>
        /// Gets a value indicating that the request originated from the Internet.
        /// </summary>
        public bool IsInternet => !IsLocalOrLan;

        /// <summary>
        /// Gets the environment that this context is wrapping.
        /// </summary>
        public OwinDictionary<object> Environment { get; }

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
        /// Gets or sets the host portion of the request address.
        /// </summary>
        public string RequestHost
        {
            get => RequestHeadersDictionary.Host;
            set => RequestHeadersDictionary.SetValues("Host", value);
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
        /// As per <see cref="RequestPathNormalised"/> except any directory traversal characters are expanded out.
        /// </summary>
        public string RequestPathFlattened
        {
            get {
                var result = new StringBuilder();

                void TerminatePathWithSlash()
                {
                    if(result.Length == 0 || result[result.Length - 1] != '/') {
                        result.Append('/');
                    }
                }

                int FindLastFolderIndex()
                {
                    var startIndex = result.Length > 0 && result[result.Length - 1] == '/' ? result.Length - 2 : result.Length - 1;
                    return result.LastIndexOf('/', startIndex);
                }

                var pathParts = RequestPathNormalised.Split('/');
                for(var i = 0;i < pathParts.Length;++i) {
                    var pathPart = pathParts[i];
                    switch(pathPart) {
                        case ".":
                            TerminatePathWithSlash();
                            break;
                        case "..":
                            var lastFolderIdx = FindLastFolderIndex();
                            if(lastFolderIdx != -1) {
                                ++lastFolderIdx;
                                result.Remove(lastFolderIdx, result.Length - lastFolderIdx);
                            }
                            TerminatePathWithSlash();
                            break;
                        default:
                            TerminatePathWithSlash();
                            result.Append(pathPart);
                            break;
                    }
                }

                return result.ToString();
            }
        }

        /// <summary>
        /// The same as <see cref="RequestPath"/> except if it is an empty path (i.e. the request specified just the path base) then it
        /// returns a forward-slash to indicate a request for the root folder under the path base and backslashes are transformed into
        /// forward-slashes.
        /// </summary>
        public string RequestPathNormalised
        {
            get {
                var result = RequestPath;
                return String.IsNullOrEmpty(result) ? "/" : result.Replace('\\', '/');
            }
        }

        /// <summary>
        /// Gets the filename portion of the request URL. If no filename has been specified then an empty string is returned.
        /// </summary>
        public string RequestPathFileName
        {
            get {
                var path = RequestPathFlattened;
                var lastFolderIndex = path.LastIndexOf('/');
                return lastFolderIndex == -1 ? "" : path.Substring(lastFolderIndex + 1);
            }
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
        /// Gets or sets the user that the request is running under or null if the request is anonymous. Attempts to overwrite
        /// an existing user with a new one will throw an exception unless they are the same user.
        /// </summary>
        public IPrincipal RequestPrincipal
        {
            get => Environment[CustomEnvironmentKey.Principal] as IPrincipal;
            set => SetIfInitialisingOrNoChange(RequestPrincipal, value, CustomEnvironmentKey.Principal, "There is already a value set for RequestPrincipal");
        }

        /// <summary>
        /// Returns the full URL that the request was made to.
        /// </summary>
        public string RequestUrl
        {
            get => OwinPath.ConstructUrl(
                RequestScheme,
                RequestHost,
                RequestPathBase,
                RequestPath,
                RequestQueryString
            );
        }

        /// <summary>
        /// Returns the URL (starting from the site root - i.e. without scheme, host or port) that the request was made to.
        /// </summary>
        public string RequestUrlFromRoot
        {
            get => OwinPath.ConstructUrlFromRoot(
                RequestPathBase,
                RequestPath,
                RequestQueryString
            );
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
        /// Gets or sets the <see cref="ServerLocalPort"/> as a number.
        /// </summary>
        public int? ServerLocalPortNumber
        {
            get => ParseInteger(ServerLocalPort);
            set => ServerLocalPort = value?.ToString(CultureInfo.InvariantCulture);
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
        /// Gets or sets <see cref="ServerRemotePort"/> as a number.
        /// </summary>
        public int? ServerRemotePortNumber
        {
            get => ParseInteger(ServerRemotePort);
            set => ServerRemotePort = value?.ToString(CultureInfo.InvariantCulture);
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
            Environment = environment != null
                ? new OwinDictionary<object>(environment)
                : new OwinDictionary<object>(caseSensitive: true);
        }

        /// <summary>
        /// Returns the existing context from the OWIN environment passed across or creates and caches a
        /// context within the environment for later use.
        /// </summary>
        /// <param name="environment">The environment to wrap or null to create a new empty environment.</param>
        /// <returns></returns>
        public static OwinContext Create(IDictionary<string, object> environment)
        {
            OwinContext result = null;

            if(environment != null && environment.TryGetValue(CustomEnvironmentKey.Context, out var contextObj)) {
                result = contextObj as OwinContext;
            }
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
        /// Reads the content body as a sequence of bytes. The bytes are cached so that they can be re-read in subsequent
        /// calls without having to rewind the stream. If there is no body then an empty array is returned. Note that this
        /// can allocate a LOT of memory.
        /// </summary>
        /// <returns></returns>
        public byte[] RequestBodyBytes()
        {
            byte[] result = null;

            var bodyStream = RequestBody;
            if(bodyStream != null && bodyStream != Stream.Null) {
                if(Object.ReferenceEquals(bodyStream, Environment[CustomEnvironmentKey.RequestBodyBytesBasis])) {
                    result = (byte[])Environment[CustomEnvironmentKey.RequestBodyBytes];
                } else {
                    if(bodyStream.CanSeek && bodyStream.Position != 0) {
                        bodyStream.Position = 0;
                    }
                    using(var memoryStream = new MemoryStream()) {
                        bodyStream.CopyTo(memoryStream);
                        result = memoryStream.ToArray();
                    }
                    Environment[CustomEnvironmentKey.RequestBodyBytes] =      result;
                    Environment[CustomEnvironmentKey.RequestBodyBytesBasis] = bodyStream;
                }
            }

            return result ?? new byte[0];
        }

        /// <summary>
        /// Reads the content body as a string, optionally caching and reusing the string in future calls.
        /// </summary>
        public string RequestBodyText()
        {
            var encoding = Parser.ParseCharset(
                RequestHeadersDictionary?.ContentTypeValue.Charset
            );
            if(encoding == null) {
                throw new UnknownCharsetException($"Charset {RequestHeadersDictionary.ContentTypeValue.Charset} is unknown");
            }

            return encoding.GetString(RequestBodyBytes());
        }

        /// <summary>
        /// Reads the content body as a URL-encoded form and parses it into a dictionary of names and values.
        /// </summary>
        /// <param name="caseSensitiveKeys"></param>
        /// <returns></returns>
        public QueryStringDictionary RequestBodyForm(bool caseSensitiveKeys)
        {
            return new QueryStringDictionary(RequestBodyText(), caseSensitiveKeys);
        }

        /// <summary>
        /// Sets up the environment for a byte response.
        /// </summary>
        /// <param name="mimeType"></param>
        /// <param name="bytes"></param>
        public void ReturnBytes(string mimeType, byte[] bytes)
        {
            ReturnBytes(mimeType, bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Sets up the environment for a byte response.
        /// </summary>
        /// <param name="mimeType"></param>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void ReturnBytes(string mimeType, byte[] bytes, int offset, int length)
        {
            var headers = ResponseHeadersDictionary;
            if(headers != null) {
                headers.ContentType = mimeType;
                headers.ContentLength = length;
            }

            var stream = ResponseBody;
            if(stream != null) {
                ResponseBody.Write(bytes, offset, length);
            }
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
        /// Sets up the environment for a redirection response.
        /// </summary>
        /// <param name="location"></param>
        public void Redirect(string location)
        {
            ResponseHttpStatusCode = HttpStatusCode.Redirect;
            ResponseHeadersDictionary.Location = location;
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

        /// <summary>
        /// Tries to determine the true IP address of the client and the address of the proxy, if any. If the
        /// values have already been calculated and aren't going to change then this does nothing.
        /// </summary>
        private void DetermineClientAndProxyAddresses()
        {
            var serverRemoteIpAddress = ServerRemoteIpAddress ?? "";
            var xForwardedFor = RequestHeadersDictionary.XForwardedFor ?? "";
            var translationBasis = new StringBuilder(serverRemoteIpAddress);
            translationBasis.Append('-');
            translationBasis.Append(xForwardedFor);

            if(translationBasis.ToString() != Environment[CustomEnvironmentKey.ClientIpAddressBasis] as string) {
                Environment[CustomEnvironmentKey.ClientIpAddressBasis] = translationBasis.ToString();

                var serverRemoteIpAddressParsed = ParseIpAddress(serverRemoteIpAddress);
                var localOrLanRequest = IPAddressHelper.IsLocalOrLanAddress(serverRemoteIpAddressParsed);
                var xff = localOrLanRequest ? xForwardedFor : null;

                IPAddress xffParsed = null;
                if(!String.IsNullOrEmpty(xff)) {
                    xff = xff.Split(',').Last().Trim();
                    if(!IPAddress.TryParse(xff, out xffParsed)) {
                        xff = null;
                    }
                }

                if(String.IsNullOrEmpty(xff)) {
                    Environment[CustomEnvironmentKey.ClientIpAddress] =         serverRemoteIpAddress;
                    Environment[CustomEnvironmentKey.ClientIpAddressParsed] =   serverRemoteIpAddressParsed;
                    Environment[CustomEnvironmentKey.ProxyIpAddress] =          null;
                } else {
                    Environment[CustomEnvironmentKey.ClientIpAddress] =         xff;
                    Environment[CustomEnvironmentKey.ClientIpAddressParsed] =   xffParsed;
                    Environment[CustomEnvironmentKey.ProxyIpAddress] =          serverRemoteIpAddress;
                }
            }
        }

        private IPAddress ParseIpAddress(string ipAddress)
        {
            var result = IPAddress.None;

            if(!String.IsNullOrEmpty(ipAddress)) {
                if(!IPAddress.TryParse(ipAddress, out result)) {
                    result = IPAddress.None;
                }
            }

            return result;
        }

        private static int? ParseInteger(string value)
        {
            int? result = null;

            if(!String.IsNullOrEmpty(value) && int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var number)) {
                result = number;
            }

            return result;
        }
    }
}
