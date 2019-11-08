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
using System.Linq;
using System.Text;

namespace AWhewell.Owin.Interface
{
    /// <summary>
    /// A dictionary that follows the OWIN spec for representing headers and provides methods
    /// that help to translate between real-world headers and the OWIN representation.
    /// </summary>
    /// <remarks>
    /// This broadly copies the extensions that could be found in Microsoft.Owin.HeaderDictionary
    /// so that it's easier to port across code written for Microsoft.Owin.
    /// </remarks>
    public class HeadersDictionary : OwinDictionary<string[]>
    {
        /// <summary>
        /// Gets or sets the header value as a single string.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <remarks>
        /// Cast the dictionary to an IDictionary&lt;string, string[]&gt; to access the underlying
        /// string array values.
        /// </remarks>
        public new string this[string key]
        {
            get => Get(key);
            set => Set(key, value);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public HeadersDictionary() : base(caseSensitive: false)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="existingDictionary"></param>
        public HeadersDictionary(IDictionary<string, string[]> existingDictionary)
            : base(existingDictionary ?? new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase))
        {
        }

        /// <summary>
        /// Returns all of the header values for a key as a single comma-separated string.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key) => base.ContainsKey(key) ? JoinCookedHeaderValues(GetValues(key)) : null;

        /// <summary>
        /// Assigns a single string as the value for the header key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, string value)
        {
            if(!String.IsNullOrWhiteSpace(value)) {
                base[key] = new string[] { value };
            } else {
                if(base.ContainsKey(key)) {
                    base.Remove(key);
                }
            }
        }

        /// <summary>
        /// Merges the existing values array into a single entry and appends the value to it to form a
        /// single string value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Append(string key, string value)
        {
            if(!String.IsNullOrWhiteSpace(value)) {
                var buffer = new StringBuilder(Get(key) ?? "");
                if(buffer.Length > 0) {
                    buffer.Append(',');
                }
                buffer.Append(value);
                Set(key, buffer.ToString());
            }
        }

        /// <summary>
        /// Gets the key's array of string values without any modification or attempt at normalisation.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <remarks>
        /// If a string entry contains an unescaped unquoted comma then it will not be split into two strings
        /// by this function, it will remain as a single string in the output. This function is here for
        /// sideways compatability with Microsoft.Owin.HeaderDictionary.
        /// </remarks>
        public IList<string> GetValues(string key)
        {
            return base[key];
        }

        /// <summary>
        /// Overwrites the key's values with a new set of values (or removes the key if assigned a null or empty array).
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        public void SetValues(string key, params string[] values)
        {
            var normalisedValues = (values ?? new string[0])
                .Where(r => !String.IsNullOrWhiteSpace(r))
                .ToArray();

            if(normalisedValues.Length > 0) {
                base[key] = normalisedValues;
            } else {
                if(base.ContainsKey(key)) {
                    base.Remove(key);
                }
            }
        }

        /// <summary>
        /// Appends the array of values to the existing set of values.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        public void AppendValues(string key, params string[] values)
        {
            var buffer = new List<string>(GetValues(key) ?? new string[0]);
            buffer.AddRange(values.Where(r => !String.IsNullOrWhiteSpace(r)));
            base[key] = buffer.ToArray();
        }

        /// <summary>
        /// Gets the key's array of string values with unquoted commas split into separate results and quoted
        /// strings left intact but with the quotes removed.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <remarks>
        /// This function is here for sideways compatability with Microsoft.Owin.HeaderDictionary.
        /// </remarks>
        public IList<string> GetCommaSeparatedValues(string key)
        {
            List<string> result = null;

            var rawStrings = GetValues(key);
            if(rawStrings != null) {
                result = new List<string>();
                var buffer = new StringBuilder();

                void copyBufferToField()
                {
                    if(buffer.Length > 0) {
                        var trimmed = buffer.ToString().Trim();
                        if(trimmed.Length > 0) {
                            result.Add(trimmed);
                        }
                        buffer.Clear();
                    }
                }

                foreach(var rawString in rawStrings) {
                    var inQuotes = false;
                    for(var i = 0;i < (rawString?.Length ?? 0);++i) {
                        var ch = rawString[i];
                        switch(ch) {
                            case '"':
                                if(!inQuotes) {
                                    inQuotes = true;
                                } else {
                                    inQuotes = false;
                                    copyBufferToField();
                                }
                                break;
                            case ',':
                                if(inQuotes) {
                                    buffer.Append(ch);
                                } else {
                                    copyBufferToField();
                                }
                                break;
                            default:
                                buffer.Append(ch);
                                break;
                        }
                    }
                    copyBufferToField();
                }
            }

            return result;
        }

        /// <summary>
        /// Overwrites the key's values with a new set of values (or removes the key if assigned a null or empty array).
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <remarks>
        /// Values containing commas are quoted.
        /// </remarks>
        public void SetCommaSeparatedValues(string key, params string[] values)
        {
            var normalisedValues = (values ?? new string[0])
                .Where(r => !String.IsNullOrWhiteSpace(r))
                .Select(r => r.Contains(',') ? $"\"{r}\"" : r)
                .ToArray();

            if(normalisedValues.Length > 0) {
                base[key] = normalisedValues;
            } else {
                if(base.ContainsKey(key)) {
                    base.Remove(key);
                }
            }
        }

        /// <summary>
        /// Appends values to the key's existing values, quoting any values that contain commas.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        public void AppendCommaSeparatedValues(string key, params string[] values)
        {
            var buffer = new List<string>(GetValues(key) ?? new string[0]);
            buffer.AddRange(values
                .Where(r => !String.IsNullOrWhiteSpace(r))
                .Select(r => r.Contains(',') ? $"\"{r}\"" : r)
                .ToArray()
            );
            base[key] = buffer.ToArray();
        }

        /// <summary>
        /// Splits a header value into an array of values separated at commas. If a comma is
        /// within double-quotes then it is kept as a part of the value and not split out.
        /// </summary>
        /// <param name="headerValue"></param>
        /// <returns></returns>
        public static IEnumerable<string> SplitRawHeaderValue(string headerValue)
        {
            IEnumerable<string> result = null;

            if(headerValue != null) {
                var splitValues = new List<string>();
                result = splitValues;
                var buffer = new StringBuilder();
                var inString = false;

                void wordCompleted()
                {
                    splitValues.Add(buffer.ToString());
                    buffer.Clear();
                    inString = false;
                }

                for(var i = 0;i < headerValue.Length;++i) {
                    var ch = headerValue[i];
                    switch(ch) {
                        case '"':
                            inString = !inString;
                            goto default;
                        case ',':
                            if(!inString) {
                                wordCompleted();
                            } else {
                                goto default;
                            }
                            break;
                        default:
                            buffer.Append(ch);
                            break;
                    }
                }

                wordCompleted();
            }

            return result ?? new string[] { "" };
        }

        /// <summary>
        /// Joins an array of cooked header values into a single string header value.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string JoinCookedHeaderValues(IEnumerable<string> values)
        {
            return values == null
            ? ""
            : String.Join(",", values.Select(r => r ?? ""));
        }
    }
}
