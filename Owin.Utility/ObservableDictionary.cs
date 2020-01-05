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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AWhewell.Owin.Utility
{
    /// <summary>
    /// An observable dictionary.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <remarks>
    /// Some host wrappers need to pass changes from an OWIN environment to the host as soon as they
    /// are made. An observable dictionary makes this possible. The intention is that for the sake of
    /// speed consumers of this class would subclass it rather than hook events.
    /// </remarks>
    public abstract class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        /// <summary>
        /// The dictionary that this is wrapping.
        /// </summary>
        private IDictionary<TKey, TValue> _Wrapped;

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue this[TKey key]
        {
            get => _Wrapped[key];
            set {
                _Wrapped[key] = value;
                OnAssigned(key, value);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public ICollection<TKey> Keys => _Wrapped.Keys;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public ICollection<TValue> Values => _Wrapped.Values;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int Count => _Wrapped.Count;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ObservableDictionary()
        {
            _Wrapped = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="equalityComparer"></param>
        public ObservableDictionary(IEqualityComparer<TKey> equalityComparer)
        {
            _Wrapped = new Dictionary<TKey, TValue>(equalityComparer);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="wrapDictionary"></param>
        protected ObservableDictionary(IDictionary<TKey, TValue> wrapDictionary)
        {
            _Wrapped = wrapDictionary;
        }

        /// <summary>
        /// Called when a value is either added or changed.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        protected abstract void OnAssigned(TKey key, TValue value);

        /// <summary>
        /// Called when a value is removed (but not when the dictionary is cleared).
        /// </summary>
        /// <param name="key"></param>
        protected abstract void OnRemoved(TKey key);

        /// <summary>
        /// Called when the entire dictionary is cleared.
        /// </summary>
        protected abstract void OnReset();

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(TKey key, TValue value)
        {
            _Wrapped.Add(key, value);
            OnAssigned(key, value);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="item"></param>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _Wrapped.Add(item);
            OnAssigned(item.Key, item.Value);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Clear()
        {
            _Wrapped.Clear();
            OnReset();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(KeyValuePair<TKey, TValue> item) => _Wrapped.Contains(item);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(TKey key) => _Wrapped.ContainsKey(key);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => _Wrapped.CopyTo(array, arrayIndex);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _Wrapped.GetEnumerator();

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_Wrapped).GetEnumerator();

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(TKey key)
        {
            var result = _Wrapped.Remove(key);
            if(result) {
                OnRemoved(key);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            var result = _Wrapped.Remove(item);
            if(result) {
                OnRemoved(item.Key);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TValue value) => _Wrapped.TryGetValue(key, out value);
    }
}
