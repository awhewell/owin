﻿// Copyright © 2019 onwards, Andrew Whewell
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
using System.Text;
using AWhewell.Owin.Interface.WebApi;
using AWhewell.Owin.Utility;
using AWhewell.Owin.Utility.Formatters;
using AWhewell.Owin.Utility.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin.WebApi
{
    [TestClass]
    public class ControllerType_Tests
    {
        [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
        public class Filter1Attribute : Attribute, IFilterAttribute
        {
            public bool AllowRequest(IDictionary<string, object> owinEnvironment) => throw new NotImplementedException();
        }

        public class Filter2Attribute : Attribute, IFilterAttribute
        {
            public bool AllowRequest(IDictionary<string, object> owinEnvironment) => throw new NotImplementedException();
        }

        public class SampleController1 : IApiController
        {
            public IDictionary<string, object> OwinEnvironment { get; set; }
        }

        [UseParser(typeof(DateTime_Iso8601_Parser), typeof(DateTimeOffset_Iso8601_Parser))]
        public class SampleController2 : IApiController
        {
            public IDictionary<string, object> OwinEnvironment { get; set; }
        }

        [UseFormatter(typeof(DateTime_Iso8601_Formatter), typeof(DateTimeOffset_Iso8601_Formatter))]
        public class SampleController3 : IApiController
        {
            public IDictionary<string, object> OwinEnvironment { get; set; }
        }

        [Filter1, Filter2]
        public class SampleController4 : IApiController
        {
            public IDictionary<string, object> OwinEnvironment { get; set; }
        }

        [TestMethod]
        public void Ctor_Fills_Type_Correctly()
        {
            var ctype = new ControllerType(typeof(SampleController1), null, null);

            Assert.AreEqual(typeof(SampleController1), ctype.Type);
        }

        [TestMethod]
        public void Ctor_Fills_FilterAttributes_Correctly()
        {
            var noFilters = new ControllerType(typeof(SampleController2), null, null);
            Assert.AreEqual(0, noFilters.FilterAttributes.Length);

            var twoFilters = new ControllerType(typeof(SampleController4), null, null);
            Assert.AreEqual(2, twoFilters.FilterAttributes.Length);
            Assert.IsTrue(twoFilters.FilterAttributes.Any(r => r is Filter1Attribute));
            Assert.IsTrue(twoFilters.FilterAttributes.Any(r => r is Filter2Attribute));
        }

        [TestMethod]
        public void Ctor_Fills_TypeParserResolver_Correctly()
        {
            var ctype1 = new ControllerType(typeof(SampleController1), null, null);
            var ctype2 = new ControllerType(typeof(SampleController2), null, null);

            Assert.IsNull(ctype1.TypeParserResolver);
            Assert.IsNotNull(ctype2.TypeParserResolver);
            Assert.IsInstanceOfType(ctype2.TypeParserResolver.Find<DateTime>(), typeof(DateTime_Iso8601_Parser));
        }

        [TestMethod]
        public void Ctor_Fills_TypeFormatterResolver_Correctly()
        {
            var ctype1 = new ControllerType(typeof(SampleController1), null, null);
            var ctype2 = new ControllerType(typeof(SampleController3), null, null);

            Assert.IsNull(ctype1.TypeFormatterResolver);
            Assert.IsNotNull(ctype2.TypeFormatterResolver);
            Assert.IsInstanceOfType(ctype2.TypeFormatterResolver.Find<DateTime>(), typeof(DateTime_Iso8601_Formatter));
        }

        [TestMethod]
        public void Ctor_Uses_Default_TypeParserResolver_When_No_Overrides_Supplied()
        {
            var defaultResolver = new TypeParserResolver(
                new ByteArray_Mime64_Parser(),
                new DateTime_Local_Parser()
            );

            var controllerType = new ControllerType(typeof(SampleController1), defaultResolver, null);

            var parsers = controllerType.TypeParserResolver.GetParsers();
            Assert.AreEqual(2, parsers.Length);
            Assert.IsTrue(parsers.Any(r => r.GetType() == typeof(ByteArray_Mime64_Parser)));
            Assert.IsTrue(parsers.Any(r => r.GetType() == typeof(DateTime_Local_Parser)));
        }

        [TestMethod]
        public void Ctor_Uses_Default_TypeFormatterResolver_When_No_Overrides_Supplied()
        {
            var defaultResolver = new TypeFormatterResolver(
                new ByteArray_HexString_Formatter(),
                new DateTime_MicrosoftJson_Formatter()
            );

            var controllerType = new ControllerType(typeof(SampleController1), null, defaultResolver);

            var formatters = controllerType.TypeFormatterResolver.GetFormatters();
            Assert.AreEqual(2, formatters.Length);
            Assert.IsTrue(formatters.Any(r => r.GetType() == typeof(ByteArray_HexString_Formatter)));
            Assert.IsTrue(formatters.Any(r => r.GetType() == typeof(DateTime_MicrosoftJson_Formatter)));
        }

        [TestMethod]
        public void Ctor_Applies_Overrides_Default_TypeParserResolver()
        {
            var defaultResolver = new TypeParserResolver(
                new ByteArray_HexString_Parser(),
                new DateTime_Local_Parser()
            );

            var controllerType = new ControllerType(typeof(SampleController2), defaultResolver, null);

            var parsers = controllerType.TypeParserResolver.GetParsers();
            Assert.AreEqual(3, parsers.Length);
            Assert.IsTrue(parsers.Any(r => r.GetType() == typeof(ByteArray_HexString_Parser)));
            Assert.IsTrue(parsers.Any(r => r.GetType() == typeof(DateTime_Iso8601_Parser)));
            Assert.IsTrue(parsers.Any(r => r.GetType() == typeof(DateTimeOffset_Iso8601_Parser)));
        }

        [TestMethod]
        public void Ctor_Applies_Overrides_Default_TypeFormatterResolver()
        {
            var defaultResolver = new TypeFormatterResolver(
                new ByteArray_HexString_Formatter(),
                new DateTime_MicrosoftJson_Formatter()
            );

            var controllerType = new ControllerType(typeof(SampleController3), null, defaultResolver);

            var formatters = controllerType.TypeFormatterResolver.GetFormatters();
            Assert.AreEqual(3, formatters.Length);
            Assert.IsTrue(formatters.Any(r => r.GetType() == typeof(ByteArray_HexString_Formatter)));
            Assert.IsTrue(formatters.Any(r => r.GetType() == typeof(DateTime_Iso8601_Formatter)));
            Assert.IsTrue(formatters.Any(r => r.GetType() == typeof(DateTimeOffset_Iso8601_Formatter)));
        }
    }
}
