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
using System.Linq;
using System.Net;
using AWhewell.Owin.Interface.Host.HttpListener.HttpListenerWrapper;
using AWhewell.Owin.Utility;

namespace AWhewell.Owin.Host.HttpListener
{
    /// <summary>
    /// A header dictionary that intercepts attempts to set HttpListenerResponse restricted header
    /// values and makes the appropriate response calls instead.
    /// </summary>
    class HeadersWrapper_Response : ObservableDictionary<string, string[]>
    {
        private IHttpListenerResponse _Response;
        private bool _DoingInitialCopy;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="collection"></param>
        public HeadersWrapper_Response(IHttpListenerResponse response) : base(StringComparer.OrdinalIgnoreCase)
        {
            _Response = response;

            _DoingInitialCopy = true;
            try {
                foreach(var key in response.Headers.AllKeys) {
                    var value = response.Headers[key];
                    base[key] = HeadersDictionary.SplitRawHeaderValue(value).ToArray();
                }
            } finally {
                _DoingInitialCopy = false;
            }
        }

        protected override void OnAssigned(string key, string[] value)
        {
            if(!_DoingInitialCopy) {
                switch((key ?? "").ToLower()) {
                    case "content-length":
                        _Response.ContentLength64 = Parser.ParseInt64(HeadersDictionary.JoinCookedHeaderValues(value)) ?? 0L;
                        break;
                    case "keep-alive":
                        _Response.KeepAlive = Parser.ParseBool(HeadersDictionary.JoinCookedHeaderValues(value)) ?? true;
                        break;
                    case "transfer-encoding":
                        if(
                            (value ?? new string[0])
                            .Any(r => (r ?? "").Trim().ToLower() == "chunked")
                        ) {
                            _Response.SendChunked = true;
                        }
                        break;
                    case "www-authenticate":
                        _Response.AddHeader("WWW-Authenticate", HeadersDictionary.JoinCookedHeaderValues(value));
                        break;
                    default:
                        _Response.Headers[key] = HeadersDictionary.JoinCookedHeaderValues(value);
                        break;
                }
            }
        }

        protected override void OnRemoved(string key)
        {
            ;
        }

        protected override void OnReset()
        {
            _Response.Headers.Clear();
        }
    }
}
