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
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using AWhewell.Owin.Interface.WebApi;
using AWhewell.Owin.Utility;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

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

        class DateTime_JustDigits_Parser : ITypeParser<DateTime>
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
        public void JsonSerialiser_Deserialise_Can_Deserialise_Vanilla_ValueTypes_Json()
        {
            var originalObj = new ValueTypes() {
                Bool = true,
                Byte = 1,
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
            var json = JsonConvert.SerializeObject(originalObj, Formatting.Indented);

            var deserialised = _Serialiser.Deserialise(typeof(ValueTypes), null, json);

            Assert.AreEqual(originalObj, deserialised);
        }

        [TestMethod]
        public void JsonSerialiser_Deserialise_Uses_Resolver_For_DateTimes()
        {
            var originalObj = new ValueTypes() {
                DateTime = new DateTime(2019, 9, 8, 7, 6, 5, 432),
            };
            var json = JsonConvert.SerializeObject(originalObj, Formatting.Indented)
                .Replace("2019-09-08T07:06:05.432", "20190908070605432");
            var resolver = new TypeParserResolver(new DateTime_JustDigits_Parser());

            var deserialised = _Serialiser.Deserialise(typeof(ValueTypes), resolver, json);

            Assert.AreEqual(originalObj, deserialised);
        }
    }
}
