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
using System.Reflection;
using System.Text;
using AWhewell.Owin.Utility;
using AWhewell.Owin.Utility.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Test.AWhewell.Owin.Utility
{
    [TestClass]
    public class TypeParserResolverTests
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
        [DataRow(typeof(bool),              typeof(Mock<ITypeParser<bool>>),            nameof(TypeParserResolver.BoolParser))]
        [DataRow(typeof(byte),              typeof(Mock<ITypeParser<byte>>),            nameof(TypeParserResolver.ByteParser))]
        [DataRow(typeof(char),              typeof(Mock<ITypeParser<char>>),            nameof(TypeParserResolver.CharParser))]
        [DataRow(typeof(Int16),             typeof(Mock<ITypeParser<Int16>>),           nameof(TypeParserResolver.Int16Parser))]
        [DataRow(typeof(UInt16),            typeof(Mock<ITypeParser<UInt16>>),          nameof(TypeParserResolver.UInt16Parser))]
        [DataRow(typeof(Int32),             typeof(Mock<ITypeParser<Int32>>),           nameof(TypeParserResolver.Int32Parser))]
        [DataRow(typeof(UInt32),            typeof(Mock<ITypeParser<UInt32>>),          nameof(TypeParserResolver.UInt32Parser))]
        [DataRow(typeof(Int64),             typeof(Mock<ITypeParser<Int64>>),           nameof(TypeParserResolver.Int64Parser))]
        [DataRow(typeof(UInt64),            typeof(Mock<ITypeParser<UInt64>>),          nameof(TypeParserResolver.UInt64Parser))]
        [DataRow(typeof(float),             typeof(Mock<ITypeParser<float>>),           nameof(TypeParserResolver.FloatParser))]
        [DataRow(typeof(double),            typeof(Mock<ITypeParser<double>>),          nameof(TypeParserResolver.DoubleParser))]
        [DataRow(typeof(decimal),           typeof(Mock<ITypeParser<decimal>>),         nameof(TypeParserResolver.DecimalParser))]
        [DataRow(typeof(DateTime),          typeof(Mock<ITypeParser<DateTime>>),        nameof(TypeParserResolver.DateTimeParser))]
        [DataRow(typeof(DateTimeOffset),    typeof(Mock<ITypeParser<DateTimeOffset>>),  nameof(TypeParserResolver.DateTimeOffsetParser))]
        [DataRow(typeof(Guid),              typeof(Mock<ITypeParser<Guid>>),            nameof(TypeParserResolver.GuidParser))]
        [DataRow(typeof(byte[]),            typeof(Mock<ITypeParser<byte[]>>),          nameof(TypeParserResolver.ByteArrayParser))]
        public void Assign_Sets_Direct_Access_Property(Type valueType, Type mockType, string parserPropertyName)
        {
            var mockParser = MockHelper.CreateMock(mockType);
            var mockTypeParser = (ITypeParser)mockParser.Object;

            var resolver = new TypeParserResolver();
            var parserProperty = resolver
                .GetType()
                .GetProperty(parserPropertyName);
            var assignMethod = resolver
                .GetType()
                .GetMethods()
                .Single(r => r.Name == nameof(TypeParserResolver.Assign) && r.IsGenericMethod);
            var genericAssignMethod = assignMethod.MakeGenericMethod(valueType);

            genericAssignMethod.Invoke(resolver, new object[] { mockTypeParser });

            var parserValue = parserProperty.GetValue(resolver, null);
            Assert.AreSame(mockTypeParser, parserValue);

            genericAssignMethod.Invoke(resolver, new object[] { null });

            parserValue = parserProperty.GetValue(resolver, null);
            Assert.IsNull(parserValue);
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
            var resolver = new TypeParserResolver();

            resolver.Assign(new DateTime_Iso8601_Parser());
            var parser = resolver.Find<DateTime>();

            Assert.IsInstanceOfType(parser, typeof(DateTime_Iso8601_Parser));
        }

        [TestMethod]
        public void Find_Returns_Assigned_Abstract_Parser()
        {
            var resolver = new TypeParserResolver();

            resolver.Assign((ITypeParser)(new DateTime_Iso8601_Parser()));
            var parser = resolver.Find<DateTime>();

            Assert.IsInstanceOfType(parser, typeof(DateTime_Iso8601_Parser));
        }

        [TestMethod]
        public void Find_Returns_Last_Assigned_Parser()
        {
            var resolver = new TypeParserResolver();

            resolver.Assign(new DateTime_Iso8601_Parser());
            resolver.Assign(new DateTime_Invariant_Parser());
            var parser = resolver.Find<DateTime>();

            Assert.IsInstanceOfType(parser, typeof(DateTime_Invariant_Parser));
        }

        [TestMethod]
        public void Find_Returns_Null_If_Parser_Assigned_To_Different_Type()
        {
            var resolver = new TypeParserResolver();

            resolver.Assign(new DateTimeOffset_Iso8601_Parser());
            var parser = resolver.Find<DateTime>();

            Assert.IsNull(parser);
        }

        [TestMethod]
        public void Find_Returns_Null_If_Typed_Null_Is_Assigned()
        {
            var resolver = new TypeParserResolver();

            resolver.Assign(new DateTime_Iso8601_Parser());
            resolver.Assign<DateTime>(null);
            var parser = resolver.Find<DateTime>();

            Assert.IsNull(parser);
        }

        [TestMethod]
        public void Find_Does_Nothing_If_Abstract_Null_Is_Assigned()
        {
            var resolver = new TypeParserResolver();

            resolver.Assign(new DateTime_Iso8601_Parser());
            resolver.Assign((ITypeParser)null);
            var parser = resolver.Find<DateTime>();

            Assert.IsInstanceOfType(parser, typeof(DateTime_Iso8601_Parser));
        }

        [TestMethod]
        public void CopyAssignmentsFrom_Copies_Assignments()
        {
            var resolver = new TypeParserResolver();
            var otherResolver = new TypeParserResolver();
            otherResolver.Assign(new DateTime_Iso8601_Parser());

            resolver.CopyAssignmentsFrom(otherResolver);
            var parser = resolver.Find<DateTime>();

            Assert.IsInstanceOfType(parser, typeof(DateTime_Iso8601_Parser));
        }

        [TestMethod]
        public void CopyAssignmentsFrom_Overrides_Existing_Assignments()
        {
            var resolver = new TypeParserResolver();
            var otherResolver = new TypeParserResolver();
            resolver.Assign(new DateTime_Local_Parser());
            otherResolver.Assign(new DateTime_Iso8601_Parser());

            resolver.CopyAssignmentsFrom(otherResolver);
            var parser = resolver.Find<DateTime>();

            Assert.IsInstanceOfType(parser, typeof(DateTime_Iso8601_Parser));
        }

        [TestMethod]
        public void CopyAssignmentsFrom_Does_Not_Delete_Other_Assignments()
        {
            var resolver = new TypeParserResolver();
            var otherResolver = new TypeParserResolver();
            resolver.Assign(new DateTimeOffset_Local_Parser());
            otherResolver.Assign(new DateTime_Iso8601_Parser());

            resolver.CopyAssignmentsFrom(otherResolver);
            var parser = resolver.Find<DateTimeOffset>();

            Assert.IsInstanceOfType(parser, typeof(DateTimeOffset_Local_Parser));
        }
    }
}
