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

namespace Test.AWhewell.Owin.WebApi
{
    [TestClass]
    public class WebApiResponder_Tests
    {
        class Guid_UpperNoHyphens_Formatter : ITypeFormatter<Guid>
        {
            public string Format(Guid value) => value.ToString().ToUpper().Replace("-", "");
        }

        class Guid_LowerNoHyphens_Formatter : ITypeFormatter<Guid>
        {
            public string Format(Guid value) => value.ToString().ToLower().Replace("-", "");
        }

        class Controller : IApiController
        {
            public IDictionary<string, object> OwinEnvironment { get; set; }

            [Route("int")]
            public int IntMethod() { return 0; }

            [Route("void")]
            public void VoidMethod() { ; }
        }

        private IClassFactory       _Snapshot;

        private IWebApiResponder    _Responder;
        private MockOwinEnvironment _Environment;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _Environment = new MockOwinEnvironment();
            _Environment.AddRequiredFields();

            _Responder = Factory.Resolve<IWebApiResponder>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        [TestMethod]
        [DataRow(typeof(string),            null,                                   "null")]
        [DataRow(typeof(string),            "Hello",                                "\"Hello\"")]
        [DataRow(typeof(bool),              true,                                   "true")]
        [DataRow(typeof(double),            -1234.56,                               "-1234.56")]
        [DataRow(typeof(DateTime),          "2019-07-01 12:34:56.789 local",        "\"2019-07-01T12:34:56.789+01:00\"")]
        [DataRow(typeof(DateTime),          "2019-07-01 12:34:56.789 utc",          "\"2019-07-01T12:34:56.789Z\"")]
        [DataRow(typeof(DateTime),          "2019-07-01 12:34:56.789",              "\"2019-07-01T12:34:56.789\"")]
        [DataRow(typeof(DateTimeOffset),    "2019-07-01 12:34:56.789 +0239",        "\"2019-07-01T12:34:56.789+02:39\"")]
        [DataRow(typeof(byte[]),            new byte[] { 1, 2, 3 },                 "\"AQID\"")]
        [DataRow(typeof(Guid),              "c151f1a8-d235-4f28-8c0d-5521a768be9e", "\"c151f1a8-d235-4f28-8c0d-5521a768be9e\"")]
        public void ReturnJsonObject_Returns_Simple_Values_Correctly(Type type, object value, string expectedBody)
        {
            var route = Route_Tests.CreateRoute<Controller>(nameof(Controller.IntMethod));
            _Environment.Environment[WebApiEnvironmentKey.Route] = route;

            using(new CultureSwap("en-GB")) {
                var parsedValue = DataRowParser.ConvertExpected(type, value);

                _Responder.ReturnJsonObject(_Environment.Environment, parsedValue);

                Assert.AreEqual("application/json; charset=utf-8",                          _Environment.ResponseHeadersDictionary["Content-Type"]);
                Assert.AreEqual(expectedBody.Length.ToString(CultureInfo.InvariantCulture), _Environment.ResponseHeadersDictionary["Content-Length"]);
                Assert.AreEqual(expectedBody,                                               _Environment.ResponseBodyText);
            }
        }

        [TestMethod]
        public void ReturnJsonObject_Uses_Formatter_From_Route()
        {
            var resolver = TypeFormatterResolverCache.Find(new Guid_UpperNoHyphens_Formatter());
            var route = Route_Tests.CreateRoute<Controller>(nameof(Controller.IntMethod), formatterResolver: resolver);
            _Environment.Environment[WebApiEnvironmentKey.Route] = route;
            var guid = Guid.Parse("c151f1a8-d235-4f28-8c0d-5521a768be9e");

            _Responder.ReturnJsonObject(_Environment.Environment, guid);

            var actual = _Environment.ResponseBodyText;
            Assert.AreEqual("\"C151F1A8D2354F288C0D5521A768BE9E\"", actual);
        }

        [TestMethod]
        public void ReturnJsonObject_Will_Write_Body_For_Void_Routes()
        {
            var route = Route_Tests.CreateRoute<Controller>(nameof(Controller.VoidMethod));
            _Environment.Environment[WebApiEnvironmentKey.Route] = route;
            var value = 7;

            _Responder.ReturnJsonObject(_Environment.Environment, value);

            Assert.AreEqual("application/json; charset=utf-8",  _Environment.ResponseHeadersDictionary["Content-Type"]);
            Assert.AreEqual("1",                                _Environment.ResponseHeadersDictionary["Content-Length"]);
            Assert.AreEqual("7",                                _Environment.ResponseBodyText);
        }

        [TestMethod]
        public void ReturnJsonObject_Copes_If_Environment_Does_Not_Contain_Route()
        {
            var value = 7;

            _Responder.ReturnJsonObject(_Environment.Environment, value);

            Assert.AreEqual("application/json; charset=utf-8",  _Environment.ResponseHeadersDictionary["Content-Type"]);
            Assert.AreEqual("1",                                _Environment.ResponseHeadersDictionary["Content-Length"]);
            Assert.AreEqual("7",                                _Environment.ResponseBodyText);
        }

        [TestMethod]
        [DataRow("©", "utf-8",  "text/plain")]
        [DataRow("©", "utf-16", "foo-foo/magoo")]
        [DataRow("©", "utf-8",  null)]
        [DataRow("©", "utf-8",  "")]
        [DataRow("©", null,     "application/json")]
        public void ReturnJsonObject_FullControlVersion_Sets_Environment_Correctly(string input, string encodingName, string mimeType)
        {
            var encoding = DataRowParser.Encoding(encodingName);
            var expectedEncoding = encoding ?? Encoding.UTF8;
            var expectedMimeType = String.IsNullOrEmpty(mimeType)
                ? "application/json"
                : mimeType;
            var expectedText = input == null
                ? "null"
                : $"\"{input}\"";
            var expectedBody = expectedEncoding.GetBytes(expectedText);
            var expectedLength = expectedBody.Length.ToString(CultureInfo.InvariantCulture);

            _Responder.ReturnJsonObject(_Environment.Environment, input, null, encoding, mimeType);

            var contentType = _Environment.ResponseHeadersDictionary.ContentTypeValue;

            Assert.AreEqual(expectedMimeType,           contentType.MediaType);
            Assert.AreEqual(expectedEncoding.WebName,   contentType.Charset);
            Assert.AreEqual(expectedLength,             _Environment.ResponseHeadersDictionary["Content-Length"]);
            Assertions.AreEqual(expectedBody,           _Environment.ResponseBodyBytes);
        }

        [TestMethod]
        public void ReturnJsonObject_FullControlVersion_Uses_Formatter_In_Preference_To_Route_Formatter()
        {
            var routeResolver = TypeFormatterResolverCache.Find(new Guid_UpperNoHyphens_Formatter());
            var usedResolver =  TypeFormatterResolverCache.Find(new Guid_LowerNoHyphens_Formatter());
            var route = Route_Tests.CreateRoute<Controller>(nameof(Controller.IntMethod), formatterResolver: routeResolver);
            _Environment.Environment[WebApiEnvironmentKey.Route] = route;
            var guid = Guid.Parse("c151f1a8-d235-4f28-8c0d-5521a768be9e");

            _Responder.ReturnJsonObject(_Environment.Environment, guid, usedResolver, Encoding.UTF8, null);

            var actual = _Environment.ResponseBodyText;
            Assert.AreEqual("\"c151f1a8d2354f288c0d5521a768be9e\"", actual);
        }
    }
}
