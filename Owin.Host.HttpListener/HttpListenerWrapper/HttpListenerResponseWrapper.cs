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
using System.IO;
using System.Net;
using System.Text;
using AWhewell.Owin.Interface.Host.HttpListener.HttpListenerWrapper;

namespace AWhewell.Owin.Host.HttpListener.HttpListenerWrapper
{
    /// <summary>
    /// The wrapper around the response.
    /// </summary>
    class HttpListenerResponseWrapper : IHttpListenerResponse
    {
        /// <summary>
        /// The response that is being wrapped.
        /// </summary>
        private HttpListenerResponse _Wrapped;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long ContentLength64
        {
            get => _Wrapped.ContentLength64;
            set => _Wrapped.ContentLength64 = value;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool SendChunked
        {
            get => _Wrapped.SendChunked;
            set => _Wrapped.SendChunked = value;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string RedirectLocation
        {
            get => _Wrapped.RedirectLocation;
            set => _Wrapped.RedirectLocation = value;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public Version ProtocolVersion
        {
            get => _Wrapped.ProtocolVersion;
            set => _Wrapped.ProtocolVersion = value;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public Stream OutputStream => _Wrapped.OutputStream;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool KeepAlive
        {
            get => _Wrapped.KeepAlive;
            set => _Wrapped.KeepAlive = value;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public WebHeaderCollection Headers
        {
            get => _Wrapped.Headers;
            set => _Wrapped.Headers = value;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public CookieCollection Cookies
        {
            get => _Wrapped.Cookies;
            set => _Wrapped.Cookies = value;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ContentType
        {
            get => _Wrapped.ContentType;
            set => _Wrapped.ContentType = value;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int StatusCode
        {
            get => _Wrapped.StatusCode;
            set => _Wrapped.StatusCode = value;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string StatusDescription
        {
            get => _Wrapped.StatusDescription;
            set => _Wrapped.StatusDescription = value;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public Encoding ContentEncoding
        {
            get => _Wrapped.ContentEncoding;
            set => _Wrapped.ContentEncoding = value;
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="wrapped"></param>
        public HttpListenerResponseWrapper(HttpListenerResponse wrapped)
        {
            _Wrapped = wrapped;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Abort() => _Wrapped.Abort();

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddHeader(string name, string value) => _Wrapped.AddHeader(name, value);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="cookie"></param>
        public void AppendCookie(Cookie cookie) => _Wrapped.AppendCookie(cookie);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AppendHeader(string name, string value) => _Wrapped.AppendHeader(name, value);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="responseEntity"></param>
        /// <param name="willBlock"></param>
        public void Close(byte[] responseEntity, bool willBlock) => _Wrapped.Close(responseEntity, willBlock);

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Close() => _Wrapped.Close();

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="templateResponse"></param>
        public void CopyFrom(IHttpListenerResponse templateResponse)
        {
            if(templateResponse is HttpListenerResponseWrapper otherWrapper) {
                _Wrapped.CopyFrom(otherWrapper._Wrapped);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="url"></param>
        public void Redirect(string url) => _Wrapped.Redirect(url);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="cookie"></param>
        public void SetCookie(Cookie cookie) => _Wrapped.SetCookie(cookie);
    }
}
