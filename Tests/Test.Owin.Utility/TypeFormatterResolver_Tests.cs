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
using AWhewell.Owin.Utility.Formatters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Test.AWhewell.Owin.Utility
{
    [TestClass]
    public class TypeFormatterResolver_Tests
    {
        [TestMethod]
        public void Formatters_Ctor_Assigns_Formatters_During_Construction()
        {
            var resolver = new TypeFormatterResolver(new DateTime_Iso8601_Formatter());

            var formatter = resolver.Find<DateTime>();

            Assert.IsInstanceOfType(formatter, typeof(DateTime_Iso8601_Formatter));
        }

        [TestMethod]
        public void Formatters_Ctor_Assigns_Last_Of_Duplicate_Formatters_During_Construction()
        {
            var resolver = new TypeFormatterResolver(
                new DateTime_MicrosoftJson_Formatter(),
                new DateTime_Iso8601_Formatter()
            );

            var formatter = resolver.Find<DateTime>();

            Assert.IsInstanceOfType(formatter, typeof(DateTime_Iso8601_Formatter));
        }

        [TestMethod]
        public void Formatters_Ctor_Ignores_Null()
        {
            var resolver = new TypeFormatterResolver(new ITypeFormatter[] { null });

            Assert.AreEqual(0, resolver.GetFormatters().Length);
        }

        [TestMethod]
        [DataRow(typeof(Mock<ITypeFormatter<string>>),          nameof(TypeFormatterResolver.StringFormatter))]
        [DataRow(typeof(Mock<ITypeFormatter<bool>>),            nameof(TypeFormatterResolver.BoolFormatter))]
        [DataRow(typeof(Mock<ITypeFormatter<byte>>),            nameof(TypeFormatterResolver.ByteFormatter))]
        [DataRow(typeof(Mock<ITypeFormatter<char>>),            nameof(TypeFormatterResolver.CharFormatter))]
        [DataRow(typeof(Mock<ITypeFormatter<Int16>>),           nameof(TypeFormatterResolver.Int16Formatter))]
        [DataRow(typeof(Mock<ITypeFormatter<UInt16>>),          nameof(TypeFormatterResolver.UInt16Formatter))]
        [DataRow(typeof(Mock<ITypeFormatter<Int32>>),           nameof(TypeFormatterResolver.Int32Formatter))]
        [DataRow(typeof(Mock<ITypeFormatter<UInt32>>),          nameof(TypeFormatterResolver.UInt32Formatter))]
        [DataRow(typeof(Mock<ITypeFormatter<Int64>>),           nameof(TypeFormatterResolver.Int64Formatter))]
        [DataRow(typeof(Mock<ITypeFormatter<UInt64>>),          nameof(TypeFormatterResolver.UInt64Formatter))]
        [DataRow(typeof(Mock<ITypeFormatter<float>>),           nameof(TypeFormatterResolver.FloatFormatter))]
        [DataRow(typeof(Mock<ITypeFormatter<double>>),          nameof(TypeFormatterResolver.DoubleFormatter))]
        [DataRow(typeof(Mock<ITypeFormatter<decimal>>),         nameof(TypeFormatterResolver.DecimalFormatter))]
        [DataRow(typeof(Mock<ITypeFormatter<DateTime>>),        nameof(TypeFormatterResolver.DateTimeFormatter))]
        [DataRow(typeof(Mock<ITypeFormatter<DateTimeOffset>>),  nameof(TypeFormatterResolver.DateTimeOffsetFormatter))]
        [DataRow(typeof(Mock<ITypeFormatter<Guid>>),            nameof(TypeFormatterResolver.GuidFormatter))]
        [DataRow(typeof(Mock<ITypeFormatter<byte[]>>),          nameof(TypeFormatterResolver.ByteArrayFormatter))]
        public void Formatters_Ctor_Sets_Direct_Access_Property(Type mockType, string formatterPropertyName)
        {
            var mockFormatter = MockHelper.CreateMock(mockType);
            var mockTypeFormatter = (ITypeFormatter)mockFormatter.Object;

            var resolver = new TypeFormatterResolver(mockTypeFormatter);

            var formatterProperty = resolver
                .GetType()
                .GetProperty(formatterPropertyName);

            var formatterValue = formatterProperty.GetValue(resolver, null);
            Assert.AreSame(mockTypeFormatter, formatterValue);
        }

        [TestMethod]
        public void GetAugmentedFormatters_Returns_Formatters()
        {
            var formatter = new DateTime_JavaScriptTicks_Formatter();
            var resolver = new TypeFormatterResolver(formatter);

            var formatters = resolver.GetAugmentedFormatters(null);
            Assert.AreEqual(1, formatters.Length);
            Assert.AreSame(formatter, formatters[0]);
        }

        [TestMethod]
        public void GetAugmentedFormatters_Augments_Registered_Formatters_With_List_Passed_In()
        {
            var originalDateTimeFormatter = new DateTime_JavaScriptTicks_Formatter();
            var originalDateTimeOffsetFormatter = new DateTimeOffset_JavaScriptTicks_Formatter();
            var newDateTimeFormatter = new DateTime_Iso8601_Formatter();
            var newByteArrayFormatter = new ByteArray_Mime64_Formatter();
            var resolver = new TypeFormatterResolver(originalDateTimeFormatter, originalDateTimeOffsetFormatter);

            var formatters = resolver.GetAugmentedFormatters(newDateTimeFormatter, null, newByteArrayFormatter);
            Assert.AreEqual(3, formatters.Length);
            Assert.IsTrue(formatters.Contains(newDateTimeFormatter));
            Assert.IsTrue(formatters.Contains(originalDateTimeOffsetFormatter));
            Assert.IsTrue(formatters.Contains(newByteArrayFormatter));

            var originalFormatters = resolver.GetFormatters();
            Assert.AreEqual(2, originalFormatters.Length);
            Assert.IsTrue(originalFormatters.Contains(originalDateTimeFormatter));
            Assert.IsTrue(originalFormatters.Contains(originalDateTimeOffsetFormatter));
        }

        [TestMethod]
        public void Find_Returns_Null_If_No_Formatter_Assigned()
        {
            var resolver = new TypeFormatterResolver();

            var formatter = resolver.Find<DateTime>();

            Assert.IsNull(formatter);
        }

        [TestMethod]
        public void Find_Returns_Assigned_Generic_TypeFormatterResolver()
        {
            var resolver = new TypeFormatterResolver(new DateTime_Iso8601_Formatter());

            var formatter = resolver.Find<DateTime>();

            Assert.IsInstanceOfType(formatter, typeof(DateTime_Iso8601_Formatter));
        }

        [TestMethod]
        public void Find_Returns_Null_If_Formatter_Assigned_To_Different_Type()
        {
            var resolver = new TypeFormatterResolver(new DateTimeOffset_Iso8601_Formatter());

            var formatter = resolver.Find<DateTime>();

            Assert.IsNull(formatter);
        }

        [TestMethod]
        public void GetFormatters_Returns_Empty_Collection_For_Empty_Resolver()
        {
            var resolver = new TypeFormatterResolver();

            var formatters = resolver.GetFormatters();

            Assert.AreEqual(0, formatters.Length);
        }

        [TestMethod]
        public void GetFormatters_Returns_Collection_Of_All_Registered_Formatters()
        {
            var formatter1 = new DateTime_Iso8601_Formatter();
            var formatter2 = new DateTimeOffset_JavaScriptTicks_Formatter();
            var resolver = new TypeFormatterResolver(formatter1, formatter2);

            var formatters = resolver.GetFormatters();

            Assert.AreEqual(2, formatters.Length);
            Assert.IsTrue(formatters.Contains(formatter1));
            Assert.IsTrue(formatters.Contains(formatter2));
        }

        [TestMethod]
        [DataRow(new Type[0],                                                                                               new Type[0],                                                                                             true)]  // LHS and RHS are empty
        [DataRow(new Type[0],                                                                                               null,                                                                                                    false)] // RHS is null
        [DataRow(new Type[] { typeof(DateTime_MicrosoftJson_Formatter) },                                                   new Type[] { typeof(DateTime_MicrosoftJson_Formatter) },                                                 true)]  // LHS and RHS contain same single type
        [DataRow(new Type[] { typeof(DateTime_MicrosoftJson_Formatter) },                                                   new Type[] { typeof(DateTime_Iso8601_Formatter) },                                                       false)] // LHS and RHS contain different single type
        [DataRow(new Type[] { typeof(DateTime_MicrosoftJson_Formatter), typeof(DateTimeOffset_MicrosoftJson_Formatter) },   new Type[] { typeof(DateTimeOffset_MicrosoftJson_Formatter), typeof(DateTime_MicrosoftJson_Formatter) }, true)]  // LHS and RHS contain same types in different order
        [DataRow(new Type[] { typeof(DateTime_MicrosoftJson_Formatter) },                                                   new Type[] { typeof(DateTimeOffset_MicrosoftJson_Formatter), typeof(DateTime_MicrosoftJson_Formatter) }, false)] // LHS is subset of RHS
        [DataRow(new Type[] { typeof(DateTime_MicrosoftJson_Formatter), typeof(DateTimeOffset_MicrosoftJson_Formatter) },   new Type[] { typeof(DateTime_MicrosoftJson_Formatter) },                                                 false)] // RHS is subset of LHS
        public void Equals_And_FormattersEquals_Returns_Expected_Value(Type[] lhsTypes, Type[] rhsTypes, bool expected)
        {
            ITypeFormatter[] typeFormatters(Type[] types)
            {
                var formatters = new List<ITypeFormatter>();
                foreach(var type in types) {
                    formatters.Add((ITypeFormatter)Activator.CreateInstance(type));
                }
                return formatters.ToArray();
            }

            var lhsResolver = new TypeFormatterResolver(typeFormatters(lhsTypes));
            var rhsFormatters =  rhsTypes == null ? null : typeFormatters(rhsTypes);
            var rhsResolver = rhsTypes == null ? null : new TypeFormatterResolver(rhsFormatters);

            var equalsActual = lhsResolver.Equals(rhsResolver);
            Assert.AreEqual(expected, equalsActual);

            // Bit naughty to be testing two functions but Equals is basically a wrapper around FormatterEquals and
            // I don't want to be doubling up the DataRows
            var formattersEqualsActual = lhsResolver.FormattersEquals(rhsFormatters);
            Assert.AreEqual(expected, formattersEqualsActual);

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
            var lhs = lhsIsNull ? null : new TypeFormatterResolver();
            var rhs = rhsIsNull ? null : new TypeFormatterResolver();

            var equal =    lhs == rhs;
            var notEqual = lhs != rhs;

            Assert.AreEqual(expectedEqual, equal);
            Assert.AreEqual(!expectedEqual, notEqual);
        }

        [TestMethod]
        public void GetHashCode_Is_Equal_For_All_Equal_Instances()
        {
            var resolver1 = new TypeFormatterResolver(
                new DateTime_MicrosoftJson_Formatter(),
                new DateTimeOffset_MicrosoftJson_Formatter()
            );
            var resolver2 = new TypeFormatterResolver(
                new DateTimeOffset_MicrosoftJson_Formatter(),
                new DateTime_MicrosoftJson_Formatter()
            );

            Assert.AreEqual(resolver1.GetHashCode(), resolver2.GetHashCode());
        }
    }
}
