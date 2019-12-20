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
    public class Formatter_Tests
    {
        [TestMethod]
        [DataRow(null,  "en-GB", null)]
        [DataRow("",    "en-GB", "")]
        [DataRow("Abc", "en-GB", "Abc")]
        public void FormatString_Formats_String(string input, string culture, string expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Formatter.FormatString(input);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [DataRow(null,  "en-GB", null)]
        [DataRow(true,  "en-GB", "true")]
        [DataRow(false, "en-GB", "false")]
        [DataRow(true,  "en-DE", "true")]
        [DataRow(false, "en-DE", "false")]
        public void FormatBool_Formats_Bool(bool? input, string culture, string expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Formatter.FormatBool(input);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [DataRow(null,      "en-GB", null)]
        [DataRow((byte)1,   "en-GB", "1")]
        [DataRow((byte)255, "en-GB", "255")]
        [DataRow((byte)255, "en-DE", "255")]
        public void FormatByte_Formats_Byte(byte? input, string culture, string expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Formatter.FormatByte(input);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [DataRow(null, "en-GB", null)]
        [DataRow('a',  "en-GB", "a")]
        [DataRow('a',  "en-DE", "a")]
        public void FormatChar_Formats_Char(char? input, string culture, string expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Formatter.FormatChar(input);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [DataRow(null,          "en-GB", null)]
        [DataRow((short)-32768, "en-GB", "-32768")]
        [DataRow((short)32767,  "en-GB", "32767")]
        [DataRow((short)-32768, "en-DE", "-32768")]
        public void FormatInt16_Formats_Short(short? input, string culture, string expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Formatter.FormatInt16(input);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [DataRow(null,          "en-GB", null)]
        [DataRow((ushort)65535, "en-GB", "65535")]
        [DataRow((ushort)65535, "en-DE", "65535")]
        public void FormatUInt16_Formats_Unsigned_Short(ushort? input, string culture, string expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Formatter.FormatUInt16(input);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [DataRow(null,          "en-GB", null)]
        [DataRow(-2147483648,   "en-GB", "-2147483648")]
        [DataRow(2147483647,    "en-GB", "2147483647")]
        [DataRow(-2147483648,   "en-DE", "-2147483648")]
        public void FormatInt32_Formats_Int(int? input, string culture, string expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Formatter.FormatInt32(input);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [DataRow(null,          "en-GB", null)]
        [DataRow(4294967295U,   "en-GB", "4294967295")]
        [DataRow(4294967295U,   "en-DE", "4294967295")]
        public void FormatUInt32_Formats_Unsigned_Int(uint? input, string culture, string expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Formatter.FormatUInt32(input);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [DataRow(null,                  "en-GB", null)]
        [DataRow(-9223372036854775808L, "en-GB", "-9223372036854775808")]
        [DataRow(9223372036854775807L,  "en-GB", "9223372036854775807")]
        [DataRow(-9223372036854775808L, "en-DE", "-9223372036854775808")]
        public void FormatInt64_Formats_Long(long? input, string culture, string expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Formatter.FormatInt64(input);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [DataRow(null,                  "en-GB", null)]
        [DataRow(18446744073709551615L, "en-GB", "18446744073709551615")]
        [DataRow(18446744073709551615L, "en-DE", "18446744073709551615")]
        public void FormatUInt64_Formats_Unsigned_Long(ulong? input, string culture, string expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Formatter.FormatUInt64(input);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [DataRow(null,           "en-GB", null)]
        [DataRow(-3.402823E+38F, "en-GB", "-3.402823E+38")]
        [DataRow(3.402823E+38F,  "en-GB", "3.402823E+38")]
        [DataRow(1234.23F,       "en-GB", "1234.23")]
        [DataRow(-1234.23F,      "en-GB", "-1234.23")]
        [DataRow(1234.23F,       "en-DE", "1234.23")]
        [DataRow(-1234.23F,      "en-DE", "-1234.23")]
        public void FormatFloat_Formats_Float(float? input, string culture, string expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Formatter.FormatFloat(input);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [DataRow(null,                   "en-GB", null)]
        [DataRow(-1.79769313486231E+308, "en-GB", "-1.79769313486231E+308")]
        [DataRow(1.79769313486231E+308,  "en-GB", "1.79769313486231E+308")]
        [DataRow(1234.23,                "en-GB", "1234.23")]
        [DataRow(-1234.23,               "en-GB", "-1234.23")]
        [DataRow(1234.23,                "en-DE", "1234.23")]
        [DataRow(-1234.23,               "en-DE", "-1234.23")]
        public void FormatDouble_Formats_Double(double? input, string culture, string expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Formatter.FormatDouble(input);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [DataRow(null,                             "en-GB", null)]
        [DataRow("-79228162514264337593543950335", "en-GB", "-79228162514264337593543950335")]
        [DataRow("79228162514264337593543950335",  "en-GB", "79228162514264337593543950335")]
        [DataRow("1234.23",                        "en-GB", "1234.23")]
        [DataRow("-1234.23",                       "en-GB", "-1234.23")]
        [DataRow("1234.23",                        "en-DE", "1234.23")]
        [DataRow("-1234.23",                       "en-DE", "-1234.23")]
        public void FormatDecimal_Formats_Decimal(string inputText, string culture, string expected)
        {
            using(new CultureSwap(culture)) {
                var input = DataRowParser.Decimal(inputText);
                var actual = Formatter.FormatDecimal(input);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [DataRow(null,                                  "en-GB", null)]
        [DataRow("2019-07-14 21:32:43.567 unspecified", "en-GB", "2019-07-14T21:32:43.567")]
        [DataRow("2019-07-14 21:32:43.567 local",       "en-GB", "2019-07-14T21:32:43.567+01:00")]
        [DataRow("2019-07-14 21:32:43.567 utc",         "en-GB", "2019-07-14T21:32:43.567Z")]
        public void FormatDateTime_Formats_DateTime(string inputText, string culture, string expected)
        {
            using(new CultureSwap(culture)) {
                var input = DataRowParser.DateTime(inputText);
                var actual = Formatter.FormatDateTime(input);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [DataRow(null,                            "en-GB", null)]
        [DataRow("2019-07-14 21:32:43.567 +0100", "en-GB", "2019-07-14T21:32:43.567+01:00")]
        [DataRow("2019-07-14 21:32:43.567 -0130", "en-GB", "2019-07-14T21:32:43.567-01:30")]
        [DataRow("2019-07-14 21:32:43.567 +0000", "en-GB", "2019-07-14T21:32:43.567Z")]
        public void FormatDateTimeOffset_Formats_DateTime(string inputText, string culture, string expected)
        {
            using(new CultureSwap(culture)) {
                var input = DataRowParser.DateTimeOffset(inputText);
                var actual = Formatter.FormatDateTimeOffset(input);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [DataRow(null,                                   "en-GB", null)]
        [DataRow("0c481493-72aa-4196-ac22-a27c14ec2d51", "en-GB", "0c481493-72aa-4196-ac22-a27c14ec2d51")]
        [DataRow("00000000-0000-0000-0000-000000000000", "en-GB", "00000000-0000-0000-0000-000000000000")]
        public void FormatGuid_Formats_Guid(string inputText, string culture, string expected)
        {
            using(new CultureSwap(culture)) {
                var input = DataRowParser.Guid(inputText);
                var actual = Formatter.FormatGuid(input);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [DataRow(null,                  "en-GB", null)]
        [DataRow(new byte[0],           "en-GB", "")]
        [DataRow(new byte[] { 1, 2 },   "en-GB", "AQI=")]
        public void FormatByteArray_Formats_ByteArray(byte[] input, string culture, string expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Formatter.FormatByteArray(input);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [DataRow(null,                  "en-GB", null)]
        [DataRow(new byte[0],           "en-GB", "")]
        [DataRow(new byte[] { 1, 2 },   "en-GB", "0102")]
        public void FormatHexBytes_Formats_ByteArray(byte[] input, string culture, string expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Formatter.FormatHexBytes(input);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [DataRow(null,                  "en-GB", null)]
        [DataRow(new byte[0],           "en-GB", "")]
        [DataRow(new byte[] { 1, 2 },   "en-GB", "AQI=")]
        public void FormatMime64Bytes_Formats_ByteArray(byte[] input, string culture, string expected)
        {
            using(new CultureSwap(culture)) {
                var actual = Formatter.FormatMime64Bytes(input);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void FormatType_Without_Resolver_Returns_Null_If_Passed_Null()
        {
            Assert.IsNull(Formatter.FormatType(null));
        }

        [TestMethod]
        [DataRow(typeof(string),         "Ab",                                   "en-GB", "Ab")]
        [DataRow(typeof(bool),           "True",                                 "en-GB", "true")]
        [DataRow(typeof(byte),           "255",                                  "en-GB", "255")]
        [DataRow(typeof(char),           "a",                                    "en-GB", "a")]
        [DataRow(typeof(Int16),          "-32768",                               "en-GB", "-32768")]
        [DataRow(typeof(Int16),          "-32768",                               "en-DE", "-32768")]
        [DataRow(typeof(UInt16),         "65535",                                "en-GB", "65535")]
        [DataRow(typeof(UInt16),         "65535",                                "en-DE", "65535")]
        [DataRow(typeof(Int32),          "-2147483648",                          "en-GB", "-2147483648")]
        [DataRow(typeof(Int32),          "-2147483648",                          "en-DE", "-2147483648")]
        [DataRow(typeof(UInt32),         "4294967295",                           "en-GB", "4294967295")]
        [DataRow(typeof(UInt32),         "4294967295",                           "en-DE", "4294967295")]
        [DataRow(typeof(Int64),          "-9223372036854775808",                 "en-GB", "-9223372036854775808")]
        [DataRow(typeof(Int64),          "-9223372036854775808",                 "en-DE", "-9223372036854775808")]
        [DataRow(typeof(UInt64),         "18446744073709551615",                 "en-GB", "18446744073709551615")]
        [DataRow(typeof(UInt64),         "18446744073709551615",                 "en-DE", "18446744073709551615")]
        [DataRow(typeof(float),          "-3.402823E+38",                        "en-GB", "-3.402823E+38")]
        [DataRow(typeof(float),          "-1234.56",                             "en-DE", "-1234.56")]
        [DataRow(typeof(double),         "-1.79769313486231E+308",               "en-GB", "-1.79769313486231E+308")]
        [DataRow(typeof(double),         "-1234.56",                             "en-DE", "-1234.56")]
        [DataRow(typeof(decimal),        "-79228162514264337593543950335",       "en-GB", "-79228162514264337593543950335")]
        [DataRow(typeof(decimal),        "-79228162514264337593543950335",       "en-DE", "-79228162514264337593543950335")]
        [DataRow(typeof(DateTime),       "2019-07-01 21:53:47.123 Utc",          "en-GB", "2019-07-01T21:53:47.123Z")]
        [DataRow(typeof(DateTimeOffset), "2019-07-01 21:53:47.123 +0100",        "en-GB", "2019-07-01T21:53:47.123+01:00")]
        [DataRow(typeof(Guid),           "0c481493-72aa-4196-ac22-a27c14ec2d51", "en-GB", "0c481493-72aa-4196-ac22-a27c14ec2d51")]
        [DataRow(typeof(byte[]),         "0102",                                 "en-GB", "AQI=")]
        public void FormatType_Without_Resolver_Formats_Objects(Type objectType, string inputText, string culture, string expected)
        {
            var inputValue = DataRowParser.ConvertExpected(objectType, inputText);

            using(new CultureSwap(culture)) {
                var actual = Formatter.FormatType(inputValue);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [DataRow(typeof(string),            nameof(Formatter.FormatString),         "Diskopunk",                            "Diskopunk",                            "Soaked")]
        [DataRow(typeof(bool),              nameof(Formatter.FormatBool),           "false",                                false,                                  true)]
        [DataRow(typeof(byte),              nameof(Formatter.FormatByte),           "255",                                  (byte)255,                              (byte)127)]
        [DataRow(typeof(char),              nameof(Formatter.FormatChar),           "1",                                    '1',                                    '2')]
        [DataRow(typeof(Int16),             nameof(Formatter.FormatInt16),          "-32768",                               (short)-32768,                          (short)32767)]
        [DataRow(typeof(UInt16),            nameof(Formatter.FormatUInt16),         "32767",                                (ushort)32767,                          (ushort)7892)]
        [DataRow(typeof(Int32),             nameof(Formatter.FormatInt32),          "-2147483648",                          -2147483648,                            2147483647)]
        [DataRow(typeof(UInt32),            nameof(Formatter.FormatUInt32),         "4294967295",                           4294967295U,                            88U)]
        [DataRow(typeof(Int64),             nameof(Formatter.FormatInt64),          "-9223372036854775808",                 -9223372036854775808L,                  9223372036854775807L)]
        [DataRow(typeof(UInt64),            nameof(Formatter.FormatUInt64),         "9223372036854775807",                  9223372036854775807UL,                  2382UL)]
        [DataRow(typeof(float),             nameof(Formatter.FormatFloat),          "12.342",                               12.342F,                                98.12F)]
        [DataRow(typeof(double),            nameof(Formatter.FormatDouble),         "12.342",                               12.342,                                 98.12)]
        [DataRow(typeof(decimal),           nameof(Formatter.FormatDecimal),        "12.342",                               "12.342",                               "98.12")]
        [DataRow(typeof(DateTime),          nameof(Formatter.FormatDateTime),       "2019-01-02",                           "2019-01-02",                           "1816-04-21")]
        [DataRow(typeof(DateTimeOffset),    nameof(Formatter.FormatDateTimeOffset), "2019-01-02",                           "2019-01-02",                           "1816-04-21")]
        [DataRow(typeof(Guid),              nameof(Formatter.FormatGuid),           "48cd065e-f78d-465b-af07-49e3b1b7cc92", "48cd065e-f78d-465b-af07-49e3b1b7cc92", "c08f84fd-c572-4bba-94c5-f22804442e62")]
        [DataRow(typeof(byte[]),            nameof(Formatter.FormatByteArray),      "DwoL",                                 new byte[] { 0x0f, 0x0a, 0x0b },        new byte[] { 99, 100 })]
        public void FormatType_With_Resolver_Uses_Resolver_When_Supplied(Type valueType, string formatterMethodName, string text, object expectedNormalRaw, object expectedCustomRaw)
        {
            throw new NotImplementedException();
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
                .Single(r => r.Name == formatterMethodName && r.GetParameters().Length == 2);

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
            else if(valueType == typeof(string))            createMock<string>((string)expectedCustom);
            else                                            throw new NotImplementedException();

            // Null type resolver should call normal parser
            Assertions.AreEqual(expectedNormal, callParser(text, null));

            // Type resolver with no parser for type should call normal parser
            var emptyTypeResolver = new TypeParserResolver();
            Assertions.AreEqual(expectedNormal, callParser(text, emptyTypeResolver));

            var typeResolver = new TypeParserResolver((ITypeParser)mockParser.Object);

            // If custom parser returns true then use the parsed value
            tryParseResult = true;
            Assertions.AreEqual(expectedCustom, callParser(text, typeResolver));

            // If custom parser returns false then it cannot be parsed
            tryParseResult = false;
            Assertions.AreEqual(null, callParser(text, typeResolver));
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
        public void FormatType_Uses_TypeResolver_If_Supplied(Type valueType, Type mockType)
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
        public void FormatHttpMethod_Returns_Correct_Enum_Value(string text, HttpMethod expected)
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
        public void FormatHttpProtocol_Returns_Correct_Enum_Value(string text, HttpProtocol expected)
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
        public void FormatHttpScheme_Returns_Correct_Enum_Value(string text, HttpScheme expected)
        {
            var actual = Parser.ParseHttpScheme(text);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow(null,                                  MediaType.Unknown)]
        [DataRow("",                                    MediaType.Unknown)]
        [DataRow("application/javascript",              MediaType.JavaScript)]
        [DataRow("APPLICATION/JAVASCRIPT",              MediaType.JavaScript)]
        [DataRow("application/json",                    MediaType.Json)]
        [DataRow("multipart/form-data",                 MediaType.MultipartForm)]
        [DataRow("text/plain",                          MediaType.PlainText)]
        [DataRow("application/x-www-form-urlencoded",   MediaType.UrlEncodedForm)]
        [DataRow("application/xml",                     MediaType.Xml)]
        [DataRow("text/xml",                            MediaType.Xml)]
        public void FormatMediaType_Returns_Correct_Enum_Value(string text, MediaType expected)
        {
            var actual = Parser.ParseMediaType(text);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow(null,              "utf-8")]
        [DataRow("",                "utf-8")]
        [DataRow("garbage",         null)]
        [DataRow("UTF-8",           "utf-8")]
        [DataRow("utf-8",           "utf-8")]
        [DataRow("csUTF8",          "utf-8")]
        [DataRow("UTF-7",           "utf-7")]
        [DataRow("utf-7",           "utf-7")]
        [DataRow("csUTF7",          "utf-7")]
        [DataRow("ASCII",           "us-ascii")]
        [DataRow("us",              "us-ascii")]
        [DataRow("Unicode",         "utf-16")]
        [DataRow("UTF-16",          "utf-16")]
        [DataRow("csUTF16",         "utf-16")]
        [DataRow("UTF-16LE",        "utf-16")]
        [DataRow("csUTF16LE",       "utf-16")]
        [DataRow("UTF-16BE",        "utf-16BE")]
        [DataRow("csUTF16BE",       "utf-16BE")]
        [DataRow("UTF-32",          "utf-32")]
        [DataRow("csUTF32",         "utf-32")]
        [DataRow("UTF-32LE",        "utf-32")]
        [DataRow("csUTF32LE",       "utf-32")]
        [DataRow("UTF-32BE",        "utf-32BE")]
        [DataRow("csUTF32BE",       "utf-32BE")]
        [DataRow("iso-8859-1",      "iso-8859-1")]
        [DataRow("latin1",          "iso-8859-1")]
        public void FormatCharset_Returns_Correct_Encoding(string charset, string expectedWebName)
        {
            var actual = Parser.ParseCharset(charset);

            if(expectedWebName == null) {
                Assert.IsNull(actual);
            } else {
                Assert.AreEqual(expectedWebName, actual.WebName);
            }
        }
    }
}
