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
    static class JsonSerialiserSettingsCache
    {
        private static object _SyncLock = new object();

        private static JsonSerializerSettings _DefaultParser;

        private static Dictionary<TypeParserResolver, JsonSerializerSettings> _Cache = new Dictionary<TypeParserResolver, JsonSerializerSettings>();

        static JsonSerialiserSettingsCache()
        {
            _DefaultParser = CreateSettings(null);
        }

        public static JsonSerializerSettings Fetch(TypeParserResolver resolver)
        {
            JsonSerializerSettings result;

            if(resolver == null) {
                result = _DefaultParser;
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

        private static JsonSerializerSettings CreateSettings(TypeParserResolver resolver)
        {
            return new JsonSerializerSettings() {
                Converters =        new JsonConverter[] { new ParserJsonConverter(resolver) },
                DateParseHandling = DateParseHandling.None,
            };
        }
    }
}
