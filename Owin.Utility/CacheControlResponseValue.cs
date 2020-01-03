// Copyright © 2020 onwards, Andrew Whewell
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
    /// A response Cache-Control header value parsed out into discrete values.
    /// </summary>
    public class CacheControlResponseValue
    {
        private static KeyValueParser _KeyValueParser = new KeyValueParser('=', "");

        public int? MaxAgeSeconds { get; }

        public int? SMaxAgeSeconds { get; }

        public bool MustRevalidate { get; }

        public bool NoCache { get; }

        public bool NoStore { get; }

        public bool NoTransform { get; }

        public bool IsPublic { get; }

        public bool IsPrivate { get; }

        public bool ProxyRevalidate { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="maxAgeSeconds"></param>
        /// <param name="sMaxAgeSeconds"></param>
        /// <param name="mustRevalidate"></param>
        /// <param name="noCache"></param>
        /// <param name="noStore"></param>
        /// <param name="noTransform"></param>
        /// <param name="isPublic"></param>
        /// <param name="isPrivate"></param>
        /// <param name="proxyRevalidate"></param>
        public CacheControlResponseValue(
            int? maxAgeSeconds = null,
            int? sMaxAgeSeconds = null,
            bool mustRevalidate = false,
            bool noCache = false,
            bool noStore = false,
            bool noTransform = false,
            bool isPublic = false,
            bool isPrivate = false,
            bool proxyRevalidate = false
        )
        {
            MaxAgeSeconds =     maxAgeSeconds;
            SMaxAgeSeconds =    sMaxAgeSeconds;
            MustRevalidate =    mustRevalidate;
            NoCache =           noCache;
            NoStore =           noStore;
            NoTransform =       noTransform;
            IsPublic =          isPublic;
            IsPrivate =         isPrivate;
            ProxyRevalidate =   proxyRevalidate;
        }

        /// <summary>
        /// Parses a header string into a <see cref="CacheControlResponseValue"/>.
        /// </summary>
        /// <param name="headerValue"></param>
        /// <returns></returns>
        public static CacheControlResponseValue Parse(string headerValue)
        {
            CacheControlResponseValue result = null;

            if(headerValue != null) {
                int? maxAgeSeconds = null;
                int? sMaxAgeSeconds = null;
                var mustRevalidate = false;
                var noCache = false;
                var noStore = false;
                var noTransform = false;
                var isPublic = false;
                var isPrivate = false;
                var proxyRevalidate = false;

                string key, value;
                foreach(var chunk in headerValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) {
                    _KeyValueParser.Parse(chunk.Trim(), out key, out value);

                    key = key.Trim().ToLower();
                    value = value.Trim();

                    switch(key.ToLower()) {
                        case "max-age":             maxAgeSeconds = Parser.ParseInt32(value); break;
                        case "s-maxage":            sMaxAgeSeconds = Parser.ParseInt32(value); break;
                        case "must-revalidate":     mustRevalidate = true; break;
                        case "no-cache":            noCache = true; break;
                        case "no-store":            noStore = true; break;
                        case "no-transform":        noTransform = true; break;
                        case "public":              isPublic = true; break;
                        case "private":             isPrivate = true; break;
                        case "proxy-revalidate":    proxyRevalidate = true; break;
                    }
                }

                result = new CacheControlResponseValue(
                    maxAgeSeconds:      maxAgeSeconds,
                    sMaxAgeSeconds:     sMaxAgeSeconds,
                    mustRevalidate:     mustRevalidate,
                    noCache:            noCache,
                    noStore:            noStore,
                    noTransform:        noTransform,
                    isPublic:           isPublic,
                    isPrivate:          isPrivate,
                    proxyRevalidate:    proxyRevalidate
                );
            }

            return result;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var result = new StringBuilder();

            if(MaxAgeSeconds != null) {
                result.AppendWithSeparator(",", $"max-age={MaxAgeSeconds}");
            }
            if(SMaxAgeSeconds != null) {
                result.AppendWithSeparator(",", $"s-maxage={SMaxAgeSeconds}");
            }
            if(MustRevalidate) {
                result.AppendWithSeparator(",", "must-revalidate");
            }
            if(NoCache) {
                result.AppendWithSeparator(",", "no-cache");
            }
            if(NoStore) {
                result.AppendWithSeparator(",", "no-store");
            }
            if(NoTransform) {
                result.AppendWithSeparator(",", "no-transform");
            }
            if(IsPublic) {
                result.AppendWithSeparator(",", "public");
            }
            if(IsPrivate) {
                result.AppendWithSeparator(",", "private");
            }
            if(ProxyRevalidate) {
                result.AppendWithSeparator(",", "proxy-revalidate");
            }

            return result.ToString();
        }
    }
}
