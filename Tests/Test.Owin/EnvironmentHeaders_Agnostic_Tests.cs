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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Owin
{
    public abstract class EnvironmentHeaders_Agnostic_Tests
    {
        // Known comma-separated values (see HeadersDictionary_Agnostic_Tests, this borrows some tests from there)
        protected const string                          _Key = "header-key";
        protected static readonly string[]              _RawStringArray = new string[] {
                                                            "simple-1",
                                                            "comma-1, separated-1",
                                                            "\"enclosed, in double-quotes\"",
                                                            "comma-2, separated-2",
                                                            "simple-2",
                                                        };
        protected const string                          _JoinedString = "simple-1,comma-1, separated-1,\"enclosed, in double-quotes\",comma-2, separated-2,simple-2";
        protected static readonly IEnumerable<string>   _NormalisedStringArray = new string[] {
                                                            "simple-1",
                                                            "comma-1",
                                                            "separated-1",
                                                            "enclosed, in double-quotes",
                                                            "comma-2",
                                                            "separated-2",
                                                            "simple-2",
                                                        };

        /// <summary>
        /// Adds or overwrites a header value in the underlying header store.
        /// </summary>
        /// <param name="headerKey"></param>
        /// <param name="headerValue"></param>
        protected abstract void SetNativeHeaderValue(string headerKey, string headerValue);

        /// <summary>
        /// Gets the header value in the underlying header store.
        /// </summary>
        /// <param name="headerKey"></param>
        /// <returns></returns>
        protected abstract string GetNativeHeaderValue(string headerKey);

        /// <summary>
        /// Gets the OWIN environment dictionary that maps onto the underlying header store.
        /// </summary>
        /// <returns></returns>
        protected abstract IDictionary<string, string[]> GetEnvironmentDictionary();

        [TestMethod]
        public void Keys_Are_Case_Insensitive()
        {
            var headers = GetEnvironmentDictionary();

            headers.Add("one", new string[] { "value" });

            Assert.AreEqual("value", headers["ONE"][0]);
        }

        [TestMethod]
        public void Indexed_Get_Returns_Null_If_Key_Is_Unknown()
        {
            var headers = GetEnvironmentDictionary();

            Assert.IsNull(headers["x"]);
        }

        [TestMethod]
        public void Indexed_Get_Merges_String_Array_Into_Single_String_Without_Removing_Quotes_Or_Spaces()
        {
            var headers = GetEnvironmentDictionary();

            headers[_Key] = _RawStringArray;

            var actual = GetNativeHeaderValue(_Key);
            Assert.AreEqual(_JoinedString, actual);
        }

        [TestMethod]
        [DataRow(new string[] { "" },                   null)]
        [DataRow(new string[] { "" },                   "")]
        [DataRow(new string[] { "a" },                  "a")]
        [DataRow(new string[] { "a", "b" },             "a,b")]
        [DataRow(new string[] { "a", "b", " c", "" },   "a,b, c,")]
        [DataRow(new string[] { "a", "b", "\"c\"" },    "a,b,\"c\"")]
        public void Indexed_Get_Splits_Header_Strings_Correctly(string[] expected, string headerValue)
        {
            SetNativeHeaderValue(_Key, headerValue);
            var headers = GetEnvironmentDictionary();

            var actual = headers[_Key];

            Assert.IsTrue(expected.SequenceEqual(actual));
        }


        [TestMethod]
        public void Indexed_Set_Can_Add_Values()
        {
            var headers = GetEnvironmentDictionary();

            headers["a"] = new string[] { "1" };

            Assert.AreEqual("1", GetNativeHeaderValue("a"));
        }

        [TestMethod]
        public void Indexed_Set_Overwrites_Existing_Value()
        {
            SetNativeHeaderValue(_Key, "original value");
            var headers = GetEnvironmentDictionary();

            headers[_Key] = new string[] { "vA", "vB" };

            Assert.AreEqual("vA,vB", GetNativeHeaderValue(_Key));
        }

        [TestMethod]
        public void Indexed_Set_Is_Case_Insensitive()
        {
            var headers = GetEnvironmentDictionary();

            headers["a"] = new string[] { "1" };
            headers["A"] = new string[] { "2" };

            Assert.AreEqual("2", GetNativeHeaderValue("a"));
            Assert.AreEqual("2", GetNativeHeaderValue("A"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Indexed_Set_Throws_If_Key_Is_Null()
        {
            var headers = GetEnvironmentDictionary();

            headers[null] = new string[] { "1" };
        }

        [TestMethod]
        [DataRow(null,                                  "")]        // null array value gets coalesced into an empty string
        [DataRow(new string[] { "" },                   "")]
        [DataRow(new string[] { "a" },                  "a")]
        [DataRow(new string[] { "a", "b" },             "a,b")]
        [DataRow(new string[] { "a", "b", " c", "" },   "a,b, c,")]
        [DataRow(new string[] { "a", "b", "\"c\"" },    "a,b,\"c\"")]
        public void Indexed_Set_Joins_Strings_Correctly(string[] headerValues, string expected)
        {
            var headers = GetEnvironmentDictionary();

            headers[_Key] = headerValues;

            var actual = GetNativeHeaderValue(_Key);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Keys_Returns_All_Keys_In_Dictionary()
        {
            SetNativeHeaderValue("key-1", "a");
            SetNativeHeaderValue("key-2", "b,c");

            var headers = GetEnvironmentDictionary();

            var keys = headers.Keys;
            Assert.AreEqual(2, keys.Count);
            Assert.IsTrue(keys.Contains("key-1"));
            Assert.IsTrue(keys.Contains("key-2"));
        }

        [TestMethod]
        public void Keys_Returns_Empty_Collection_When_There_Are_No_Keys()
        {
            var headers = GetEnvironmentDictionary();

            var keys = headers.Keys;
            Assert.AreEqual(0, keys.Count);
        }

        [TestMethod]
        public void Values_Returns_All_Values_In_Dictionary()
        {
            SetNativeHeaderValue("key-1", "a");
            SetNativeHeaderValue("key-2", "b,c");

            var headers = GetEnvironmentDictionary();

            var values = headers.Values;
            Assert.AreEqual(2, values.Count);
            Assert.IsTrue(values.Any(r => new string[] { "a" }.SequenceEqual(r)));
            Assert.IsTrue(values.Any(r => new string[] { "b", "c" }.SequenceEqual(r)));
        }

        [TestMethod]
        // [DataRow(new string[] { "" }, null)]  <-- Underlying collections might or might not coalesce nulls, can't really test against native nulls
        [DataRow(new string[] { "" },                   "")]
        [DataRow(new string[] { "a" },                  "a")]
        [DataRow(new string[] { "a", "b" },             "a,b")]
        [DataRow(new string[] { "a", "b", " c", "" },   "a,b, c,")]
        [DataRow(new string[] { "a", "b", "\"c\"" },    "a,b,\"c\"")]
        public void Values_Splits_Native_Strings_Correctly(string[] expected, string headerValue)
        {
            SetNativeHeaderValue(_Key, headerValue);
            var headers = GetEnvironmentDictionary();

            var values = headers.Values;
            Assert.AreEqual(1, values.Count);

            var actual = values.Single();
            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void Count_Returns_Number_Of_Elements_In_Wrapped_Collection()
        {
            var headers = GetEnvironmentDictionary();
            Assert.AreEqual(0, headers.Count);

            headers["a"] = new string[] { "1" };
            Assert.AreEqual(1, headers.Count);

            headers["b"] = new string[] { "2" };
            Assert.AreEqual(2, headers.Count);

            headers["a"] = new string[] { "3" };
            Assert.AreEqual(2, headers.Count);
        }

        [TestMethod]
        public void IsReadOnly_Returns_False()
        {
            var headers = GetEnvironmentDictionary();
            Assert.AreEqual(false, headers.IsReadOnly);
        }

        [TestMethod]
        [DataRow(null,                                  "")]    // null arrays get coalesced into an empty string
        [DataRow(new string[] { "" },                   "")]
        [DataRow(new string[] { "a" },                  "a")]
        [DataRow(new string[] { "a", "b" },             "a,b")]
        [DataRow(new string[] { "a", "b", " c", "" },   "a,b, c,")]
        [DataRow(new string[] { "a", "b", "\"c\"" },    "a,b,\"c\"")]
        public void Add_Joins_Strings_Correctly(string[] headerValues, string expected)
        {
            var headers = GetEnvironmentDictionary();

            headers.Add(_Key, headerValues);

            var actual = GetNativeHeaderValue(_Key);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Add_Throws_If_Key_Already_Exists()
        {
            var headers = GetEnvironmentDictionary();

            // I'm in two minds about this. On the one hand IDictionary<A,B>.Add() is defined as throwing an
            // exception when adding duplicate keys. On the other it can be expensive to implement if the
            // underlying collection is a NameValueCollection.
            //
            // I think users of the wrapper should be encouraged to add elements via the string index setter
            // which does not have to test whether keys exist or not.
            headers.Add("a", new string[] { "1" });
            headers.Add("a", new string[] { "2" });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Add_Throws_If_Key_Already_Exists_In_Different_Case_()
        {
            var headers = GetEnvironmentDictionary();

            headers.Add("a", new string[] { "1" });
            headers.Add("A", new string[] { "2" });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Add_Throws_If_Adding_A_Null_Key()
        {
            var headers = GetEnvironmentDictionary();

            headers.Add(null, new string[] { "1" });
        }

        [TestMethod]
        [DataRow(null,                                  "")]    // <-- null array must be coalesced into an empty string
        [DataRow(new string[] { "" },                   "")]
        [DataRow(new string[] { "a" },                  "a")]
        [DataRow(new string[] { "a", "b" },             "a,b")]
        [DataRow(new string[] { "a", "b", " c", "" },   "a,b, c,")]
        [DataRow(new string[] { "a", "b", "\"c\"" },    "a,b,\"c\"")]
        public void Add_KeyValuePair_Joins_Strings_Correctly(string[] headerValues, string expected)
        {
            var headers = GetEnvironmentDictionary();

            headers.Add(new KeyValuePair<string, string[]>(_Key, headerValues));

            var actual = GetNativeHeaderValue(_Key);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Add_KeyValuePair_Throws_If_Key_Already_Exists()
        {
            var headers = GetEnvironmentDictionary();

            headers.Add(new KeyValuePair<string, string[]>("a", new string[] { "1" }));
            headers.Add(new KeyValuePair<string, string[]>("a", new string[] { "2" }));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Add_KeyValuePair_Throws_If_Key_Already_Exists_In_Different_Case_()
        {
            var headers = GetEnvironmentDictionary();

            headers.Add(new KeyValuePair<string, string[]>("a", new string[] { "1" }));
            headers.Add(new KeyValuePair<string, string[]>("A", new string[] { "2" }));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Add_KeyValuePair_Throws_If_Adding_A_Null_Key()
        {
            var headers = GetEnvironmentDictionary();

            headers.Add(new KeyValuePair<string, string[]>(null, new string[] { "1" }));
        }

        [TestMethod]
        public void Clear_Empties_Collection()
        {
            var headers = GetEnvironmentDictionary();
            headers.Add("a", new string[] { "1" });

            headers.Clear();

            Assert.AreEqual(0, headers.Count);
            Assert.IsNull(GetNativeHeaderValue("a"));
        }

        [TestMethod]
        [DataRow("key", "1",    "key", new string[] { "1" },        true)]
        [DataRow("key", "1",    "key", new string[] { "2" },        false)]
        [DataRow("key", "1",    "KEY", new string[] { "1" },        true)]
        [DataRow("key", "1,2",  "key", new string[] { "2", "1" },   false)]
        [DataRow("key", "",     "key", new string[] { "" },         true)]
        // [DataRow("key", null, "key", new string[] { "" }, true)]     <-- native collection might coalesce nulls, we can't test native nulls reliably
        // [DataRow("key", null, "key", null,                false)]    <-- native collection might coalesce nulls, we can't test native nulls reliably
        public void Contains_Returns_True_If_Collection_Contains_KeyValuePair(string key, string value, string searchKey, string[] searchValue, bool expected)
        {
            SetNativeHeaderValue(key, value);
            var headers = GetEnvironmentDictionary();

            var searchKvp = new KeyValuePair<string, string[]>(searchKey, searchValue);
            var actual = headers.Contains(searchKvp);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("key", "not-key", false)]
        [DataRow("key", "key",     true)]
        [DataRow("key", "KEY",     true)]
        public void ContainsKey_Returns_Correct_Value(string key, string searchFor, bool expected)
        {
            var headers = GetEnvironmentDictionary();

            headers[key] = new string[0];
            var actual = headers.ContainsKey(searchFor);

            Assert.AreEqual(expected,actual);
        }

        [TestMethod]
        public void CopyTo_Copies_KeyValuePairs_Into_Array()
        {
            var headers = GetEnvironmentDictionary();
            var values1 = new string[] { "1" };
            var values2 = new string[] { "2" };
            headers.Add("a", values1);
            headers.Add("b", values2);

            var array = new KeyValuePair<string, string[]>[2];
            headers.CopyTo(array, 0);

            Assert.IsTrue(values1.SequenceEqual(array.Single(r => r.Key == "a").Value));
            Assert.IsTrue(values2.SequenceEqual(array.Single(r => r.Key == "b").Value));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyTo_Throws_Exception_If_Array_Is_Null()
        {
            var headers = GetEnvironmentDictionary();
            headers.CopyTo(null, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CopyTo_Throws_If_Index_Less_Than_Zero()
        {
            var headers = GetEnvironmentDictionary();
            headers.CopyTo(new KeyValuePair<string, string[]>[0], -1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CopyTo_Array_Not_Large_Enough()
        {
            var headers = GetEnvironmentDictionary();
            headers.Add("a", new string[]{});

            var target = new KeyValuePair<string, string[]>[0];
            headers.CopyTo(target, 0);
        }

        [TestMethod]
        public void CopyTo_Can_Copy_At_Offset()
        {
            var headers = GetEnvironmentDictionary();
            headers.Add("a", new string[]{});

            var target = new KeyValuePair<string, string[]>[2];
            target[0] = new KeyValuePair<string, string[]>(" ", null);
            headers.CopyTo(target, 1);

            Assert.AreEqual(" ", target[0].Key);
            Assert.AreEqual("a", target[1].Key);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CopyTo_Throws_Exception_If_Offset_Forces_Copy_Out_Of_Bounds()
        {
            var headers = GetEnvironmentDictionary();
            headers.Add("a", new string[]{});

            var target = new KeyValuePair<string, string[]>[1];
            headers.CopyTo(target, 1);
        }

        [TestMethod]
        // [DataRow(new string[] { "" }, null)] <-- do not test assignment of native null, underlying collection might coalesce them into empty string
        [DataRow(new string[] { "" },                   "")]
        [DataRow(new string[] { "a" },                  "a")]
        [DataRow(new string[] { "a", "b" },             "a,b")]
        [DataRow(new string[] { "a", "b", " c", "" },   "a,b, c,")]
        [DataRow(new string[] { "a", "b", "\"c\"" },    "a,b,\"c\"")]
        public void CopyTo_Splits_Strings_Correctly(string[] expected, string headerValue)
        {
            SetNativeHeaderValue(_Key, headerValue);
            var headers = GetEnvironmentDictionary();

            var array = new KeyValuePair<string, string[]>[1];
            headers.CopyTo(array, 0);

            Assert.IsTrue(expected.SequenceEqual(array[0].Value));
        }

        [TestMethod]
        public void GetEnumerator_Starts_Before_First_Item()
        {
            var headers = GetEnvironmentDictionary();
            headers.Add("a", new string[] { "1" });

            using(var enumerator = headers.GetEnumerator()) {
                enumerator.MoveNext();
                Assert.AreEqual("a", enumerator.Current.Key);
                Assert.IsTrue(new string[] { "1" }.SequenceEqual(enumerator.Current.Value));
            }
        }

        [TestMethod]
        public void GetEnumerator_Supports_Foreach()
        {
            var headers = GetEnvironmentDictionary();
            headers.Add("a", new string[] { "1" });
            headers.Add("b", new string[] { "2" });

            var idx = 0;
            foreach(var kvp in headers) {
                string expectedKey;
                string[] expectedValue;
                switch(idx++) {
                    case 0:     expectedKey = "a"; expectedValue = new string[] { "1" }; break;
                    case 1:     expectedKey = "b"; expectedValue = new string[] { "2" }; break;
                    default:    throw new NotImplementedException();
                }
                Assert.AreEqual(expectedKey, kvp.Key);
                Assert.IsTrue(expectedValue.SequenceEqual(kvp.Value));
            }
        }

        [TestMethod]
        // [DataRow(new string[] { "" }, null)] <-- do not test assignment of native null, underlying collection might coalesce them into empty string
        [DataRow(new string[] { "" },                   "")]
        [DataRow(new string[] { "a" },                  "a")]
        [DataRow(new string[] { "a", "b" },             "a,b")]
        [DataRow(new string[] { "a", "b", " c", "" },   "a,b, c,")]
        [DataRow(new string[] { "a", "b", "\"c\"" },    "a,b,\"c\"")]
        public void GetEnumerator_Splits_Strings_Correctly(string[] expected, string headerValue)
        {
            SetNativeHeaderValue(_Key, headerValue);
            var headers = GetEnvironmentDictionary();
            var enumerator = headers.GetEnumerator();

            enumerator.MoveNext();
            var actual = enumerator.Current.Value;

            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        public void IEnumerable_GetEnumerator_Works()
        {
            // I'm not doing a big test on this, it's expected to call down to the class version
            var headers = GetEnvironmentDictionary();
            headers.Add("a", new string[] { "1" });
            headers.Add("b", new string[] { "2" });

            var enumerator = ((System.Collections.IEnumerable)headers).GetEnumerator();

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual("a", ((KeyValuePair<string, string[]>)enumerator.Current).Key);

            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual("b", ((KeyValuePair<string, string[]>)enumerator.Current).Key);

            Assert.IsFalse(enumerator.MoveNext());
        }

        [TestMethod]
        public void Remove_Removes_Header_With_Key()
        {
            SetNativeHeaderValue(_Key, null);
            var headers = GetEnvironmentDictionary();

            var removed = headers.Remove(_Key);

            Assert.IsTrue(removed);
            Assert.AreEqual(0, headers.Count);
        }

        [TestMethod]
        public void Remove_Returns_False_If_Key_Not_Present()
        {
            var headers = GetEnvironmentDictionary();

            var removed = headers.Remove(_Key);

            Assert.IsFalse(removed);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Remove_Throws_Exception_If_Passed_Null_Key()
        {
            var headers = GetEnvironmentDictionary();
            headers.Remove(null);
        }

        [TestMethod]
        public void Remove_KeyValuePair_Removes_Matching_Header()
        {
            SetNativeHeaderValue("a", "1");
            var headers = GetEnvironmentDictionary();

            var removed = headers.Remove(new KeyValuePair<string, string[]>("a", new string[] { "1" }));

            Assert.IsTrue(removed);
            Assert.AreEqual(0, headers.Count);
        }

        [TestMethod]
        public void Remove_KeyValuePair_Does_Not_Remove_If_Only_Key_Matches()
        {
            SetNativeHeaderValue("a", "1");
            var headers = GetEnvironmentDictionary();

            var removed = headers.Remove(new KeyValuePair<string, string[]>("a", new string[] { "2" }));

            Assert.IsFalse(removed);
            Assert.AreEqual(1, headers.Count);
        }

        [TestMethod]
        public void Remove_KeyValuePair_Does_Not_Remove_If_Only_Value_Matches()
        {
            SetNativeHeaderValue("a", "1");
            var headers = GetEnvironmentDictionary();

            var removed = headers.Remove(new KeyValuePair<string, string[]>("b", new string[] { "1" }));

            Assert.IsFalse(removed);
            Assert.AreEqual(1, headers.Count);
        }

        [TestMethod]
        // [DataRow(new string[] { "" }, null)] <-- do not test assignment of native null, underlying collection might coalesce them into empty string
        [DataRow(new string[] { "" },                   "")]
        [DataRow(new string[] { "a" },                  "a")]
        [DataRow(new string[] { "a", "b" },             "a,b")]
        [DataRow(new string[] { "a", "b", " c", "" },   "a,b, c,")]
        [DataRow(new string[] { "a", "b", "\"c\"" },    "a,b,\"c\"")]
        public void RemoveKeyValuePair_Converts_Between_Strings_And_Arrays_Correctly(string[] kvpValue, string headerValue)
        {
            SetNativeHeaderValue(_Key, headerValue);
            var headers = GetEnvironmentDictionary();

            var removed = headers.Remove(new KeyValuePair<string, string[]>(_Key, kvpValue));

            Assert.AreEqual(true, removed);
        }

        [TestMethod]
        public void TryGetValue_Retrieves_Existing_Key()
        {
            SetNativeHeaderValue("a", "1");
            var headers = GetEnvironmentDictionary();

            var success = headers.TryGetValue("a", out var value);

            Assert.IsTrue(success);
            Assert.IsTrue(new string[] { "1" }.SequenceEqual(value));
        }

        [TestMethod]
        public void TryGetValue_Does_Not_Throw_On_Missing_Key()
        {
            var headers = GetEnvironmentDictionary();

            var success = headers.TryGetValue("a", out var value);

            Assert.IsFalse(success);
            Assert.IsNull(value);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TryGetValue_Throws_If_Passed_Null()
        {
            var headers = GetEnvironmentDictionary();
            headers.TryGetValue(null, out var value);
        }

        [TestMethod]
        // [DataRow(new string[] { "" }, null)] <-- do not test assignment of native null, underlying collection might coalesce them into empty string
        [DataRow(new string[] { "" },                   "")]
        [DataRow(new string[] { "a" },                  "a")]
        [DataRow(new string[] { "a", "b" },             "a,b")]
        [DataRow(new string[] { "a", "b", " c", "" },   "a,b, c,")]
        [DataRow(new string[] { "a", "b", "\"c\"" },    "a,b,\"c\"")]
        public void TryGetValue_Splits_Strings_Correctly(string[] expected, string headerValue)
        {
            SetNativeHeaderValue(_Key, headerValue);
            var headers = GetEnvironmentDictionary();

            headers.TryGetValue(_Key, out var actual);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }
    }
}
