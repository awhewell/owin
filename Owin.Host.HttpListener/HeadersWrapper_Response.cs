﻿// Copyright © 2019 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Linq;
using System.Net;
using Owin.Interface;
using Owin.Interface.Host.HttpListener.HttpListenerWrapper;

namespace Owin.Host.HttpListener
{
    /// <summary>
    /// Extends <see cref="HeadersWrapper"/> to intercept attempts to set restricted headers
    /// and set the appropriate reponse properties / call response functions instead.
    /// </summary>
    class HeadersWrapper_Response : HeadersWrapper
    {
        private IHttpListenerResponse _Response;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="collection"></param>
        public HeadersWrapper_Response(IHttpListenerResponse response, WebHeaderCollection collection) : base(collection)
        {
            _Response = response;
        }

        public override string[] this[string key]
        {
            get => base[key];
            set {
                var normalisedKey = (key ?? "").ToLower();
                switch(normalisedKey) {
                    case "content-length":      _Response.ContentLength64 = Parser.ParseInt64(FirstElement(value)) ?? 0L; break;
                    case "keep-alive":          _Response.KeepAlive =       Parser.ParseBool(FirstElement(value)) ?? true; break;
                    case "transfer-encoding":
                        if(
                            (value ?? new string[0])
                            .Any(r => (r ?? "").Trim().ToLower() == "chunked")
                        ) {
                            _Response.SendChunked = true;
                        }
                        break;
                    default:                    base[key] = value; break;
                }
            }
        }
    }
}
