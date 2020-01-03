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
using System.Text;

namespace AWhewell.Owin.Utility
{
    /// <summary>
    /// Exposes a <see cref="HeadersDictionary"/> on the request headers in an OWIN environment.
    /// </summary>
    public class RequestHeadersDictionary : HeadersDictionary
    {
        static readonly string[] EmptyStringArray = new string[0];

        /// <summary>
        /// Gets the response MIME types that the client can accept.
        /// </summary>
        public IList<string> Accept => base.GetValues("Accept") ?? EmptyStringArray;

        /// <summary>
        /// Gets the authorization string sent by the browser.
        /// </summary>
        public string Authorization => base["Authorization"];

        /// <summary>
        /// Gets the content of the Cache-Control header.
        /// </summary>
        public string CacheControl => base["Cache-Control"];

        /// <summary>
        /// Gets the MIME type of the body.
        /// </summary>
        public string ContentType => base["Content-Type"];

        /// <summary>
        /// Gets <see cref="ContentType"/> parsed into a <see cref="ContentTypeValue"/>.
        /// </summary>
        public ContentTypeValue ContentTypeValue => ContentTypeValue.Parse(ContentType ?? "");

        /// <summary>
        /// Gets the origin in a CORS pre-flight request.
        /// </summary>
        public string Origin => base["Origin"];

        /// <summary>
        /// Gets the referrer header.
        /// </summary>
        public string Referer => base["Referer"];

        /// <summary>
        /// Gets the user agent string.
        /// </summary>
        public string UserAgent => base["User-Agent"];

        /// <summary>
        /// Gets the X-Forwarded-For IP address as a string.
        /// </summary>
        public string XForwardedFor => base["X-Forwarded-For"];

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public RequestHeadersDictionary() : base()
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="existingDictionary"></param>
        public RequestHeadersDictionary(IDictionary<string, string[]> existingDictionary) : base(existingDictionary)
        {
        }
    }
}
