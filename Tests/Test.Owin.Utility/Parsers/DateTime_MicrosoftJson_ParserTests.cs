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
    public class DateTime_MicrosoftJson_ParserTests
    {
        /*
         * These were checked against the following LINQPad script:
<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Serialization.dll</Reference>
  <Namespace>System.Xml.Serialization</Namespace>
  <Namespace>System.Runtime.Serialization.Json</Namespace>
  <Namespace>System.Runtime.Serialization</Namespace>
</Query>

void Main()
{
    var date = new DateTime(2007, 7, 29, 05, 11, 57, 056, DateTimeKind.Local);

    var x = new X() { D = date, };
    var serialiser = new DataContractJsonSerializer(typeof(X));
    using(var memoryStream = new MemoryStream()) {
        serialiser.WriteObject(memoryStream, x);
        Console.WriteLine(Encoding.UTF8.GetString(memoryStream.ToArray()));
    }

    Console.WriteLine($"Original:           {date.ToString("yyyy-MM-dd HH:mm:ss.fff K")}");
    
    var ms = 1185685917056;
    Console.WriteLine($"{ms} ms is {new DateTime(1970, 1, 1).AddMilliseconds(ms).ToString("yyyy-MM-dd HH:mm:ss.fff K")}");
    
    var dateString = "/Date(1185682317056-0100)/";
    var dateJson = $"{{\"D\":\"{dateString}\"}}";

    using(var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(dateJson))) {
        var obj = (X)serialiser.ReadObject(memoryStream);
        Console.WriteLine();
        Console.WriteLine($"{dateString} resolves to {obj.D.ToString("yyyy-MM-dd HH:mm:ss.fff K")}");
    }
}

[DataContract]
class X
{
    [DataMember]
    public DateTime D { get; set; }
}
        */
        [TestMethod]
        [DataRow("en-GB", null,                             false,  "0001-01-01 00:00:00.000")]             // Cannot parse null
        [DataRow("en-GB", @"rubbish",                        false,  "0001-01-01 00:00:00.000")]             // Cannot parse non-date
        [DataRow("en-GB", @"2019-07-01 17:42:32.123",        false,  "0001-01-01 00:00:00.000")]             // Not a Microsoft JSON date
        [DataRow("en-GB", @"2019-07-01T17:42:32.123Z",       false,  "0001-01-01 00:00:00.000")]             // Not a Microsoft JSON date
        [DataRow("en-GB", @"/Date(1185682317056+0100)/",     true,   "2007-07-29 05:11:57.056 Local")]
        [DataRow("en-GB", @"/Date(1185682317056+0000)/",     true,   "2007-07-29 05:11:57.056 Local")]       // In testing DataContractJsonSerializer it turned out that +0000, +0100, -0100, +0200 and -0200 all produced the same local time. I think on deserialisation the timezone just indicates "local" for DateTime
        [DataRow("en-GB", @"/Date(1185682317056-0100)/",     true,   "2007-07-29 05:11:57.056 Local")]       // See above
        [DataRow("en-GB", @"/Date(1185682317056+0200)/",     true,   "2007-07-29 05:11:57.056 Local")]       // See above
        [DataRow("en-GB", @"/Date(1185682317056)/",          true,   "2007-07-29 04:11:57.056 Utc")]
        [DataRow("en-GB", @"\/Date(1185682317056+0100)\/",   true,   "2007-07-29 05:11:57.056 Local")]
        [DataRow("en-GB", @"\/Date(1185682317056+0000)\/",   true,   "2007-07-29 05:11:57.056 Local")]       // In testing DataContractJsonSerializer it turned out that +0000, +0100, -0100, +0200 and -0200 all produced the same local time. I think on deserialisation the timezone just indicates "local" for DateTime
        [DataRow("en-GB", @"\/Date(1185682317056-0100)\/",   true,   "2007-07-29 05:11:57.056 Local")]       // See above
        [DataRow("en-GB", @"\/Date(1185682317056+0200)\/",   true,   "2007-07-29 05:11:57.056 Local")]       // See above
        [DataRow("en-GB", @"\/Date(1185682317056)\/",        true,   "2007-07-29 04:11:57.056 Utc")]
        public void TryParse_Behaves_Correctly(string culture, string text, bool expectedResult, string expectedValueText)
        {
            using(new CultureSwap(culture)) {
                var parser = new DateTime_MicrosoftJson_Parser();

                var actualResult = parser.TryParse(text, out var actualValue);

                var expectedValue = DataRowParser.DateTime(expectedValueText);
                Assert.AreEqual(expectedResult, actualResult);
                Assert.AreEqual(expectedValue, actualValue);
                Assert.AreEqual(expectedValue?.Kind, actualValue.Kind);
            }
        }
    }
}
