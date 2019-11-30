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
    public class DateTimeOffset_MicrosoftJson_ParserTests
    {
        [TestMethod]
        [DataRow("en-GB", null,                             false,  "0001-01-01 00:00:00.000")]         // Cannot parse null
        [DataRow("en-GB", "rubbish",                        false,  "0001-01-01 00:00:00.000")]         // Cannot parse non-date
        [DataRow("en-GB", "2019-07-01 17:42:32.123",        false,  "0001-01-01 00:00:00.000")]         // Not a Microsoft JSON date
        [DataRow("en-GB", "2019-07-01T17:42:32.123Z",       false,  "0001-01-01 00:00:00.000")]         // Not a Microsoft JSON date
        [DataRow("en-GB", "/Date(1185689517056)/",          true,   "2007-07-29 06:11:57.056")]         // No time zone
        [DataRow("en-GB", "/Date(1185689517056+0100)/",     true,   "2007-07-29 06:11:57.056 +0100")]   // +ve offset
        [DataRow("en-GB", "/Date(1185689517056-0100)/",     true,   "2007-07-29 06:11:57.056 -0100")]   // +ve offset
        [DataRow("en-GB", "/Date(1185689517056+1234)/",     true,   "2007-07-29 06:11:57.056 +1234")]   // +ve offset
        [DataRow("en-GB", "/Date(1185689517056-1234)/",     true,   "2007-07-29 06:11:57.056 -1234")]   // +ve offset
        public void TryParse_Behaves_Correctly(string culture, string text, bool expectedResult, string expectedValueText)
        {
            using(new CultureSwap(culture)) {
                var parser = new DateTimeOffset_MicrosoftJson_Parser();

                var actualResult = parser.TryParse(text, out var actualValue);

                var expectedValue = DataRowParser.DateTimeOffset(expectedValueText);
                Assert.AreEqual(expectedResult, actualResult);
                Assert.AreEqual(expectedValue, actualValue);
            }
        }
    }
}
