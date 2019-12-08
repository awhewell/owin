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
using System.Linq;
using AWhewell.Owin.Utility;
using AWhewell.Owin.Utility.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Test.AWhewell.Owin.Utility
{
    [TestClass]
    public class TypeParserResolver_Tests
    {
        [TestMethod]
        public void Parsers_Ctor_Assigns_Parsers_During_Construction()
        {
            var resolver = new TypeParserResolver(new DateTime_Iso8601_Parser());

            var parser = resolver.Find<DateTime>();

            Assert.IsInstanceOfType(parser, typeof(DateTime_Iso8601_Parser));
        }

        [TestMethod]
        public void Parsers_Ctor_Assigns_Last_Of_Duplicate_Parsers_During_Construction()
        {
            var resolver = new TypeParserResolver(
                new DateTime_Local_Parser(),
                new DateTime_Iso8601_Parser()
            );

            var parser = resolver.Find<DateTime>();

            Assert.IsInstanceOfType(parser, typeof(DateTime_Iso8601_Parser));
        }

        [TestMethod]
        public void Parsers_Ctor_Ignores_Null()
        {
            var resolver = new TypeParserResolver(new ITypeParser[] { null });

            Assert.AreEqual(0, resolver.GetParsers().Length);
        }

        [TestMethod]
        [DataRow(typeof(Mock<ITypeParser<bool>>),            nameof(TypeParserResolver.BoolParser))]
        [DataRow(typeof(Mock<ITypeParser<byte>>),            nameof(TypeParserResolver.ByteParser))]
        [DataRow(typeof(Mock<ITypeParser<char>>),            nameof(TypeParserResolver.CharParser))]
        [DataRow(typeof(Mock<ITypeParser<Int16>>),           nameof(TypeParserResolver.Int16Parser))]
        [DataRow(typeof(Mock<ITypeParser<UInt16>>),          nameof(TypeParserResolver.UInt16Parser))]
        [DataRow(typeof(Mock<ITypeParser<Int32>>),           nameof(TypeParserResolver.Int32Parser))]
        [DataRow(typeof(Mock<ITypeParser<UInt32>>),          nameof(TypeParserResolver.UInt32Parser))]
        [DataRow(typeof(Mock<ITypeParser<Int64>>),           nameof(TypeParserResolver.Int64Parser))]
        [DataRow(typeof(Mock<ITypeParser<UInt64>>),          nameof(TypeParserResolver.UInt64Parser))]
        [DataRow(typeof(Mock<ITypeParser<float>>),           nameof(TypeParserResolver.FloatParser))]
        [DataRow(typeof(Mock<ITypeParser<double>>),          nameof(TypeParserResolver.DoubleParser))]
        [DataRow(typeof(Mock<ITypeParser<decimal>>),         nameof(TypeParserResolver.DecimalParser))]
        [DataRow(typeof(Mock<ITypeParser<DateTime>>),        nameof(TypeParserResolver.DateTimeParser))]
        [DataRow(typeof(Mock<ITypeParser<DateTimeOffset>>),  nameof(TypeParserResolver.DateTimeOffsetParser))]
        [DataRow(typeof(Mock<ITypeParser<Guid>>),            nameof(TypeParserResolver.GuidParser))]
        [DataRow(typeof(Mock<ITypeParser<byte[]>>),          nameof(TypeParserResolver.ByteArrayParser))]
        public void Parsers_Ctor_Sets_Direct_Access_Property(Type mockType, string parserPropertyName)
        {
            var mockParser = MockHelper.CreateMock(mockType);
            var mockTypeParser = (ITypeParser)mockParser.Object;

            var resolver = new TypeParserResolver(mockTypeParser);

            var parserProperty = resolver
                .GetType()
                .GetProperty(parserPropertyName);

            var parserValue = parserProperty.GetValue(resolver, null);
            Assert.AreSame(mockTypeParser, parserValue);
        }

        [TestMethod]
        public void GetAugmentedParsers_Returns_Parsers()
        {
            var parser = new DateTime_Local_Parser();
            var resolver = new TypeParserResolver(parser);

            var parsers = resolver.GetAugmentedParsers(null);
            Assert.AreEqual(1, parsers.Length);
            Assert.AreSame(parser, parsers[0]);
        }

        [TestMethod]
        public void GetAugmentedParsers_Augments_Registered_Parsers_With_List_Passed_In()
        {
            var originalDateTimeParser = new DateTime_Local_Parser();
            var originalDateTimeOffsetParser = new DateTimeOffset_Invariant_Parser();
            var newDateTimeParser = new DateTime_Iso8601_Parser();
            var newByteArrayParser = new ByteArray_Mime64_Parser();
            var resolver = new TypeParserResolver(originalDateTimeParser, originalDateTimeOffsetParser);

            var parsers = resolver.GetAugmentedParsers(newDateTimeParser, null, newByteArrayParser);
            Assert.AreEqual(3, parsers.Length);
            Assert.IsTrue(parsers.Contains(newDateTimeParser));
            Assert.IsTrue(parsers.Contains(originalDateTimeOffsetParser));
            Assert.IsTrue(parsers.Contains(newByteArrayParser));

            var originalParsers = resolver.GetParsers();
            Assert.AreEqual(2, originalParsers.Length);
            Assert.IsTrue(originalParsers.Contains(originalDateTimeParser));
            Assert.IsTrue(originalParsers.Contains(originalDateTimeOffsetParser));
        }

        [TestMethod]
        public void Find_Returns_Null_If_No_Parser_Assigned()
        {
            var resolver = new TypeParserResolver();

            var parser = resolver.Find<DateTime>();

            Assert.IsNull(parser);
        }

        [TestMethod]
        public void Find_Returns_Assigned_Generic_Parser()
        {
            var resolver = new TypeParserResolver(new DateTime_Iso8601_Parser());

            var parser = resolver.Find<DateTime>();

            Assert.IsInstanceOfType(parser, typeof(DateTime_Iso8601_Parser));
        }

        [TestMethod]
        public void Find_Returns_Null_If_Parser_Assigned_To_Different_Type()
        {
            var resolver = new TypeParserResolver(new DateTimeOffset_Iso8601_Parser());

            var parser = resolver.Find<DateTime>();

            Assert.IsNull(parser);
        }

        [TestMethod]
        public void GetParsers_Returns_Empty_Collection_For_Empty_Resolver()
        {
            var resolver = new TypeParserResolver();

            var parsers = resolver.GetParsers();

            Assert.AreEqual(0, parsers.Length);
        }

        [TestMethod]
        public void GetParsers_Returns_Collection_Of_All_Registered_Parsers()
        {
            var parser1 = new DateTime_Invariant_Parser();
            var parser2 = new DateTimeOffset_Local_Parser();
            var resolver = new TypeParserResolver(parser1, parser2);

            var parsers = resolver.GetParsers();

            Assert.AreEqual(2, parsers.Length);
            Assert.IsTrue(parsers.Contains(parser1));
            Assert.IsTrue(parsers.Contains(parser2));
        }

        [TestMethod]
        [DataRow(new Type[0],                                                                       new Type[0],                                                                        true)]  // LHS and RHS are empty
        [DataRow(new Type[0],                                                                       null,                                                                               false)] // RHS is null
        [DataRow(new Type[] { typeof(DateTime_Local_Parser) },                                      new Type[] { typeof(DateTime_Local_Parser) },                                       true)]  // LHS and RHS contain same single type
        [DataRow(new Type[] { typeof(DateTime_Local_Parser) },                                      new Type[] { typeof(DateTime_Iso8601_Parser) },                                     false)] // LHS and RHS contain different single type
        [DataRow(new Type[] { typeof(DateTime_Local_Parser), typeof(DateTimeOffset_Local_Parser) }, new Type[] { typeof(DateTimeOffset_Local_Parser), typeof(DateTime_Local_Parser) },  true)]  // LHS and RHS contain same types in different order
        [DataRow(new Type[] { typeof(DateTime_Local_Parser) },                                      new Type[] { typeof(DateTimeOffset_Local_Parser), typeof(DateTime_Local_Parser) },  false)] // LHS is subset of RHS
        [DataRow(new Type[] { typeof(DateTime_Local_Parser), typeof(DateTimeOffset_Local_Parser) }, new Type[] { typeof(DateTime_Local_Parser) },                                       false)] // RHS is subset of LHS
        public void Equals_And_ParserEquals_Returns_Expected_Value(Type[] lhsTypes, Type[] rhsTypes, bool expected)
        {
            ITypeParser[] typeParsers(Type[] types)
            {
                var parsers = new List<ITypeParser>();
                foreach(var type in types) {
                    parsers.Add((ITypeParser)Activator.CreateInstance(type));
                }
                return parsers.ToArray();
            }

            var lhsResolver = new TypeParserResolver(typeParsers(lhsTypes));
            var rhsParsers =  rhsTypes == null ? null : typeParsers(rhsTypes);
            var rhsResolver = rhsTypes == null ? null : new TypeParserResolver(rhsParsers);

            var equalsActual = lhsResolver.Equals(rhsResolver);
            Assert.AreEqual(expected, equalsActual);

            // Bit naughty to be testing two functions but Equals is basically a wrapper around ParserEquals and
            // I don't want to be doubling up the DataRows
            var parserEqualsActual = lhsResolver.ParsersEquals(rhsParsers);
            Assert.AreEqual(expected, parserEqualsActual);

            // Check that the == and != operators are calling down to .Equals
            Assert.AreEqual(expected, lhsResolver == rhsResolver);
            Assert.AreEqual(!expected, lhsResolver != rhsResolver);
        }

        [TestMethod]
        [DataRow(true,  true,  true)]
        [DataRow(false, true,  false)]
        [DataRow(true,  false, false)]
        public void Equality_Operators_Handle_Null_Correctly(bool lhsIsNull, bool rhsIsNull, bool expectedEqual)
        {
            var lhs = lhsIsNull ? null : new TypeParserResolver();
            var rhs = rhsIsNull ? null : new TypeParserResolver();

            var equal =    lhs == rhs;
            var notEqual = lhs != rhs;

            Assert.AreEqual(expectedEqual, equal);
            Assert.AreEqual(!expectedEqual, notEqual);
        }

        [TestMethod]
        public void GetHashCode_Is_Equal_For_All_Equal_Instances()
        {
            var resolver1 = new TypeParserResolver(
                new DateTime_Local_Parser(),
                new DateTimeOffset_Local_Parser()
            );
            var resolver2 = new TypeParserResolver(
                new DateTimeOffset_Local_Parser(),
                new DateTime_Local_Parser()
            );

            Assert.AreEqual(resolver1.GetHashCode(), resolver2.GetHashCode());
        }
    }
}
