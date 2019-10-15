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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Owin.Interface;

namespace Test.Owin
{
    public class OwinDictionary_Agnostic_Tests
    {
        protected OwinDictionary<object> _Dictionary;

        [TestMethod]
        public void Index_Operator_Returns_Null_For_Missing_Keys()
        {
            Assert.IsNull(_Dictionary["missing"]);
        }

        [TestMethod]
        public void Index_Operator_Returns_Null_For_Missing_Keys_When_Cast_To_IDictionary()
        {
            Assert.IsNull(((IDictionary<string, object>)_Dictionary)["missing"]);
        }

        [TestMethod]
        public void Index_Operator_Supports_Assignment_For_New_Keys()
        {
            _Dictionary["a.b"] = 12;

            Assert.AreEqual(12, (int)_Dictionary["a.b"]);
        }

        [TestMethod]
        public void Index_Operator_Supports_Assignment_For_Existing_Keys()
        {
            _Dictionary["a.b"] = 12;
            _Dictionary["a.b"] = 24;

            Assert.AreEqual(24, (int)_Dictionary["a.b"]);
        }

        [TestMethod]
        public void Keys_Property_Returns_Unordered_Collection_Of_Keys()
        {
            _Dictionary.Add("one", 1);
            _Dictionary.Add("two", 2);

            var keys = _Dictionary.Keys;

            Assert.AreEqual(2, keys.Count);
            Assert.IsTrue(keys.Contains("one"));
            Assert.IsTrue(keys.Contains("two"));
        }

        [TestMethod]
        public void Values_Property_Returns_Unordered_Collection_Of_Values()
        {
            _Dictionary.Add("one", 1);
            _Dictionary.Add("two", 2);

            var values = _Dictionary.Values;

            Assert.AreEqual(2, values.Count);
            Assert.IsTrue(values.Contains(1));
            Assert.IsTrue(values.Contains(2));
        }

        [TestMethod]
        public void IsReadOnly_Returns_False()
        {
            Assert.IsFalse(_Dictionary.IsReadOnly);
        }

        [TestMethod]
        public void Add_Adds_Value_To_Dictionary()
        {
            _Dictionary.Add("key", "value");

            Assert.AreEqual(1, _Dictionary.Count);
            Assert.AreEqual("value", _Dictionary["key"]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Adding_The_Same_Value_Twice_Throws_An_Exception()
        {
            _Dictionary.Add("key", "old-value");
            _Dictionary.Add("key", "new-value");
        }

        [TestMethod]
        public void Add_KeyValuePair_Adds_Value_To_Dictionary()
        {
            _Dictionary.Add(new KeyValuePair<string, object>("key", "value"));

            Assert.AreEqual(1, _Dictionary.Count);
            Assert.AreEqual("value", _Dictionary["key"]);
        }

        [TestMethod]
        public void Clear_Removes_All_Items()
        {
            _Dictionary.Add("one", 1);
            _Dictionary.Add("two", 2);

            _Dictionary.Clear();

            Assert.AreEqual(0, _Dictionary.Count);
        }

        [TestMethod]
        public void Contains_Returns_True_If_Dictionary_Contains_Key_And_Value()
        {
            _Dictionary.Add("key", 1);

            Assert.IsTrue(_Dictionary.Contains(new KeyValuePair<string, object>("key", 1)));
        }

        [TestMethod]
        public void Contains_Returns_False_If_Dictionary_Does_Not_Contain_Key()
        {
            _Dictionary.Add("key", 1);

            Assert.IsFalse(_Dictionary.Contains(new KeyValuePair<string, object>("wrong-key", 1)));
        }

        [TestMethod]
        public void Contains_Returns_False_If_Dictionary_Does_Not_Contain_Value()
        {
            _Dictionary.Add("key", 1);

            Assert.IsFalse(_Dictionary.Contains(new KeyValuePair<string, object>("key", 2)));
        }

        [TestMethod]
        public void ContainsKey_Returns_True_If_Dictionary_Contains_Key()
        {
            _Dictionary.Add("key", 1);

            Assert.IsTrue(_Dictionary.ContainsKey("key"));
        }

        [TestMethod]
        public void ContainsKey_Returns_False_If_Dictionary_Does_Not_Contain_Key()
        {
            _Dictionary.Add("key", 1);

            Assert.IsFalse(_Dictionary.ContainsKey("not-key"));
        }

        [TestMethod]
        public void CopyTo_Copies_Content_To_An_Array()
        {
            _Dictionary.Add("one", 1);
            _Dictionary.Add("two", 2);

            var destination = new KeyValuePair<string, object>[2];
            _Dictionary.CopyTo(destination, 0);

            Assert.AreEqual(1, destination.Single(r => r.Key == "one").Value);
            Assert.AreEqual(2, destination.Single(r => r.Key == "two").Value);
        }

        [TestMethod]
        public void CopyTo_Copies_Content_To_An_Array_At_An_Offset()
        {
            _Dictionary.Add("one", 1);
            _Dictionary.Add("two", 2);

            var destination = new KeyValuePair<string, object>[3];
            _Dictionary.CopyTo(destination, 1);

            Assert.AreEqual(default(KeyValuePair<string, object>), destination[0]);
            Assert.AreEqual(1, destination.Single(r => r.Key == "one").Value);
            Assert.AreEqual(2, destination.Single(r => r.Key == "two").Value);
        }

        [TestMethod]
        public void GetEnumerator_Can_Enumerate_Through_Dictionary()
        {
            _Dictionary.Add("one", 1);

            using(var enumerator = _Dictionary.GetEnumerator()) {
                Assert.IsTrue(enumerator.MoveNext());
                Assert.AreEqual(new KeyValuePair<string, object>("one", 1), enumerator.Current);
                Assert.IsFalse(enumerator.MoveNext());
            }
        }

        [TestMethod]
        public void Explicit_IEnumerable_GetEnumerator_Can_Enumerate_Through_Dictionary()
        {
            _Dictionary.Add("one", 1);

            var enumerator = ((IEnumerable)_Dictionary).GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(new KeyValuePair<string, object>("one", 1), enumerator.Current);
            Assert.IsFalse(enumerator.MoveNext());
        }

        [TestMethod]
        public void Remove_Can_Remove_By_Key()
        {
            _Dictionary.Add("one", 1);

            _Dictionary.Remove("one");

            Assert.AreEqual(0, _Dictionary.Count);
        }

        [TestMethod]
        public void Remove_Can_Remove_By_KeyValuePair()
        {
            _Dictionary.Add("one", 1);

            _Dictionary.Remove(new KeyValuePair<string, object>("one", 1));

            Assert.AreEqual(0, _Dictionary.Count);
        }

        [TestMethod]
        public void TryGetValue_Can_Retrieve_Value_By_Key()
        {
            _Dictionary.Add("one", 1);

            Assert.AreEqual(true, _Dictionary.TryGetValue("one", out var value));
            Assert.AreEqual(1, value);
        }

        [TestMethod]
        public void TryGetValue_Does_Not_Throw_Exception_For_Unknown_Key()
        {
            _Dictionary.Add("one", 1);

            Assert.AreEqual(false, _Dictionary.TryGetValue("two", out var value));
            Assert.AreEqual(default, value);
        }
    }
}
