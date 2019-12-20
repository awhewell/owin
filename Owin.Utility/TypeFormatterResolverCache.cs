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
    /// A cache that tracks <see cref="TypeFormatterResolver"/> instances and returns
    /// existing instances for given combinations of formatters.
    /// </summary>
    public static class TypeFormatterResolverCache
    {
        /// <summary>
        /// The object used to lock <see cref="_Resolvers"/> for writing.
        /// </summary>
        private static readonly object _SyncLock = new object();

        /// <summary>
        /// The list of cached resolvers. Take a copy of the reference before reading, lock
        /// before writing.
        /// </summary>
        private static TypeFormatterResolver[] _Resolvers = new TypeFormatterResolver[0];

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public static void Clear()
        {
            lock(_SyncLock) {
                _Resolvers = new TypeFormatterResolver[0];
            }
        }

        /// <summary>
        /// Returns an existing <see cref="TypeFormatterResolver"/> that has the exact same formatters as
        /// those passed in or creates and returns a new resolver if no such formatter combination exists.
        /// </summary>
        /// <param name="formatters"></param>
        /// <returns></returns>
        public static TypeFormatterResolver Find(params ITypeFormatter[] formatters)
        {
            formatters = formatters ?? new ITypeFormatter[0];
            var result = FindInCache(formatters);

            if(result == null) {
                lock(_SyncLock) {
                    result = FindInCache(formatters);
                    if(result == null) {
                        result = new TypeFormatterResolver(formatters);
                        AddToCache(result);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the resolver that contains the same set of formatters as passed across or null if no such
        /// resolver has been cached.
        /// </summary>
        /// <param name="formatters"></param>
        /// <returns></returns>
        private static TypeFormatterResolver FindInCache(ITypeFormatter[] formatters)
        {
            var resolvers = _Resolvers;
            return Array.Find(resolvers, r => r.FormattersEquals(formatters));
        }

        /// <summary>
        /// Adds the resolver passed across to the cache. Must be called within a lock.
        /// </summary>
        /// <param name="resolver"></param>
        private static void AddToCache(TypeFormatterResolver resolver)
        {
            lock(_SyncLock) {
                var copy = new TypeFormatterResolver[_Resolvers.Length + 1];
                Array.Copy(_Resolvers, 0, copy, 0, _Resolvers.Length);
                copy[copy.Length - 1] = resolver;

                _Resolvers = copy;
            }
        }
    }
}
