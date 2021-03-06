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
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace AWhewell.Owin.Interface.WebApi
{
    /// <summary>
    /// An exception that can be thrown to force an HTTP response back to the caller.
    /// </summary>
    [Serializable]
    public class HttpResponseException : Exception
    {
        /// <summary>
        /// Gets the status code to return to the caller.
        /// </summary>
        public HttpStatusCode StatusCode { get; } = HttpStatusCode.OK;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public HttpResponseException() : this("") { }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="message"></param>
        public HttpResponseException(string message) : base(message) { }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public HttpResponseException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="statusCode"></param>
        public HttpResponseException(HttpStatusCode statusCode) : this(statusCode, "") { }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="message"></param>
        public HttpResponseException(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected HttpResponseException(SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
