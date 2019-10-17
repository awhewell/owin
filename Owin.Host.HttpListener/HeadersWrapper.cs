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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using Owin.Interface;

namespace Owin.Host.HttpListener
{
    /// <summary>
    /// A headers dictionary that wraps a System.Net.WebHeaderCollection.
    /// </summary>
    /// <remarks>
    /// The HttpListener
    /// </remarks>
    class HeadersWrapper : IDictionary<string, string[]>
    {
        /// <summary>
        /// The wrapped headers collection.
        /// </summary>
        private WebHeaderCollection _Collection;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="collection"></param>
        public HeadersWrapper(WebHeaderCollection collection)
        {
            _Collection = collection;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual string[] this[string key]
        {
            get {
                var headerValue = _Collection[key];
                return headerValue == null
                    ? null
                    : HeadersDictionary.SplitRawHeaderValue(headerValue)?.ToArray();
            }
            set {
                if(key == null) {
                    throw new ArgumentNullException(nameof(key));
                }
                _Collection[key] = HeadersDictionary.JoinCookedHeaderValues(value);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public ICollection<string> Keys => _Collection.AllKeys;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public ICollection<string[]> Values
        {
            get {
                var result = new List<string[]>();

                foreach(var key in _Collection.AllKeys) {
                    result.Add(HeadersDictionary.SplitRawHeaderValue(_Collection[key])?.ToArray());
                }

                return result;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int Count => _Collection.Count;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, string[] value)
        {
            if(key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            // This can potentially be expensive... but at the same time the IDictionary documentation
            // explicitly calls out that duplicate adds trigger the exception.
            if(ContainsKey(key)) {
                throw new ArgumentException($"There is already a key of {key} in the collection");
            }

            this[key] = value;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="item"></param>
        public void Add(KeyValuePair<string, string[]> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Clear() => _Collection.Clear();

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(KeyValuePair<string, string[]> item)
        {
            var value = this[item.Key];
            return value != null && item.Value != null      // <-- note that WebHeaderCollection coalesces null values, a null string[] in the kvp can't match anything
                ? value.SequenceEqual(item.Value)
                : false;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key) => _Collection.AllKeys.Contains(key, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="startIndex"></param>
        public void CopyTo(KeyValuePair<string, string[]>[] array, int startIndex)
        {
            if(array == null) {
                throw new ArgumentNullException(nameof(array));
            }
            if(startIndex < 0) {
                throw new ArgumentOutOfRangeException($"{nameof(startIndex)} must be positive");
            }
            if(array.Length < startIndex + _Collection.Count) {
                throw new ArgumentException($"An array of {array.Length} elements is not large enough to accommodate {_Collection.Count} items starting at {startIndex}");
            }

            var keys = _Collection.AllKeys;
            for(int keyIdx = 0, arrayIdx = startIndex;keyIdx < keys.Length;++keyIdx, ++arrayIdx) {
                var key = keys[keyIdx];
                array[arrayIdx] = new KeyValuePair<string, string[]>(key, this[key]);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
        {
            var keys = _Collection.AllKeys;
            for(var i = 0;i < keys.Length;++i) {
                var key = keys[i];
                yield return new KeyValuePair<string, string[]>(key, this[key]);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(string key)
        {
            if(key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            var countBeforeRemove = Count;
            _Collection.Remove(key);

            return Count != countBeforeRemove;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(KeyValuePair<string, string[]> item)
        {
            var result = false;

            if(Contains(item)) {
                result = Remove(item.Key);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(string key, out string[] value)
        {
            if(key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            value = this[key];
            return ContainsKey(key);
        }

        /// <summary>
        /// Returns the first element in the array.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        protected static string FirstElement(string[] array)
        {
            return array == null || array.Length == 0 ? null : array[0];
        }
    }
}
