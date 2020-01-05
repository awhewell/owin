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
using System.Linq;
using System.Text;
using AWhewell.Owin.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin.Utility
{
    [TestClass]
    public class ObservableDictionary_Tests
    {
        class SampleDictionary : ObservableDictionary<string, int?>
        {
            public SampleDictionary() : base() {;}
            public SampleDictionary(IEqualityComparer<string> comparer) : base(comparer) {;}
            public SampleDictionary(IDictionary<string, int?> wrapped) : base(wrapped) {;}

            public int    AssignedCount;
            public string AssignedKey;
            public int?   AssignedValue;
            protected override void OnAssigned(string key, int? value)
            {
                ++AssignedCount;
                AssignedKey = key;
                AssignedValue = value;
            }

            public int    RemovedCount;
            public string RemovedKey;
            protected override void OnRemoved(string key)
            {
                ++RemovedCount;
                RemovedKey = key;
            }

            public int ResetCount;
            protected override void OnReset()
            {
                ++ResetCount;
            }
        }

        private SampleDictionary _Dictionary;

        [TestInitialize]
        public void TestInitialise()
        {
            _Dictionary = new SampleDictionary();
        }

        [TestMethod]
        public void Default_Ctor_Initialises_Properties()
        {
            Assert.AreEqual(0, _Dictionary.Count);
            Assert.AreEqual(false, _Dictionary.IsReadOnly);
            Assert.AreEqual(0, _Dictionary.Keys.Count);
            Assert.AreEqual(0, _Dictionary.Values.Count);
        }

        [TestMethod]
        public void EqualityComparer_Ctor_Uses_Equality_Comparer()
        {
            _Dictionary = new SampleDictionary(StringComparer.OrdinalIgnoreCase);

            _Dictionary["aa"] = 1;
            _Dictionary["Aa"] = 2;

            Assert.AreEqual(1, _Dictionary.Count);
            Assert.AreEqual(2, _Dictionary["AA"]);
        }

        [TestMethod]
        public void Wrapped_Ctor_Wraps_Existing_Dictionary()
        {
            var wrapped = new Dictionary<string, int?>();
            _Dictionary = new SampleDictionary(wrapped);

            _Dictionary["a"] = 8;

            Assert.AreEqual(1, _Dictionary.Count);
            Assert.AreEqual(8, _Dictionary["a"]);

            Assert.AreEqual(1, wrapped.Count);
            Assert.AreEqual(8, wrapped["a"]);
        }

        [TestMethod]
        public void Indexed_Set_Add_Assigns_Value()
        {
            _Dictionary["a"] = 9;

            Assert.AreEqual(1,   _Dictionary.AssignedCount);
            Assert.AreEqual("a", _Dictionary.AssignedKey);
            Assert.AreEqual(9,   _Dictionary.AssignedValue);
            Assert.AreEqual(9,   _Dictionary["a"]);
        }

        [TestMethod]
        public void Indexed_Set_Update_Assigns_Value()
        {
            _Dictionary = new SampleDictionary(new Dictionary<string, int?>() {
                ["a"] = 3
            });
            _Dictionary["a"] = 9;

            Assert.AreEqual(1,   _Dictionary.AssignedCount);
            Assert.AreEqual("a", _Dictionary.AssignedKey);
            Assert.AreEqual(9,   _Dictionary.AssignedValue);
            Assert.AreEqual(9,   _Dictionary["a"]);
        }

        [TestMethod]
        public void Keys_Returns_Keys()
        {
            _Dictionary["a"] = 1;
            _Dictionary["b"] = 2;

            var keys = _Dictionary.Keys;

            Assert.AreEqual(2, keys.Count);
            Assert.IsTrue(keys.Contains("a"));
            Assert.IsTrue(keys.Contains("b"));
        }

        [TestMethod]
        public void Values_Returns_Values()
        {
            _Dictionary["a"] = 1;
            _Dictionary["b"] = 2;

            var values = _Dictionary.Values;

            Assert.AreEqual(2, values.Count);
            Assert.IsTrue(values.Contains(1));
            Assert.IsTrue(values.Contains(2));
        }

        [TestMethod]
        public void Count_Returns_Number_Of_Entries()
        {
            _Dictionary["a"] = 1;
            _Dictionary["b"] = 2;

            Assert.AreEqual(2, _Dictionary.Count);
        }

        [TestMethod]
        public void Add_Key_Value_Assigns_Value()
        {
            _Dictionary.Add("a", 123);

            Assert.AreEqual(123, _Dictionary["a"]);
            Assert.AreEqual(1,   _Dictionary.AssignedCount);
            Assert.AreEqual("a", _Dictionary.AssignedKey);
            Assert.AreEqual(123, _Dictionary.AssignedValue);
        }

        [TestMethod]
        public void Add_KeyValuePair_Assigns_Value()
        {
            _Dictionary.Add(new KeyValuePair<string, int?>("a", 123));

            Assert.AreEqual(123, _Dictionary["a"]);
            Assert.AreEqual(1,   _Dictionary.AssignedCount);
            Assert.AreEqual("a", _Dictionary.AssignedKey);
            Assert.AreEqual(123, _Dictionary.AssignedValue);
        }

        [TestMethod]
        public void Clear_Resets_Dictionary()
        {
            _Dictionary = new SampleDictionary(new Dictionary<string, int?>() {
                ["a"] = 48,
                ["b"] = 886,
            });

            _Dictionary.Clear();

            Assert.AreEqual(0, _Dictionary.Count);
            Assert.AreEqual(1, _Dictionary.ResetCount);
        }

        [TestMethod]
        public void Contains_Returns_Correct_Value()
        {
            _Dictionary.Add("a", 89);

            Assert.IsFalse(_Dictionary.Contains(new KeyValuePair<string, int?>("a", 42)));
            Assert.IsTrue (_Dictionary.Contains(new KeyValuePair<string, int?>("a", 89)));
        }

        [TestMethod]
        public void ContainsKey_Returns_Correct_Value()
        {
            _Dictionary.Add("a", 89);

            Assert.IsFalse(_Dictionary.ContainsKey("b"));
            Assert.IsTrue (_Dictionary.ContainsKey("a"));
        }

        [TestMethod]
        public void CopyTo_Copies_To_Array()
        {
            var a = new KeyValuePair<string, int?>("a", 1);
            var b = new KeyValuePair<string, int?>("b", 1);
            _Dictionary.Add(a);
            _Dictionary.Add(b);
            var array = new KeyValuePair<string, int?>[2];

            _Dictionary.CopyTo(array, 0);
            Assert.IsTrue(array.Contains(a)); 
            Assert.IsTrue(array.Contains(b));
        }

        [TestMethod]
        public void GetEnumerator_Can_Enumerate_Through_Dictionary()
        {
            _Dictionary.Add("one", 1);

            using(var enumerator = _Dictionary.GetEnumerator()) {
                Assert.IsTrue(enumerator.MoveNext());
                Assert.AreEqual(new KeyValuePair<string, int?>("one", 1), enumerator.Current);
                Assert.IsFalse(enumerator.MoveNext());
            }
        }

        [TestMethod]
        public void Explicit_IEnumerable_GetEnumerator_Can_Enumerate_Through_Dictionary()
        {
            _Dictionary.Add("one", 1);

            var enumerator = ((IEnumerable)_Dictionary).GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(new KeyValuePair<string, int?>("one", 1), enumerator.Current);
            Assert.IsFalse(enumerator.MoveNext());
        }

        [TestMethod]
        public void Remove_Removes_Key()
        {
            _Dictionary = new SampleDictionary(new Dictionary<string, int?>() {
                ["a"] = 48,
            });

            var actual = _Dictionary.Remove("a");

            Assert.AreEqual(true, actual);
            Assert.IsFalse(_Dictionary.ContainsKey("a"));
            Assert.AreEqual(1,   _Dictionary.RemovedCount);
            Assert.AreEqual("a", _Dictionary.RemovedKey);
        }

        [TestMethod]
        public void Remove_Copes_When_Key_Not_Present()
        {
            var actual = _Dictionary.Remove("a");

            Assert.AreEqual(false, actual);
            Assert.AreEqual(0, _Dictionary.RemovedCount);
        }

        [TestMethod]
        public void Remove_Removes_KeyValuePair()
        {
            _Dictionary = new SampleDictionary(new Dictionary<string, int?>() {
                ["a"] = 48,
            });

            var actual = _Dictionary.Remove(new KeyValuePair<string, int?>("a", 48));

            Assert.AreEqual(true, actual);
            Assert.IsFalse(_Dictionary.ContainsKey("a"));
            Assert.AreEqual(1,   _Dictionary.RemovedCount);
            Assert.AreEqual("a", _Dictionary.RemovedKey);
        }

        [TestMethod]
        public void Remove_Copes_When_KeyValuePair_Not_Present()
        {
            var actual = _Dictionary.Remove(new KeyValuePair<string, int?>("a", 48));

            Assert.AreEqual(false, actual);
            Assert.AreEqual(0, _Dictionary.RemovedCount);
        }

        [TestMethod]
        public void TryGetValue_Tries_To_Get_Value()
        {
            _Dictionary = new SampleDictionary(new Dictionary<string, int?>() {
                ["a"] = 48,
            });

            var notFoundOutcome = _Dictionary.TryGetValue("b", out var notFoundValue);
            Assert.AreEqual(false, notFoundOutcome);
            Assert.AreEqual(default(int?), notFoundValue);

            var foundOutcome = _Dictionary.TryGetValue("a", out var foundValue);
            Assert.AreEqual(true, foundOutcome);
            Assert.AreEqual(48, foundValue);
        }
    }
}
