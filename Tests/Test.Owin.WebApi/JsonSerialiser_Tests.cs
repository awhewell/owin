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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using AWhewell.Owin.Interface.WebApi;
using AWhewell.Owin.Utility;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Test.AWhewell.Owin.WebApi
{
    [TestClass]
    public class JsonSerialiser_Tests
    {
        class ValueTypes
        {
            public bool? Bool { get; set; }
            public byte? Byte { get; set; }
            public char? Char { get; set; }
            public short? Short { get; set; }
            public ushort? UShort { get; set; }
            public int? Int { get; set; }
            public uint? UInt { get; set; }
            public long? Long { get; set; }
            public ulong? ULong { get; set; }
            public float? Float { get; set; }
            public double? Double { get; set; }
            public decimal? Decimal { get; set; }
            public DateTime? DateTime { get; set; }
            public DateTimeOffset? DateTimeOffset { get; set; }
            public Guid? Guid { get; set; }
            public byte[] ByteArray { get; set; }
            public string String { get; set; }  // not technically a value type in .NET

            public override bool Equals(object obj)
            {
                var result = Object.ReferenceEquals(this, obj);
                if(!result && obj is ValueTypes other) {
                    result = Bool ==            other.Bool
                          && Byte ==            other.Byte
                          && Char ==            other.Char
                          && Short ==           other.Short
                          && UShort ==          other.UShort
                          && Int ==             other.Int
                          && UInt ==            other.UInt
                          && Long ==            other.Long
                          && ULong ==           other.ULong
                          && Float ==           other.Float
                          && Double ==          other.Double
                          && Decimal ==         other.Decimal
                          && DateTime ==        other.DateTime
                          && DateTimeOffset ==  other.DateTimeOffset
                          && Guid ==            other.Guid
                          && String ==          other.String;
                }

                return result;
            }

            public override int GetHashCode() => 0;

            public override string ToString()
            {
                return JsonConvert.SerializeObject(this);
            }
        }

        public class DateTime_JustDigits_Parser : ITypeParser<DateTime>
        {
            public bool TryParse(string text, out DateTime value) => DateTime.TryParseExact(text, "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture, DateTimeStyles.None, out value);
        }

        private IJsonSerialiser _Serialiser;

        [TestInitialize]
        public void TestInitialise()
        {
            _Serialiser = Factory.Resolve<IJsonSerialiser>();
        }

        [TestMethod]
        public void Serialise_Can_Serialise_Vanilla_ValueTypes_Json()
        {
            var originalObj = new ValueTypes() {
                Bool = true,
                Byte = 1,
                ByteArray = new byte[] { 1, 2, 3, 4, 5 },
                Char = '2',
                DateTime = new DateTime(2019, 9, 8, 7, 6, 5, 432),
                DateTimeOffset = new DateTimeOffset(2011, 2, 3, 4, 5, 6, 789, new TimeSpan(7, 30, 0)),
                Decimal = 3.456M,
                Double = 4.567,
                Float = 5.678F,
                Guid = Guid.Parse("77FDC6F0-B033-4069-A78A-E02C2E6EAE92"),
                Int = 6,
                Long = 7L,
                Short = 8,
                String = "Hello World!",
                UInt = 9,
                ULong = 10,
                UShort = 11,
            };
            var json = _Serialiser.Serialise(originalObj, null);

            var deserialised = JsonConvert.DeserializeObject<ValueTypes>(json);

            Assert.AreEqual(originalObj, deserialised);
        }

        [TestMethod]
        [DataRow(typeof(DateTime),          true,  nameof(ValueTypes.DateTime),        "2019-01-02")]
        [DataRow(typeof(DateTimeOffset),    true,  nameof(ValueTypes.DateTimeOffset),  "2019-01-02")]
        [DataRow(typeof(string),            false, nameof(ValueTypes.String),          "Diskopunk")]
        [DataRow(typeof(bool),              false, nameof(ValueTypes.Bool),            false)]
        [DataRow(typeof(byte),              false, nameof(ValueTypes.Byte),            (byte)255)]
        [DataRow(typeof(char),              false, nameof(ValueTypes.Char),            '1')]
        [DataRow(typeof(Int16),             false, nameof(ValueTypes.Short),           (short)-32768)]
        [DataRow(typeof(UInt16),            false, nameof(ValueTypes.UShort),          (ushort)32767)]
        [DataRow(typeof(Int32),             false, nameof(ValueTypes.Int),             -2147483648)]
        [DataRow(typeof(UInt32),            false, nameof(ValueTypes.UInt),            4294967295U)]
        [DataRow(typeof(Int64),             false, nameof(ValueTypes.Long),            -9223372036854775808L)]
        [DataRow(typeof(UInt64),            false, nameof(ValueTypes.ULong),           9223372036854775807UL)]
        [DataRow(typeof(float),             false, nameof(ValueTypes.Float),           12.342F)]
        [DataRow(typeof(double),            false, nameof(ValueTypes.Double),          12.342)]
        [DataRow(typeof(decimal),           false, nameof(ValueTypes.Decimal),         "12.342")]
        [DataRow(typeof(Guid),              true,  nameof(ValueTypes.Guid),            "48cd065e-f78d-465b-af07-49e3b1b7cc92")]
        [DataRow(typeof(byte[]),            true,  nameof(ValueTypes.ByteArray),       new byte[] { 0x0f, 0x0a, 0x0b })]
        public void Serialise_Uses_Formatter_For_Dates_And_Byte_Arrays_But_Not_Json_Spec_Types(Type valueType, bool mustUseFormatter, string propertyName, object originalValueRaw)
        {
            Mock mockFormatter = null;

            void createMock<T>(T expectedValue)
            {
                var mock = MockHelper.CreateMock<ITypeFormatter<T>>();
                mock.Setup(r => r.Format(expectedValue)).Returns("!!!");
                mockFormatter = mock;
            }

            var expectedNormal = DataRowParser.ConvertExpected(valueType, originalValueRaw);
            var valueProperty = typeof(ValueTypes).GetProperty(propertyName);

            if(valueType == typeof(bool))                   createMock<bool>((bool)expectedNormal);
            else if(valueType == typeof(byte))              createMock<byte>((byte)expectedNormal);
            else if(valueType == typeof(char))              createMock<char>((char)expectedNormal);
            else if(valueType == typeof(Int16))             createMock<short>((short)expectedNormal);
            else if(valueType == typeof(UInt16))            createMock<ushort>((ushort)expectedNormal);
            else if(valueType == typeof(Int32))             createMock<int>((int)expectedNormal);
            else if(valueType == typeof(UInt32))            createMock<uint>((uint)expectedNormal);
            else if(valueType == typeof(Int64))             createMock<long>((long)expectedNormal);
            else if(valueType == typeof(UInt64))            createMock<ulong>((ulong)expectedNormal);
            else if(valueType == typeof(float))             createMock<float>((float)expectedNormal);
            else if(valueType == typeof(double))            createMock<double>((double)expectedNormal);
            else if(valueType == typeof(decimal))           createMock<decimal>((decimal)expectedNormal);
            else if(valueType == typeof(DateTime))          createMock<DateTime>((DateTime)expectedNormal);
            else if(valueType == typeof(DateTimeOffset))    createMock<DateTimeOffset>((DateTimeOffset)expectedNormal);
            else if(valueType == typeof(Guid))              createMock<Guid>((Guid)expectedNormal);
            else if(valueType == typeof(string))            createMock<string>((string)expectedNormal);
            else if(valueType == typeof(byte[]))            createMock<byte[]>((byte[])expectedNormal);
            else                                            throw new NotImplementedException();

            var typeResolver = new TypeFormatterResolver((ITypeFormatter)mockFormatter.Object);

            var originalInstance = new ValueTypes();
            valueProperty.SetValue(originalInstance, expectedNormal);

            var jsonText = _Serialiser.Serialise(originalInstance, typeResolver);
            var jObject = JObject.Parse(jsonText);
            var jProperty = jObject.Property(propertyName);

            if(mustUseFormatter) {
                Assert.AreEqual("!!!", jProperty.Value.ToString());
            } else {
                Assert.AreEqual(expectedNormal, jProperty.Value.ToObject(valueProperty.PropertyType));
            }
        }
    }
}
