// Copyright © 2019 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using AWhewell.Owin.Utility.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin.Utility.Parsers
{
    [TestClass]
    public class DateTimeOffset_JavaScriptTicks_ParserTests
    {
        [TestMethod]
        [DataRow("en-GB", null,             false,  "0001-01-01 00:00:00.000")]     // Cannot parse null
        [DataRow("en-GB", "rubbish",        false,  "0001-01-01 00:00:00.000")]     // Cannot parse non-date
        [DataRow("en-GB", "0",              true,   "1970-01-01 00:00:00.000")]     // Epoch
        [DataRow("en-GB", "1",              true,   "1970-01-01 00:00:00.001")]     // 1 millisecond after the epoch
        [DataRow("en-GB", "1575120094615",  true,   "2019-11-30 13:21:34.615")]     // More than 32 bit milliseconds after epoch
        public void TryParse_Behaves_Correctly(string culture, string text, bool expectedResult, string expectedValueText)
        {
            using(new CultureSwap(culture)) {
                var parser = new DateTimeOffset_JavaScriptTicks_Parser();

                var actualResult = parser.TryParse(text, out var actualValue);

                var expectedValue = DataRowParser.DateTimeOffset(expectedValueText);
                Assert.AreEqual(expectedResult, actualResult);
                Assert.AreEqual(expectedValue, actualValue);
            }
        }
    }
}
