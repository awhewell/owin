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
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using AWhewell.Owin.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Test.AWhewell.Owin.Utility
{
    [TestClass]
    public class Parser_Tests
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
                    var expected = DataRowParser.DateTime(expectedString);
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
                    var expected = DataRowParser.DateTimeOffset(expectedString);
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
        public void ParseByteArray_Parses_String_In_MIME64_Format(string input, string culture, byte[] expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Parser.ParseByteArray(input);

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
        [DataRow(typeof(bool?),             "true",                                 "en-GB",    true)]
        [DataRow(typeof(bool?),             "false",                                "en-GB",    false)]
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
                    expected = DataRowParser.DateTime((string)rawExpected);
                } else if(type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?)) {
                    expected = DataRowParser.DateTimeOffset((string)rawExpected);
                } else if(type == typeof(Guid) || type == typeof(Guid?)) {
                    expected = DataRowParser.Guid((string)rawExpected);
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

        [TestMethod]
        [DataRow(typeof(bool),              nameof(Parser.ParseBool),           "false",                                false,                                  true)]
        [DataRow(typeof(byte),              nameof(Parser.ParseByte),           "255",                                  (byte)255,                              (byte)127)]
        [DataRow(typeof(char),              nameof(Parser.ParseChar),           "1",                                    '1',                                    '2')]
        [DataRow(typeof(Int16),             nameof(Parser.ParseInt16),          "-32768",                               (short)-32768,                          (short)32767)]
        [DataRow(typeof(UInt16),            nameof(Parser.ParseUInt16),         "32767",                                (ushort)32767,                          (ushort)7892)]
        [DataRow(typeof(Int32),             nameof(Parser.ParseInt32),          "-2147483648",                          -2147483648,                            2147483647)]
        [DataRow(typeof(UInt32),            nameof(Parser.ParseUInt32),         "4294967295",                           4294967295U,                            88U)]
        [DataRow(typeof(Int64),             nameof(Parser.ParseInt64),          "-9223372036854775808",                 -9223372036854775808L,                  9223372036854775807L)]
        [DataRow(typeof(UInt64),            nameof(Parser.ParseUInt64),         "9223372036854775807",                  9223372036854775807UL,                  2382UL)]
        [DataRow(typeof(float),             nameof(Parser.ParseFloat),          "12.342",                               12.342F,                                98.12F)]
        [DataRow(typeof(double),            nameof(Parser.ParseDouble),         "12.342",                               12.342,                                 98.12)]
        [DataRow(typeof(decimal),           nameof(Parser.ParseDecimal),        "12.342",                               "12.342",                               "98.12")]
        [DataRow(typeof(DateTime),          nameof(Parser.ParseDateTime),       "2019-01-02",                           "2019-01-02",                           "1816-04-21")]
        [DataRow(typeof(DateTimeOffset),    nameof(Parser.ParseDateTimeOffset), "2019-01-02",                           "2019-01-02",                           "1816-04-21")]
        [DataRow(typeof(Guid),              nameof(Parser.ParseGuid),           "48cd065e-f78d-465b-af07-49e3b1b7cc92", "48cd065e-f78d-465b-af07-49e3b1b7cc92", "c08f84fd-c572-4bba-94c5-f22804442e62")]
        [DataRow(typeof(byte[]),            nameof(Parser.ParseByteArray),      "DwoL",                                 new byte[] { 0x0f, 0x0a, 0x0b },        new byte[] { 99, 100 })]
        public void Parse_Explicit_Type_Methods_Use_TypeResolver_When_Supplied(Type valueType, string parserMethodName, string text, object expectedNormalRaw, object expectedCustomRaw)
        {
            Mock mockParser = null;
            var tryParseResult = true;

            void createMock<T>(T expectedValue)
            {
                var mock = MockHelper.CreateMock<ITypeParser<T>>();
                mock.Setup(r => r.TryParse(text, out expectedValue)).Returns(() => tryParseResult);
                mockParser = mock;
            }

            Func<string, TypeParserResolver, object> callParser;
            var expectedNormal = DataRowParser.ConvertExpected(valueType, expectedNormalRaw);
            var expectedCustom = DataRowParser.ConvertExpected(valueType, expectedCustomRaw);

            var parserMethod = typeof(Parser)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Single(r => r.Name == parserMethodName && r.GetParameters().Length == 2);

            callParser = (t, r) => parserMethod.Invoke(null, new object[] { t, r });

            if(valueType == typeof(bool))                   createMock<bool>((bool)expectedCustom);
            else if(valueType == typeof(byte))              createMock<byte>((byte)expectedCustom);
            else if(valueType == typeof(char))              createMock<char>((char)expectedCustom);
            else if(valueType == typeof(Int16))             createMock<short>((short)expectedCustom);
            else if(valueType == typeof(UInt16))            createMock<ushort>((ushort)expectedCustom);
            else if(valueType == typeof(Int32))             createMock<int>((int)expectedCustom);
            else if(valueType == typeof(UInt32))            createMock<uint>((uint)expectedCustom);
            else if(valueType == typeof(Int64))             createMock<long>((long)expectedCustom);
            else if(valueType == typeof(UInt64))            createMock<ulong>((ulong)expectedCustom);
            else if(valueType == typeof(float))             createMock<float>((float)expectedCustom);
            else if(valueType == typeof(double))            createMock<double>((double)expectedCustom);
            else if(valueType == typeof(decimal))           createMock<decimal>((decimal)expectedCustom);
            else if(valueType == typeof(DateTime))          createMock<DateTime>((DateTime)expectedCustom);
            else if(valueType == typeof(DateTimeOffset))    createMock<DateTimeOffset>((DateTimeOffset)expectedCustom);
            else if(valueType == typeof(Guid))              createMock<Guid>((Guid)expectedCustom);
            else if(valueType == typeof(byte[]))            createMock<byte[]>((byte[])expectedCustom);
            else                                            throw new NotImplementedException();

            void areEqual(object expected, object actual)
            {
                var expectedCollection = expected as IList;
                var actualCollection = actual as IList;

                if(expectedCollection == null && actualCollection == null) {
                    Assert.AreEqual(expected, actual);
                } else {
                    if(expectedCollection == null) {
                        Assert.IsNull(actual);
                    } else {
                        Assert.AreEqual(expectedCollection.Count, actualCollection.Count);
                        for(var i = 0;i < expectedCollection.Count;++i) {
                            Assert.AreEqual(expectedCollection[i], actualCollection[i]);
                        }
                    }
                }
            }

            // Null type resolver should call normal parser
            areEqual(expectedNormal, callParser(text, null));

            // Type resolver with no parser for type should call normal parser
            var emptyTypeResolver = new TypeParserResolver();
            areEqual(expectedNormal, callParser(text, emptyTypeResolver));

            var typeResolver = new TypeParserResolver((ITypeParser)mockParser.Object);

            // If custom parser returns true then use the parsed value
            tryParseResult = true;
            areEqual(expectedCustom, callParser(text, typeResolver));

            // If custom parser returns false then it cannot be parsed
            tryParseResult = false;
            areEqual(null, callParser(text, typeResolver));
        }

        [TestMethod]
        [DataRow(typeof(bool),              typeof(Mock<ITypeParser<bool>>))]
        [DataRow(typeof(byte),              typeof(Mock<ITypeParser<byte>>))]
        [DataRow(typeof(char),              typeof(Mock<ITypeParser<char>>))]
        [DataRow(typeof(Int16),             typeof(Mock<ITypeParser<Int16>>))]
        [DataRow(typeof(UInt16),            typeof(Mock<ITypeParser<UInt16>>))]
        [DataRow(typeof(Int32),             typeof(Mock<ITypeParser<Int32>>))]
        [DataRow(typeof(UInt32),            typeof(Mock<ITypeParser<UInt32>>))]
        [DataRow(typeof(Int64),             typeof(Mock<ITypeParser<Int64>>))]
        [DataRow(typeof(UInt64),            typeof(Mock<ITypeParser<UInt64>>))]
        [DataRow(typeof(float),             typeof(Mock<ITypeParser<float>>))]
        [DataRow(typeof(double),            typeof(Mock<ITypeParser<double>>))]
        [DataRow(typeof(decimal),           typeof(Mock<ITypeParser<decimal>>))]
        [DataRow(typeof(DateTime),          typeof(Mock<ITypeParser<DateTime>>))]
        [DataRow(typeof(DateTimeOffset),    typeof(Mock<ITypeParser<DateTimeOffset>>))]
        [DataRow(typeof(Guid),              typeof(Mock<ITypeParser<Guid>>))]
        [DataRow(typeof(byte[]),            typeof(Mock<ITypeParser<byte[]>>))]
        public void ParseType_Uses_TypeResolver_If_Supplied(Type valueType, Type mockType)
        {
            var mockParser = MockHelper.CreateMock(mockType);
            var mockTypeParser = (ITypeParser)mockParser.Object;

            var resolver = new TypeParserResolver(mockTypeParser);

            Parser.ParseType(valueType, "text", resolver);

            var tryParseCalls = mockParser
                .Invocations
                .Where(r => r.Method.Name == nameof(ITypeParser<int>.TryParse))
                .ToArray();

            Assert.AreEqual(1, tryParseCalls.Length);
            Assert.AreEqual(2, tryParseCalls[0].Arguments.Count);
            Assert.AreEqual("text", tryParseCalls[0].Arguments[0]);
        }

        [TestMethod]
        [DataRow(null,          HttpMethod.Unknown)]
        [DataRow("",            HttpMethod.Unknown)]
        [DataRow("connect",     HttpMethod.Connect)]
        [DataRow("CONNECT",     HttpMethod.Connect)]
        [DataRow(" Connect ",   HttpMethod.Connect)]
        [DataRow("Delete",      HttpMethod.Delete)]
        [DataRow("Get",         HttpMethod.Get)]
        [DataRow("Head",        HttpMethod.Head)]
        [DataRow("Options",     HttpMethod.Options)]
        [DataRow("Patch",       HttpMethod.Patch)]
        [DataRow("Post",        HttpMethod.Post)]
        [DataRow("Put",         HttpMethod.Put)]
        [DataRow("Trace",       HttpMethod.Trace)]
        public void ParseHttpMethod_Returns_Correct_Enum_Value(string text, HttpMethod expected)
        {
            var actual = Parser.ParseHttpMethod(text);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow(null,          HttpProtocol.Unknown)]
        [DataRow("",            HttpProtocol.Unknown)]
        [DataRow("http/0.9",    HttpProtocol.Http0_9)]
        [DataRow(" HTTP/0.9 ",  HttpProtocol.Http0_9)]
        [DataRow("HTTP/0.9",    HttpProtocol.Http0_9)]
        [DataRow("HTTP/1.0",    HttpProtocol.Http1_0)]
        [DataRow("HTTP/1.1",    HttpProtocol.Http1_1)]
        [DataRow("HTTP/2.0",    HttpProtocol.Http2_0)]
        [DataRow("HTTP/3.0",    HttpProtocol.Http3_0)]
        public void ParseHttpProtocol_Returns_Correct_Enum_Value(string text, HttpProtocol expected)
        {
            var actual = Parser.ParseHttpProtocol(text);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow(null,      HttpScheme.Unknown)]
        [DataRow("",        HttpScheme.Unknown)]
        [DataRow("HTTP",    HttpScheme.Http)]
        [DataRow(" http ",  HttpScheme.Http)]
        [DataRow("http",    HttpScheme.Http)]
        [DataRow("https",   HttpScheme.Https)]
        public void ParseHttpScheme_Returns_Correct_Enum_Value(string text, HttpScheme expected)
        {
            var actual = Parser.ParseHttpScheme(text);

            Assert.AreEqual(expected, actual);
        }
    }
}
