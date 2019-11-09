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
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using AWhewell.Owin.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin.Utility
{
    [TestClass]
    public class ParserTests
    {
        static Regex ExpectedDateTimeRegex = new Regex(
            @"^" +
            @"(" +
                @"((?<year>\d\d\d\d)-(?<month>\d\d)-(?<day>\d\d))" +
                @"|" +
                @"(?<named>today)" +
            @")" +
            @"( " +
                @"(?<hour>\d\d):(?<minute>\d\d):(?<second>\d\d)" +
                @"(.(?<ms>\d\d\d))?" +
            @")?" +
            @"( " +
                @"(?<kind>local|utc|unspecified)" +
            @")?" +
            @"$",
            RegexOptions.IgnoreCase
        );

        static Regex ExpectedDateTimeOffsetRegex = new Regex(
            @"^" +
            @"(" +
                @"((?<year>\d\d\d\d)-(?<month>\d\d)-(?<day>\d\d))" +
                @"|" +
                @"(?<named>today)" +
            @")" +
            @"( " +
                @"(?<hour>\d\d):(?<minute>\d\d):(?<second>\d\d)" +
                @"(.(?<ms>\d\d\d))?" +
            @")?" +
            @"( " +
                @"(?<offset>[+|-]\d+)" +
            @")?" +
            @"$",
            RegexOptions.IgnoreCase
        );

        public static DateTime? ExpectedDateTime(string expected, Action expectedIsUtc)
        {
            DateTime? result = null;

            if(expected != null) {
                var match = ExpectedDateTimeRegex.Match(expected);
                if(match.Success) {
                    int groupToInt(string group, int missingValue)
                    {
                        var text = match.Groups[group].Value;
                        return text == "" ? missingValue : int.Parse(text);
                    }

                    var year =   groupToInt("year", 1);
                    var month =  groupToInt("month", 1);
                    var day =    groupToInt("day", 1);
                    var hour =   groupToInt("hour", 0);
                    var minute = groupToInt("minute", 0);
                    var second = groupToInt("second", 0);
                    var ms =     groupToInt("ms", 0);

                    switch(match.Groups["named"].Value.ToLower()) {
                        case "today":
                            var today = DateTime.Today;
                            year = today.Year;
                            month = today.Month;
                            day = today.Day;
                            break;
                    }

                    var kind = DateTimeKind.Unspecified;
                    switch(match.Groups["kind"].Value.ToLower()) {
                        case "local":   kind = DateTimeKind.Local; break;
                        case "utc":     kind = DateTimeKind.Utc; break;
                    }

                    result = new DateTime(year, month, day, hour, minute, second, ms, kind);

                    if(result.Value.Kind == DateTimeKind.Utc) {
                        expectedIsUtc();
                    }
                }
            }

            return result;
        }

        public static DateTimeOffset? ExpectedDateTimeOffset(string expected)
        {
            DateTimeOffset? result = null;

            if(expected != null) {
                var match = ExpectedDateTimeOffsetRegex.Match(expected);
                if(match.Success) {
                    int groupToInt(string group, int missingValue)
                    {
                        var text = match.Groups[group].Value;
                        return text == "" ? missingValue : int.Parse(text);
                    }

                    var year =   groupToInt("year", 1);
                    var month =  groupToInt("month", 1);
                    var day =    groupToInt("day", 1);
                    var hour =   groupToInt("hour", 0);
                    var minute = groupToInt("minute", 0);
                    var second = groupToInt("second", 0);
                    var ms =     groupToInt("ms", 0);
                    var offset = groupToInt("offset", 0);

                    switch(match.Groups["named"].Value.ToLower()) {
                        case "today":
                            var today = DateTime.Today;
                            year = today.Year;
                            month = today.Month;
                            day = today.Day;
                            break;
                    }

                    result = new DateTimeOffset(year, month, day, hour, minute, second, ms, new TimeSpan(0, offset, 0));
                }
            }

            return result;
        }

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
        [DataRow("0",       "en-GB",    false)]
        [DataRow("1",       "en-GB",    true)]
        [DataRow("2",       "en-GB",    null)]
        [DataRow("yes",     "en-GB",    true)]
        [DataRow("no",      "en-GB",    false)]
        [DataRow("on",      "en-GB",    true)]
        [DataRow("off",     "en-GB",    false)]
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
        [DataRow(null,      "en-GB",    null)]
        [DataRow("",        "en-GB",    null)]
        [DataRow("0",       "en-GB",    (byte)0)]
        [DataRow("255",     "en-GB",    (byte)255)]
        [DataRow("0",       "de-DE",    (byte)0)]
        [DataRow("255",     "de-DE",    (byte)255)]
        [DataRow("-1",      "en-GB",    null)]
        [DataRow("256",     "en-GB",    null)]
        [DataRow(" 0",      "en-GB",    (byte)0)]
        [DataRow("255 ",    "en-GB",    (byte)255)]
        public void ParseByte_Parses_String_Into_Nullable_Byte(string input, string culture, byte? expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Parser.ParseByte(input);

                if(expected == null) {
                    Assert.IsNull(actual);
                } else {
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        [TestMethod]
        [DataRow(null,      "en-GB",    null)]
        [DataRow("",        "en-GB",    null)]
        [DataRow("a",       "en-GB",    'a')]
        [DataRow("Z",       "en-GB",    'Z')]
        [DataRow(".",       "de-DE",    '.')]
        [DataRow("aa",      "en-GB",    null)]
        public void ParseChar_Parses_String_Into_Nullable_Char(string input, string culture, char? expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Parser.ParseChar(input);

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
        [DataRow("-32768",      "en-GB",    (Int16)(-32768))]
        [DataRow("32767",       "en-GB",    (Int16)32767)]
        [DataRow("-32768",      "de-DE",    (Int16)(-32768))]
        [DataRow("32767",       "de-DE",    (Int16)32767)]
        [DataRow("-32769",      "en-GB",    null)]
        [DataRow("32768",       "en-GB",    null)]
        [DataRow(" 1 ",         "en-GB",    (Int16)1)]
        [DataRow("1,234",       "en-GB",    null)]
        [DataRow("1 234",       "de-DE",    null)]
        [DataRow("(1)",         "en-GB",    null)]
        [DataRow("a",           "en-GB",    null)]
        [DataRow("1.2",         "en-GB",    null)]
        public void ParseInt16_Parses_String_Into_Nullable_Short(string input, string culture, Int16? expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Parser.ParseInt16(input);

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
        [DataRow("0",           "en-GB",    (UInt16)0)]
        [DataRow("65535",       "en-GB",    (UInt16)65535)]
        [DataRow("-1",          "en-GB",    null)]
        [DataRow("65536",       "en-GB",    null)]
        [DataRow("a",           "en-GB",    null)]
        [DataRow("1.2",         "en-GB",    null)]
        [DataRow("1,2",         "en-GB",    null)]
        public void ParseUInt16_Parses_String_Into_Nullable_Unsigned_Short(string input, string culture, UInt16? expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Parser.ParseUInt16(input);

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
        [DataRow("-2147483649", "en-GB",    null)]
        [DataRow("2147483648",  "en-GB",    null)]
        [DataRow(" 1 ",         "en-GB",    1)]
        [DataRow("-1",          "de-DE",    -1)]
        [DataRow("(1)",         "en-GB",    null)]
        [DataRow("1.2",         "en-GB",    null)]
        [DataRow("a",           "en-GB",    null)]
        public void ParseInt32_Parses_String_Into_Nullable_Int(string input, string culture, Int32? expected)
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

        [TestMethod]
        [DataRow(null,          "en-GB",    null)]
        [DataRow("",            "en-GB",    null)]
        [DataRow("0",           "en-GB",    0U)]
        [DataRow("4294967295",  "en-GB",    4294967295U)]
        [DataRow("-1",          "en-GB",    null)]
        [DataRow("4294967296",  "en-GB",    null)]
        [DataRow(" 1 ",         "en-GB",    1U)]
        [DataRow("1,2",         "de-DE",    null)]
        [DataRow("a",           "en-GB",    null)]
        public void ParseUInt32_Parses_String_Into_Nullable_Unsigned_Int(string input, string culture, UInt32? expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Parser.ParseUInt32(input);

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
        [DataRow("-9223372036854775808",    "en-GB",    -9223372036854775808L)]
        [DataRow("9223372036854775807",     "en-GB",    9223372036854775807L)]
        [DataRow("-9223372036854775809",    "en-GB",    null)]
        [DataRow("9223372036854775808",     "en-GB",    null)]
        [DataRow(" 1 ",                     "en-GB",    1L)]
        [DataRow("-1",                      "de-DE",    -1L)]
        [DataRow("(1)",                     "en-GB",    null)]
        [DataRow("1.2",                     "en-GB",    null)]
        [DataRow("a",                       "en-GB",    null)]
        public void ParseInt64_Parses_String_Into_Nullable_Long(string input, string culture, Int64? expected)
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
        [DataRow(null,                      "en-GB",    null)]
        [DataRow("",                        "en-GB",    null)]
        [DataRow("18446744073709551615",    "en-GB",    18446744073709551615UL)]
        [DataRow("-1",                      "en-GB",    null)]
        [DataRow("18446744073709551616",    "en-GB",    null)]
        [DataRow(" 1 ",                     "en-GB",    1UL)]
        [DataRow("(1)",                     "en-GB",    null)]
        [DataRow("1,2",                     "de-DE",    null)]
        [DataRow("a",                       "en-GB",    null)]
        public void ParseUInt64_Parses_String_Into_Nullable_Unsigned_Long(string input, string culture, UInt64? expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Parser.ParseUInt64(input);

                if(expected == null) {
                    Assert.IsNull(actual);
                } else {
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        [TestMethod]
        [DataRow(null,              "en-GB", null)]
        [DataRow("",                "en-GB", null)]
        [DataRow("-3.402823E+38",   "en-GB", -3.402823E+38F)]
        [DataRow("3.402823E+38",    "en-GB", 3.402823E+38F)]
        [DataRow("1.23",            "en-GB", 1.23F)]
        [DataRow("-1.23",           "de-DE", -1.23F)]
        [DataRow("1,23",            "en-GB", null)]
        [DataRow("1,23",            "de-DE", null)]
        [DataRow("1,230",           "en-GB", null)]
        [DataRow("1 230",           "de-DE", null)]
        [DataRow(" 1 ",             "en-GB", 1.0F)]
        [DataRow("a",               "en-GB", null)]
        [DataRow("(1)",             "en-GB", null)]
        public void ParseFloat_Parses_String_Into_Nullable_Float(string input, string culture, float? expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Parser.ParseFloat(input);

                if(expected == null) {
                    Assert.IsNull(actual);
                } else {
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        [TestMethod]
        [DataRow(null,                      "en-GB", null)]
        [DataRow("",                        "en-GB", null)]
        [DataRow("-1.79769313486231E+308",  "en-GB", -1.79769313486231E+308)]
        [DataRow("1.79769313486231E+308",   "en-GB", 1.79769313486231E+308)]
        [DataRow("1.23",                    "en-GB", 1.23)]
        [DataRow("-1.23",                   "de-DE", -1.23)]
        [DataRow("1,23",                    "en-GB", null)]
        [DataRow("1,23",                    "de-DE", null)]
        [DataRow("1,230",                   "en-GB", null)]
        [DataRow("1 230",                   "de-DE", null)]
        [DataRow(" 1 ",                     "en-GB", 1.0)]
        [DataRow("a",                       "en-GB", null)]
        [DataRow("(1)",                     "en-GB", null)]
        public void ParseDouble_Parses_String_Into_Nullable_Double(string input, string culture, double? expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Parser.ParseDouble(input);

                if(expected == null) {
                    Assert.IsNull(actual);
                } else {
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        [TestMethod]
        [DataRow(null,                              "en-GB", null)]
        [DataRow("",                                "en-GB", null)]
        [DataRow("-79228162514264337593543950335",  "en-GB", "-79228162514264337593543950335")]
        [DataRow("79228162514264337593543950335",   "en-GB", "79228162514264337593543950335")]
        [DataRow("-79228162514264337593543950336",  "en-GB", null)]
        [DataRow("79228162514264337593543950336",   "en-GB", null)]
        [DataRow("1.23",                            "en-GB", "1.23")]
        [DataRow("-1.23",                           "de-DE", "-1.23")]
        [DataRow("1,23",                            "en-GB", null)]
        [DataRow("1,23",                            "de-DE", null)]
        [DataRow("1,230",                           "en-GB", null)]
        [DataRow("1 230",                           "de-DE", null)]
        [DataRow(" 1 ",                             "en-GB", "1.0")]
        [DataRow("a",                               "en-GB", null)]
        [DataRow("(1)",                             "en-GB", null)]
        public void ParseDecimal_Parses_String_Into_Nullable_Decimal(string input, string culture, string expectedString)
        {
            using(new CultureSwap(culture)) {
                var actual = Parser.ParseDecimal(input);

                if(expectedString == null) {
                    Assert.IsNull(actual);
                } else {
                    var expected = decimal.Parse(expectedString, CultureInfo.InvariantCulture);
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        [TestMethod]
        [DataRow(null,                          "en-GB", null)]
        [DataRow("",                            "en-GB", null)]
        [DataRow("2019-01-02",                  "en-GB", "2019-01-02 Unspecified")]
        [DataRow("2019-01-02",                  "en-US", "2019-01-02 Unspecified")]
        [DataRow("01/02/2019",                  "en-GB", "2019-01-02 Unspecified")]
        [DataRow("01/02/2019",                  "en-US", "2019-01-02 Unspecified")]
        [DataRow("2019-01-30 13:14:15",         "en-GB", "2019-01-30 13:14:15 Utc")]
        [DataRow("2019-01-30 13:14:15.123",     "en-US", "2019-01-30 13:14:15.123 Utc")]
        [DataRow("2019-07-01T22:53:47+01:00",   "en-GB", "2019-07-01 22:53:47 Local")]
        [DataRow("2019-07-01T22:53:47+01:00",   "en-GB", "2019-07-01 21:53:47 Utc")]
        [DataRow("13:14:15",                    "en-GB", "today 13:14:15 Utc")]
        [DataRow("13:14:15.123",                "en-US", "today 13:14:15.123 Utc")]
        [DataRow("today",                       "en-GB", null)]
        [DataRow("20190130",                    "en-GB", null)]
        public void ParseDateTime_Parses_String_Into_Nullable_DateTime(string input, string culture, string expectedString)
        {
            using(new CultureSwap(culture)) {
                var actual = Parser.ParseDateTime(input);

                if(expectedString == null) {
                    Assert.IsNull(actual);
                } else {
                    var expected = ExpectedDateTime(expectedString, () => {
                        if(actual.Value.Kind == DateTimeKind.Local) {
                            actual = actual.Value.ToUniversalTime();
                        }
                    });
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        [TestMethod]
        [DataRow(null,                          "en-GB", null)]
        [DataRow("",                            "en-GB", null)]
        [DataRow("2019-01-02",                  "en-GB", "2019-01-02")]
        [DataRow("2019-01-02",                  "en-US", "2019-01-02")]
        [DataRow("01/02/2019",                  "en-GB", "2019-01-02")]
        [DataRow("01/02/2019",                  "en-US", "2019-01-02")]
        [DataRow("2019-01-30 13:14:15",         "en-GB", "2019-01-30 13:14:15")]
        [DataRow("2019-01-30 13:14:15.123",     "en-US", "2019-01-30 13:14:15.123")]
        [DataRow("2019-07-01T22:53:47+01:00",   "en-GB", "2019-07-01 22:53:47 +60")]
        [DataRow("2019-07-01T22:53:47-01:35",   "en-GB", "2019-07-01 22:53:47 -95")]
        [DataRow("13:14:15",                    "en-GB", "today 13:14:15")]
        [DataRow("today",                       "en-GB", null)]
        [DataRow("20190130",                    "en-GB", null)]
        public void ParseDateTimeOffset_Parses_String_Into_Nullable_DateTimeOffset(string input, string culture, string expectedString)
        {
            using(new CultureSwap(culture)) {
                var actual = Parser.ParseDateTimeOffset(input);

                if(expectedString == null) {
                    Assert.IsNull(actual);
                } else {
                    var expected = ExpectedDateTimeOffset(expectedString);
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        [TestMethod]
        [DataRow(null,                                      "en-GB", null)]
        [DataRow("",                                        "en-GB", null)]
        [DataRow("0c481493-72aa-4196-ac22-a27c14ec2d51",    "en-GB", "0c481493-72aa-4196-ac22-a27c14ec2d51")]
        [DataRow("0C481493-72AA-4196-AC22-A27C14EC2D51",    "en-GB", "0c481493-72aa-4196-ac22-a27c14ec2d51")]
        [DataRow("0c48149372aa4196ac22a27c14ec2d51",        "en-GB", "0c481493-72aa-4196-ac22-a27c14ec2d51")]
        [DataRow("0C48149372AA4196AC22A27C14EC2D51",        "en-GB", "0c481493-72aa-4196-ac22-a27c14ec2d51")]
        [DataRow("00000000-0000-0000-0000-000000000000",    "en-GB", "00000000-0000-0000-0000-000000000000")]
        [DataRow("00000000000000000000000000000000",        "en-GB", "00000000-0000-0000-0000-000000000000")]
        public void ParseGuid_Parses_String_Into_Nullable_Guid(string input, string culture, string expectedString)
        {
            using(new CultureSwap(culture)) {
                var actual = Parser.ParseGuid(input);

                if(expectedString == null) {
                    Assert.IsNull(actual);
                } else {
                    var expected = Guid.Parse(expectedString);
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        [TestMethod]
        [DataRow(null,      "en-GB", null)]
        [DataRow("",        "en-GB", new byte[0])]
        [DataRow("0a",      "en-GB", new byte[] { 0x0a })]
        [DataRow("000B",    "en-GB", new byte[] { 0x00, 0x0b })]
        [DataRow("0",       "en-GB", null)]
        [DataRow("0ba",     "en-GB", null)]
        [DataRow("g1",      "en-GB", null)]
        [DataRow("0x0102",  "en-GB", new byte[] { 0x01, 0x02 })]
        [DataRow("0x",      "en-GB", new byte[0])]
        [DataRow("0X0102",  "en-GB", null)]
        public void ParseHexBytes_Parses_String_Into_Nullable_Byte_Array(string input, string culture, byte[] expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Parser.ParseHexBytes(input);

                if(expected == null) {
                    Assert.IsNull(actual);
                } else {
                    Assert.IsTrue(expected.SequenceEqual(actual));
                }
            }
        }

        [TestMethod]
        [DataRow(null,      "en-GB", null)]
        [DataRow("",        "en-GB", new byte[0])]
        [DataRow("DwoL",    "en-GB", new byte[] { 0x0f, 0x0a, 0x0b })]
        [DataRow("A",       "en-GB", null)]
        public void ParseMime64Bytes_Parses_String_Into_Nullable_Byte_Array(string input, string culture, byte[] expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Parser.ParseMime64Bytes(input);

                if(expected == null) {
                    Assert.IsNull(actual);
                } else {
                    Assert.IsTrue(expected.SequenceEqual(actual));
                }
            }
        }

        [TestMethod]
        [DataRow(typeof(String),            null,                                   "en-GB",    null)]
        [DataRow(typeof(String),            "",                                     "en-GB",    "")]
        [DataRow(typeof(String),            "aB",                                   "en-GB",    "aB")]
        [DataRow(typeof(String),            " a",                                   "en-GB",    " a")]
        [DataRow(typeof(String),            "a ",                                   "en-GB",    "a ")]
        [DataRow(typeof(String),            " a ",                                  "en-GB",    " a ")]
        [DataRow(typeof(bool),              null,                                   "en-GB",    null)]
        [DataRow(typeof(bool),              "",                                     "en-GB",    null)]
        [DataRow(typeof(bool),              "true",                                 "en-GB",    true)]
        [DataRow(typeof(bool),              "false",                                "en-GB",    false)]
        [DataRow(typeof(bool),              " true",                                "en-GB",    true)]
        [DataRow(typeof(bool),              "TRUE ",                                "en-GB",    true)]
        [DataRow(typeof(bool),              "yes",                                  "en-GB",    true)]
        [DataRow(typeof(bool),              "no",                                   "en-GB",    false)]
        [DataRow(typeof(bool),              "on",                                   "en-GB",    true)]
        [DataRow(typeof(bool),              "off",                                  "en-GB",    false)]
        [DataRow(typeof(bool),              "1",                                    "en-GB",    true)]
        [DataRow(typeof(bool),              "0",                                    "en-GB",    false)]
        [DataRow(typeof(bool),              "2",                                    "en-GB",    null)]
        [DataRow(typeof(bool),              "ja",                                   "de-DE",    null)]
        [DataRow(typeof(byte),              null,                                   "en-GB",    null)]
        [DataRow(typeof(byte),              "",                                     "en-GB",    null)]
        [DataRow(typeof(byte),              "0 ",                                   "en-GB",    0)]
        [DataRow(typeof(byte),              " 255",                                 "en-GB",    255)]
        [DataRow(typeof(byte),              "-1",                                   "en-GB",    null)]
        [DataRow(typeof(byte?),             null,                                   "en-GB",    null)]
        [DataRow(typeof(byte?),             "",                                     "en-GB",    null)]
        [DataRow(typeof(byte?),             "0 ",                                   "en-GB",    0)]
        [DataRow(typeof(byte?),             " 255",                                 "en-GB",    255)]
        [DataRow(typeof(byte?),             "-1",                                   "en-GB",    null)]
        [DataRow(typeof(char),              null,                                   "en-GB",    null)]
        [DataRow(typeof(char),              "",                                     "en-GB",    null)]
        [DataRow(typeof(char),              "a",                                    "en-GB",    'a')]
        [DataRow(typeof(char),              "aa",                                   "en-GB",    null)]
        [DataRow(typeof(char?),             null,                                   "en-GB",    null)]
        [DataRow(typeof(char?),             "",                                     "en-GB",    null)]
        [DataRow(typeof(char?),             "a",                                    "en-GB",    'a')]
        [DataRow(typeof(char?),             "aa",                                   "en-GB",    null)]
        [DataRow(typeof(Int16),             null,                                   "en-GB",    null)]
        [DataRow(typeof(Int16),             "",                                     "en-GB",    null)]
        [DataRow(typeof(Int16),             "-32768",                               "en-GB",    (Int16)(-32768))]
        [DataRow(typeof(Int16),             "32767",                                "en-GB",    (Int16)32767)]
        [DataRow(typeof(Int16),             "-32768",                               "de-DE",    (Int16)(-32768))]
        [DataRow(typeof(Int16),             "32767",                                "de-DE",    (Int16)32767)]
        [DataRow(typeof(Int16),             "32768",                                "en-GB",    null)]
        [DataRow(typeof(Int16),             "1 234",                                "de-DE",    null)]
        [DataRow(typeof(Int16),             "(1)",                                  "en-GB",    null)]
        [DataRow(typeof(Int16),             "1.2",                                  "en-GB",    null)]
        [DataRow(typeof(Int16?),            null,                                   "en-GB",    null)]
        [DataRow(typeof(Int16?),            "",                                     "en-GB",    null)]
        [DataRow(typeof(Int16?),            "-32768",                               "en-GB",    (Int16)(-32768))]
        [DataRow(typeof(Int16?),            "32767",                                "en-GB",    (Int16)32767)]
        [DataRow(typeof(Int16?),            "-32768",                               "de-DE",    (Int16)(-32768))]
        [DataRow(typeof(Int16?),            "32767",                                "de-DE",    (Int16)32767)]
        [DataRow(typeof(Int16?),            "32768",                                "en-GB",    null)]
        [DataRow(typeof(Int16?),            "1 234",                                "de-DE",    null)]
        [DataRow(typeof(Int16?),            "1.2",                                  "en-GB",    null)]
        [DataRow(typeof(UInt16),            null,                                   "en-GB",    null)]
        [DataRow(typeof(UInt16),            "",                                     "en-GB",    null)]
        [DataRow(typeof(UInt16),            "0",                                    "en-GB",    (UInt16)0)]
        [DataRow(typeof(UInt16),            "65535",                                "en-GB",    (UInt16)65535)]
        [DataRow(typeof(UInt16),            "-1",                                   "en-GB",    null)]
        [DataRow(typeof(UInt16),            "65536",                                "en-GB",    null)]
        [DataRow(typeof(UInt16),            "a",                                    "en-GB",    null)]
        [DataRow(typeof(UInt16),            "1.2",                                  "en-GB",    null)]
        [DataRow(typeof(UInt16),            "1,2",                                  "en-GB",    null)]
        [DataRow(typeof(UInt16?),           null,                                   "en-GB",    null)]
        [DataRow(typeof(UInt16?),           "",                                     "en-GB",    null)]
        [DataRow(typeof(UInt16?),           "0",                                    "en-GB",    (UInt16)0)]
        [DataRow(typeof(UInt16?),           "65535",                                "en-GB",    (UInt16)65535)]
        [DataRow(typeof(UInt16?),           "-1",                                   "en-GB",    null)]
        [DataRow(typeof(UInt16?),           "65536",                                "en-GB",    null)]
        [DataRow(typeof(UInt16?),           "a",                                    "en-GB",    null)]
        [DataRow(typeof(UInt16?),           "1.2",                                  "en-GB",    null)]
        [DataRow(typeof(UInt16?),           "1,2",                                  "en-GB",    null)]
        [DataRow(typeof(Int32),             null,                                   "en-GB",    null)]
        [DataRow(typeof(Int32),             "",                                     "en-GB",    null)]
        [DataRow(typeof(Int32),             "-2147483648",                          "en-GB",    -2147483648)]
        [DataRow(typeof(Int32),             "2147483647",                           "en-GB",    2147483647)]
        [DataRow(typeof(Int32),             "-2147483649",                          "en-GB",    null)]
        [DataRow(typeof(Int32),             "2147483648",                           "en-GB",    null)]
        [DataRow(typeof(Int32),             " 1 ",                                  "en-GB",    1)]
        [DataRow(typeof(Int32),             "-1",                                   "de-DE",    -1)]
        [DataRow(typeof(Int32),             "(1)",                                  "en-GB",    null)]
        [DataRow(typeof(Int32),             "1.2",                                  "en-GB",    null)]
        [DataRow(typeof(Int32),             "a",                                    "en-GB",    null)]
        [DataRow(typeof(Int32?),            null,                                   "en-GB",    null)]
        [DataRow(typeof(Int32?),            "",                                     "en-GB",    null)]
        [DataRow(typeof(Int32?),            "-2147483648",                          "en-GB",    -2147483648)]
        [DataRow(typeof(Int32?),            "2147483647",                           "en-GB",    2147483647)]
        [DataRow(typeof(Int32?),            "-2147483649",                          "en-GB",    null)]
        [DataRow(typeof(Int32?),            "2147483648",                           "en-GB",    null)]
        [DataRow(typeof(Int32?),            " 1 ",                                  "en-GB",    1)]
        [DataRow(typeof(Int32?),            "-1",                                   "de-DE",    -1)]
        [DataRow(typeof(Int32?),            "(1)",                                  "en-GB",    null)]
        [DataRow(typeof(Int32?),            "1.2",                                  "en-GB",    null)]
        [DataRow(typeof(Int32?),            "a",                                    "en-GB",    null)]
        [DataRow(typeof(UInt32),            null,                                   "en-GB",    null)]
        [DataRow(typeof(UInt32),            "",                                     "en-GB",    null)]
        [DataRow(typeof(UInt32),            "0",                                    "en-GB",    (UInt32)0)]
        [DataRow(typeof(UInt32),            "4294967295",                           "en-GB",    (UInt32)4294967295)]
        [DataRow(typeof(UInt32),            "-1",                                   "en-GB",    null)]
        [DataRow(typeof(UInt32),            "4294967296",                           "en-GB",    null)]
        [DataRow(typeof(UInt32),            " 1 ",                                  "en-GB",    (UInt32)1)]
        [DataRow(typeof(UInt32),            "1,2",                                  "de-DE",    null)]
        [DataRow(typeof(UInt32),            "a",                                    "en-GB",    null)]
        [DataRow(typeof(UInt32?),           null,                                   "en-GB",    null)]
        [DataRow(typeof(UInt32?),           "",                                     "en-GB",    null)]
        [DataRow(typeof(UInt32?),           "0",                                    "en-GB",    (UInt32)0)]
        [DataRow(typeof(UInt32?),           "4294967295",                           "en-GB",    (UInt32)4294967295)]
        [DataRow(typeof(UInt32?),           "-1",                                   "en-GB",    null)]
        [DataRow(typeof(UInt32?),           "4294967296",                           "en-GB",    null)]
        [DataRow(typeof(UInt32?),           " 1 ",                                  "en-GB",    (UInt32)1)]
        [DataRow(typeof(UInt32?),           "1,2",                                  "de-DE",    null)]
        [DataRow(typeof(UInt32?),           "a",                                    "en-GB",    null)]
        [DataRow(typeof(Int64),             null,                                   "en-GB",    null)]
        [DataRow(typeof(Int64),             "",                                     "en-GB",    null)]
        [DataRow(typeof(Int64),             "-9223372036854775808",                 "en-GB",    -9223372036854775808L)]
        [DataRow(typeof(Int64),             "9223372036854775807",                  "en-GB",    9223372036854775807L)]
        [DataRow(typeof(Int64),             "-9223372036854775809",                 "en-GB",    null)]
        [DataRow(typeof(Int64),             "9223372036854775808",                  "en-GB",    null)]
        [DataRow(typeof(Int64),             " 1 ",                                  "en-GB",    1L)]
        [DataRow(typeof(Int64),             "-1",                                   "de-DE",    -1L)]
        [DataRow(typeof(Int64),             "(1)",                                  "en-GB",    null)]
        [DataRow(typeof(Int64),             "1.2",                                  "en-GB",    null)]
        [DataRow(typeof(Int64),             "a",                                    "en-GB",    null)]
        [DataRow(typeof(Int64?),            null,                                   "en-GB",    null)]
        [DataRow(typeof(Int64?),            "",                                     "en-GB",    null)]
        [DataRow(typeof(Int64?),            "-9223372036854775808",                 "en-GB",    -9223372036854775808L)]
        [DataRow(typeof(Int64?),            "9223372036854775807",                  "en-GB",    9223372036854775807L)]
        [DataRow(typeof(Int64?),            "-9223372036854775809",                 "en-GB",    null)]
        [DataRow(typeof(Int64?),            "9223372036854775808",                  "en-GB",    null)]
        [DataRow(typeof(Int64?),            " 1 ",                                  "en-GB",    1L)]
        [DataRow(typeof(Int64?),            "-1",                                   "de-DE",    -1L)]
        [DataRow(typeof(Int64?),            "(1)",                                  "en-GB",    null)]
        [DataRow(typeof(Int64?),            "1.2",                                  "en-GB",    null)]
        [DataRow(typeof(Int64?),            "a",                                    "en-GB",    null)]
        [DataRow(typeof(UInt64),            null,                                   "en-GB",    null)]
        [DataRow(typeof(UInt64),            "",                                     "en-GB",    null)]
        [DataRow(typeof(UInt64),            "18446744073709551615",                 "en-GB",    18446744073709551615UL)]
        [DataRow(typeof(UInt64),            "-1",                                   "en-GB",    null)]
        [DataRow(typeof(UInt64),            "18446744073709551616",                 "en-GB",    null)]
        [DataRow(typeof(UInt64),            " 1 ",                                  "en-GB",    1UL)]
        [DataRow(typeof(UInt64),            "(1)",                                  "en-GB",    null)]
        [DataRow(typeof(UInt64),            "1,2",                                  "de-DE",    null)]
        [DataRow(typeof(UInt64),            "a",                                    "en-GB",    null)]
        [DataRow(typeof(UInt64?),           null,                                   "en-GB",    null)]
        [DataRow(typeof(UInt64?),           "",                                     "en-GB",    null)]
        [DataRow(typeof(UInt64?),           "18446744073709551615",                 "en-GB",    18446744073709551615UL)]
        [DataRow(typeof(UInt64?),           "-1",                                   "en-GB",    null)]
        [DataRow(typeof(UInt64?),           "18446744073709551616",                 "en-GB",    null)]
        [DataRow(typeof(UInt64?),           " 1 ",                                  "en-GB",    1UL)]
        [DataRow(typeof(UInt64?),           "(1)",                                  "en-GB",    null)]
        [DataRow(typeof(UInt64?),           "1,2",                                  "de-DE",    null)]
        [DataRow(typeof(UInt64?),           "a",                                    "en-GB",    null)]
        [DataRow(typeof(float),             null,                                   "en-GB",    null)]
        [DataRow(typeof(float),             "",                                     "en-GB",    null)]
        [DataRow(typeof(float),             "-3.402823E+38",                        "en-GB",    -3.402823E+38F)]
        [DataRow(typeof(float),             "3.402823E+38",                         "en-GB",    3.402823E+38F)]
        [DataRow(typeof(float),             "1.23",                                 "en-GB",    1.23F)]
        [DataRow(typeof(float),             "-1.23",                                "de-DE",    -1.23F)]
        [DataRow(typeof(float),             "1,23",                                 "en-GB",    null)]
        [DataRow(typeof(float),             "1,23",                                 "de-DE",    null)]
        [DataRow(typeof(float),             "1,230",                                "en-GB",    null)]
        [DataRow(typeof(float),             "1 230",                                "de-DE",    null)]
        [DataRow(typeof(float),             " 1 ",                                  "en-GB",    1F)]
        [DataRow(typeof(float),             "a",                                    "en-GB",    null)]
        [DataRow(typeof(float),             "(1)",                                  "en-GB",    null)]
        [DataRow(typeof(float?),            null,                                   "en-GB",    null)]
        [DataRow(typeof(float?),            "",                                     "en-GB",    null)]
        [DataRow(typeof(float?),            "-3.402823E+38",                        "en-GB",    -3.402823E+38F)]
        [DataRow(typeof(float?),            "3.402823E+38",                         "en-GB",    3.402823E+38F)]
        [DataRow(typeof(float?),            "1.23",                                 "en-GB",    1.23F)]
        [DataRow(typeof(float?),            "-1.23",                                "de-DE",    -1.23F)]
        [DataRow(typeof(float?),            "1,23",                                 "en-GB",    null)]
        [DataRow(typeof(float?),            "1,23",                                 "de-DE",    null)]
        [DataRow(typeof(float?),            "1,230",                                "en-GB",    null)]
        [DataRow(typeof(float?),            "1 230",                                "de-DE",    null)]
        [DataRow(typeof(float?),            " 1 ",                                  "en-GB",    1F)]
        [DataRow(typeof(float?),            "a",                                    "en-GB",    null)]
        [DataRow(typeof(float?),            "(1)",                                  "en-GB",    null)]
        [DataRow(typeof(double),            null,                                   "en-GB",    null)]
        [DataRow(typeof(double),            "",                                     "en-GB",    null)]
        [DataRow(typeof(double),            "-1.79769313486231E+308",               "en-GB",    -1.79769313486231E+308)]
        [DataRow(typeof(double),            "1.79769313486231E+308",                "en-GB",    1.79769313486231E+308)]
        [DataRow(typeof(double),            "1.23",                                 "en-GB",    1.23)]
        [DataRow(typeof(double),            "-1.23",                                "de-DE",    -1.23)]
        [DataRow(typeof(double),            "1,23",                                 "en-GB",    null)]
        [DataRow(typeof(double),            "1,23",                                 "de-DE",    null)]
        [DataRow(typeof(double),            "1,230",                                "en-GB",    null)]
        [DataRow(typeof(double),            "1 230",                                "de-DE",    null)]
        [DataRow(typeof(double),            " 1 ",                                  "en-GB",    1.0)]
        [DataRow(typeof(double),            "a",                                    "en-GB",    null)]
        [DataRow(typeof(double),            "(1)",                                  "en-GB",    null)]
        [DataRow(typeof(double?),           null,                                   "en-GB",    null)]
        [DataRow(typeof(double?),           "",                                     "en-GB",    null)]
        [DataRow(typeof(double?),           "-1.79769313486231E+308",               "en-GB",    -1.79769313486231E+308)]
        [DataRow(typeof(double?),           "1.79769313486231E+308",                "en-GB",    1.79769313486231E+308)]
        [DataRow(typeof(double?),           "1.23",                                 "en-GB",    1.23)]
        [DataRow(typeof(double?),           "-1.23",                                "de-DE",    -1.23)]
        [DataRow(typeof(double?),           "1,23",                                 "en-GB",    null)]
        [DataRow(typeof(double?),           "1,23",                                 "de-DE",    null)]
        [DataRow(typeof(double?),           "1,230",                                "en-GB",    null)]
        [DataRow(typeof(double?),           "1 230",                                "de-DE",    null)]
        [DataRow(typeof(double?),           " 1 ",                                  "en-GB",    1.0)]
        [DataRow(typeof(double?),           "a",                                    "en-GB",    null)]
        [DataRow(typeof(double?),           "(1)",                                  "en-GB",    null)]
        [DataRow(typeof(decimal),           null,                                   "en-GB", null)]
        [DataRow(typeof(decimal),           "",                                     "en-GB", null)]
        [DataRow(typeof(decimal),           "-79228162514264337593543950335",       "en-GB", "-79228162514264337593543950335")]
        [DataRow(typeof(decimal),           "79228162514264337593543950335",        "en-GB", "79228162514264337593543950335")]
        [DataRow(typeof(decimal),           "-79228162514264337593543950336",       "en-GB", null)]
        [DataRow(typeof(decimal),           "79228162514264337593543950336",        "en-GB", null)]
        [DataRow(typeof(decimal),           "1.23",                                 "en-GB", "1.23")]
        [DataRow(typeof(decimal),           "-1.23",                                "de-DE", "-1.23")]
        [DataRow(typeof(decimal),           "1,23",                                 "en-GB", null)]
        [DataRow(typeof(decimal),           "1,23",                                 "de-DE", null)]
        [DataRow(typeof(decimal),           "1,230",                                "en-GB", null)]
        [DataRow(typeof(decimal),           "1 230",                                "de-DE", null)]
        [DataRow(typeof(decimal),           " 1 ",                                  "en-GB", "1.0")]
        [DataRow(typeof(decimal),           "a",                                    "en-GB", null)]
        [DataRow(typeof(decimal),           "(1)",                                  "en-GB", null)]
        [DataRow(typeof(decimal?),          null,                                   "en-GB", null)]
        [DataRow(typeof(decimal?),          "",                                     "en-GB", null)]
        [DataRow(typeof(decimal?),          "-79228162514264337593543950335",       "en-GB", "-79228162514264337593543950335")]
        [DataRow(typeof(decimal?),          "79228162514264337593543950335",        "en-GB", "79228162514264337593543950335")]
        [DataRow(typeof(decimal?),          "-79228162514264337593543950336",       "en-GB", null)]
        [DataRow(typeof(decimal?),          "79228162514264337593543950336",        "en-GB", null)]
        [DataRow(typeof(decimal?),          "1.23",                                 "en-GB", "1.23")]
        [DataRow(typeof(decimal?),          "-1.23",                                "de-DE", "-1.23")]
        [DataRow(typeof(decimal?),          "1,23",                                 "en-GB", null)]
        [DataRow(typeof(decimal?),          "1,23",                                 "de-DE", null)]
        [DataRow(typeof(decimal?),          "1,230",                                "en-GB", null)]
        [DataRow(typeof(decimal?),          "1 230",                                "de-DE", null)]
        [DataRow(typeof(decimal?),          " 1 ",                                  "en-GB", "1.0")]
        [DataRow(typeof(decimal?),          "a",                                    "en-GB", null)]
        [DataRow(typeof(decimal?),          "(1)",                                  "en-GB", null)]
        [DataRow(typeof(DateTime),          null,                                   "en-GB", null)]
        [DataRow(typeof(DateTime),          "",                                     "en-GB", null)]
        [DataRow(typeof(DateTime),          "2019-01-30",                           "en-GB", "2019-01-30 Unspecified")]
        [DataRow(typeof(DateTime),          "2019-01-30",                           "en-US", "2019-01-30 Unspecified")]
        [DataRow(typeof(DateTime),          "01/02/2019",                           "en-GB", "2019-01-02 Unspecified")]
        [DataRow(typeof(DateTime),          "01/02/2019",                           "en-US", "2019-01-02 Unspecified")]
        [DataRow(typeof(DateTime),          "2019-01-30 13:14:15",                  "en-GB", "2019-01-30 13:14:15 Utc")]
        [DataRow(typeof(DateTime),          "2019-01-30 13:14:15",                  "en-US", "2019-01-30 13:14:15 Utc")]
        [DataRow(typeof(DateTime),          "2019-07-01T22:53:47+01:00",            "en-GB", "2019-07-01 22:53:47 Local")]
        [DataRow(typeof(DateTime),          "2019-07-01T22:53:47+01:00",            "en-GB", "2019-07-01 21:53:47 Utc")]
        [DataRow(typeof(DateTime),          "13:14:15",                             "en-GB", "today 13:14:15 Utc")]
        [DataRow(typeof(DateTime),          "13:14:15.123",                         "en-US", "today 13:14:15.123 Utc")]
        [DataRow(typeof(DateTime),          "today",                                "en-GB", null)]
        [DataRow(typeof(DateTime),          "20190130",                             "en-GB", null)]
        [DataRow(typeof(DateTime?),         null,                                   "en-GB", null)]
        [DataRow(typeof(DateTime?),         "",                                     "en-GB", null)]
        [DataRow(typeof(DateTime?),         "2019-01-30",                           "en-GB", "2019-01-30 Unspecified")]
        [DataRow(typeof(DateTime?),         "2019-01-30",                           "en-US", "2019-01-30 Unspecified")]
        [DataRow(typeof(DateTime?),         "01/02/2019",                           "en-GB", "2019-01-02 Unspecified")]
        [DataRow(typeof(DateTime?),         "01/02/2019",                           "en-US", "2019-01-02 Unspecified")]
        [DataRow(typeof(DateTime?),         "2019-01-30 13:14:15",                  "en-GB", "2019-01-30 13:14:15 Utc")]
        [DataRow(typeof(DateTime?),         "2019-01-30 13:14:15",                  "en-US", "2019-01-30 13:14:15 Utc")]
        [DataRow(typeof(DateTime?),         "2019-07-01T22:53:47+01:00",            "en-GB", "2019-07-01 22:53:47 Local")]
        [DataRow(typeof(DateTime?),         "2019-07-01T22:53:47+01:00",            "en-GB", "2019-07-01 21:53:47 Utc")]
        [DataRow(typeof(DateTime?),         "13:14:15",                             "en-GB", "today 13:14:15 Utc")]
        [DataRow(typeof(DateTime?),         "13:14:15.123",                         "en-US", "today 13:14:15.123 Utc")]
        [DataRow(typeof(DateTime?),         "today",                                "en-GB", null)]
        [DataRow(typeof(DateTime?),         "20190130",                             "en-GB", null)]
        [DataRow(typeof(DateTimeOffset),    null,                                   "en-GB", null)]
        [DataRow(typeof(DateTimeOffset),    "",                                     "en-GB", null)]
        [DataRow(typeof(DateTimeOffset),    "2019-01-02",                           "en-GB", "2019-01-02")]
        [DataRow(typeof(DateTimeOffset),    "2019-01-02",                           "en-US", "2019-01-02")]
        [DataRow(typeof(DateTimeOffset),    "01/02/2019",                           "en-GB", "2019-01-02")]
        [DataRow(typeof(DateTimeOffset),    "01/02/2019",                           "en-US", "2019-01-02")]
        [DataRow(typeof(DateTimeOffset),    "2019-01-30 13:14:15",                  "en-GB", "2019-01-30 13:14:15")]
        [DataRow(typeof(DateTimeOffset),    "2019-01-30 13:14:15.123",              "en-US", "2019-01-30 13:14:15.123")]
        [DataRow(typeof(DateTimeOffset),    "2019-07-01T22:53:47+01:00",            "en-GB", "2019-07-01 22:53:47 +60")]
        [DataRow(typeof(DateTimeOffset),    "2019-07-01T22:53:47-01:35",            "en-GB", "2019-07-01 22:53:47 -95")]
        [DataRow(typeof(DateTimeOffset),    "13:14:15",                             "en-GB", "today 13:14:15")]
        [DataRow(typeof(DateTimeOffset),    "today",                                "en-GB", null)]
        [DataRow(typeof(DateTimeOffset),    "20190130",                             "en-GB", null)]
        [DataRow(typeof(DateTimeOffset?),   null,                                   "en-GB", null)]
        [DataRow(typeof(DateTimeOffset?),   "",                                     "en-GB", null)]
        [DataRow(typeof(DateTimeOffset?),   "2019-01-02",                           "en-GB", "2019-01-02")]
        [DataRow(typeof(DateTimeOffset?),   "2019-01-02",                           "en-US", "2019-01-02")]
        [DataRow(typeof(DateTimeOffset?),   "01/02/2019",                           "en-GB", "2019-01-02")]
        [DataRow(typeof(DateTimeOffset?),   "01/02/2019",                           "en-US", "2019-01-02")]
        [DataRow(typeof(DateTimeOffset?),   "2019-01-30 13:14:15",                  "en-GB", "2019-01-30 13:14:15")]
        [DataRow(typeof(DateTimeOffset?),   "2019-01-30 13:14:15.123",              "en-US", "2019-01-30 13:14:15.123")]
        [DataRow(typeof(DateTimeOffset?),   "2019-07-01T22:53:47+01:00",            "en-GB", "2019-07-01 22:53:47 +60")]
        [DataRow(typeof(DateTimeOffset?),   "2019-07-01T22:53:47-01:35",            "en-GB", "2019-07-01 22:53:47 -95")]
        [DataRow(typeof(DateTimeOffset?),   "13:14:15",                             "en-GB", "today 13:14:15")]
        [DataRow(typeof(DateTimeOffset?),   "today",                                "en-GB", null)]
        [DataRow(typeof(DateTimeOffset?),   "20190130",                             "en-GB", null)]
        [DataRow(typeof(Guid),              null,                                   "en-GB", null)]
        [DataRow(typeof(Guid),              "",                                     "en-GB", null)]
        [DataRow(typeof(Guid),              "0c481493-72aa-4196-ac22-a27c14ec2d51", "en-GB", "0c481493-72aa-4196-ac22-a27c14ec2d51")]
        [DataRow(typeof(Guid),              "0C481493-72AA-4196-AC22-A27C14EC2D51", "en-GB", "0c481493-72aa-4196-ac22-a27c14ec2d51")]
        [DataRow(typeof(Guid),              "0c48149372aa4196ac22a27c14ec2d51",     "en-GB", "0c481493-72aa-4196-ac22-a27c14ec2d51")]
        [DataRow(typeof(Guid),              "0C48149372AA4196AC22A27C14EC2D51",     "en-GB", "0c481493-72aa-4196-ac22-a27c14ec2d51")]
        [DataRow(typeof(Guid),              "00000000-0000-0000-0000-000000000000", "en-GB", "00000000-0000-0000-0000-000000000000")]
        [DataRow(typeof(Guid),              "00000000000000000000000000000000",     "en-GB", "00000000-0000-0000-0000-000000000000")]
        [DataRow(typeof(Guid?),             null,                                   "en-GB", null)]
        [DataRow(typeof(Guid?),             "",                                     "en-GB", null)]
        [DataRow(typeof(Guid?),             "0c481493-72aa-4196-ac22-a27c14ec2d51", "en-GB", "0c481493-72aa-4196-ac22-a27c14ec2d51")]
        [DataRow(typeof(Guid?),             "0C481493-72AA-4196-AC22-A27C14EC2D51", "en-GB", "0c481493-72aa-4196-ac22-a27c14ec2d51")]
        [DataRow(typeof(Guid?),             "0c48149372aa4196ac22a27c14ec2d51",     "en-GB", "0c481493-72aa-4196-ac22-a27c14ec2d51")]
        [DataRow(typeof(Guid?),             "0C48149372AA4196AC22A27C14EC2D51",     "en-GB", "0c481493-72aa-4196-ac22-a27c14ec2d51")]
        [DataRow(typeof(Guid?),             "00000000-0000-0000-0000-000000000000", "en-GB", "00000000-0000-0000-0000-000000000000")]
        [DataRow(typeof(Guid?),             "00000000000000000000000000000000",     "en-GB", "00000000-0000-0000-0000-000000000000")]
        [DataRow(typeof(byte[]),            "0x01",                                 "en-GB", new byte[] { 211, 29, 53 })]   // defaults to MIME-64 strings
        [DataRow(typeof(byte[]),            "DwoL",                                 "en-GB", new byte[] { 0x0f, 0x0a, 0x0b })]
        public void ParseType_Parses_Different_Types_Correctly(Type type, string text, string culture, object rawExpected)
        {
            using(new CultureSwap(culture)) {
                var actual = Parser.ParseType(type, text);
                AssertParseTypeOutcome(type, rawExpected, actual);
            }
        }

        [TestMethod]
        [DataRow(typeof(byte[]), "0x01", ParserOptions.ByteArrayFormat.HexString, "en-GB", new byte[] { 1 })]
        [DataRow(typeof(byte[]), "0102", ParserOptions.ByteArrayFormat.HexString, "en-GB", new byte[] { 1, 2 })]
        [DataRow(typeof(byte[]), "0x01", ParserOptions.ByteArrayFormat.Mime64,    "en-GB", new byte[] { 211, 29, 53 })]
        public void ParseType_Honours_ParserOptions_For_Byte_Array(Type type, string text, ParserOptions.ByteArrayFormat byteArrayFormat, string culture, object rawExpected)
        {
            using(new CultureSwap(culture)) {
                var options = new ParserOptions() {
                    ByteArray = byteArrayFormat,
                };
                var actual = Parser.ParseType(type, text, options);
                AssertParseTypeOutcome(type, rawExpected, actual);
            }
        }

        private static void AssertParseTypeOutcome(Type type, object rawExpected, object actual)
        {
            if(rawExpected == null) {
                Assert.IsNull(actual);
            } else if(type == typeof(byte[])) {
                var expected = (byte[])rawExpected;
                Assert.IsTrue(expected.SequenceEqual((byte[])actual));
            } else {
                object expected;

                if(type == typeof(DateTime) || type == typeof(DateTime?)) {
                    expected = ExpectedDateTime((string)rawExpected, () => {
                        if(((DateTime?)actual).Value.Kind == DateTimeKind.Local) {
                            actual = ((DateTime?)actual).Value.ToUniversalTime();
                        }
                    });
                } else if(type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?)) {
                    expected = ExpectedDateTimeOffset((string)rawExpected);
                } else if(type == typeof(Guid) || type == typeof(Guid?)) {
                    expected = Guid.Parse((string)rawExpected);
                } else {
                    var convertType = type;
                    if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                        convertType = type.GetGenericArguments()[0];
                    }
                    expected = Convert.ChangeType(rawExpected, convertType, CultureInfo.InvariantCulture);
                }

                Assert.AreEqual(expected, actual);
            }
        }
    }
}
