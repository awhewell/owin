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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AWhewell.Owin.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin.Utility
{
    [TestClass]
    public class QualityValue_Tests
    {
        [TestMethod]
        [DataRow(null, null)]
        [DataRow("",   "")]
        [DataRow("Ab", "Ab")]
        public void Value_Ctor_Sets_Properties_Correctly(string value, string expectedValue)
        {
            var qv = new QualityValue(value);

            Assert.AreEqual(expectedValue, qv.Value);
            Assert.IsNull(qv.Quality);
        }

        [TestMethod]
        [DataRow(null, null, null, null)]
        [DataRow(null, 0.8,  null, 0.8)]
        [DataRow("",   1.0,  "",   1.0)]
        [DataRow("",   1.1,  "",   1.1)]        // Ctor does not care if you go out of bounds
        [DataRow("",   -0.1, "",   -0.1)]       // Ctor does not care if you go out of bounds
        [DataRow("Ab", 0.5,  "Ab", 0.5)]
        [DataRow("Ab", null, "Ab", null)]
        public void Value_And_Quality_Ctor_Sets_Properties_Correctly(string value, double? quality, string expectedValue, double? expectedQuality)
        {
            var qv = new QualityValue(value, quality);

            Assert.AreEqual(expectedValue, qv.Value);
            Assert.AreEqual(expectedQuality, qv.Quality);
        }

        [TestMethod]
        [DataRow("en-GB", null,  null,   "")]
        [DataRow("en-GB", "Abc", 1.0,    "Abc;q=1.0")]
        [DataRow("de-DE", "Abc", 1.0,    "Abc;q=1.0")]
        [DataRow("de-DE", "Abc", 0.1234, "Abc;q=0.123")]
        public void ToString_Returns_Correct_Description(string culture, string value, double? quality, string expected)
        {
            using(new CultureSwap(culture)) {
                var qv = new QualityValue(value, quality);
                var actual = qv.ToString();
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [DataRow(null,          "en-GB", true,  null,           null)]      // Null header
        [DataRow("",            "en-GB", false, "",             null)]      // Empty header specified
        [DataRow("a",           "en-GB", false, "a",            null)]      // No quality specified
        [DataRow("a;q=0.8",     "en-GB", false, "a",            0.8)]       // Value and quality
        [DataRow("a;q=0.8",     "de-DE", false, "a",            0.8)]       // Uses invariant culture when parsing decimal
        [DataRow("a; q=0.8",    "en-GB", false, "a; q=0.8",     null)]      // Leading whitespace before Q not allowed
        [DataRow("a;q =0.8",    "en-GB", false, "a;q =0.8",     null)]      // Trailing whitespace after Q not allowed
        [DataRow("a;q= 0.8",    "en-GB", false, "a;q= 0.8",     null)]      // Leading whitespace on q-value not allowed
        [DataRow("a;q=0.8 ",    "en-GB", false, "a",            0.8)]       // Trailing whitespace on q-value is allowed
        [DataRow("a;q=0.8x",    "en-GB", false, "a;q=0.8x",     null)]      // Trailing characters after q-value not allowed
        [DataRow("a;Q=0.8",     "en-GB", false, "a",            0.8)]       // Q-value is case insensitive
        [DataRow(" a ;Q=0.8",   "en-GB", false, " a ",          0.8)]       // Value is not trimmed
        [DataRow(";Q=0.8",      "en-GB", false, "",             0.8)]       // Value can be empty
        [DataRow("a;q=0.000",   "en-GB", false, "a",            0.0)]       // Value can be as low as zero
        [DataRow("a;q=-0.001",  "en-GB", false, "a;q=-0.001",   null)]      // Value cannot be negative
        [DataRow("a;q=1.000",   "en-GB", false, "a",            1.0)]       // Value can be as high as 1
        [DataRow("a;q=1.001",   "en-GB", false, "a;q=1.001",    null)]      // Value cannot exceed 1.0
        [DataRow("a;q=0.123",   "en-GB", false, "a",            0.123)]     // Values can go to three decimal places
        [DataRow("a;q=0.1234",  "en-GB", false, "a;q=0.1234",   null)]      // Values cannot exceed three decimal places
        [DataRow("a;q=.123",    "en-GB", false, "a;q=.123",     null)]      // Values must start with a digit
        public void Parse_Parses_Header_Values_Correctly(string headerValue, string culture, bool expectNull, string expectedValue, double? expectedQuality)
        {
            using(new CultureSwap(culture)) {
                var qv = QualityValue.Parse(headerValue);

                if(expectNull) {
                    Assert.IsNull(qv);
                } else {
                    Assert.AreEqual(expectedValue, qv.Value);
                    Assert.AreEqual(expectedQuality, qv.Quality);
                }
            }
        }

        [TestMethod]
        [DataRow(null,                          "en-GB", new string[] { })]
        [DataRow("",                            "en-GB", new string[] { })]
        [DataRow(" ",                           "en-GB", new string[] { })]
        [DataRow("Value1;q=0.1",                "en-GB", new string[] { "Value1;q=0.1" })]
        [DataRow("Value1;q=0.1,Value2;q=0.2",   "en-GB", new string[] { "Value1;q=0.1", "Value2;q=0.2" })]
        [DataRow("Value1;q=0.1, Value2;q=0.2",  "en-GB", new string[] { "Value1;q=0.1", "Value2;q=0.2" })]
        [DataRow("Value1;q=0.1,,Value2;q=0.2",  "en-GB", new string[] { "Value1;q=0.1", "", "Value2;q=0.2" })]
        public void ParseCommaSeparated_Parses_Lists_Of_Quality_Values_Correctly(string headerValue, string culture, string[] expectedQualityValues)
        {
            using(new CultureSwap(culture)) {
                var actual = QualityValue.ParseCommaSeparated(headerValue);

                Assertions.AreEqual(
                    expectedQualityValues,
                    actual.Select(r => r.ToString()).ToArray()
                );
            }
        }
    }
}
