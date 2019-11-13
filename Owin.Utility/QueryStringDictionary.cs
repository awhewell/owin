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
    /// Wraps an OWIN owin.RequestQueryString query string to access keys and values from within it.
    /// </summary>
    /// <remarks>
    /// Query strings are split on ampersands or semicolons. Keys are case sensitive by default but can
    /// be made case insensitive to comply with Microsoft's query string implementations. If the same
    /// key is seen more than once then each value is added to the string result. Indexed searches can
    /// use a key that does not exist - in that case they will return null. Keys with no value return
    /// an empty array. Keys with an equals sign but not value return an array containing a single
    /// empty string.
    /// </remarks>
    public class QueryStringDictionary : IReadOnlyDictionary<string, string[]>
    {
        /// <summary>
        /// The backing dictionary.
        /// </summary>
        private Dictionary<string, string[]> _KeyValueMap;

        /// <summary>
        /// Gets the query string used to initialise the dictionary.
        /// </summary>
        public string QueryString { get; }

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
            BuildKeyValueMap(caseSensitiveKeys);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="queryString">The owin.RequestQueryString value being wrapped.</param>
        public QueryStringDictionary(string queryString) : this(queryString, caseSensitiveKeys: true)
        {
        }

        private void BuildKeyValueMap(bool caseSensitiveKeys)
        {
            _KeyValueMap = new Dictionary<string, string[]>(caseSensitiveKeys ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);

            void addKeyValue(string key, string value)
            {
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

            foreach(var nameValue in QueryString.Split('&', ';')) {
                var valueIdx = nameValue.IndexOf('=');
                if(valueIdx == -1) {
                    addKeyValue(nameValue, null);
                } else if(valueIdx > 0) {
                    var key = Uri.UnescapeDataString(nameValue.Substring(0, valueIdx));
                    var value = Uri.UnescapeDataString(nameValue.Substring(valueIdx + 1));

                    addKeyValue(key, value);
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string[] this[string key]
        {
            get {
                string[] result = null;
                if(key != null) {
                    _KeyValueMap.TryGetValue(key, out result);
                }
                return result;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IEnumerable<string> Keys => _KeyValueMap.Keys;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IEnumerable<string[]> Values => _KeyValueMap.Values;

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
        public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator() => _KeyValueMap.GetEnumerator();

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(string key, out string[] value) => _KeyValueMap.TryGetValue(key, out value);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_KeyValueMap).GetEnumerator();

        /// <summary>
        /// Returns a value as a single string instead of a string array. Values with more than one element
        /// are joined together into a single string. Unknown keys return null. Keys with no value or keys
        /// with a single empty string value both return an empty string.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="join">The string to use to join values together</param>
        /// <returns></returns>
        public string GetValue(string key, string join)
        {
            if(join == null) {
                throw new ArgumentNullException(nameof(join));
            }

            _KeyValueMap.TryGetValue(key, out var value);
            return value == null ? null : String.Join(join, value);
        }

        /// <summary>
        /// Returns a value as a single string instead of a string array. Values with more than one element
        /// are joined together into a single string. Unknown keys return null. Keys with no value or keys
        /// with a single empty string value both return an empty string.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValue(string key) => GetValue(key, ",");
    }
}
