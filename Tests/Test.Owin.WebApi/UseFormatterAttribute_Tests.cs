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
using AWhewell.Owin.Utility.Formatters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin.WebApi
{
    [TestClass]
    public class UseFormatterAttribute_Tests
    {
        [TestMethod]
        public void Usage_Is_Set_Correctly()
        {
            var usageAttribute = typeof(UseFormatterAttribute)
                .GetCustomAttributes(inherit: true)
                .OfType<AttributeUsageAttribute>()
                .FirstOrDefault();

            Assert.IsNotNull(usageAttribute);
            Assert.AreEqual(AttributeTargets.Class | AttributeTargets.Method, usageAttribute.ValidOn);
            Assert.AreEqual(true, usageAttribute.Inherited);
            Assert.AreEqual(false, usageAttribute.AllowMultiple);
        }

        [TestMethod]
        public void Ctor_Accepts_Single_Formatter()
        {
            var attr = new UseFormatterAttribute(typeof(ByteArray_HexString_Formatter));

            Assert.IsTrue(attr.IsValid);
            Assert.IsTrue(String.IsNullOrEmpty(attr.CtorErrorMessage));

            Assert.AreEqual(1, attr.Formatters.Length);
            Assert.IsInstanceOfType(attr.Formatters[0], typeof(ByteArray_HexString_Formatter));
        }

        [TestMethod]
        public void Ctor_Accepts_More_Than_One_Formatter()
        {
            var attr = new UseFormatterAttribute(typeof(DateTime_Iso8601_Formatter), typeof(DateTimeOffset_Iso8601_Formatter));

            Assert.IsTrue(attr.IsValid);
            Assert.IsTrue(String.IsNullOrEmpty(attr.CtorErrorMessage));

            Assert.AreEqual(2, attr.Formatters.Length);
            Assert.IsTrue(attr.Formatters.Any(r => r as DateTime_Iso8601_Formatter != null));
            Assert.IsTrue(attr.Formatters.Any(r => r as DateTimeOffset_Iso8601_Formatter != null));
        }

        [TestMethod]
        public void Ctor_Accepts_Empty_Set()
        {
            var attr = new UseFormatterAttribute();

            Assert.IsTrue(attr.IsValid);
            Assert.IsTrue(String.IsNullOrEmpty(attr.CtorErrorMessage));
            Assert.AreEqual(0, attr.Formatters.Length);
        }

        [TestMethod]
        public void Ctor_Rejects_Non_Formatter_Types()
        {
            var attr = new UseFormatterAttribute(typeof(string));

            Assert.IsFalse(attr.IsValid);
            Assert.IsFalse(String.IsNullOrEmpty(attr.CtorErrorMessage));
            Assert.AreEqual(0, attr.Formatters.Length);
        }

        [TestMethod]
        public void Ctor_Rejects_Multiple_Formatters_For_Same_Type()
        {
            var attr = new UseFormatterAttribute(typeof(DateTime_Iso8601_Formatter), typeof(DateTime_JavaScriptTicks_Formatter));

            Assert.IsFalse(attr.IsValid);
            Assert.IsFalse(String.IsNullOrEmpty(attr.CtorErrorMessage));
            Assert.AreEqual(0, attr.Formatters.Length);
        }

        [TestMethod]
        public void ToTypeFormatterResolver_NullDefault_Returns_Resolver_Filled_With_Formatters_From_Ctor()
        {
            var attr = new UseFormatterAttribute(typeof(DateTime_Iso8601_Formatter), typeof(DateTimeOffset_Iso8601_Formatter));

            var resolver = UseFormatterAttribute.ToTypeFormatterResolver(attr, null);

            Assert.IsInstanceOfType(resolver.Find<DateTime>(),       typeof(DateTime_Iso8601_Formatter));
            Assert.IsInstanceOfType(resolver.Find<DateTimeOffset>(), typeof(DateTimeOffset_Iso8601_Formatter));
        }

        [TestMethod]
        public void ToTypeFormatterResolver_NullDefault_Returns_Null_When_Attribute_Is_Null()
        {
            Assert.IsNull(UseFormatterAttribute.ToTypeFormatterResolver(null, null));
        }

        [TestMethod]
        public void ToTypeFormatterResolver_NullDefault_Returns_Null_If_Invalid()
        {
            var attr = new UseFormatterAttribute(typeof(string));

            Assert.IsNull(UseFormatterAttribute.ToTypeFormatterResolver(attr, null));
        }

        [TestMethod]
        public void ToTypeFormatterResolver_WithDefault_Returns_Resolver_Filled_With_Formatters_From_Ctor()
        {
            var attr = new UseFormatterAttribute(typeof(DateTime_Iso8601_Formatter), typeof(DateTimeOffset_Iso8601_Formatter));

            var defaultResolver = new TypeFormatterResolver(new DateTime_MicrosoftJson_Formatter(), new ByteArray_Mime64_Formatter());
            var resolver = UseFormatterAttribute.ToTypeFormatterResolver(attr, defaultResolver);

            Assert.AreEqual(3, resolver.GetFormatters().Length);
            Assert.IsInstanceOfType(resolver.DateTimeFormatter,       typeof(DateTime_Iso8601_Formatter));
            Assert.IsInstanceOfType(resolver.DateTimeOffsetFormatter, typeof(DateTimeOffset_Iso8601_Formatter));
            Assert.IsInstanceOfType(resolver.ByteArrayFormatter,      typeof(ByteArray_Mime64_Formatter));
        }

        [TestMethod]
        public void ToTypeFormatterResolver_WithDefault_Returns_Default_When_Attribute_Is_Null()
        {
            var defaultResolver = new TypeFormatterResolver(new DateTime_MicrosoftJson_Formatter(), new ByteArray_Mime64_Formatter());
            var resolver = UseFormatterAttribute.ToTypeFormatterResolver(null, defaultResolver);

            Assert.AreEqual(2, resolver.GetFormatters().Length);
            Assert.IsInstanceOfType(resolver.DateTimeFormatter,       typeof(DateTime_MicrosoftJson_Formatter));
            Assert.IsInstanceOfType(resolver.ByteArrayFormatter,      typeof(ByteArray_Mime64_Formatter));
        }

        [TestMethod]
        public void ToTypeFormatterResolver_WithDefault_Returns_Default_If_Invalid()
        {
            var attr = new UseFormatterAttribute(typeof(string));
            var defaultResolver = new TypeFormatterResolver(new DateTime_MicrosoftJson_Formatter(), new ByteArray_Mime64_Formatter());
            var resolver = UseFormatterAttribute.ToTypeFormatterResolver(attr, defaultResolver);

            Assert.AreEqual(2, resolver.GetFormatters().Length);
            Assert.IsInstanceOfType(resolver.DateTimeFormatter,       typeof(DateTime_MicrosoftJson_Formatter));
            Assert.IsInstanceOfType(resolver.ByteArrayFormatter,      typeof(ByteArray_Mime64_Formatter));
        }
    }
}
