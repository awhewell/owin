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
using System.Text;
using AWhewell.Owin.Interface.WebApi;
using AWhewell.Owin.Utility;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin.WebApi
{
    [TestClass]
    public class ModelBuilderTests
    {
        class StringA
        {
            public string A { get; set; }
        }

        class AllNativeTypes
        {
            public bool     Boolean   { get; set; }
            public bool?    NBoolean  { get; set; }
            public byte     Byte      { get; set; }
            public byte?    NByte     { get; set; }
            public short    Short     { get; set; }
            public short?   NShort    { get; set; }
            public int      Int       { get; set; }
            public int?     NInt      { get; set; }
            public long     Long      { get; set; }
            public long?    NLong     { get; set; }
            public float    Float     { get; set; }
            public float?   NFloat    { get; set; }
            public double   Double    { get; set; }
            public double?  NDouble   { get; set; }
            public decimal  Decimal   { get; set; }
            public decimal? NDecimal  { get; set; }
        }

        private IModelBuilder _ModelBuilder;

        [TestInitialize]
        public void TestInitialise()
        {
            _ModelBuilder = Factory.Resolve<IModelBuilder>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BuildModel_QueryStringDictionary_Throws_If_Passed_Null_Type()
        {
            _ModelBuilder.BuildModel(null, new QueryStringDictionary("a=b"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BuildModel_QueryStringDictionary_Throws_If_Passed_Null_QueryStringDictionary()
        {
            _ModelBuilder.BuildModel(typeof(StringA), null);
        }

        [TestMethod]
        public void BuildModel_QueryStringDictionary_Can_Build_StringA_Model_With_Correct_Case()
        {
            var dictionary = new QueryStringDictionary("A=Abc");

            var actual = _ModelBuilder.BuildModel(typeof(StringA), dictionary) as StringA;

            Assert.AreEqual("Abc", actual.A);
        }

        [TestMethod]
        [DataRow(nameof(AllNativeTypes.Boolean),    "true",         true,           "en-GB")]
        [DataRow(nameof(AllNativeTypes.Boolean),    "false",        false,          "en-GB")]
        [DataRow(nameof(AllNativeTypes.Boolean),    "true",         true,           "de-DE")]
        [DataRow(nameof(AllNativeTypes.Boolean),    "false",        false,          "de-DE")]
        [DataRow(nameof(AllNativeTypes.Boolean),    "TRUE",         true,           "en-GB")]
        [DataRow(nameof(AllNativeTypes.Boolean),    "FALSE",        false,          "en-GB")]
        [DataRow(nameof(AllNativeTypes.Boolean),    "1",            true,           "en-GB")]
        [DataRow(nameof(AllNativeTypes.Boolean),    "0",            false,          "en-GB")]
        [DataRow(nameof(AllNativeTypes.Boolean),    "yes",          true,           "en-GB")]
        [DataRow(nameof(AllNativeTypes.Boolean),    "no",           false,          "en-GB")]
        [DataRow(nameof(AllNativeTypes.Boolean),    "on",           true,           "en-GB")]
        [DataRow(nameof(AllNativeTypes.Boolean),    "off",          false,          "en-GB")]
        [DataRow(nameof(AllNativeTypes.NBoolean),   null,           null,           "en-GB")]
        [DataRow(nameof(AllNativeTypes.NBoolean),   "",             null,           "en-GB")]
        [DataRow(nameof(AllNativeTypes.NBoolean),   "null",         null,           "en-GB")]
        [DataRow(nameof(AllNativeTypes.NBoolean),   "true",         true,           "en-GB")]
        [DataRow(nameof(AllNativeTypes.NBoolean),   "false",        false,          "en-GB")]
        [DataRow(nameof(AllNativeTypes.Byte),       "255",          (byte)255,      "en-GB")]
        [DataRow(nameof(AllNativeTypes.Byte),       "255",          (byte)255,      "en-DE")]
        [DataRow(nameof(AllNativeTypes.NByte),      null,           null,           "en-GB")]
        [DataRow(nameof(AllNativeTypes.NByte),      "null",         null,           "en-GB")]
        [DataRow(nameof(AllNativeTypes.NByte),      "41",           (byte)41,       "en-GB")]
        [DataRow(nameof(AllNativeTypes.NByte),      "41",           (byte)41,       "en-DE")]
        [DataRow(nameof(AllNativeTypes.Short),      "16500",        (short)16500,   "en-GB")]
        [DataRow(nameof(AllNativeTypes.Short),      "-75",          (short)-75,     "en-DE")]
        [DataRow(nameof(AllNativeTypes.NShort),     null,           null,           "en-GB")]
        [DataRow(nameof(AllNativeTypes.NShort),     "null",         null,           "en-GB")]
        [DataRow(nameof(AllNativeTypes.NShort),     "16500",        (short)16500,   "en-GB")]
        [DataRow(nameof(AllNativeTypes.NShort),     "-75",        (short)-75,       "en-DE")]
        [DataRow(nameof(AllNativeTypes.Int),        "2000000000",   2000000000,     "en-GB")]
        [DataRow(nameof(AllNativeTypes.Int),        "-400101023",   -400101023,     "en-DE")]
        [DataRow(nameof(AllNativeTypes.NInt),       null,           null,           "en-GB")]
        [DataRow(nameof(AllNativeTypes.NInt),       "null",         null,           "en-GB")]
        [DataRow(nameof(AllNativeTypes.NInt),       "2000000000",   2000000000,     "en-GB")]
        [DataRow(nameof(AllNativeTypes.NInt),       "-400101023",   -400101023,     "en-DE")]
        [DataRow(nameof(AllNativeTypes.Long),       "6123456789",   6123456789L,    "en-GB")]
        [DataRow(nameof(AllNativeTypes.Long),       "-6123456789",  -6123456789L,   "en-DE")]
        [DataRow(nameof(AllNativeTypes.NLong),      null,           null,           "en-GB")]
        [DataRow(nameof(AllNativeTypes.NLong),      "null",         null,           "en-GB")]
        [DataRow(nameof(AllNativeTypes.NLong),      "6123456789",   6123456789L,    "en-GB")]
        [DataRow(nameof(AllNativeTypes.NLong),      "-6123456789",  -6123456789L,   "en-DE")]
        [DataRow(nameof(AllNativeTypes.Float),      "782.123",      782.123F,       "en-GB")]
        [DataRow(nameof(AllNativeTypes.Float),      "-782.123",     -782.123F,      "en-DE")]
        [DataRow(nameof(AllNativeTypes.NFloat),     null,           null,           "en-GB")]
        [DataRow(nameof(AllNativeTypes.NFloat),     "null",         null,           "en-GB")]
        [DataRow(nameof(AllNativeTypes.NFloat),     "782.123",      782.123F,       "en-GB")]
        [DataRow(nameof(AllNativeTypes.NFloat),     "-782.123",     -782.123F,      "en-DE")]
        [DataRow(nameof(AllNativeTypes.Double),     "782.123",      782.123,        "en-GB")]
        [DataRow(nameof(AllNativeTypes.Double),     "-782.123",     -782.123,       "en-DE")]
        [DataRow(nameof(AllNativeTypes.NDouble),    null,           null,           "en-GB")]
        [DataRow(nameof(AllNativeTypes.NDouble),    "null",         null,           "en-GB")]
        [DataRow(nameof(AllNativeTypes.NDouble),    "782.123",      782.123,        "en-GB")]
        [DataRow(nameof(AllNativeTypes.NDouble),    "-782.123",     -782.123,       "en-DE")]
        [DataRow(nameof(AllNativeTypes.Decimal),    "782.123",      "782.123",      "en-GB")]
        [DataRow(nameof(AllNativeTypes.Decimal),    "-782.123",     "-782.123",     "en-DE")]
        [DataRow(nameof(AllNativeTypes.NDecimal),   null,           null,           "en-GB")]
        [DataRow(nameof(AllNativeTypes.NDecimal),   "null",         null,           "en-GB")]
        [DataRow(nameof(AllNativeTypes.NDecimal),   "782.123",      "782.123",      "en-GB")]
        [DataRow(nameof(AllNativeTypes.NDecimal),   "-782.123",     "-782.123",     "en-DE")]
        public void BuildModel_QueryStringDictionary_Can_Parse_All_Native_Types(string propertyName, string value, object expected, string culture)
        {
            using(new CultureSwap(culture)) {
                var propertyInfo = typeof(AllNativeTypes).GetProperty(propertyName);
                var dictionary = new QueryStringDictionary($"{propertyName}={value ?? ""}");

                var model = _ModelBuilder.BuildModel(typeof(AllNativeTypes), dictionary) as AllNativeTypes;
                var actual = propertyInfo.GetValue(model, null);

                expected = DataRowParser.ConvertExpected(propertyInfo.PropertyType, expected);

                Assert.AreEqual(expected, actual);
            }
        }
    }
}
