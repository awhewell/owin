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
    public class DateTime_Local_ParserTests
    {
        [TestMethod]
        [DataRow("en-GB", null,                             false,  "0001-01-01 00:00:00.000 Unspecified")] // Cannot parse null
        [DataRow("en-GB", "rubbish",                        false,  "0001-01-01 00:00:00.000 Unspecified")] // Cannot parse non-date
        [DataRow("en-GB", "2019-07-01 17:42:32.123",        true,   "2019-07-01 17:42:32.123 Local")]       // No indication of timezone, defaults to local
        [DataRow("en-GB", "01/07/2019 17:42:32.123",        true,   "2019-07-01 17:42:32.123 Local")]       // Current culture dictates the day/month order
        [DataRow("en-US", "07/01/2019 17:42:32.123",        true,   "2019-07-01 17:42:32.123 Local")]       // Current culture dictates the day/month order
        [DataRow("en-GB", "2019-07-01T17:42:32.123Z",       true,   "2019-07-01 18:42:32.123 Local")]       // If UTC is specified then output is converted to local (GMT +1 hour for the UK on this date)
        [DataRow("en-GB", "2019-07-01T17:42:32.123+0800",   true,   "2019-07-01 10:42:32.123 Local")]       // If timezone is specified then parser converts to UTC (8 hours ahead of GMT) and then to local (1 hour ahead = overall -7 hours from stated time)
        public void TryParse_Behaves_Correctly(string culture, string text, bool expectedResult, string expectedValueText)
        {
            using(new CultureSwap(culture)) {
                var parser = new DateTime_Local_Parser();

                var actualResult = parser.TryParse(text, out var actualValue);

                var expectedValue = DataRowParser.DateTime(expectedValueText);
                Assert.AreEqual(expectedResult, actualResult);
                Assert.AreEqual(expectedValue, actualValue);
                Assert.AreEqual(expectedValue?.Kind, actualValue.Kind);
            }
        }
    }
}
