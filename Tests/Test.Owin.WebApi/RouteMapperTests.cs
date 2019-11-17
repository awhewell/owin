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
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AWhewell.Owin.Interface.WebApi;
using System.Collections;

namespace Test.AWhewell.Owin.WebApi
{
    [TestClass]
    public class RouteMapperTests
    {
        public class Controller : IApiController
        {
            public IDictionary<string, object> OwinEnvironment { get; set; }
        }

        private IRouteMapper        _RouteMapper;
        private MockOwinEnvironment _Environment;

        [TestInitialize]
        public void TestInitialise()
        {
            _Environment = new MockOwinEnvironment();
            _RouteMapper = Factory.Resolve<IRouteMapper>();
        }

        [TestMethod]
        public void Ctor_Initialises_Properties()
        {
            Assert.IsTrue(_RouteMapper.AreFormNamesCaseSensitive);
            Assert.IsTrue(_RouteMapper.AreQueryStringNamesCaseSensitive);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AreFormNamesCaseSensitive_Throws_If_Changed_After_Initialise_Called()
        {
            _RouteMapper.Initialise(new Route[0]);
            _RouteMapper.AreFormNamesCaseSensitive = !_RouteMapper.AreFormNamesCaseSensitive;
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AreQueryStringNamesCaseSensitive_Throws_If_Changed_After_Initialise_Called()
        {
            _RouteMapper.Initialise(new Route[0]);
            _RouteMapper.AreQueryStringNamesCaseSensitive = !_RouteMapper.AreQueryStringNamesCaseSensitive;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Initialise_Throws_If_Passed_Null()
        {
            _RouteMapper.Initialise(null);
        }

        [TestMethod]
        public void Initialise_Accepts_Empty_Routes()
        {
            _RouteMapper.Initialise(new Route[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Initialise_Throws_If_Called_Twice()
        {
            _RouteMapper.Initialise(new Route[0]);
            _RouteMapper.Initialise(new Route[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FindRouteForPath_Throws_If_HttpMethod_Is_Null()
        {
            _RouteMapper.FindRouteForPath(null, new string[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FindRouteForPath_Throws_If_PathParts_Are_Null()
        {
            _RouteMapper.FindRouteForPath("GET", null);
        }

        public class SimplePathController : Controller { [HttpGet, Route("simple-path")] public int Method() { return 0; } }

        [TestMethod]
        [DataRow("GET",     "simple-path", true)]   // Case matches normalised cases in Route
        [DataRow("get",     "simple-path", true)]   // HTTP method does not match Route's normalised case
        [DataRow("GET",     "SIMPLE-PATH", true)]   // Path parts does not match Route's normalised case
        [DataRow("POST",    "simple-path", false)]
        public void FindRouteForPath_Returns_Correct_Candidates(string httpMethod, string pathPart, bool expectResult)
        {
            var expected = RouteTests.CreateRoute<SimplePathController>(nameof(SimplePathController.Method));
            _RouteMapper.Initialise(new Route[] { expected });

            var actual = _RouteMapper.FindRouteForPath(httpMethod, new string[] { pathPart });

            if(!expectResult) {
                Assert.IsNull(actual);
            } else {
                Assert.AreSame(expected, actual);
            }
        }

        public class ApiEntityController : Controller                   { [HttpGet, Route("api/entity")]            public int Method() { return 0; } }
        public class ApiEntityWithIDController : Controller             { [HttpGet, Route("api/entity/{id}")]       public int Method(int id) { return 0; } }
        public class ApiEntityWithOptionalIDController : Controller     { [HttpGet, Route("api/entity/{id}")]       public int Method(int id = -1) { return 0; } }
        public class ApiEntityOptionalThenNotController : Controller    { [HttpGet, Route("api/entity/{id}/{not}")] public int Method(int not, int id = 0) { return 0; } }

        [TestMethod]
        [DataRow("GET", typeof(ApiEntityController),                new string[] { "api", "entity" },           true)]
        [DataRow("GET", typeof(ApiEntityController),                new string[] { "api", },                    false)]
        [DataRow("GET", typeof(ApiEntityController),                new string[] { "api", "entity", "leaf" },   false)]
        [DataRow("GET", typeof(ApiEntityWithIDController),          new string[] { "api", "entity", "1" },      true)]
        [DataRow("GET", typeof(ApiEntityWithIDController),          new string[] { "api", "entity" },           false)]
        [DataRow("GET", typeof(ApiEntityWithOptionalIDController),  new string[] { "api", "entity", "1" },      true)]
        [DataRow("GET", typeof(ApiEntityWithOptionalIDController),  new string[] { "api", "entity" },           true)]
        [DataRow("GET", typeof(ApiEntityOptionalThenNotController), new string[] { "api", "entity", "1", "2" }, true)]
        [DataRow("GET", typeof(ApiEntityOptionalThenNotController), new string[] { "api", "entity", "1" },      false)]
        [DataRow("GET", typeof(ApiEntityOptionalThenNotController), new string[] { "api", "entity" },           false)]
        public void FindRouteForPath_Matches_MultiPart_Paths(string httpMethod, Type controllerType, string[] pathParts, bool expectMatch)
        {
            var route = RouteTests.CreateRoute(controllerType, "Method");
            _RouteMapper.Initialise(new Route[] { route });

            var actual = _RouteMapper.FindRouteForPath(httpMethod, pathParts);

            if(!expectMatch) {
                Assert.IsNull(actual);
            } else {
                Assert.IsNotNull(actual);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BuildRouteParameters_Throws_If_Route_Is_Null()
        {
            var route = RouteTests.CreateRoute(typeof(ApiEntityWithIDController), nameof(ApiEntityWithIDController.Method));
            _RouteMapper.Initialise(new Route[] { route });

            _RouteMapper.BuildRouteParameters(null, new string[] { "api", "entity", "1" }, _Environment.Environment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BuildRouteParameters_Throws_If_PathParts_Is_Null()
        {
            var route = RouteTests.CreateRoute(typeof(ApiEntityWithIDController), nameof(ApiEntityWithIDController.Method));
            _RouteMapper.Initialise(new Route[] { route });

            _RouteMapper.BuildRouteParameters(route, null, _Environment.Environment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BuildRouteParameters_Throws_If_Environment_Is_Null()
        {
            var route = RouteTests.CreateRoute(typeof(ApiEntityWithIDController), nameof(ApiEntityWithIDController.Method));
            _RouteMapper.Initialise(new Route[] { route });

            _RouteMapper.BuildRouteParameters(route, new string[] { "api", "entity", "1" }, null);
        }

        [TestMethod]
        public void BuildRouteParameters_Rejects_Call_If_Route_Was_Not_Initialised()
        {
            var initialisedRoute = RouteTests.CreateRoute(typeof(ApiEntityWithIDController), nameof(ApiEntityWithIDController.Method));
            var uninitialisedRoute = RouteTests.CreateRoute(typeof(ApiEntityWithOptionalIDController), nameof(ApiEntityWithOptionalIDController.Method));
            _RouteMapper.Initialise(new Route[] { initialisedRoute });

            var parameters = _RouteMapper.BuildRouteParameters(uninitialisedRoute, new string[] { "api", "entity", "1" }, _Environment.Environment);

            Assert.IsFalse(parameters.IsValid);
        }

        [TestMethod]
        public void BuildRouteParameters_Extracts_Values_From_Path_Parts()
        {
            var route = RouteTests.CreateRoute(typeof(ApiEntityWithIDController), nameof(ApiEntityWithIDController.Method));
            _RouteMapper.Initialise(new Route[] { route });

            var parameters = _RouteMapper.BuildRouteParameters(route, new string[] { "api", "entity", "12" }, _Environment.Environment);

            Assert.IsTrue(parameters.IsValid);
            Assert.AreEqual(1, parameters.Parameters.Length);
            Assert.IsInstanceOfType(parameters.Parameters[0], typeof(int));
            Assert.AreEqual(12, (int)parameters.Parameters[0]);
        }

        [TestMethod]
        public void BuildRouteParameters_Uses_Default_If_Optional_Path_Part_Is_Missing()
        {
            var route = RouteTests.CreateRoute(typeof(ApiEntityWithOptionalIDController), nameof(ApiEntityWithOptionalIDController.Method));
            _RouteMapper.Initialise(new Route[] { route });

            var parameters = _RouteMapper.BuildRouteParameters(route, new string[] { "api", "entity" }, _Environment.Environment);

            Assert.IsTrue(parameters.IsValid);
            Assert.AreEqual(1, parameters.Parameters.Length);
            Assert.IsInstanceOfType(parameters.Parameters[0], typeof(int));
            Assert.AreEqual(-1, (int)parameters.Parameters[0]);
        }

        [TestMethod]
        public void BuildRouteParameters_Clears_IsValid_When_Parameter_Cannot_Be_Parsed()
        {
            var route = RouteTests.CreateRoute(typeof(ApiEntityWithIDController), nameof(ApiEntityWithIDController.Method));
            _RouteMapper.Initialise(new Route[] { route });

            var parameters = _RouteMapper.BuildRouteParameters(route, new string[] { "api", "entity", "not-an-int" }, _Environment.Environment);

            Assert.IsFalse(parameters.IsValid);
        }

        // The expectation is that the implementation uses Owin.Utility.Parser to parse the path part string into the
        // correct type. There are a lot of tests on Parser to make sure it copes with various possible inputs, it would
        // be a lot of effort to reproduce them all here. These tests just cover the basics re. types and corner cases.
        public class PathPartTypeController : Controller
        {
            [HttpGet, Route("string/{param}")]          public int StringPP(string param)                                           { return 0; }
            [HttpGet, Route("int/{param}")]             public int IntPP(int param)                                                 { return 0; }
            [HttpGet, Route("n-int/{param}")]           public int NIntPP(int? param)                                               { return 0; }
            [HttpGet, Route("double/{param}")]          public int DoublePP(double param)                                           { return 0; }
            [HttpGet, Route("datetime/{param}")]        public int DateTimePP(DateTime param)                                       { return 0; }
            [HttpGet, Route("datetimeoffset/{param}")]  public int DateTimeOffsetPP(DateTimeOffset param)                           { return 0; }
            [HttpGet, Route("default-bytes/{param}")]   public int DefaultByteArrayPP(byte[] param)                                 { return 0; }
            [HttpGet, Route("hex-bytes/{param}")]       public int HexByteArrayPP([Expect(ExpectFormat.HexString)] byte[] param)    { return 0; }
            [HttpGet, Route("mime64-bytes/{param}")]    public int Mime64ByteArrayPP([Expect(ExpectFormat.Mime64)] byte[] param)    { return 0; }

        }

        [DataRow(nameof(PathPartTypeController.StringPP),           "",                             "en-GB", "")]
        [DataRow(nameof(PathPartTypeController.IntPP),              "1",                            "en-GB", 1)]
        [DataRow(nameof(PathPartTypeController.NIntPP),             "1",                            "en-GB", 1)]
        [DataRow(nameof(PathPartTypeController.DoublePP),           "1.2",                          "en-GB", 1.2)]
        [DataRow(nameof(PathPartTypeController.DoublePP),           "1.2",                          "de-DE", 1.2)]
        [DataRow(nameof(PathPartTypeController.DateTimePP),         "2019-07-01T22:53:47+00:00",    "en-GB", "2019-07-01T22:53:47+00:00")]
        [DataRow(nameof(PathPartTypeController.DateTimeOffsetPP),   "2019-07-01T22:53:47+00:00",    "en-US", "2019-07-01T22:53:47+00:00")]
        [DataRow(nameof(PathPartTypeController.DefaultByteArrayPP), "0x01",  /* mime encoded */     "en-GB", new byte[] { 211, 29, 53 })]
        [DataRow(nameof(PathPartTypeController.HexByteArrayPP),     "0x01",                         "en-GB", new byte[] { 1 })]
        [DataRow(nameof(PathPartTypeController.HexByteArrayPP),     "01",                           "en-GB", new byte[] { 1 })]
        [DataRow(nameof(PathPartTypeController.Mime64ByteArrayPP),  "0x01",                         "en-GB", new byte[] { 211, 29, 53 })]
        [TestMethod]
        public void BuildRouteParameters_Parses_Different_Types_From_Path_Parts(string methodName, string pathPart, string culture, object rawExpected)
        {
            using(new CultureSwap(culture)) {
                var route = RouteTests.CreateRoute(typeof(PathPartTypeController), methodName);
                _RouteMapper.Initialise(new Route[] { route });

                var parameters = _RouteMapper.BuildRouteParameters(route, new string[] { route.PathParts[0].Part, pathPart }, _Environment.Environment);

                var expected = rawExpected;
                switch(methodName) {
                    case nameof(PathPartTypeController.DateTimePP):
                        expected = DateTime.Parse((string)rawExpected, CultureInfo.InvariantCulture);
                        break;
                    case nameof(PathPartTypeController.DateTimeOffsetPP):
                        expected = DateTimeOffset.Parse((string)rawExpected, CultureInfo.InvariantCulture);
                        break;
                }

                Assert.IsTrue(parameters.IsValid);
                Assert.AreEqual(1, parameters.Parameters.Length);
                var actual = parameters.Parameters[0];
                if(actual is byte[] byteArray) {
                    var expectedByteArray = (byte[])expected;
                    Assert.IsTrue(expectedByteArray.SequenceEqual(byteArray));
                } else {
                    Assert.AreEqual(expected, parameters.Parameters[0]);
                }
            }
        }

        public class MultipleParameterController : Controller
        {
            [HttpGet, Route("multiple/{stringValue}/{intValue}")]
            public int Method(string stringValue, int intValue) { return 0; }
        }

        [TestMethod]
        public void BuildRouteParameters_Can_Fill_Multiple_Path_Part_Parameters()
        {
            var route = RouteTests.CreateRoute(typeof(MultipleParameterController), nameof(MultipleParameterController.Method));
            _RouteMapper.Initialise(new Route[] { route });

            var parameters = _RouteMapper.BuildRouteParameters(route, new string[] { route.PathParts[0].Part, "string-value", "55" }, _Environment.Environment);

            Assert.IsTrue(parameters.IsValid);
            Assert.AreEqual(2, parameters.Parameters.Length);
            Assert.AreEqual("string-value", parameters.Parameters[0]);
            Assert.AreEqual(55, parameters.Parameters[1]);
        }

        public class OwinEnvController : Controller
        {
            [HttpGet, Route("static-env")]
            public static int JustEnvironment(IDictionary<string, object> env) { return 0; }

            [HttpGet, Route("not-static-env")]
            public int NotStatic(IDictionary<string, object> env) { return 0; }
        }

        [TestMethod]
        public void BuildRouteParameters_Can_Inject_Owin_Environment_To_Static_Route_Method()
        {
            var route = RouteTests.CreateRoute(typeof(OwinEnvController), nameof(OwinEnvController.JustEnvironment));
            _RouteMapper.Initialise(new Route[] { route });

            var parameters = _RouteMapper.BuildRouteParameters(route, new string[] { route.PathParts[0].Part }, _Environment.Environment);

            Assert.IsTrue(parameters.IsValid);
            Assert.AreEqual(1, parameters.Parameters.Length);
            Assert.AreSame(_Environment.Environment, parameters.Parameters[0]);
        }

        [TestMethod]
        public void BuildRouteParameters_Does_Not_Inject_Owin_Environment_To_Instance_Route_Method()
        {
            var route = RouteTests.CreateRoute(typeof(OwinEnvController), nameof(OwinEnvController.NotStatic));
            _RouteMapper.Initialise(new Route[] { route });

            var parameters = _RouteMapper.BuildRouteParameters(route, new string[] { route.PathParts[0].Part }, _Environment.Environment);

            Assert.IsFalse(parameters.IsValid);
        }

        // This is expected to use Parser.ParseType so the test of conversions is not exhaustive, it's just a handful
        // of basic tests to make sure things appear to be working OK
        public class QueryStringController : Controller
        {
            [HttpGet, Route("string")]      public int StringParam(string param)                                        { return 0; }
            [HttpGet, Route("int")]         public int IntParam(int param)                                              { return 0; }
            [HttpGet, Route("nint")]        public int NullableInt(int? param)                                          { return 0; }
            [HttpGet, Route("double")]      public int DoubleParam(double param)                                        { return 0; }
            [HttpGet, Route("date")]        public int DateParam(DateTime param)                                        { return 0; }
            [HttpGet, Route("dateoffset")]  public int DateOffsetParam(DateTimeOffset param)                            { return 0; }
            [HttpGet, Route("string-arr")]  public int StringArrayParam(string[] param)                                 { return 0; }
            [HttpGet, Route("int-arr")]     public int IntArrayParam(int[] param)                                       { return 0; }
            [HttpGet, Route("byte-arr-1")]  public int ByteArray1Param(byte[] param)                                    { return 0; }
            [HttpGet, Route("byte-arr-2")]  public int ByteArray2Param([Expect(ExpectFormat.HexString)] byte[] param)   { return 0; }
            [HttpGet, Route("byte-arr-3")]  public int ByteArray3Param([Expect(ExpectFormat.Mime64)] byte[] param)      { return 0; }
            [HttpGet, Route("byte-arr-4")]  public int ByteArray4Param([Expect(ExpectFormat.Array)] byte[] param)       { return 0; }
        }

        [TestMethod]
        [DataRow(nameof(QueryStringController.StringParam),         "param", "param=Andrew",                    "en-GB", "Andrew")]
        [DataRow(nameof(QueryStringController.StringParam),         "param", "param=1&Param=2",                 "en-GB", "1")]
        [DataRow(nameof(QueryStringController.StringParam),         "param", "Param=1&param=2",                 "en-GB", "2")]
        [DataRow(nameof(QueryStringController.IntParam),            "param", "param=123",                       "en-GB", 123)]
        [DataRow(nameof(QueryStringController.NullableInt),         "param", "param=",                          "en-GB", null)]
        [DataRow(nameof(QueryStringController.DoubleParam),         "param", "param=12.3",                      "en-GB", 12.3)]
        [DataRow(nameof(QueryStringController.DoubleParam),         "param", "param=12.3",                      "de-DE", 12.3)]
        [DataRow(nameof(QueryStringController.DateParam),           "param", "param=2019-07-01T22:53:47+00:00", "en-US", "2019-07-01T22:53:47+00:00")]
        [DataRow(nameof(QueryStringController.DateOffsetParam),     "param", "param=2019-07-01T22:53:47+00:00", "de-DE", "2019-07-01T22:53:47+00:00")]
        [DataRow(nameof(QueryStringController.StringArrayParam),    "param", "param=1&param=2;param=3",         "en-GB", new string[] { "1", "2", "3" })]
        [DataRow(nameof(QueryStringController.IntArrayParam),       "param", "param=1&param=2;param=3",         "en-GB", new int[] { 1, 2, 3 })]
        [DataRow(nameof(QueryStringController.ByteArray1Param),     "param", "param=0x01",  /* mime encoded */  "en-GB", new byte[] { 211, 29, 53 })]
        [DataRow(nameof(QueryStringController.ByteArray2Param),     "param", "param=0x01",                      "en-GB", new byte[] { 1 })]
        [DataRow(nameof(QueryStringController.ByteArray2Param),     "param", "param=01",                        "en-GB", new byte[] { 1 })]
        [DataRow(nameof(QueryStringController.ByteArray3Param),     "param", "param=0x01",  /* mime encoded */  "en-GB", new byte[] { 211, 29, 53 })]
        [DataRow(nameof(QueryStringController.ByteArray4Param),     "param", "param=1&param=2&param=3",         "en-GB", new byte[] { 1, 2, 3 })]
        public void BuildRouteParameters_Can_Parse_Parameter_From_Query_String(string methodName, string parameterName, string queryString, string culture, object rawExpected)
        {
            using(new CultureSwap(culture)) {
                var route = RouteTests.CreateRoute(typeof(QueryStringController), methodName);
                _RouteMapper.Initialise(new Route[] { route });
                _Environment.RequestQueryString = queryString;

                var parameters = _RouteMapper.BuildRouteParameters(route, new string[] { route.PathParts[0].Part }, _Environment.Environment);

                var expected = rawExpected;
                switch(methodName) {
                    case nameof(QueryStringController.DateParam):
                        expected = DateTime.Parse((string)rawExpected, CultureInfo.InvariantCulture);
                        break;
                    case nameof(QueryStringController.DateOffsetParam):
                        expected = DateTimeOffset.Parse((string)rawExpected, CultureInfo.InvariantCulture);
                        break;
                }

                Assert.IsTrue(parameters.IsValid);
                Assert.AreEqual(1, parameters.Parameters.Length);
                var param = parameters.Parameters[0];

                if(expected is IList expectedList) {
                    var actualList = (IList)param;
                    Assert.AreEqual(expectedList.Count, actualList.Count);
                    for(var i = 0;i < expectedList.Count;++i) {
                        Assert.AreEqual(expectedList[i], actualList[i]);
                    }
                } else {
                    Assert.AreEqual(expected, param);
                }
            }
        }

        [TestMethod]
        public void BuildRouteParameters_Can_Use_Case_Sensitive_Query_String_Matching()
        {
            var route = RouteTests.CreateRoute(typeof(QueryStringController), nameof(QueryStringController.StringParam));
            _RouteMapper.AreQueryStringNamesCaseSensitive = true;
            _RouteMapper.Initialise(new Route[] { route });

            _Environment.RequestQueryString = "PARAM=1";
            var parameters = _RouteMapper.BuildRouteParameters(route, new string[] { route.PathParts[0].Part }, _Environment.Environment);
            Assert.IsFalse(parameters.IsValid);

            _Environment.RequestQueryString = "param=1";
            parameters = _RouteMapper.BuildRouteParameters(route, new string[] { route.PathParts[0].Part }, _Environment.Environment);
            Assert.IsTrue(parameters.IsValid);
            Assert.AreEqual("1", parameters.Parameters[0]);
        }

        [TestMethod]
        public void BuildRouteParameters_Can_Use_Case_Insensitive_Query_String_Matching()
        {
            var route = RouteTests.CreateRoute(typeof(QueryStringController), nameof(QueryStringController.StringParam));
            _RouteMapper.AreQueryStringNamesCaseSensitive = false;
            _RouteMapper.Initialise(new Route[] { route });

            _Environment.RequestQueryString = "PARAM=1";
            var parameters = _RouteMapper.BuildRouteParameters(route, new string[] { route.PathParts[0].Part }, _Environment.Environment);
            Assert.IsTrue(parameters.IsValid);
            Assert.AreEqual("1", parameters.Parameters[0]);

            _Environment.RequestQueryString = "param=1";
            parameters = _RouteMapper.BuildRouteParameters(route, new string[] { route.PathParts[0].Part }, _Environment.Environment);
            Assert.IsTrue(parameters.IsValid);
            Assert.AreEqual("1", parameters.Parameters[0]);
        }

        // This is expected to use Parser.ParseType so the test of conversions is not exhaustive
        public class PostFormController : Controller
        {
            [HttpPost, Route("string")]     public int StringParam(string param)                                        { return 0; }
            [HttpPost, Route("int")]        public int IntParam(int param)                                              { return 0; }
            [HttpGet, Route("string-arr")]  public int StringArrayParam(string[] param)                                 { return 0; }
        }

        [TestMethod]
        [DataRow(nameof(PostFormController.StringParam),            "param", "param=Andrew",            "en-GB", "Andrew")]
        [DataRow(nameof(PostFormController.StringParam),            "param", "param=1&Param=2",         "en-GB", "1")]
        [DataRow(nameof(PostFormController.StringParam),            "param", "Param=1&param=2",         "en-GB", "2")]
        [DataRow(nameof(PostFormController.IntParam),               "param", "param=123",               "en-GB", 123)]
        [DataRow(nameof(PostFormController.StringArrayParam),       "param", "param=1&param=2;param=3", "en-GB", new string[] { "1", "2", "3" })]
        public void BuildRouteParameters_Can_Parse_Parameter_From_Form_Body(string methodName, string parameterName, string body, string culture, object rawExpected)
        {
            using(new CultureSwap(culture)) {
                var route = RouteTests.CreateRoute(typeof(PostFormController), methodName);
                _RouteMapper.Initialise(new Route[] { route });
                _Environment.SetRequestBody(body, contentType: "application/x-www-form-urlencoded");

                var parameters = _RouteMapper.BuildRouteParameters(route, new string[] { route.PathParts[0].Part }, _Environment.Environment);

                var expected = rawExpected;

                Assert.IsTrue(parameters.IsValid);
                Assert.AreEqual(1, parameters.Parameters.Length);
                var param = parameters.Parameters[0];

                if(expected is IList expectedList) {
                    var actualList = (IList)param;
                    Assert.AreEqual(expectedList.Count, actualList.Count);
                    for(var i = 0;i < expectedList.Count;++i) {
                        Assert.AreEqual(expectedList[i], actualList[i]);
                    }
                } else {
                    Assert.AreEqual(expected, param);
                }
            }
        }

        [TestMethod]
        public void BuildRouteParameters_Can_Use_Case_Sensitive_Form_Body_Matching()
        {
            var route = RouteTests.CreateRoute(typeof(PostFormController), nameof(PostFormController.StringParam));
            _RouteMapper.AreFormNamesCaseSensitive = true;
            _RouteMapper.Initialise(new Route[] { route });

            _Environment.SetRequestBody("PARAM=1", contentType: "application/x-www-form-urlencoded");
            var parameters = _RouteMapper.BuildRouteParameters(route, new string[] { route.PathParts[0].Part }, _Environment.Environment);
            Assert.IsFalse(parameters.IsValid);

            _Environment.SetRequestBody("param=1", contentType: "application/x-www-form-urlencoded");
            parameters = _RouteMapper.BuildRouteParameters(route, new string[] { route.PathParts[0].Part }, _Environment.Environment);
            Assert.IsTrue(parameters.IsValid);
            Assert.AreEqual("1", parameters.Parameters[0]);
        }

        [TestMethod]
        public void BuildRouteParameters_Can_Use_Case_Insensitive_Form_Body_Matching()
        {
            var route = RouteTests.CreateRoute(typeof(PostFormController), nameof(PostFormController.StringParam));
            _RouteMapper.AreFormNamesCaseSensitive = false;
            _RouteMapper.Initialise(new Route[] { route });

            _Environment.SetRequestBody("PARAM=1", contentType: "application/x-www-form-urlencoded");
            var parameters = _RouteMapper.BuildRouteParameters(route, new string[] { route.PathParts[0].Part }, _Environment.Environment);
            Assert.IsTrue(parameters.IsValid);
            Assert.AreEqual("1", parameters.Parameters[0]);

            _Environment.SetRequestBody("param=1", contentType: "application/x-www-form-urlencoded");
            parameters = _RouteMapper.BuildRouteParameters(route, new string[] { route.PathParts[0].Part }, _Environment.Environment);
            Assert.IsTrue(parameters.IsValid);
            Assert.AreEqual("1", parameters.Parameters[0]);
        }
    }
}
