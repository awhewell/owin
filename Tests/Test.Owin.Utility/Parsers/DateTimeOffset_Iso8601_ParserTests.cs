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
    public class DateTimeOffset_Iso8601_ParserTests
    {
        [TestMethod]
        [DataRow("en-GB", null,                             false,  "0001-01-01 00:00:00.000")]         // Cannot parse null
        [DataRow("en-GB", "rubbish",                        false,  "0001-01-01 00:00:00.000")]         // Cannot parse non-date
        [DataRow("en-GB", "2019-07-01T17:42",               true,   "2019-07-01 17:42:00.000")]         // Missing Z means the time zone is unknown, assume universal
        [DataRow("en-US", "2019-07-01T17:42+01:00",         true,   "2019-07-01 17:42:00.000 +0100")]   // Timezone offset is local
        [DataRow("en-US", "2019-07-01T17:42+0100",          true,   "2019-07-01 17:42:00.000 +0100")]   // Timezone offset is local, no colon variant
        [DataRow("en-US", "2019-07-01T17:42Z",              true,   "2019-07-01 17:42:00.000")]         // Zulu indicator means it is in UTC
        [DataRow("en-US", "2019-07-01T17:42+0100Z",         false,  "0001-01-01 00:00:00.000")]         // Can't specify both offset from UTC *and* the UTC indicator
        [DataRow("en-GB", "20190701T1742",                  true,   "2019-07-01 17:42:00.000")]         // Variant without separators
        [DataRow("en-US", "20190701T1742+0100",             true,   "2019-07-01 17:42:00.000 +0100")]   // Variant without separators
        [DataRow("en-US", "20190701T1742Z",                 true,   "2019-07-01 17:42:00.000")]         // Variant without separators
        [DataRow("en-GB", "2019-07-01T17:42:32",            true,   "2019-07-01 17:42:32.000")]         // Variant with seconds
        [DataRow("en-US", "2019-07-01T17:42:32+0100",       true,   "2019-07-01 17:42:32.000 +0100")]   // Variant with seconds
        [DataRow("en-GB", "2019-07-01T17:42:32Z",           true,   "2019-07-01 17:42:32.000")]         // Variant with seconds
        [DataRow("en-GB", "20190701T174232",                true,   "2019-07-01 17:42:32.000")]         // Variant with seconds and without separators
        [DataRow("en-US", "20190701T174232+0100",           true,   "2019-07-01 17:42:32.000 +0100")]   // Variant with seconds and without separators
        [DataRow("en-GB", "20190701T174232Z",               true,   "2019-07-01 17:42:32.000")]         // Variant with seconds and without separators
        [DataRow("en-GB", "2019-07-01T17:42:32.1",          true,   "2019-07-01 17:42:32.100")]         // Variant with tenths of second
        [DataRow("en-GB", "2019-07-01T17:42:32.1+0100",     true,   "2019-07-01 17:42:32.100 +0100")]   // Variant with tenths of second
        [DataRow("en-GB", "2019-07-01T17:42:32.1Z",         true,   "2019-07-01 17:42:32.100")]         // Variant with tenths of second
        [DataRow("en-GB", "20190701T174232.1",              true,   "2019-07-01 17:42:32.100")]         // Variant with tenths of second and without separators
        [DataRow("en-GB", "20190701T174232.1+0100",         true,   "2019-07-01 17:42:32.100 +0100")]   // Variant with tenths of second and without separators
        [DataRow("en-GB", "20190701T174232.1Z",             true,   "2019-07-01 17:42:32.100")]         // Variant with tenths of second and without separators
        [DataRow("en-GB", "2019-07-01T17:42:32.12",         true,   "2019-07-01 17:42:32.120")]         // Variant with hundredths of second
        [DataRow("en-GB", "2019-07-01T17:42:32.12+0100",    true,   "2019-07-01 17:42:32.120 +0100")]   // Variant with hundredths of second
        [DataRow("en-GB", "2019-07-01T17:42:32.12Z",        true,   "2019-07-01 17:42:32.120")]         // Variant with hundredths of second
        [DataRow("en-GB", "20190701T174232.12",             true,   "2019-07-01 17:42:32.120")]         // Variant with hundredths of second and without separators
        [DataRow("en-GB", "20190701T174232.12+0100",        true,   "2019-07-01 17:42:32.120 +0100")]   // Variant with hundredths of second and without separators
        [DataRow("en-GB", "20190701T174232.12Z",            true,   "2019-07-01 17:42:32.120")]         // Variant with hundredths of second and without separators
        [DataRow("en-GB", "2019-07-01T17:42:32.123",        true,   "2019-07-01 17:42:32.123")]         // Variant with milliseconds
        [DataRow("en-GB", "2019-07-01T17:42:32.123-0100",   true,   "2019-07-01 17:42:32.123 -0100")]   // Variant with milliseconds
        [DataRow("en-GB", "2019-07-01T17:42:32.123Z",       true,   "2019-07-01 17:42:32.123")]         // Variant with milliseconds
        [DataRow("en-GB", "20190701T174232.123",            true,   "2019-07-01 17:42:32.123")]         // Variant with milliseconds and without separators
        [DataRow("en-GB", "20190701T174232.123-0100",       true,   "2019-07-01 17:42:32.123 -0100")]   // Variant with milliseconds and without separators
        [DataRow("en-GB", "20190701T174232.123Z",           true,   "2019-07-01 17:42:32.123")]         // Variant with milliseconds and without separators
        public void TryParse_Behaves_Correctly(string culture, string text, bool expectedResult, string expectedValueText)
        {
            using(new CultureSwap(culture)) {
                var parser = new DateTimeOffset_Iso8601_Parser();

                var actualResult = parser.TryParse(text, out var actualValue);

                var expectedValue = DataRowParser.DateTimeOffset(expectedValueText);
                Assert.AreEqual(expectedResult, actualResult);
                Assert.AreEqual(expectedValue, actualValue);
            }
        }
    }
}
