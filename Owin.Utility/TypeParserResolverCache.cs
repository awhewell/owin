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
    /// A cache that tracks <see cref="TypeParserResolver"/> instances and returns
    /// existing instances for given combinations of parsers.
    /// </summary>
    public static class TypeParserResolverCache
    {
        /// <summary>
        /// The object used to lock <see cref="_Resolvers"/> for writing.
        /// </summary>
        private static object _SyncLock = new object();

        /// <summary>
        /// The list of cached resolvers. Take a copy of the reference before reading, lock
        /// before writing.
        /// </summary>
        private static TypeParserResolver[] _Resolvers = new TypeParserResolver[0];

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public static void Clear()
        {
            lock(_SyncLock) {
                _Resolvers = new TypeParserResolver[0];
            }
        }

        /// <summary>
        /// Returns an existing <see cref="TypeParserResolver"/> that has the exact same parsers
        /// as those passed in or creates and returns a new resolver if no such parser exists.
        /// </summary>
        /// <param name="parsers"></param>
        /// <returns></returns>
        public static TypeParserResolver Find(params ITypeParser[] parsers)
        {
            parsers = parsers ?? new ITypeParser[0];
            var result = FindInCache(parsers);

            if(result == null) {
                lock(_SyncLock) {
                    result = FindInCache(parsers);
                    if(result == null) {
                        result = new TypeParserResolver(parsers);
                        AddToCache(result);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the resolver that contains the same set of parsers as passed across or null
        /// if no such resolver has been cached.
        /// </summary>
        /// <param name="parsers"></param>
        /// <returns></returns>
        private static TypeParserResolver FindInCache(ITypeParser[] parsers)
        {
            var resolvers = _Resolvers;
            return Array.Find(resolvers, r => r.ParsersEquals(parsers));
        }

        /// <summary>
        /// Adds the resolver passed across to the cache. Must be called within a lock.
        /// </summary>
        /// <param name="resolver"></param>
        private static void AddToCache(TypeParserResolver resolver)
        {
            lock(_SyncLock) {
                var copy = new TypeParserResolver[_Resolvers.Length + 1];
                Array.Copy(_Resolvers, 0, copy, 0, _Resolvers.Length);
                copy[copy.Length - 1] = resolver;

                _Resolvers = copy;
            }
        }
    }
}
