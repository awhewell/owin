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
        [DataRow(typeof(string),            nameof(Formatter.FormatString),          "Diskopunk")]
        [DataRow(typeof(bool),              nameof(Formatter.FormatBool),            false)]
        [DataRow(typeof(byte),              nameof(Formatter.FormatByte),            (byte)255)]
        [DataRow(typeof(char),              nameof(Formatter.FormatChar),            '1')]
        [DataRow(typeof(Int16),             nameof(Formatter.FormatInt16),           (short)-32768)]
        [DataRow(typeof(UInt16),            nameof(Formatter.FormatUInt16),          (ushort)32767)]
        [DataRow(typeof(Int32),             nameof(Formatter.FormatInt32),           -2147483648)]
        [DataRow(typeof(UInt32),            nameof(Formatter.FormatUInt32),          4294967295U)]
        [DataRow(typeof(Int64),             nameof(Formatter.FormatInt64),           -9223372036854775808L)]
        [DataRow(typeof(UInt64),            nameof(Formatter.FormatUInt64),          9223372036854775807UL)]
        [DataRow(typeof(float),             nameof(Formatter.FormatFloat),           12.342F)]
        [DataRow(typeof(double),            nameof(Formatter.FormatDouble),          12.342)]
        [DataRow(typeof(decimal),           nameof(Formatter.FormatDecimal),         "12.342")]
        [DataRow(typeof(DateTime),          nameof(Formatter.FormatDateTime),        "2019-01-02")]
        [DataRow(typeof(DateTimeOffset),    nameof(Formatter.FormatDateTimeOffset),  "2019-01-02")]
        [DataRow(typeof(Guid),              nameof(Formatter.FormatGuid),            "48cd065e-f78d-465b-af07-49e3b1b7cc92")]
        [DataRow(typeof(byte[]),            nameof(Formatter.FormatByteArray),       new byte[] { 0x0f, 0x0a, 0x0b })]
        public void FormatType_With_Resolver_Uses_Resolver_When_Supplied(Type valueType, string formatterMethodName, object rawValue)
        {
            Mock mockFormatter = null;

            void createMock<T>(T expectedValue)
            {
                var mock = MockHelper.CreateMock<ITypeFormatter<T>>();
                mock.Setup(r => r.Format(expectedValue)).Returns("!!!");
                mockFormatter = mock;
            }

            var formatterMethod = typeof(Formatter)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Single(r => r.Name == formatterMethodName && r.GetParameters().Length == 2);
            Func<object, TypeFormatterResolver, string> callFormatter;
            callFormatter = (t, r) => formatterMethod.Invoke(null, new object[] { t, r }) as string;

            var value = DataRowParser.ConvertExpected(valueType, rawValue);
            if(valueType == typeof(bool))                   createMock<bool>((bool)value);
            else if(valueType == typeof(byte))              createMock<byte>((byte)value);
            else if(valueType == typeof(char))              createMock<char>((char)value);
            else if(valueType == typeof(Int16))             createMock<short>((short)value);
            else if(valueType == typeof(UInt16))            createMock<ushort>((ushort)value);
            else if(valueType == typeof(Int32))             createMock<int>((int)value);
            else if(valueType == typeof(UInt32))            createMock<uint>((uint)value);
            else if(valueType == typeof(Int64))             createMock<long>((long)value);
            else if(valueType == typeof(UInt64))            createMock<ulong>((ulong)value);
            else if(valueType == typeof(float))             createMock<float>((float)value);
            else if(valueType == typeof(double))            createMock<double>((double)value);
            else if(valueType == typeof(decimal))           createMock<decimal>((decimal)value);
            else if(valueType == typeof(DateTime))          createMock<DateTime>((DateTime)value);
            else if(valueType == typeof(DateTimeOffset))    createMock<DateTimeOffset>((DateTimeOffset)value);
            else if(valueType == typeof(Guid))              createMock<Guid>((Guid)value);
            else if(valueType == typeof(byte[]))            createMock<byte[]>((byte[])value);
            else if(valueType == typeof(string))            createMock<string>((string)value);
            else                                            throw new NotImplementedException();

            // Null type resolver should call normal formatter
            Assert.AreNotEqual("!!!", callFormatter(value, null));

            // Type resolver with no formatter for type should call normal formatter
            var emptyTypeResolver = new TypeFormatterResolver();
            Assert.AreNotEqual("!!!", callFormatter(value, emptyTypeResolver));

            // If resolver contains formatter for type then it should be used
            var typeResolver = new TypeFormatterResolver((ITypeFormatter)mockFormatter.Object);
            Assert.AreEqual("!!!", callFormatter(value, typeResolver));
        }

        class UnknownType
        {
            public override string ToString() => "Hello!";
        }

        [TestMethod]
        public void FormatType_Falls_Back_To_ToString()
        {
            var x = new UnknownType();

            Assert.AreEqual("Hello!", Formatter.FormatType(x));
        }

        [TestMethod]
        public void FormatHttpMethod_Returns_Correct_Value()
        {
            foreach(HttpMethod httpMethod in Enum.GetValues(typeof(HttpMethod))) {
                var actual = Formatter.FormatHttpMethod(httpMethod);

                switch(httpMethod) {
                    case HttpMethod.Connect:    Assert.AreEqual("CONNECT", actual); break;
                    case HttpMethod.Delete:     Assert.AreEqual("DELETE", actual); break;
                    case HttpMethod.Get:        Assert.AreEqual("GET", actual); break;
                    case HttpMethod.Head:       Assert.AreEqual("HEAD", actual); break;
                    case HttpMethod.Options:    Assert.AreEqual("OPTIONS", actual); break;
                    case HttpMethod.Patch:      Assert.AreEqual("PATCH", actual); break;
                    case HttpMethod.Post:       Assert.AreEqual("POST", actual); break;
                    case HttpMethod.Put:        Assert.AreEqual("PUT", actual); break;
                    case HttpMethod.Trace:      Assert.AreEqual("TRACE", actual); break;
                    case HttpMethod.Unknown:    Assert.IsNull(actual); break;
                    default:                    throw new NotImplementedException($"Need test code for {httpMethod}");
                }
            }
        }

        [TestMethod]
        public void FormatHttpProtocol_Returns_Correct_Value()
        {
            foreach(HttpProtocol httpProtocol in Enum.GetValues(typeof(HttpProtocol))) {
                var actual = Formatter.FormatHttpProtocol(httpProtocol);

                switch(httpProtocol) {
                    case HttpProtocol.Http0_9:  Assert.AreEqual("HTTP/0.9", actual); break;
                    case HttpProtocol.Http1_0:  Assert.AreEqual("HTTP/1.0", actual); break;
                    case HttpProtocol.Http1_1:  Assert.AreEqual("HTTP/1.1", actual); break;
                    case HttpProtocol.Http2_0:  Assert.AreEqual("HTTP/2.0", actual); break;
                    case HttpProtocol.Http3_0:  Assert.AreEqual("HTTP/3.0", actual); break;
                    case HttpProtocol.Unknown:  Assert.IsNull(actual); break;
                    default:                    throw new NotImplementedException($"Need test code for {httpProtocol}");
                }
            }
        }

        [TestMethod]
        public void FormatHttpScheme_Returns_Correct_Value()
        {
            foreach(HttpScheme httpScheme in Enum.GetValues(typeof(HttpScheme))) {
                var actual = Formatter.FormatHttpScheme(httpScheme);

                switch(httpScheme) {
                    case HttpScheme.Http:       Assert.AreEqual("http", actual); break;
                    case HttpScheme.Https:      Assert.AreEqual("https", actual); break;
                    case HttpScheme.Unknown:    Assert.IsNull(actual); break;
                    default:                    throw new NotImplementedException($"Need test code for {httpScheme}");
                }
            }
        }

        [TestMethod]
        public void FormatMediaType_Returns_Correct_Enum_Value()
        {
            foreach(MediaType mediaType in Enum.GetValues(typeof(MediaType))) {
                var actual = Formatter.FormatMediaType(mediaType);

                switch(mediaType) {
                    case MediaType.JavaScript:      Assert.AreEqual("application/javascript", actual); break;
                    case MediaType.Json:            Assert.AreEqual("application/json", actual); break;
                    case MediaType.MultipartForm:   Assert.AreEqual("multipart/form-data", actual); break;
                    case MediaType.PlainText:       Assert.AreEqual("text/plain", actual); break;
                    case MediaType.UrlEncodedForm:  Assert.AreEqual("application/x-www-form-urlencoded", actual); break;
                    case MediaType.Xml:             Assert.AreEqual("application/xml", actual); break;
                    case MediaType.Unknown:         Assert.IsNull(actual); break;
                    default:                        throw new NotImplementedException($"Need test code for {mediaType}");
                }
            }
        }
    }
}
