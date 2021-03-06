﻿// Copyright © 2019 onwards, Andrew Whewell
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
    /// Functions that help deal with OWIN paths.
    /// </summary>
    public static class OwinPath
    {
        /// <summary>
        /// Rebuilds a request URL from the constituent parts as exposed in an OWIN environment.
        /// </summary>
        /// <param name="scheme"></param>
        /// <param name="host"></param>
        /// <param name="pathBase"></param>
        /// <param name="path"></param>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public static string ConstructUrl(string scheme, string host, string pathBase, string path, string queryString)
        {
            var result = new StringBuilder();

            result.Append(String.IsNullOrEmpty(scheme) ? "http" : scheme);
            result.Append("://");
            result.Append(String.IsNullOrEmpty(host) ? "127.0.0.1" : host);     // note that OWIN host headers can include the port

            ConstructUrlFromRoot(result, pathBase, path, queryString);

            return result.ToString();
        }

        /// <summary>
        /// Rebuilds a request URL starting from the path base (aka root).
        /// </summary>
        /// <param name="pathBase"></param>
        /// <param name="path"></param>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public static string ConstructUrlFromRoot(string pathBase, string path, string queryString)
        {
            var result = new StringBuilder();

            ConstructUrlFromRoot(result, pathBase, path, queryString);

            return result.ToString();
        }

        private static void ConstructUrlFromRoot(StringBuilder buffer, string pathBase, string path, string queryString)
        {
            buffer.Append(pathBase);
            buffer.Append(path);

            if(!String.IsNullOrEmpty(queryString)) {
                buffer.Append($"?{queryString}");       // note that OWIN presents query strings in their percent encoded form
            }
        }

        /// <summary>
        /// Splits a request path into a collection of path parts.
        /// </summary>
        /// <param name="requestPath"></param>
        /// <returns>A string array built by splitting the RequestPath at slashes after ignoring the initial slash.</returns>
        public static string[] RequestPathParts(string requestPath)
        {
            requestPath = requestPath ?? "";
            if(requestPath.Length > 0 && requestPath[0] == '/') {
                requestPath = requestPath.Substring(1);
            }

            return requestPath == "" ? new string[0] : requestPath.Split(new char[] { '/' });
        }

        /// <summary>
        /// Splits a request path into a collection of parts. Optionally caches the split or uses the cache if present.
        /// </summary>
        /// <param name="owinEnvironment"></param>
        /// <param name="createAndUseCachedResult"></param>
        /// <returns>A string array built by splitting the RequestPath at slashes after ignoring the initial slash.</returns>
        public static string[] RequestPathParts(IDictionary<string, object> owinEnvironment, bool createAndUseCachedResult)
        {
            var requestPath = owinEnvironment[EnvironmentKey.RequestPath] as string;

            if(!createAndUseCachedResult
               || !(owinEnvironment[CustomEnvironmentKey.RequestPathParts] is string[] result)
               || requestPath != (string)owinEnvironment[CustomEnvironmentKey.RequestPathPartsBasis]
            ) {
                result = RequestPathParts(requestPath);

                if(createAndUseCachedResult) {
                    owinEnvironment[CustomEnvironmentKey.RequestPathParts] = result;
                    owinEnvironment[CustomEnvironmentKey.RequestPathPartsBasis] = requestPath;
                }
            }

            return result;
        }
    }
}
