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

namespace AWhewell.Owin.Utility
{
    /// <summary>
    /// A dictionary whose indexed get returns default(TValue) when fetching a non-existent key instead of
    /// throwing an exception.
    /// </summary>
    /// <typeparam name="TValue">The type of value stored by the dictionary.</typeparam>
    public class OwinDictionary<TValue> : IDictionary<string, TValue>
    {
        /// <summary>
        /// The dictionary that we are wrapping.
        /// </summary>
        private IDictionary<string, TValue> _Dictionary;

        /// <summary>
        /// Gets the dictionary that has been wrapped.
        /// </summary>
        public IDictionary<string, TValue> WrappedDictionary => _Dictionary;

        /// <summary>
        /// See class summary.
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public TValue this[string idx]
        {
            get {
                _Dictionary.TryGetValue(idx, out var result);
                return result;
            }
            set => _Dictionary[idx] = value;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public ICollection<string> Keys => _Dictionary.Keys;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public ICollection<TValue> Values => _Dictionary.Values;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int Count => _Dictionary.Count;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsReadOnly => _Dictionary.IsReadOnly;

        /// <summary>
        /// Creates a new object. Defaults to being case sensitive.
        /// </summary>
        public OwinDictionary() : this(caseSensitive: true)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="caseSensitive"></param>
        public OwinDictionary(bool caseSensitive)
        {
            _Dictionary = new Dictionary<string, TValue>(
                caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase
            );
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="existingDictionary"></param>
        public OwinDictionary(IDictionary<string, TValue> existingDictionary)
        {
            if(existingDictionary == null) {
                throw new ArgumentNullException(nameof(existingDictionary));
            }
            if(existingDictionary.IsReadOnly) {
                throw new InvalidOperationException("The OWIN specification requires that dictionaries are mutable. Read-only dictionaries cannot be wrapped.");
            }
            _Dictionary = existingDictionary;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, TValue value) => _Dictionary.Add(key, value);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="item"></param>
        public void Add(KeyValuePair<string, TValue> item) => _Dictionary.Add(item);

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Clear() => _Dictionary.Clear();

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(KeyValuePair<string, TValue> item) => _Dictionary.Contains(item);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key) => _Dictionary.ContainsKey(key);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex) => _Dictionary.CopyTo(array, arrayIndex);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator() => _Dictionary.GetEnumerator();

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_Dictionary).GetEnumerator();

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(string key) => _Dictionary.Remove(key);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(KeyValuePair<string, TValue> item) => _Dictionary.Remove(item);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(string key, out TValue value) => _Dictionary.TryGetValue(key, out value);
    }
}
