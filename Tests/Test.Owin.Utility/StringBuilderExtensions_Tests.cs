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
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AWhewell.Owin.Utility;

namespace Test.AWhewell.Owin.Utility
{
    [TestClass]
    public class StringBuilderExtensions_Tests
    {
        [TestMethod]
        [DataRow("",       'e', -1)]
        [DataRow("Tested", 'e', 1)]
        [DataRow("Tested", 'T', 0)]
        [DataRow("Tested", 'E', -1)]
        public void IndexOf_Char_Returns_Expected_Results(string text, char searchChar, int expected)
        {
            var builder = new StringBuilder(text);

            var actual = builder.IndexOf(searchChar);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("Tested", 'T', 0,  0)]
        [DataRow("Tested", 'E', 0,  -1)]
        [DataRow("Tested", 't', 6,  -1)]
        [DataRow("Tested", 't', 60, -1)]
        [DataRow("Tested", 't', -1, null)]
        [DataRow("Tested", 'e', 0,  1)]
        [DataRow("Tested", 'e', 1,  1)]
        [DataRow("Tested", 'e', 2,  4)]
        [DataRow("Tested", 'e', 3,  4)]
        [DataRow("Tested", 'e', 4,  4)]
        [DataRow("Tested", 'e', 5,  -1)]
        public void IndexOf_Char_And_StartIndex_Returns_Expected_Results(string text, char searchChar, int startIndex, int? expected)
        {
            var builder = new StringBuilder(text);

            var outOfRange = false;
            var actual = int.MinValue;
            try {
                actual = builder.IndexOf(searchChar, startIndex);
            } catch(ArgumentOutOfRangeException) {
                outOfRange = true;
            }

            if(expected == null) {
                Assert.IsTrue(outOfRange);
            } else {
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [DataRow("Tested", 'e', 4)]
        [DataRow("Tested", 'T', 0)]
        [DataRow("Tested", 'E', -1)]
        public void LastIndexOf_Char_Returns_Expected_Results(string text, char searchChar, int expected)
        {
            var builder = new StringBuilder(text);

            var actual = builder.LastIndexOf(searchChar);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("Tested", 'T', 5,   0)]
        [DataRow("Tested", 'E', 5,   -1)]
        [DataRow("Tested", 't', 6,   null)]
        [DataRow("Tested", 't', -1,  -1)]
        [DataRow("Tested", 't', -10, -1)]
        [DataRow("Tested", 'e', 5,   4)]
        [DataRow("Tested", 'e', 4,   4)]
        [DataRow("Tested", 'e', 3,   1)]
        [DataRow("Tested", 'e', 2,   1)]
        [DataRow("Tested", 'e', 1,   1)]
        [DataRow("Tested", 'e', 0,   -1)]
        public void LastIndexOf_Char_And_StartIndex_Returns_Expected_Results(string text, char searchChar, int startIndex, int? expected)
        {
            var builder = new StringBuilder(text);

            var outOfRange = false;
            var actual = int.MinValue;
            try {
                actual = builder.LastIndexOf(searchChar, startIndex);
            } catch(ArgumentOutOfRangeException) {
                outOfRange = true;
            }

            if(expected == null) {
                Assert.IsTrue(outOfRange);
            } else {
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [DataRow("Tested", 'e', true)]
        [DataRow("Tested", 'E', false)]
        public void Contains_Returns_Expected_Results(string text, char searchChar, bool expected)
        {
            var builder = new StringBuilder(text);

            var actual = builder.Contains(searchChar);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("",  ",", "a",  "a")]
        [DataRow("a", ",", "b",  "a,b")]
        [DataRow("a", ",", "",   "a,")]
        [DataRow("a", ",", null, "a,")]
        public void AppendWithSeparator_Returns_Expected_Results(string initialBuilderContent, string separator, object value, string expected)
        {
            var builder = new StringBuilder(initialBuilderContent);

            builder.AppendWithSeparator(separator, value);

            Assert.AreEqual(expected, builder.ToString());
        }
    }
}
