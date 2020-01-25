// Copyright © 2017 onwards, Andrew Whewell
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
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using AWhewell.Owin.Utility;
using Newtonsoft.Json;

namespace Test.AWhewell.Owin
{
    /// <summary>
    /// Exposes a dictionary of OWIN objects that can be used to test middleware.
    /// </summary>
    public class MockOwinEnvironment
    {
        /// <summary>
        /// Gets the environment.
        /// </summary>
        public IDictionary<string, object> Environment { get; private set; } = new OwinDictionary<object>();

        public CancellationToken CallCancelled
        {
            get => (CancellationToken)Environment["owin.CallCancelled"];
            set => Environment["owin.CallCancelled"] = value;
        }

        /// <summary>
        /// Gets or sets the request's method.
        /// </summary>
        public string RequestMethod
        {
            get => Environment["owin.RequestMethod"] as string;
            set => Environment["owin.RequestMethod"] = value;
        }

        /// <summary>
        /// Gets or sets the request's path base.
        /// </summary>
        public string RequestPathBase
        {
            get => Environment["owin.RequestPath"] as string;
            set => Environment["owin.RequestPathBase"] = value;
        }

        /// <summary>
        /// Gets or sets the request's path and file.
        /// </summary>
        public string RequestPath
        {
            get => Environment["owin.RequestPath"] as string;
            set => Environment["owin.RequestPath"] = value;
        }

        /// <summary>
        /// Gets or sets the request's query string.
        /// </summary>
        public string RequestQueryString
        {
            get => Environment["owin.RequestQueryString"] as string;
            set => Environment["owin.RequestQueryString"] = value;
        }

        /// <summary>
        /// Gets the request headers.
        /// </summary>
        public HeadersDictionary RequestHeaders
        {
            get => new HeadersDictionary(Environment["owin.RequestHeaders"] as IDictionary<string, string[]>);
        }

        /// <summary>
        /// Gets or sets the Authorization header value from the request.
        /// </summary>
        public string RequestAuthorizationHeader
        {
            get => RequestHeaders["Authorization"];
            set => RequestHeaders["Authorization"] = value;
        }

        private MemoryStream _ResponseBodyStream = new MemoryStream();
        /// <summary>
        /// Gets a byte array that represents the content of the response body.
        /// </summary>
        public byte[] ResponseBodyBytes => _ResponseBodyStream.ToArray();

        /// <summary>
        /// Gets the response body as a string whose encoding is either UTF-8 or extracted
        /// from the content type and whose length is as per the content length header.
        /// </summary>
        public string ResponseBodyText
        {
            get {
                var contentType = ResponseHeadersDictionary.ContentTypeValue;
                var encoding = Parser.ParseCharset(contentType?.Charset);
                var length = (int)(ResponseHeadersDictionary.ContentLength ?? 0L);

                return encoding.GetString(ResponseBodyBytes, 0, length);
            }
        }

        /// <summary>
        /// Gets the response status code or null if none has been set.
        /// </summary>
        public int? ResponseStatusCode => Environment[EnvironmentKey.ResponseStatusCode] as int?;

        /// <summary>
        /// Gets the response headers.
        /// </summary>
        public HeadersDictionary ResponseHeaders
        {
            get => new HeadersDictionary(Environment["owin.ResponseHeaders"] as IDictionary<string, string[]>);
        }

