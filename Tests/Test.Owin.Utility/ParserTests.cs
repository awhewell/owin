// Copyright © 2019 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using AWhewell.Owin.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin.Utility
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        [DataRow(null,      "en-GB",    null)]
        [DataRow("",        "en-GB",    null)]
        [DataRow("true",    "en-GB",    true)]
        [DataRow("true",    "de-DE",    true)]
        [DataRow("True",    "en-GB",    true)]
        [DataRow("TRUE",    "en-GB",    true)]
        [DataRow("false",   "en-GB",    false)]
        [DataRow("false",   "de-DE",    false)]
        [DataRow("False",   "en-GB",    false)]
        [DataRow("FALSE",   "en-GB",    false)]
        [DataRow(" true",   "en-GB",    true)]
        [DataRow("true ",   "en-GB",    true)]
        [DataRow(" true ",  "en-GB",    true)]
        [DataRow("0",       "en-GB",    null)]
        [DataRow("1",       "en-GB",    null)]
        [DataRow("yes",     "en-GB",    null)]
        [DataRow("no",      "en-GB",    null)]
        [DataRow("on",      "en-GB",    null)]
        [DataRow("off",     "en-GB",    null)]
        public void ParseBool_Parses_String_Into_Nullable_Bool(string input, string culture, bool? expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Parser.ParseBool(input);

                if(expected == null) {
                    Assert.IsNull(actual);
                } else {
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        [TestMethod]
        [DataRow(null,                      "en-GB",    null)]
        [DataRow("",                        "en-GB",    null)]
        [DataRow("-9223372036854775808",    "en-GB",    -9223372036854775808)]
        [DataRow("9223372036854775807",     "en-GB",    9223372036854775807)]
        [DataRow("-9223372036854775808",    "de-DE",    -9223372036854775808)]
        [DataRow("9223372036854775807",     "de-DE",    9223372036854775807)]
        [DataRow(" 1",                      "en-GB",    (long)1)]
        [DataRow("1 ",                      "en-GB",    (long)1)]
        [DataRow(" 1 ",                     "en-GB",    (long)1)]
        [DataRow("1,234",                   "en-GB",    null)]
        [DataRow("1 234",                   "en-GB",    null)]
        [DataRow("1,234",                   "de-DE",    null)]
        [DataRow("1 234",                   "de-DE",    null)]
        [DataRow("(1)",                     "en-GB",    null)]
        public void ParseInt64_Parses_String_Into_Nullable_Long(string input, string culture, long? expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Parser.ParseInt64(input);

                if(expected == null) {
                    Assert.IsNull(actual);
                } else {
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        [TestMethod]
        [DataRow(null,          "en-GB",    null)]
        [DataRow("",            "en-GB",    null)]
        [DataRow("-2147483648", "en-GB",    -2147483648)]
        [DataRow("2147483647",  "en-GB",    2147483647)]
        [DataRow("-2147483648", "de-DE",    -2147483648)]
        [DataRow("2147483647",  "de-DE",    2147483647)]
        [DataRow(" 1",          "en-GB",    1)]
        [DataRow("1 ",          "en-GB",    1)]
        [DataRow(" 1 ",         "en-GB",    1)]
        [DataRow("1,234",       "en-GB",    null)]
        [DataRow("1 234",       "en-GB",    null)]
        [DataRow("1,234",       "de-DE",    null)]
        [DataRow("1 234",       "de-DE",    null)]
        [DataRow("(1)",         "en-GB",    null)]
        public void ParseInt32_Parses_String_Into_Nullable_Int(string input, string culture, int? expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Parser.ParseInt32(input);

                if(expected == null) {
                    Assert.IsNull(actual);
                } else {
                    Assert.AreEqual(expected, actual);
                }
            }
        }
    }
}
