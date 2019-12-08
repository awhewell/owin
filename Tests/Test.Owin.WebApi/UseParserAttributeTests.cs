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
using System.Linq;
using AWhewell.Owin.Interface.WebApi;
using AWhewell.Owin.Utility;
using AWhewell.Owin.Utility.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin.WebApi
{
    [TestClass]
    public class UseParserAttributeTests
    {
        [TestMethod]
        public void Usage_Is_Set_Correctly()
        {
            var usageAttribute = typeof(UseParserAttribute)
                .GetCustomAttributes(inherit: true)
                .OfType<AttributeUsageAttribute>()
                .FirstOrDefault();

            Assert.IsNotNull(usageAttribute);
            Assert.AreEqual(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter, usageAttribute.ValidOn);
            Assert.AreEqual(true, usageAttribute.Inherited);
            Assert.AreEqual(false, usageAttribute.AllowMultiple);
        }

        [TestMethod]
        public void Ctor_Accepts_Single_Parser()
        {
            var attr = new UseParserAttribute(typeof(DateTime_Local_Parser));

            Assert.IsTrue(attr.IsValid);
            Assert.IsTrue(String.IsNullOrEmpty(attr.CtorErrorMessage));

            Assert.AreEqual(1, attr.Parsers.Length);
            Assert.IsInstanceOfType(attr.Parsers[0], typeof(DateTime_Local_Parser));
        }

        [TestMethod]
        public void Ctor_Accepts_More_Than_One_Parser()
        {
            var attr = new UseParserAttribute(typeof(DateTime_Local_Parser), typeof(DateTimeOffset_Local_Parser));

            Assert.IsTrue(attr.IsValid);
            Assert.IsTrue(String.IsNullOrEmpty(attr.CtorErrorMessage));

            Assert.AreEqual(2, attr.Parsers.Length);
            Assert.IsTrue(attr.Parsers.Any(r => r as DateTime_Local_Parser != null));
            Assert.IsTrue(attr.Parsers.Any(r => r as DateTimeOffset_Local_Parser != null));
        }

        [TestMethod]
        public void Ctor_Accepts_Empty_Set()
        {
            var attr = new UseParserAttribute();

            Assert.IsTrue(attr.IsValid);
            Assert.IsTrue(String.IsNullOrEmpty(attr.CtorErrorMessage));
            Assert.AreEqual(0, attr.Parsers.Length);
        }

        [TestMethod]
        public void Ctor_Rejects_Non_Parser_Types()
        {
            var attr = new UseParserAttribute(typeof(string));

            Assert.IsFalse(attr.IsValid);
            Assert.IsFalse(String.IsNullOrEmpty(attr.CtorErrorMessage));
            Assert.AreEqual(0, attr.Parsers.Length);
        }

        [TestMethod]
        public void Ctor_Rejects_Multiple_Parsers_For_Same_Type()
        {
            var attr = new UseParserAttribute(typeof(DateTime_Invariant_Parser), typeof(DateTime_Local_Parser));

            Assert.IsFalse(attr.IsValid);
            Assert.IsFalse(String.IsNullOrEmpty(attr.CtorErrorMessage));
            Assert.AreEqual(0, attr.Parsers.Length);
        }

        [TestMethod]
        public void ToTypeParserResolver_NullDefault_Returns_Resolver_Filled_With_Parsers_From_Ctor()
        {
            var attr = new UseParserAttribute(typeof(DateTime_Iso8601_Parser), typeof(DateTimeOffset_Iso8601_Parser));

            var resolver = UseParserAttribute.ToTypeParserResolver(attr, null);

            Assert.IsInstanceOfType(resolver.Find<DateTime>(),       typeof(DateTime_Iso8601_Parser));
            Assert.IsInstanceOfType(resolver.Find<DateTimeOffset>(), typeof(DateTimeOffset_Iso8601_Parser));
        }

        [TestMethod]
        public void ToTypeParserResolver_NullDefault_Returns_Null_When_Attribute_Is_Null()
        {
            Assert.IsNull(UseParserAttribute.ToTypeParserResolver(null, null));
        }

        [TestMethod]
        public void ToTypeParserResolver_NullDefault_Returns_Null_If_Invalid()
        {
            var attr = new UseParserAttribute(typeof(string));

            Assert.IsNull(UseParserAttribute.ToTypeParserResolver(attr, null));
        }

        [TestMethod]
        public void ToTypeParserResolver_WithDefault_Returns_Resolver_Filled_With_Parsers_From_Ctor()
        {
            var attr = new UseParserAttribute(typeof(DateTime_Iso8601_Parser), typeof(DateTimeOffset_Iso8601_Parser));

            var defaultResolver = new TypeParserResolver(new DateTime_Local_Parser(), new ByteArray_Mime64_Parser());
            var resolver = UseParserAttribute.ToTypeParserResolver(attr, defaultResolver);

            Assert.AreEqual(3, resolver.GetParsers().Length);
            Assert.IsInstanceOfType(resolver.DateTimeParser,       typeof(DateTime_Iso8601_Parser));
            Assert.IsInstanceOfType(resolver.DateTimeOffsetParser, typeof(DateTimeOffset_Iso8601_Parser));
            Assert.IsInstanceOfType(resolver.ByteArrayParser,      typeof(ByteArray_Mime64_Parser));
        }

        [TestMethod]
        public void ToTypeParserResolver_WithDefault_Returns_Default_When_Attribute_Is_Null()
        {
            var defaultResolver = new TypeParserResolver(new DateTime_Local_Parser(), new ByteArray_Mime64_Parser());
            var resolver = UseParserAttribute.ToTypeParserResolver(null, defaultResolver);

            Assert.AreEqual(2, resolver.GetParsers().Length);
            Assert.IsInstanceOfType(resolver.DateTimeParser,       typeof(DateTime_Local_Parser));
            Assert.IsInstanceOfType(resolver.ByteArrayParser,      typeof(ByteArray_Mime64_Parser));
        }

        [TestMethod]
        public void ToTypeParserResolver_WithDefault_Returns_Default_If_Invalid()
        {
            var attr = new UseParserAttribute(typeof(string));
            var defaultResolver = new TypeParserResolver(new DateTime_Local_Parser(), new ByteArray_Mime64_Parser());
            var resolver = UseParserAttribute.ToTypeParserResolver(attr, defaultResolver);

            Assert.AreEqual(2, resolver.GetParsers().Length);
            Assert.IsInstanceOfType(resolver.DateTimeParser,       typeof(DateTime_Local_Parser));
            Assert.IsInstanceOfType(resolver.ByteArrayParser,      typeof(ByteArray_Mime64_Parser));
        }
    }
}
