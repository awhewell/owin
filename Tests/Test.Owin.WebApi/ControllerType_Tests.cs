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
using System.Text;
using AWhewell.Owin.Interface.WebApi;
using AWhewell.Owin.Utility;
using AWhewell.Owin.Utility.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin.WebApi
{
    [TestClass]
    public class ControllerType_Tests
    {
        public class SampleController1 : IApiController
        {
            public IDictionary<string, object> OwinEnvironment { get; set; }
        }

        [UseParser(typeof(DateTime_Iso8601_Parser), typeof(DateTimeOffset_Iso8601_Parser))]
        public class SampleController2 : IApiController
        {
            public IDictionary<string, object> OwinEnvironment { get; set; }
        }

        [TestMethod]
        public void Ctor_Fills_Type_Correctly()
        {
            var ctype = new ControllerType(typeof(SampleController1), null);

            Assert.AreEqual(typeof(SampleController1), ctype.Type);
        }

        [TestMethod]
        public void Ctor_Fills_TypeParserResolver_Correctly()
        {
            var ctype1 = new ControllerType(typeof(SampleController1), null);
            var ctype2 = new ControllerType(typeof(SampleController2), null);

            Assert.IsNull(ctype1.TypeParserResolver);
            Assert.IsNotNull(ctype2.TypeParserResolver);
            Assert.IsInstanceOfType(ctype2.TypeParserResolver.Find<DateTime>(), typeof(DateTime_Iso8601_Parser));
        }

        [TestMethod]
        public void Ctor_Uses_Default_TypeParserResolver_When_No_Overrides_Supplied()
        {
            var defaultResolver = new TypeParserResolver(
                new ByteArray_Mime64_Parser(),
                new DateTime_Local_Parser()
            );

            var controllerType = new ControllerType(typeof(SampleController1), defaultResolver);

            var parsers = controllerType.TypeParserResolver.GetParsers();
            Assert.AreEqual(2, parsers.Length);
            Assert.IsTrue(parsers.Any(r => r.GetType() == typeof(ByteArray_Mime64_Parser)));
            Assert.IsTrue(parsers.Any(r => r.GetType() == typeof(DateTime_Local_Parser)));
        }

        [TestMethod]
        public void Ctor_Applies_Overrides_Default_TypeParserResolver()
        {
            var defaultResolver = new TypeParserResolver(
                new ByteArray_Mime64_Parser(),
                new DateTime_Local_Parser()
            );

            var controllerType = new ControllerType(typeof(SampleController2), defaultResolver);

            var parsers = controllerType.TypeParserResolver.GetParsers();
            Assert.AreEqual(3, parsers.Length);
            Assert.IsTrue(parsers.Any(r => r.GetType() == typeof(ByteArray_Mime64_Parser)));
            Assert.IsTrue(parsers.Any(r => r.GetType() == typeof(DateTime_Iso8601_Parser)));
            Assert.IsTrue(parsers.Any(r => r.GetType() == typeof(DateTimeOffset_Iso8601_Parser)));
        }
    }
}
