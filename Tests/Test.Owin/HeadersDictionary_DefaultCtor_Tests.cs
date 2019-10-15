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
using Owin.Interface;

namespace Test.Owin
{
    [TestClass]
    public class HeadersDictionary_DefaultCtor_Tests : HeadersDictionary_Agnostic_Tests
    {
        [TestInitialize]
        public void TestInitialise()
        {
            _Headers = new HeadersDictionary();
        }

        protected override void Reset_To_RawStringArray()
        {
            _Headers = new HeadersDictionary() {
                { _Key, _RawStringArray },
            };
        }

        [TestMethod]
        public void Initialising_Dictonary_To_Null_Array_Produces_Null_Results()
        {
            _Headers = new HeadersDictionary(null);

            Assert.IsNull(_Headers[_Key]);
            Assert.IsNull(_Headers.Get(_Key));
            Assert.IsNull(_Headers.GetValues(_Key));
            Assert.IsNull(_Headers.GetCommaSeparatedValues(_Key));
        }

        [TestMethod]
        [DataRow(new string[] { "" },                   null)]      // Null input is expected to be coalesced to an empty string before use
        [DataRow(new string[] { "" },                   "")]
        [DataRow(new string[] { "a" },                  "a")]
        [DataRow(new string[] { "a", "b" },             "a,b")]
        [DataRow(new string[] { "a", "b", "", "" },     "a,b,,")]
        [DataRow(new string[] { "a", "b", "", " c" },   "a,b,, c")]
        [DataRow(new string[] { "\"a,b\"" },            "\"a,b\"")]
        public void Static_SplitRawHeaderValue_Returns_Correct_Array_For_Header_Value(string[] expected, string rawString)
        {
            var actual = HeadersDictionary.SplitRawHeaderValue(rawString);

            Assert.IsTrue(expected.SequenceEqual(actual));
        }

        [TestMethod]
        [DataRow(null,                                      "")]    // Null input is expected to be coalesced to an empty array before use
        [DataRow(new string[] { null },                     "")]
        [DataRow(new string[] { "" },                       "")]
        [DataRow(new string[] { " " },                      " ")]
        [DataRow(new string[] { "a", null, "", " ", "b" },  "a,,, ,b")]
        [DataRow(new string[] { "\"a, b\"", " c" },         "\"a, b\", c")]
        public void Static_JoinCookedHeaderValues_Ignores_Null_And_Empty_Values(string[] rawStringArray, string expected)
        {
            var actual = HeadersDictionary.JoinCookedHeaderValues(rawStringArray);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow(new string[] { "" },               "")]
        [DataRow(new string[] { " " },              " ")]
        [DataRow(new string[] { "\"a, b\"", " c" }, "\"a, b\", c")]
        public void Static_SplitRawHeaderValue_Round_Trips_With_JoinCookedHeaderValues(string [] splitValues, string headerValue)
        {
            var splitHeaderValues = HeadersDictionary.SplitRawHeaderValue(headerValue);
            if(splitValues == null) {
                Assert.IsNull(splitHeaderValues);
            } else {
                Assert.IsTrue(splitValues.SequenceEqual(splitHeaderValues));
            }

            var rejoinedHeader = HeadersDictionary.JoinCookedHeaderValues(splitHeaderValues);
            if(headerValue == null) {
                Assert.IsNull(rejoinedHeader);
            } else {
                Assert.AreEqual(headerValue, rejoinedHeader);
            }
        }
    }
}
