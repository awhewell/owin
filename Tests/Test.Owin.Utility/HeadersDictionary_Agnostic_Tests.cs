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
using AWhewell.Owin.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin.Utility
{
    // Some of these tests are cribbed pretty heavily from Microsoft.Owin's header tests. We want
    // to exhibit the same behaviour for cross-compatability, it makes sense to make sure the
    // dictionary can pass their tests:
    //
    // https://github.com/aspnet/AspNetKatana/blob/dev/tests/Microsoft.Owin.Tests/HeaderTests.cs
    public abstract class HeadersDictionary_Agnostic_Tests
    {
        protected HeadersDictionary _Headers;

        // Known comma-separated values
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

        protected abstract void Reset_To_RawStringArray();

        [TestMethod]
        public void Keys_Are_Case_Insensitive()
        {
            _Headers.Add("one", new string[] { "value" });

            Assert.AreEqual("value", _Headers["ONE"]);
        }

        [TestMethod]
        public void Indexed_Get_Merges_String_Array_Into_Single_String_Without_Removing_Quotes_Or_Spaces()
        {
            Reset_To_RawStringArray();
            Assert.AreEqual(_JoinedString, _Headers[_Key]);
        }

        [TestMethod]
        public void Indexed_Set_Overwrites_Existing_Value()
        {
            Reset_To_RawStringArray();

            _Headers[_Key] = "vA, vB";

            var values = _Headers.GetValues(_Key);
            Assert.IsTrue(new[] { "vA, vB" }.SequenceEqual(values));
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void Indexed_Set_Of_Null_Or_Empty_String_Removes_Header(string value)
        {
            Reset_To_RawStringArray();
            _Headers[_Key] = value;
            Assert.IsFalse(_Headers.ContainsKey(_Key));
        }

        [TestMethod]
        public void Get_Merges_String_Array_Into_Single_String_Without_Removing_Quotes_Or_Spaces()
        {
            Reset_To_RawStringArray();
            Assert.AreEqual(_JoinedString, _Headers.Get(_Key));
        }

        [TestMethod]
        public void Set_Overwrites_Existing_Value()
        {
            Reset_To_RawStringArray();

            _Headers.Set(_Key, "vA, vB");

            var values = _Headers.GetValues(_Key);
            Assert.IsTrue(new[] { "vA, vB" }.SequenceEqual(values));
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void Set_Of_Null_Or_Empty_String_Removes_Header(string value)
        {
            Reset_To_RawStringArray();
            _Headers.Set(_Key, value);
            Assert.IsFalse(_Headers.ContainsKey(_Key));
        }

        [TestMethod]
        public void Append_Merges_And_Appends_Into_Single_String()
        {
            Reset_To_RawStringArray();

            _Headers.Append(_Key, "vA, vB");

            var values = _Headers.GetValues(_Key);
            Assert.IsTrue(new[] { _JoinedString +  ",vA, vB" }.SequenceEqual(values));
        }

        [TestMethod]
        public void Append_Assigns_Single_String_If_Key_Missing()
        {
            _Headers.Append(_Key, "vA, vB");

            var values = _Headers.GetValues(_Key);
            Assert.IsTrue(new[] { "vA, vB" }.SequenceEqual(values));
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void Append_Ignores_Null_Or_Empty_Values(string value)
        {
            Reset_To_RawStringArray();
            _Headers.Append(_Key, value);
            Assert.IsTrue(_RawStringArray.SequenceEqual(_Headers.GetValues(_Key)));
        }

        [TestMethod]
        public void GetValues_Returns_The_Original_Raw_Header_String_Array()
        {
            Reset_To_RawStringArray();
            var getValuesList = _Headers.GetValues(_Key);
            Assert.IsTrue(_RawStringArray.SequenceEqual(getValuesList));
        }

        [TestMethod]
        public void SetValues_Overwrites_Existing_Array()
        {
            Reset_To_RawStringArray();

            _Headers.SetValues(_Key, "vA, vB", "vC");

            var values = _Headers.GetValues(_Key);
            Assert.IsTrue(new[] { "vA, vB", "vC" }.SequenceEqual(values));
        }

        [TestMethod]
        public void SetValues_Ignores_Null_Or_Empty_Values()
        {
            Reset_To_RawStringArray();

            _Headers.SetValues(_Key, "vA, vB", null, "", " ", "vC");

            var values = _Headers.GetValues(_Key);
            Assert.IsTrue(new[] { "vA, vB", "vC" }.SequenceEqual(values));
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow(new string[0])]
        [DataRow(new string[] { null, "", " " })]
        public void SetValues_Of_Null_Or_Empty_Array_Removes_Header(string[] value)
        {
            Reset_To_RawStringArray();
            _Headers.SetValues(_Key, value);
            Assert.IsFalse(_Headers.ContainsKey(_Key));
        }

        [TestMethod]
        public void AppendValues_Appends_To_Existing_Values()
        {
            Reset_To_RawStringArray();

            _Headers.AppendValues(_Key, "vA, vB", "vC");

            var values = _Headers.GetValues(_Key);
            Assert.IsTrue(_RawStringArray.Concat(new[] { "vA, vB", "vC" }).SequenceEqual(values));
        }

        [TestMethod]
        public void AppendValues_Ignores_Empty_Values()
        {
            Reset_To_RawStringArray();

            _Headers.AppendValues(_Key, null, "", " ");

            var values = _Headers.GetValues(_Key);
            Assert.IsTrue(_RawStringArray.SequenceEqual(values));
        }

        [TestMethod]
        public void AppendValues_Assigns_Array_If_Key_Missing()
        {
            _Headers.AppendValues(_Key, "vA, vB", "vC");

            var values = _Headers.GetValues(_Key);
            Assert.IsTrue(new[] { "vA, vB", "vC" }.SequenceEqual(values));
        }

        [TestMethod]
        public void GetCommaSeparatedValues_Returns_Header_Strings_Split_At_Unquoted_Commas_And_Commas_Removed()
        {
            Reset_To_RawStringArray();
            var getCommaSeparatedValuesList = _Headers.GetCommaSeparatedValues(_Key);
            Assert.IsTrue(_NormalisedStringArray.SequenceEqual(getCommaSeparatedValuesList));
        }

        [TestMethod]
        public void SetCommaSeparatedValues_Overwrites_Existing_Array_And_Quotes_Strings_With_Quotes()
        {
            Reset_To_RawStringArray();

            _Headers.SetCommaSeparatedValues(_Key, "vA, vB", "vC");

            var values = _Headers.GetValues(_Key);
            Assert.IsTrue(new[] { "\"vA, vB\"", "vC" }.SequenceEqual(values));
        }

        [TestMethod]
        public void SetCommaSeparatedValues_Ignores_Null_Or_Empty_Values()
        {
            Reset_To_RawStringArray();

            _Headers.SetCommaSeparatedValues(_Key, "vA, vB", null, "", " ", "vC");

            var values = _Headers.GetValues(_Key);
            Assert.IsTrue(new[] { "\"vA, vB\"", "vC" }.SequenceEqual(values));
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow(new string[0])]
        [DataRow(new string[] { null, "", " " })]
        public void SetCommaSeparatedValues_Of_Null_Or_Empty_Array_Removes_Header(string[] value)
        {
            Reset_To_RawStringArray();
            _Headers.SetCommaSeparatedValues(_Key, value);
            Assert.IsFalse(_Headers.ContainsKey(_Key));
        }

        [TestMethod]
        public void AppendCommaSeparatedValues_Appends_To_Existing_Values()
        {
            Reset_To_RawStringArray();

            _Headers.AppendCommaSeparatedValues(_Key, "vA, vB", "vC");

            var values = _Headers.GetValues(_Key);
            Assert.IsTrue(_RawStringArray.Concat(new[] { "\"vA, vB\"", "vC" }).SequenceEqual(values));
        }

        [TestMethod]
        public void AppendCommaSeparatedValues_Ignores_Null_Or_Empty_Values()
        {
            Reset_To_RawStringArray();

            _Headers.AppendCommaSeparatedValues(_Key, null, "", " ");

            var values = _Headers.GetValues(_Key);
            Assert.IsTrue(_RawStringArray.SequenceEqual(values));
        }
    }
}
