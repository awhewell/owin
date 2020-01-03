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
using System.Text;

namespace AWhewell.Owin.Utility
{
    /// <summary>
    /// Describes the content of a Cache-Control header value in a request.
    /// </summary>
    public class CacheControlRequestValue
    {
        private static KeyValueParser _KeyValueParser = new KeyValueParser('=', "");

        public int? MaxAgeSeconds { get; }

        public int? MaxStaleSeconds { get; }

        public bool NoCache { get; }

        public bool NoStore { get; }

        public bool NoTransform { get; }

        public bool OnlyIfCached { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="maxAgeSeconds"></param>
        /// <param name="maxStaleSeconds"></param>
        /// <param name="noCache"></param>
        /// <param name="noStore"></param>
        /// <param name="noTransform"></param>
        /// <param name="onlyIfCached"></param>
        public CacheControlRequestValue(
            int? maxAgeSeconds = null,
            int? maxStaleSeconds = null,
            bool noCache = false,
            bool noStore = false,
            bool noTransform = false,
            bool onlyIfCached = false
        )
        {
            MaxAgeSeconds =     maxAgeSeconds;
            MaxStaleSeconds =   maxStaleSeconds;
            NoCache =           noCache;
            NoStore =           noStore;
            NoTransform =       noTransform;
            OnlyIfCached =      onlyIfCached;
        }

        /// <summary>
        /// Parses a request cache control value into an object.
        /// </summary>
        /// <param name="headerValue"></param>
        /// <returns></returns>
        public static CacheControlRequestValue Parse(string headerValue)
        {
            CacheControlRequestValue result = null;

            if(headerValue != null) {
                int? maxAgeSeconds = null;
                int? maxStaleSeconds = null;
                var noCache = false;
                var noStore = false;
                var noTransform = false;
                var onlyIfCached = false;

                string key, value;
                foreach(var chunk in headerValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) {
                    _KeyValueParser.Parse(chunk.Trim(), out key, out value);

                    key = key.Trim().ToLower();
                    value = value.Trim();

                    switch(key.ToLower()) {
                        case "max-age":         maxAgeSeconds = Parser.ParseInt32(value); break;
                        case "max-stale":       maxStaleSeconds = value == "" ? int.MaxValue : Parser.ParseInt32(value); break;
                        case "no-cache":        noCache = true; break;
                        case "no-store":        noStore = true; break;
                        case "no-transform":    noTransform = true; break;
                        case "only-if-cached":  onlyIfCached = true; break;
                    }
                }

                result = new CacheControlRequestValue(
                    maxAgeSeconds:      maxAgeSeconds,
                    maxStaleSeconds:    maxStaleSeconds,
                    noCache:            noCache,
                    noStore:            noStore,
                    noTransform:        noTransform,
                    onlyIfCached:       onlyIfCached
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
            if(MaxStaleSeconds != null) {
                result.AppendWithSeparator(
                    ",",
                    MaxStaleSeconds == int.MaxValue
                        ? "max-stale"
                        : $"max-stale={MaxStaleSeconds}"
                );
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
            if(OnlyIfCached) {
                result.AppendWithSeparator(",", "only-if-cached");
            }

            return result.ToString();
        }
    }
}
