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
using System.Linq;

namespace AWhewell.Owin.Utility
{
    /// <summary>
    /// Wraps an OWIN owin.RequestQueryString query string to access keys and values from within it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Query strings are split on ampersands or semicolons. Keys are case sensitive by default but can
    /// be made case insensitive to comply with Microsoft's query string implementations.
    /// </para><para>
    /// Query strings can be expressed as either a string or an array of strings. If the same
    /// key is seen more than once during parsing then each value is added to the array of values.
    /// </para><para>
    /// Searches for a non-existent key return a null string and a null array.
    /// </para><para>
    /// Searches for keys that had no value return an empty string and an empty array.
    /// </para><para>
    /// Searches for keys with an equals sign but no value return an empty string and an array with
    /// a single empty string.
    /// </para><para>
    /// Searches for keys with multiple values return a comma-separated string of all values and an
    /// array with all values.
    /// </para>
    /// </remarks>
    public class QueryStringDictionary : IReadOnlyDictionary<string, string>
    {
        /// <summary>
        /// An enumerator that exposes values in the underlying Dictionary&lt;string, string[]&gt; as
        /// strings.
        /// </summary>
        class Enumerator : IEnumerator<KeyValuePair<string, string>>
        {
            private readonly IEnumerator<KeyValuePair<string, string[]>> _WrappedEnumerator;

            public KeyValuePair<string, string> Current => TranslateWrappedValue(_WrappedEnumerator.Current);

            object IEnumerator.Current => TranslateWrappedValue(_WrappedEnumerator.Current);

            public Enumerator(QueryStringDictionary parentDictionary)
            {
                _WrappedEnumerator = parentDictionary._KeyValueMap.GetEnumerator();
            }

            public void Dispose() => _WrappedEnumerator.Dispose();

            public bool MoveNext() => _WrappedEnumerator.MoveNext();

            public void Reset() => _WrappedEnumerator.Reset();

            private KeyValuePair<string, string> TranslateWrappedValue(KeyValuePair<string, string[]> wrappedKvp)
            {
                return new KeyValuePair<string, string>(
                    wrappedKvp.Key,
                    wrappedKvp.Value == null ? null : JoinValue(wrappedKvp.Value)
                );
            }
        }

        /// <summary>
        /// The object that splits key=value pairs up for us.
        /// </summary>
        private static KeyValueParser _KeyValueParser = new KeyValueParser('=', null);

        /// <summary>
        /// The backing dictionary.
        /// </summary>
        private Dictionary<string, string[]> _KeyValueMap;

        /// <summary>
        /// Gets the query string used to initialise the dictionary.
        /// </summary>
        public string QueryString { get; }

        /// <summary>
        /// Gets a value indicating that the keys are case sensitive.
        /// </summary>
        public bool CaseSensitiveKeys { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="queryString">The owin.RequestQueryString value being wrapped.</param>
        /// <param name="caseSensitiveKeys">
        /// True if keys are case sensitive, otherwise false. The RFCs say that query strings are case
        /// sensitive but Microsoft's implementations are not. By default the dictionary is case
        /// sensitive.
        /// </param>
        public QueryStringDictionary(string queryString, bool caseSensitiveKeys)
        {
            QueryString = queryString ?? "";
            CaseSensitiveKeys = caseSensitiveKeys;
            BuildKeyValueMap();
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="queryString">The owin.RequestQueryString value being wrapped.</param>
        public QueryStringDictionary(string queryString) : this(queryString, caseSensitiveKeys: true)
        {
        }

        private void BuildKeyValueMap()
        {
            _KeyValueMap = new Dictionary<string, string[]>(CaseSensitiveKeys ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);

            string key, value;
            foreach(var nameValue in QueryString.Split('&', ';')) {
                _KeyValueParser.Parse(nameValue, out key, out value);
                AddKeyValue(key, value);
            }
        }

        void AddKeyValue(string key, string value)
        {
            key = key == null ? null : Uri.UnescapeDataString(key);
            value = value == null ? null : Uri.UnescapeDataString(value);

            if(key != "") {
                if(!_KeyValueMap.TryGetValue(key, out var existing)) {
                    var valueArray = value == null ? new string[0] : new string[] { value };
                    _KeyValueMap.Add(key, valueArray);
                } else if(value != null) {
                    var newArray = new string[existing.Length + 1];
                    Array.Copy(existing, newArray, existing.Length);
                    newArray[newArray.Length - 1] = value;
                    _KeyValueMap[key] = newArray;
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key]
        {
            get {
                string[] result = null;
                if(key != null) {
                    _KeyValueMap.TryGetValue(key, out result);
                }
                return result == null ? null : JoinValue(result);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IEnumerable<string> Keys => _KeyValueMap.Keys;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IEnumerable<string> Values => _KeyValueMap.Values.Select(r => JoinValue(r));

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int Count => _KeyValueMap.Count;

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key) => _KeyValueMap.ContainsKey(key);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => new Enumerator(this);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(string key, out string value)
        {
            var result = _KeyValueMap.TryGetValue(key, out var arrayValue);
            value = arrayValue == null ? null : JoinValue(arrayValue);

            return result;
        }

        /// <summary>
        /// Returns the query string as an array of strings. If the key does not exist then null is returned.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string[] GetValues(string key)
        {
            string[] result = null;
            if(key != null) {
                _KeyValueMap.TryGetValue(key, out result);
            }
            return result;
        }

        /// <summary>
        /// Returns a value as a single string. Values with more than one element are joined together into a
        /// single string. Unknown keys return null. Keys with no value or keys with a single empty string
        /// value both return an empty string.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="join">The string to use to join values together</param>
        /// <returns></returns>
        public string GetValue(string key, string join)
        {
            _KeyValueMap.TryGetValue(key, out var value);
            return JoinValue(value, join);
        }

        /// <summary>
        /// Returns a value as a single string. Values with more than one element are joined together into a
        /// single string. Unknown keys return null. Keys with no value or keys with a single empty string
        /// value both return an empty string.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValue(string key) => GetValue(key, ",");

        /// <summary>
        /// Returns the value joined together using the join string supplied.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="join"></param>
        /// <returns></returns>
        public static string JoinValue(string[] value, string join)
        {
            if(join == null) {
                throw new ArgumentNullException(nameof(join));
            }

            return value == null ? null : String.Join(join, value);
        }

        /// <summary>
        /// Returns the value joined together using a comma separator.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string JoinValue(string[] value) => JoinValue(value, ",");
    }
}
