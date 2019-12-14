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
using AWhewell.Owin.Utility;
using Newtonsoft.Json;

namespace AWhewell.Owin.WebApi.JsonNetWrapper
{
    /// <summary>
    /// Caches instances of JsonSerializerSettings for different instances of
    /// <see cref="TypeParserResolver"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Resolvers are immutable and, if they've been created through the <see cref="TypeParserResolverCache"/>,
    /// there should be a relatively small number of them. We always want to use the same settings with a given
    /// resolver, the settings are just a vehicle for a <see cref="JsonConverter"/> that uses that resolver, so
    /// we can associate instances of JsonSerializerSettings against resolvers and keep reusing them.
    /// </para><para>
    /// This assumes that JsonSerializerSettings are thread-safe. I don't know if that's actually the case?
    /// </para>
    /// </remarks>
    static class JsonSerialiserSettingsCache
    {
        // The object that protects writes to fields.
        private static object _SyncLock = new object();

        // The settings to return when no resolver is being used.
        private static JsonSerializerSettings _DefaultSettings;

        // A map of settings indexed by resolver.
        private static Dictionary<TypeParserResolver, JsonSerializerSettings> _Cache = new Dictionary<TypeParserResolver, JsonSerializerSettings>();

        // Initialises the static.
        static JsonSerialiserSettingsCache()
        {
            _DefaultSettings = CreateSettings(null);
        }

        /// <summary>
        /// Either returns the existing settings for the resolver passed across or creates a
        /// new settings object, caches it for future use and then returns it.
        /// </summary>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static JsonSerializerSettings Fetch(TypeParserResolver resolver)
        {
            JsonSerializerSettings result;

            if(resolver == null) {
                result = _DefaultSettings;
            } else {
                var cache = _Cache;
                if(!cache.TryGetValue(resolver, out result)) {
                    lock(_SyncLock) {
                        if(!_Cache.TryGetValue(resolver, out result)) {
                            result = CreateSettings(resolver);
                            var newCache = new Dictionary<TypeParserResolver, JsonSerializerSettings>(_Cache) {
                                [resolver] = result,
                            };
                            _Cache = newCache;
                        }
                    }
                }
            }

            return result;
        }

        private static JsonSerializerSettings CreateSettings(TypeParserResolver typeParserResolver)
        {
            return new JsonSerializerSettings() {
                Converters =        new JsonConverter[] { new ParserJsonConverter(typeParserResolver) },
                DateParseHandling = DateParseHandling.None,
            };
        }
    }
}
