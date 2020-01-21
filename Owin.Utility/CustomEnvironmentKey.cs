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
    /// A collection of custom environment keys.
    /// </summary>
    public static class CustomEnvironmentKey
    {
        /// <summary>
        /// Value is a string representing the client's IP address after taking possible proxies into account.
        /// </summary>
        public const string ClientIpAddress = "awowin.ClientIpAddress";

        /// <summary>
        /// Value is the basis on which <see cref="ClientIpAddress"/> and <see cref="ProxyIpAddress"/> was calculated.
        /// </summary>
        public const string ClientIpAddressBasis = "awowin.ClientIpAddressBasis";

        /// <summary>
        /// Value is an IPAddress parsed from <see cref="ClientIpAddress"/>.
        /// </summary>
        public const string ClientIpAddressParsed = "awowin.ClientIpAddressParsed";

        /// <summary>
        /// Value is an <see cref="OwinContext"/> object that was created by a prior call to <see cref="OwinContext.Create"/>.
        /// </summary>
        public const string Context =  "awowin.Context";

        /// <summary>
        /// Value is a string representing the last proxy that the request was routed through, if any.
        /// </summary>
        public const string ProxyIpAddress = "awowin.ProxyIpAddress";

        /// <summary>
        /// Value is the content body bytes. See <see cref="OwinContext.RequestBodyBytes"/>.
        /// </summary>
        public const string RequestBodyBytes = "awowin.RequestBodyBytes";

        /// <summary>
        /// Value is the stream reference used to build the <see cref="RequestBodyBytes"/> cache.
        /// </summary>
        public const string RequestBodyBytesBasis = "awowin.RequestBodyBytesBasis";

        /// <summary>
        /// Value is a long that uniquely identifies the request. The pipeline initialises this to a value that is unique
        /// across all instances of all pipelines but the Owin library makes no assumptions about it and places no meaning
        /// in its value, other than a null value indicates the request has no ID. Pipelines or hosts can replace it with
        /// their own ID if convenient.
        /// </summary>
        public const string RequestID = "aowin.RequestID";

        /// <summary>
        /// Value is a string array resulting from splitting the RequestPath at slashes after ignoring the initial slash.
        /// </summary>
        public const string RequestPathParts = "awowin.RequestPathParts";

        /// <summary>
        /// Value is the path that <see cref="RequestPathParts"/> was built from.
        /// </summary>
        public const string RequestPathPartsBasis = "awowin.RequestPathPartsBasis";
    }
}
