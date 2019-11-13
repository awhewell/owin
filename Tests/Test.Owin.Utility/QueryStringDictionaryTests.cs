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
using System.Text;
using AWhewell.Owin.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin.Utility
{
    [TestClass]
    public class QueryStringDictionaryTests
    {
        [TestMethod]
        public void Ctor_Defaults_To_Case_Sensitive_Keys()
        {
            var dictionary = new QueryStringDictionary("a=1");
            Assert.IsNull(dictionary["A"]);
        }

        [TestMethod]
        [DataRow(null,              true,  "")]
        [DataRow("",                true,  "")]
        [DataRow("name=Andrew",     true,  "name=Andrew")]
        [DataRow("space=%20&x=1",   true,  "space=%20&x=1")]
        [DataRow("name=Andrew",     false, "name=Andrew")]
        public void QueryString_Returns_Ctor_Parameter(string queryString, bool caseSensitiveKey, string expected)
        {
            var dictionary = new QueryStringDictionary(queryString, caseSensitiveKey);

            Assert.AreEqual(expected, dictionary.QueryString);
        }

        [TestMethod]
        [DataRow("name=Andrew",             true,   "name", new string[] { "Andrew" })]
        [DataRow("name=Andrew",             true,   "Name", null)]
        [DataRow("name=Andrew",             false,  "Name", new string[] { "Andrew" })]
        [DataRow("name=Andrew%20Whewell",   true,   "name", new string[] { "Andrew Whewell" })]
        [DataRow("a=1&b=2",                 true,   "a",    new string[] { "1" })]
        [DataRow("a=1&b=2",                 true,   "b",    new string[] { "2" })]
        [DataRow("a=1%262&b=3",             true,   "a",    new string[] { "1&2" })]
        [DataRow("a=1%262&b=3",             true,   "b",    new string[] { "3" })]
        [DataRow("a",                       true,   "a",    new string[] { })]
        [DataRow("a=",                      true,   "a",    new string[] { "" })]
        [DataRow("a=1",                     true,   null,   null)]
        [DataRow("a=1",                     true,   "",     null)]
        [DataRow("=1",                      true,   "",     null)]
        [DataRow("a=1&a=2",                 true,   "a",    new string[] { "1", "2" })]
        [DataRow("a=1&A=2",                 true,   "a",    new string[] { "1" })]
        [DataRow("a=1&A=2",                 true,   "A",    new string[] { "2" })]
        [DataRow("a=1&A=2",                 false,  "a",    new string[] { "1", "2" })]
        [DataRow("a&a=",                    true,   "a",    new string[] { "" })]
        [DataRow("a=&a",                    true,   "a",    new string[] { "" })]
        [DataRow("a&a=&a=1",                true,   "a",    new string[] { "", "1" })]
        [DataRow("a&a=1&a=",                true,   "a",    new string[] { "1", "" })]
        [DataRow("a%20=%201",               true,   "a",    null)]
        [DataRow("a%20=%201",               true,   "a ",   new string[] { " 1" })]
        [DataRow("a=1;b=2",                 true,   "a",    new string[] { "1" })]
        [DataRow("a=1;b=2",                 true,   "b",    new string[] { "2" })]
        [DataRow("a=1&b=2;c=3",             true,   "b",    new string[] { "2" })]
        [DataRow("a=1&b=2;c=3",             true,   "c",    new string[] { "3" })]
        public void QueryString_Index_Returns_Expected_Value(string queryString, bool caseSensitiveKey, string indexValue, string[] expected)
        {
            var dictionary = new QueryStringDictionary(queryString, caseSensitiveKey);

            var actual = dictionary[indexValue];

            if(expected == null) {
                Assert.IsNull(actual);
            } else {
                Assert.IsTrue(expected.SequenceEqual(actual));
            }
        }

        [TestMethod]
        [DataRow("Name=Andrew", true,   new string[] { "Name" })]
        [DataRow("Name=Andrew", false,  new string[] { "Name" })]
        [DataRow("a=1&b=2",     false,  new string[] { "a", "b" })]
        [DataRow("a=&b=2",      false,  new string[] { "a", "b" })]
        [DataRow("a&b=2",       false,  new string[] { "a", "b" })]
        [DataRow("a%20=%201",   false,  new string[] { "a " })]
        public void Keys_Returns_All_Keys(string queryString, bool caseSensitiveKey, string[] expectedKeys)
        {
            var dictionary = new QueryStringDictionary(queryString, caseSensitiveKey);
            var actual = dictionary.Keys.ToList();

            Assert.AreEqual(expectedKeys.Length, actual.Count);
            foreach(var expectedKey in expectedKeys) {
                var actualIdx = actual.IndexOf(expectedKey);
                Assert.AreNotEqual(-1, actualIdx, $"Cannot find {expectedKey} in Keys");
                actual.RemoveAt(actualIdx);
            }
        }

        [TestMethod]
        [DataRow("Name=Andrew", true, new string[] { "Andrew" })]
        [DataRow("a=1&b=2;c=3", true, new string[] { "1", "2", "3" })]
        [DataRow("=1",          true, null)]
        public void Values_Returns_All_Values(string queryString, bool caseSensitiveKey, string[] expectedSingleElementValues)
        {
            // It appears that you can't have arrays of arrays in attributes, hence the test being restricted to single element values

            var dictionary = new QueryStringDictionary(queryString, caseSensitiveKey);
            var actual = dictionary.Values.ToArray();

            if(expectedSingleElementValues == null) {
                Assert.AreEqual(0, actual.Length);
            } else {
                Assert.AreEqual(expectedSingleElementValues.Length, actual.Length);
                var copy = new List<string>(actual.Select(r => r.Single()));
                foreach(var expectedValue in expectedSingleElementValues) {
                    var actualIdx = copy.IndexOf(expectedValue);
                    Assert.AreNotEqual(-1, actualIdx, $"Cannot find {expectedValue} in Values");
                    copy.RemoveAt(actualIdx);
                }
            }
        }

        [TestMethod]
        public void Values_Returns_Array_Values_Correctly()
        {
            var dictionary = new QueryStringDictionary("a=1&a=2;a=3;a=;a");

            var actual = dictionary.Values.Single();

            Assert.IsTrue(new string[] { "1", "2", "3", "" }.SequenceEqual(actual));
        }

        [TestMethod]
        [DataRow("",    0)]
        [DataRow("a",   1)]
        [DataRow("a&b", 2)]
        [DataRow("a&a", 1)]
        public void Count_Returns_The_Number_Of_Keys_Correctly(string queryString, int expected)
        {
            var dictionary = new QueryStringDictionary(queryString);

            Assert.AreEqual(expected, dictionary.Count);
        }

        [TestMethod]
        [DataRow("a=1", true,  "a", true)]
        [DataRow("a=1", true,  "A", false)]
        [DataRow("a=1", false, "A", true)]
        [DataRow("",    false, "",  false)]
        [DataRow("=1",  false, "",  false)]
        public void ContainsKey_Returns_Expected_Value(string queryString, bool caseSensitiveKeys, string searchKey, bool expected)
        {
            var dictionary = new QueryStringDictionary(queryString, caseSensitiveKeys);

            Assert.AreEqual(expected, dictionary.ContainsKey(searchKey));
        }

        [TestMethod]
        public void GetEnumerator_Enumerates_Over_KeyValuePairs()
        {
            var dictionary = new QueryStringDictionary("a=1;a=2;b=3&c=4");

            var seenKeys = new HashSet<string>();
            using(var enumerator = dictionary.GetEnumerator()) {
                while(enumerator.MoveNext()) {
                    var key = enumerator.Current.Key;
                    var value = enumerator.Current.Value;

                    Assert.IsFalse(seenKeys.Contains(key));
                    seenKeys.Add(key);

                    string[] expected = null;
                    switch(key) {
                        case "a":   expected = new string[] { "1", "2" }; break;
                        case "b":   expected = new string[] { "3" }; break;
                        case "c":   expected = new string[] { "4" }; break;
                        default:    Assert.Fail($"Unexpected key {key}"); break;
                    }

                    Assert.IsTrue(expected.SequenceEqual(value));
                }
            }

            Assert.AreEqual(3, seenKeys.Count);
        }

        [TestMethod]
        public void IEnumerator_GetEnumerator_Enumerates_Over_KeyValuePairs()
        {
            var dictionary = new QueryStringDictionary("a=1;a=2;b=3&c=4");

            var seenKeys = new HashSet<string>();
            var enumerator = ((IEnumerable)dictionary).GetEnumerator();
            while(enumerator.MoveNext()) {
                var kvp = (KeyValuePair<string, string[]>)enumerator.Current;
                var key = kvp.Key;
                var value = kvp.Value;

                Assert.IsFalse(seenKeys.Contains(key));
                seenKeys.Add(key);

                string[] expected = null;
                switch(key) {
                    case "a":   expected = new string[] { "1", "2" }; break;
                    case "b":   expected = new string[] { "3" }; break;
                    case "c":   expected = new string[] { "4" }; break;
                    default:    Assert.Fail($"Unexpected key {key}"); break;
                }

                Assert.IsTrue(expected.SequenceEqual(value));
            }

            Assert.AreEqual(3, seenKeys.Count);
        }

        [TestMethod]
        [DataRow("a=1", true,  "a", true, new string[] { "1" })]
        [DataRow("a=1", true,  "A", false, null)]
        [DataRow("a=1", false, "A", true, new string[] { "1" })]
        public void TryGetValue_Returns_Expected_Value(string queryString, bool caseSensitiveKeys, string searchKey, bool expectedReturn, string[] expectedValue)
        {
            var dictionary = new QueryStringDictionary(queryString, caseSensitiveKeys);

            var actualReturn = dictionary.TryGetValue(searchKey, out var actualValue);

            Assert.AreEqual(expectedReturn, actualReturn);
            if(expectedValue == null) {
                Assert.IsNull(actualValue);
            } else {
                Assert.IsTrue(expectedValue.SequenceEqual(actualValue));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetValue_Throws_If_Passed_Null_Join()
        {
            var dictionary = new QueryStringDictionary("a=1");
            dictionary.GetValue("a", null);
        }

        [TestMethod]
        [DataRow("a=1",     true,  "a",  ",", "1")]
        [DataRow("a=1",     true,  "A",  ",", null)]
        [DataRow("a=1",     false, "A",  ",", "1")]
        [DataRow("a=1&a=2", true,  "a",  ",", "1,2")]
        [DataRow("a=1&a=2", true,  "a",  ";", "1;2")]
        [DataRow("a=1&a=2", true,  "a",  "",  "12")]
        [DataRow("a",       true,  "a",  ",", "")]
        [DataRow("a=",      true,  "a",  ",", "")]
        [DataRow("a=&a=1",  true,  "a",  ",", ",1")]
        [DataRow("a&a=1",   true,  "a",  ",", "1")]
        [DataRow("a&a=1",   true,  "",   ",", null)]
        public void GetValue_Returns_Expected_Value(string queryString, bool caseSensitiveKeys, string key, string join, string expected)
        {
            var dictionary = new QueryStringDictionary(queryString, caseSensitiveKeys);

            var actual = dictionary.GetValue(key, join);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetValue_Defaults_To_Comma()
        {
            var dictionary = new QueryStringDictionary("a=1&a=2");
            Assert.AreEqual("1,2", dictionary.GetValue("a"));
        }
    }
}
