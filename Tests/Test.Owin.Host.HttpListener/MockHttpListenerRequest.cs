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
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using Moq;
using Owin.Interface.HttpListenerWrapper;

namespace Test.Owin.Host.HttpListener
{
    class MockHttpListenerRequest : Mock<IHttpListenerRequest>
    {
        public WebHeaderCollection Headers { get; } = new WebHeaderCollection();

        public bool OptionalBodyPresent { get; set; }

        public MockHttpListenerRequest() : base()
        {
            DefaultValue = DefaultValue.Mock;
            SetupAllProperties();

            SetupGet(r => r.Headers).Returns(Headers);
            SetUrl("/");

            SetupGet(r => r.LocalEndPoint).Returns((IPEndPoint)null);
            SetupGet(r => r.RemoteEndPoint).Returns((IPEndPoint)null);

            SetupGet(r => r.HasEntityBody)
            .Returns(() => {
                switch((Object.HttpMethod ?? "").ToUpper()) {
                    case "PATCH":
                    case "POST":
                    case "PUT":
                        return true;
                    case "DELETE":
                        return OptionalBodyPresent;
                    default:
                        return false;
                }
            });
        }

        public void SetMethodAndStream(string method, Stream stream)
        {
            SetupGet(r => r.HttpMethod).Returns(method);
            SetupGet(r => r.InputStream).Returns(stream);
        }

        public void SetUrl(string url, bool httpListenerUrlUnescapesPath = true, string scheme = "http")
        {
            // In testing under .NET 4 the Uri presented by HttpListenerRequest.Url
            // has its path unescaped and its query string left escaped. However,
            // under .NET Core 3 it does not, percent-encoded characters are not
            // unescaped in the path. In both environments RawUrl has the unescaped
            // path and query string without the protocol, host or port.

            SetupGet(r => r.RawUrl).Returns(url);

            Uri.TryCreate($"{scheme}://127.0.0.1:8080{url}", UriKind.Absolute, out var uri);
            if(httpListenerUrlUnescapesPath) {
                var unescapedUrl = $"{uri.Scheme}://{uri.Host}";
                if(uri.Port != 80) {
                    unescapedUrl = $"{unescapedUrl}:{uri.Port}";
                }
                var path = uri.AbsolutePath;
                while(path != Uri.UnescapeDataString(path)) {
                    path = Uri.UnescapeDataString(path);
                }
                unescapedUrl = $"{unescapedUrl}{path}";
                if(!String.IsNullOrEmpty(uri.Query)) {
                    unescapedUrl = $"{unescapedUrl}{uri.Query}";
                }

                Uri.TryCreate(unescapedUrl, UriKind.Absolute, out var uriUnescaped);
                uri = uriUnescaped;
            }

            SetupGet(r => r.Url).Returns(uri);
        }
    }
}