        /// <summary>
        /// Gets the response headers formatted as a <see cref="ResponseHeadersDictionary"/>.
        /// </summary>
        public ResponseHeadersDictionary ResponseHeadersDictionary
        {
            get => new ResponseHeadersDictionary(Environment["owin.ResponseHeaders"] as IDictionary<string, string[]>);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public MockOwinEnvironment()
        {
            AddRequiredFields();
        }

        /// <summary>
        /// Resets the environment.
        /// </summary>
        /// <param name="addRequiredFields"></param>
        public void Reset(bool addRequiredFields = true)
        {
            Environment.Clear();
            _ResponseBodyStream.Dispose();
            _ResponseBodyStream = new MemoryStream();

            if(addRequiredFields) {
                AddRequiredFields();
            }
        }

        /// <summary>
        /// Adds or overwrites the OWIN, request and response required fields.
        /// </summary>
        public void AddRequiredFields()
        {
            AddOwinEnvironment();
            AddRequestEnvironment();
            AddResponseEnvironment(body: _ResponseBodyStream);
        }

        /// <summary>
        /// Adds or overwrites the required OWIN environment fields.
        /// </summary>
        /// <param name="owinVersion"></param>
        public void AddOwinEnvironment(string owinVersion = "1.0.0")
        {
            Environment["owin.Version"] = owinVersion;
            CallCancelled = new CancellationToken();
        }

        /// <summary>
        /// Adds or overwrites required request fields to the environment.
        /// </summary>
        /// <param name="pathBase"></param>
        /// <param name="path"></param>
        /// <param name="protocol"></param>
        /// <param name="queryString"></param>
        /// <param name="requestScheme"></param>
        /// <param name="headers"></param>
        public void AddRequestEnvironment(
            string pathBase = "/Root",
            string path = "/",
            string protocol = "HTTP/1.0",
            string queryString = "",
            string requestScheme = "http",
            IDictionary<string, string[]> headers = null,
            Stream body = null
        )
        {
            Environment["owin.RequestPathBase"] = pathBase;
            Environment["owin.RequestPath"] = path;
            Environment["owin.RequestProtocol"] = protocol;
            Environment["owin.RequestQueryString"] = queryString;
            Environment["owin.RequestScheme"] = requestScheme;
            Environment["owin.RequestHeaders"] = headers ?? new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            Environment["owin.RequestBody"] = body ?? Stream.Null;
        }

        /// <summary>
        /// Sets the request body stream up.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="contentType"></param>
        /// <param name="contentLength"></param>
        public MemoryStream AddRequestBody(byte[] bytes, string contentType = null, int? contentLength = null)
        {
            var result = new MemoryStream(bytes);
            Environment["owin.RequestBody"] = result;
            if(contentType != null) {
                RequestHeaders.Set("Content-Type", contentType);
            }
            RequestHeaders.Set("Content-Length", (contentLength ?? bytes.Length).ToString(CultureInfo.InvariantCulture));

            return result;
        }

        /// <summary>
        /// Sets the request body stream up.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="contentType"></param>
        /// <param name="contentLength"></param>
        public MemoryStream AddRequestBody(byte[] bytes, ContentTypeValue contentType, int? contentLength = null)
        {
            var result = new MemoryStream(bytes);
            Environment["owin.RequestBody"] = result;
            if(contentType != null) {
                RequestHeaders.Set("Content-Type", contentType.ToString());
            }
            RequestHeaders.Set("Content-Length", (contentLength ?? bytes.Length).ToString(CultureInfo.InvariantCulture));

            return result;
        }

        /// <summary>
        /// Sets the request body stream up.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="encoding"></param>
        /// <param name="contentType"></param>
        /// <param name="contentLength"></param>
        public MemoryStream SetRequestBody(string text, Encoding encoding = null, string contentType = null, int? contentLength = null)
        {
            return AddRequestBody((encoding ?? Encoding.UTF8).GetBytes(text ?? ""), contentType, contentLength);
        }

        /// <summary>
        /// Sets the request body up with JSON content.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public void SetJsonRequestBody<T>(T obj)
        {
            var text = JsonConvert.SerializeObject(obj);
            SetRequestBody(text, Encoding.UTF8, Formatter.FormatMediaType(MediaType.Json));
        }

        /// <summary>
        /// Adds or overwrites the required response fields.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="headers"></param>
        public void AddResponseEnvironment(Stream body = null, IDictionary<string, string[]> headers = null)
        {
            Environment["owin.ResponseBody"] = body ?? Stream.Null;
            Environment["owin.ResponseHeaders"] = headers ?? new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Sets up a text response body and content type header.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="mediaType"></param>
        /// <param name="encoding"></param>
        public void AddResponseText(string text, string mediaType = "text/plain", Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            var bytes = encoding.GetBytes(text ?? "");

            _ResponseBodyStream.Write(bytes, 0, bytes.Length);
            ResponseHeadersDictionary.ContentLength = _ResponseBodyStream.ToArray().Length;
            ResponseHeadersDictionary.ContentTypeValue = new ContentTypeValue(mediaType, encoding.WebName);
        }

        /// <summary>
        /// Adds a principal to the request.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="authType"></param>
        /// <param name="roles"></param>
        public void SetRequestPrincipal(string userName, string authType, params string[] roles)
        {
            Environment[EnvironmentKey.ServerUser] = new GenericPrincipal(
                new GenericIdentity(userName, authType),
                roles
            );
        }

        /// <summary>
        /// Adds a principal to the request.
        /// </summary>
        /// <param name="principal"></param>
        public void SetRequestPrincipal(IPrincipal principal)
        {
            Environment[EnvironmentKey.ServerUser] = principal;
        }

        /// <summary>
        /// Sets up the Authorize header for the username and password.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public void SetBasicCredentials(string userName, string password)
        {
            var encoded = EncodeBasicCredentials(userName, password);
            RequestAuthorizationHeader = $"Basic {encoded}";
        }

        /// <summary>
        /// Sets the request URL.
        /// </summary>
        /// <param name="unencodedPathAndFile"></param>
        /// <param name="unencodedQueryStrings">Pass unencoded keys and values. Keys with null values are added as '?value' whereas keys with empty strings are added as '?value='.</param>
        public void SetRequestUrl(string unencodedPathAndFile, string[,] unencodedQueryStrings = null)
        {
            unencodedPathAndFile = unencodedPathAndFile ?? "";
            if(unencodedPathAndFile == "" || unencodedPathAndFile[0] != '/') {
                unencodedPathAndFile = $"/{unencodedPathAndFile}";
            }

            RequestPath = unencodedPathAndFile;

            if(unencodedQueryStrings != null) {
                if(unencodedQueryStrings.GetLength(1) != 2) {
                    throw new ArgumentOutOfRangeException($"The unencoded query strings array must be 2D");
                }

                var buffer = new StringBuilder();
                for(var i = 0;i < unencodedQueryStrings.GetLength(0);++i) {
                    var key =   unencodedQueryStrings[i, 0];
                    var value = unencodedQueryStrings[i, 1];

                    if(i > 0) {
                        buffer.Append('&');
                    }
                    buffer.Append(HttpUtility.UrlEncode(key));

                    if(value != null) {
                        buffer.AppendFormat("={0}", HttpUtility.UrlEncode(value));
                    }
                }

                RequestQueryString = buffer.ToString();
            }
        }

        /// <summary>
        /// Sets the request path.
        /// </summary>
        /// <param name="pathParts"></param>
        public void SetRequestPath(string[] pathParts)
        {
            SetRequestPath(String.Join("/", pathParts ?? new string[0]));
        }

        /// <summary>
        /// Sets the request path.
        /// </summary>
        /// <param name="path"></param>
        public void SetRequestPath(string path)
        {
            path = path ?? "";

            if(path.Length == 0 || path[0] != '/') {
                path = "/" + path;
            }

            if(path == "/" && !String.IsNullOrEmpty(RequestPathBase)) {
                path = "";
            }

            RequestPath = path;
        }

        /// <summary>
        /// Converts the username and password into a MIME64 string of username:password.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string EncodeBasicCredentials(string userName, string password)
        {
            var encodeString = $"{userName}:{password}";
            var bytes = Encoding.UTF8.GetBytes(encodeString);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Adds a cookie to the request.
        /// </summary>
        /// <param name="cookie"></param>
        /// <param name="value"></param>
        public void AddCookie(string cookie, string value)
        {
            RequestHeaders.Append("Cookie", $"{HttpUtility.UrlEncode(cookie)}={HttpUtility.UrlEncode(value)}");
        }
    }
}
