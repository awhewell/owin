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
using AWhewell.Owin.Utility;
using AWhewell.Owin.Utility.Parsers;

namespace Test.AWhewell.Owin.WebApi
{
    [TestClass]
    public class RouteMapper_Tests
    {
        public class Controller : IApiController
        {
            public IDictionary<string, object> OwinEnvironment { get; set; }
        }

        public class StringModel
        {
            public string StringValue { get; set; }
        }

        public class DateModel
        {
            public DateTime DateValue { get; set; }
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
        public void FindRouteForRequest_Throws_If_Environment_Is_Null()
        {
            _RouteMapper.FindRouteForRequest(null);
        }

        public class SimplePathController : Controller { [HttpGet, Route("simple-path")] public int Method() { return 0; } }

        [TestMethod]
        [DataRow("GET",     "/simple-path", true)]   // Case matches normalised cases in Route
        [DataRow("get",     "/simple-path", true)]   // HTTP method does not match Route's normalised case
        [DataRow("GET",     "/SIMPLE-PATH", true)]   // Path parts does not match Route's normalised case
        [DataRow("POST",    "/simple-path", false)]
        public void FindRouteForRequest_Returns_Correct_Candidates(string httpMethod, string pathPart, bool expectResult)
        {
            var expected = Route_Tests.CreateRoute<SimplePathController>(nameof(SimplePathController.Method));
            _RouteMapper.Initialise(new Route[] { expected });
            _Environment.Environment[EnvironmentKey.RequestMethod] = httpMethod;
            _Environment.Environment[EnvironmentKey.RequestPath] = pathPart;

            var actual = _RouteMapper.FindRouteForRequest(_Environment.Environment);

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
        public void FindRouteForRequest_Matches_MultiPart_Paths(string httpMethod, Type controllerType, string[] pathParts, bool expectMatch)
        {
            var route = Route_Tests.CreateRoute(controllerType, "Method");
            _RouteMapper.Initialise(new Route[] { route });
            _Environment.Environment[EnvironmentKey.RequestMethod] = httpMethod;
            _Environment.SetRequestPath(pathParts);

            var actual = _RouteMapper.FindRouteForRequest(_Environment.Environment);

            if(!expectMatch) {
                Assert.IsNull(actual);
            } else {
                Assert.IsNotNull(actual);
            }
        }

        public class Issue_1_Controller1 : Controller
        {
            [HttpGet]
            [Route("api/3.00/settings/server")]
            public int GetServerConfig() { return 1; }
        }

        public class Issue_1_Controller2 : Controller
        {
            [HttpGet]
            [Route("api/3.00/feeds/polar-plot/{feedId}")]
            public int GetPolarPlot(int feedId = -1) { return 1; }
        }

        [TestMethod]
        public void Issue_1_Not_Choosing_Correct_Multipart_Route()
        {
            // This was found in an alpha build
            var getServerConfigRoute = Route_Tests.CreateRoute<Issue_1_Controller1>(nameof(Issue_1_Controller1.GetServerConfig));
            var getPolarPlotRoute =    Route_Tests.CreateRoute<Issue_1_Controller2>(nameof(Issue_1_Controller2.GetPolarPlot));

            _RouteMapper.Initialise(new Route[] {
                getPolarPlotRoute,
                getServerConfigRoute,
            });

            _Environment.RequestMethod = "GET";
            _Environment.RequestPath = "/api/3.00/settings/server";

            var actual = _RouteMapper.FindRouteForRequest(_Environment.Environment);

            Assert.IsNotNull(actual);
            Assert.AreSame(getServerConfigRoute, actual);   // Bug is that it's returning the GetPolarPlot route
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BuildRouteParameters_Throws_If_Route_Is_Null()
        {
            var route = Route_Tests.CreateRoute(typeof(ApiEntityWithIDController), nameof(ApiEntityWithIDController.Method));
            _RouteMapper.Initialise(new Route[] { route });

            _Environment.SetRequestPath("/api/entity/1");
            _RouteMapper.BuildRouteParameters(null, _Environment.Environment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BuildRouteParameters_Throws_If_Environment_Is_Null()
        {
            var route = Route_Tests.CreateRoute(typeof(ApiEntityWithIDController), nameof(ApiEntityWithIDController.Method));
            _RouteMapper.Initialise(new Route[] { route });

            _RouteMapper.BuildRouteParameters(route, null);
        }

        [TestMethod]
        public void BuildRouteParameters_Rejects_Call_If_Route_Was_Not_Initialised()
        {
            var initialisedRoute = Route_Tests.CreateRoute(typeof(ApiEntityWithIDController), nameof(ApiEntityWithIDController.Method));
            var uninitialisedRoute = Route_Tests.CreateRoute(typeof(ApiEntityWithOptionalIDController), nameof(ApiEntityWithOptionalIDController.Method));
            _RouteMapper.Initialise(new Route[] { initialisedRoute });

            _Environment.SetRequestPath("/api/entity/1");
            var parameters = _RouteMapper.BuildRouteParameters(uninitialisedRoute, _Environment.Environment);

            Assert.IsFalse(parameters.IsValid);
        }

        [TestMethod]
        public void BuildRouteParameters_Extracts_Values_From_Path_Parts()
        {
            var route = Route_Tests.CreateRoute(typeof(ApiEntityWithIDController), nameof(ApiEntityWithIDController.Method));
            _RouteMapper.Initialise(new Route[] { route });

            _Environment.SetRequestPath("/api/entity/12");
            var parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);

            Assert.IsTrue(parameters.IsValid);
            Assert.AreEqual(1, parameters.Parameters.Length);
            Assert.IsInstanceOfType(parameters.Parameters[0], typeof(int));
            Assert.AreEqual(12, (int)parameters.Parameters[0]);
        }

        [TestMethod]
        public void BuildRouteParameters_Uses_Default_If_Optional_Path_Part_Is_Missing()
        {
            var route = Route_Tests.CreateRoute(typeof(ApiEntityWithOptionalIDController), nameof(ApiEntityWithOptionalIDController.Method));
            _RouteMapper.Initialise(new Route[] { route });

            _Environment.SetRequestPath("/api/entity");
            var parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);

            Assert.IsTrue(parameters.IsValid);
            Assert.AreEqual(1, parameters.Parameters.Length);
            Assert.IsInstanceOfType(parameters.Parameters[0], typeof(int));
            Assert.AreEqual(-1, (int)parameters.Parameters[0]);
        }

        [TestMethod]
        public void BuildRouteParameters_Clears_IsValid_When_Parameter_Cannot_Be_Parsed()
        {
            var route = Route_Tests.CreateRoute(typeof(ApiEntityWithIDController), nameof(ApiEntityWithIDController.Method));
            _RouteMapper.Initialise(new Route[] { route });

            _Environment.SetRequestPath("/api/entity/not-an-int");
            var parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);

            Assert.IsFalse(parameters.IsValid);
        }

        // The expectation is that the implementation uses Owin.Utility.Parser to parse the path part string into the
        // correct type. There are a lot of tests on Parser to make sure it copes with various possible inputs, it would
        // be a lot of effort to reproduce them all here. These tests just cover the basics re. types and corner cases.
        public class PathPartTypeController : Controller
        {
            [HttpGet, Route("string/{param}")]          public int StringPP(string param)                                                       { return 0; }
            [HttpGet, Route("int/{param}")]             public int IntPP(int param)                                                             { return 0; }
            [HttpGet, Route("n-int/{param}")]           public int NIntPP(int? param)                                                           { return 0; }
            [HttpGet, Route("double/{param}")]          public int DoublePP(double param)                                                       { return 0; }
            [HttpGet, Route("datetime/{param}")]        public int DateTimePP(DateTime param)                                                   { return 0; }
            [HttpGet, Route("datetimeoffset/{param}")]  public int DateTimeOffsetPP(DateTimeOffset param)                                       { return 0; }
            [HttpGet, Route("default-bytes/{param}")]   public int DefaultByteArrayPP(byte[] param)                                             { return 0; }
            [HttpGet, Route("hex-bytes/{param}")]       public int HexByteArrayPP([UseParser(typeof(ByteArray_HexString_Parser))] byte[] param) { return 0; }
            [HttpGet, Route("mime64-bytes/{param}")]    public int Mime64ByteArrayPP([UseParser(typeof(ByteArray_Mime64_Parser))] byte[] param) { return 0; }

        }

        [DataRow(nameof(PathPartTypeController.StringPP),           "",                             "en-GB", "")]
        [DataRow(nameof(PathPartTypeController.IntPP),              "1",                            "en-GB", 1)]
        [DataRow(nameof(PathPartTypeController.NIntPP),             "1",                            "en-GB", 1)]
        [DataRow(nameof(PathPartTypeController.DoublePP),           "1.2",                          "en-GB", 1.2)]
        [DataRow(nameof(PathPartTypeController.DoublePP),           "1.2",                          "de-DE", 1.2)]
        [DataRow(nameof(PathPartTypeController.DateTimePP),         "2019-07-01T22:53:47Z",         "en-GB", "2019-07-01 22:53:47")]
        [DataRow(nameof(PathPartTypeController.DateTimeOffsetPP),   "2019-07-01T22:53:47Z",         "en-US", "2019-07-01 22:53:47")]
        [DataRow(nameof(PathPartTypeController.DefaultByteArrayPP), "0x01",  /* mime encoded */     "en-GB", new byte[] { 211, 29, 53 })]
        [DataRow(nameof(PathPartTypeController.HexByteArrayPP),     "0x01",                         "en-GB", new byte[] { 1 })]
        [DataRow(nameof(PathPartTypeController.HexByteArrayPP),     "01",                           "en-GB", new byte[] { 1 })]
        [DataRow(nameof(PathPartTypeController.Mime64ByteArrayPP),  "0x01",                         "en-GB", new byte[] { 211, 29, 53 })]
        [TestMethod]
        public void BuildRouteParameters_Parses_Different_Types_From_Path_Parts(string methodName, string pathPart, string culture, object rawExpected)
        {
            using(new CultureSwap(culture)) {
                var route = Route_Tests.CreateRoute(typeof(PathPartTypeController), methodName);
                _RouteMapper.Initialise(new Route[] { route });

                _Environment.SetRequestPath(new string[] { route.PathParts[0].Part, pathPart });
                var parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);

                var expected = rawExpected;
                switch(methodName) {
                    case nameof(PathPartTypeController.DateTimePP):         expected = DataRowParser.DateTime((string)expected); break;
                    case nameof(PathPartTypeController.DateTimeOffsetPP):   expected = DataRowParser.DateTimeOffset((string)expected); break;
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
            var route = Route_Tests.CreateRoute(typeof(MultipleParameterController), nameof(MultipleParameterController.Method));
            _RouteMapper.Initialise(new Route[] { route });

            _Environment.SetRequestPath(new string[] { route.PathParts[0].Part, "string-value", "55" });
            var parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);

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
            var route = Route_Tests.CreateRoute(typeof(OwinEnvController), nameof(OwinEnvController.JustEnvironment));
            _RouteMapper.Initialise(new Route[] { route });

            _Environment.SetRequestPath(route.PathParts[0].Part);
            var parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);

            Assert.IsTrue(parameters.IsValid);
            Assert.AreEqual(1, parameters.Parameters.Length);
            Assert.AreSame(_Environment.Environment, parameters.Parameters[0]);
        }

        [TestMethod]
        public void BuildRouteParameters_Does_Not_Inject_Owin_Environment_To_Instance_Route_Method()
        {
            var route = Route_Tests.CreateRoute(typeof(OwinEnvController), nameof(OwinEnvController.NotStatic));
            _RouteMapper.Initialise(new Route[] { route });

            _Environment.SetRequestPath(route.PathParts[0].Part);
            var parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);

            Assert.IsFalse(parameters.IsValid);
        }

        public class DefaultsController : Controller
        {
            [HttpGet, Route("defaulted")]
            public int DefaultedRoute(int id = -1) => id + 1;
        }

        [TestMethod]
        public void BuildRouteParameters_Will_Use_Defaults_If_No_Value_Is_Supplied()
        {
            var route = Route_Tests.CreateRoute(typeof(DefaultsController), nameof(DefaultsController.DefaultedRoute));
            _RouteMapper.Initialise(new Route[] { route });

            _Environment.SetRequestPath(route.PathParts[0].Part);
            var parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);

            Assert.IsTrue(parameters.IsValid);
            Assert.AreEqual(1, parameters.Parameters.Length);
            Assert.AreEqual(-1, parameters.Parameters[0]);
        }

        // This is expected to use Parser.ParseType so the test of conversions is not exhaustive, it's just a handful
        // of basic tests to make sure things appear to be working OK
        public class QueryStringController : Controller
        {
            [HttpGet, Route("string")]      public int StringParam(string param)                                                     { return 0; }
            [HttpGet, Route("int")]         public int IntParam(int param)                                                           { return 0; }
            [HttpGet, Route("nint")]        public int NullableInt(int? param)                                                       { return 0; }
            [HttpGet, Route("double")]      public int DoubleParam(double param)                                                     { return 0; }
            [HttpGet, Route("date")]        public int DateParam(DateTime param)                                                     { return 0; }
            [HttpGet, Route("dateoffset")]  public int DateOffsetParam(DateTimeOffset param)                                         { return 0; }
            [HttpGet, Route("string-arr")]  public int StringArrayParam(string[] param)                                              { return 0; }
            [HttpGet, Route("int-arr")]     public int IntArrayParam(int[] param)                                                    { return 0; }
            [HttpGet, Route("byte-arr-1")]  public int ByteArray1Param(byte[] param)                                                 { return 0; }
            [HttpGet, Route("byte-arr-2")]  public int ByteArray2Param([UseParser(typeof(ByteArray_HexString_Parser))] byte[] param) { return 0; }
            [HttpGet, Route("byte-arr-3")]  public int ByteArray3Param([UseParser(typeof(ByteArray_Mime64_Parser))] byte[] param)    { return 0; }
            [HttpGet, Route("byte-arr-4")]  public int ByteArray4Param([ExpectArray] byte[] param)                                   { return 0; }
        }

        [TestMethod]
        [DataRow(nameof(QueryStringController.StringParam),         "param", "param=Andrew",                    "en-GB", "Andrew")]
        [DataRow(nameof(QueryStringController.StringParam),         "param", "param=1&Param=2",                 "en-GB", "1")]
        [DataRow(nameof(QueryStringController.StringParam),         "param", "Param=1&param=2",                 "en-GB", "2")]
        [DataRow(nameof(QueryStringController.IntParam),            "param", "param=123",                       "en-GB", 123)]
        [DataRow(nameof(QueryStringController.NullableInt),         "param", "param=",                          "en-GB", null)]
        [DataRow(nameof(QueryStringController.DoubleParam),         "param", "param=12.3",                      "en-GB", 12.3)]
        [DataRow(nameof(QueryStringController.DoubleParam),         "param", "param=12.3",                      "de-DE", 12.3)]
        [DataRow(nameof(QueryStringController.DateParam),           "param", "param=2019-07-01T22:53:47Z",      "en-US", "2019-07-01 22:53:47")]
        [DataRow(nameof(QueryStringController.DateOffsetParam),     "param", "param=2019-07-01T22:53:47Z",      "de-DE", "2019-07-01 22:53:47")]
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
                var route = Route_Tests.CreateRoute(typeof(QueryStringController), methodName);
                _RouteMapper.Initialise(new Route[] { route });
                _Environment.RequestQueryString = queryString;

                _Environment.SetRequestPath(route.PathParts[0].Part);
                var parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);

                var expected = rawExpected;
                switch(methodName) {
                    case nameof(QueryStringController.DateParam):       expected = DataRowParser.DateTime((string)expected); break;
                    case nameof(QueryStringController.DateOffsetParam): expected = DataRowParser.DateTimeOffset((string)expected); break;
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
        public void BuildRouteParameters_Uses_TypeParserResolver_When_Parsing_Array_Elements()
        {
            var resolver = new TypeParserResolver(new ModelBuilder_Tests.String_Reverse_Parser());
            var route = Route_Tests.CreateRoute(typeof(QueryStringController), nameof(QueryStringController.StringArrayParam), parserResolver: resolver);
            _RouteMapper.Initialise(new Route[] { route });
            _Environment.RequestQueryString = "param=Abc";

            _Environment.SetRequestPath(route.PathParts[0].Part);
            var parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);

            Assert.AreEqual(true, parameters.IsValid);
            Assert.AreEqual(1, parameters.Parameters.Length);
            Assert.AreEqual("cbA", ((string[])parameters.Parameters[0])[0]);
        }

        [TestMethod]
        public void BuildRouteParameters_Can_Use_Case_Sensitive_Query_String_Matching()
        {
            var route = Route_Tests.CreateRoute(typeof(QueryStringController), nameof(QueryStringController.StringParam));
            _RouteMapper.AreQueryStringNamesCaseSensitive = true;
            _RouteMapper.Initialise(new Route[] { route });

            _Environment.SetRequestPath(route.PathParts[0].Part);

            _Environment.RequestQueryString = "PARAM=1";
            var parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);
            Assert.IsFalse(parameters.IsValid);

            _Environment.RequestQueryString = "param=1";
            parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);
            Assert.IsTrue(parameters.IsValid);
            Assert.AreEqual("1", parameters.Parameters[0]);
        }

        [TestMethod]
        public void BuildRouteParameters_Can_Use_Case_Insensitive_Query_String_Matching()
        {
            var route = Route_Tests.CreateRoute(typeof(QueryStringController), nameof(QueryStringController.StringParam));
            _RouteMapper.AreQueryStringNamesCaseSensitive = false;
            _RouteMapper.Initialise(new Route[] { route });

            _Environment.SetRequestPath(route.PathParts[0].Part);

            _Environment.RequestQueryString = "PARAM=1";
            var parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);
            Assert.IsTrue(parameters.IsValid);
            Assert.AreEqual("1", parameters.Parameters[0]);

            _Environment.RequestQueryString = "param=1";
            parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);
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
                var route = Route_Tests.CreateRoute(typeof(PostFormController), methodName);
                _RouteMapper.Initialise(new Route[] { route });
                _Environment.SetRequestBody(body, contentType: "application/x-www-form-urlencoded");
                _Environment.SetRequestPath(route.PathParts[0].Part);

                var parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);

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
            var route = Route_Tests.CreateRoute(typeof(PostFormController), nameof(PostFormController.StringParam));
            _RouteMapper.AreFormNamesCaseSensitive = true;
            _RouteMapper.Initialise(new Route[] { route });
            _Environment.SetRequestPath(route.PathParts[0].Part);

            _Environment.SetRequestBody("PARAM=1", contentType: "application/x-www-form-urlencoded");
            var parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);
            Assert.IsFalse(parameters.IsValid);

            _Environment.SetRequestBody("param=1", contentType: "application/x-www-form-urlencoded");
            parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);
            Assert.IsTrue(parameters.IsValid);
            Assert.AreEqual("1", parameters.Parameters[0]);
        }

        [TestMethod]
        public void BuildRouteParameters_Can_Use_Case_Insensitive_Form_Body_Matching()
        {
            var route = Route_Tests.CreateRoute(typeof(PostFormController), nameof(PostFormController.StringParam));
            _RouteMapper.AreFormNamesCaseSensitive = false;
            _RouteMapper.Initialise(new Route[] { route });
            _Environment.SetRequestPath(route.PathParts[0].Part);

            _Environment.SetRequestBody("PARAM=1", contentType: "application/x-www-form-urlencoded");
            var parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);
            Assert.IsTrue(parameters.IsValid);
            Assert.AreEqual("1", parameters.Parameters[0]);

            _Environment.SetRequestBody("param=1", contentType: "application/x-www-form-urlencoded");
            parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);
            Assert.IsTrue(parameters.IsValid);
            Assert.AreEqual("1", parameters.Parameters[0]);
        }

        public class QueryStringModelController : Controller
        {
            [HttpGet, Route("x1")] public int StringModelFunc(StringModel model) { return 0; }
        }

        [TestMethod]
        public void BuildRouteParameters_Can_Build_Model_Object_From_Query_String()
        {
            var route = Route_Tests.CreateRoute(typeof(QueryStringModelController), nameof(QueryStringModelController.StringModelFunc));
            _RouteMapper.Initialise(new Route[] { route });
            _Environment.SetRequestPath(route.PathParts[0].Part);

            _Environment.RequestQueryString = "StringValue=Ab";
            var parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);

            Assert.IsTrue(parameters.IsValid);
            Assert.AreEqual(1, parameters.Parameters.Length);
            var model = parameters.Parameters[0] as StringModel;
            Assert.AreEqual("Ab", model.StringValue);
        }

        [TestMethod]
        public void BuildRouteParameters_Can_Be_Case_Insensitive_For_Building_Models_From_Query_String()
        {
            var route = Route_Tests.CreateRoute(typeof(QueryStringModelController), nameof(QueryStringModelController.StringModelFunc));
            _RouteMapper.AreQueryStringNamesCaseSensitive = false;
            _RouteMapper.Initialise(new Route[] { route });
            _Environment.SetRequestPath(route.PathParts[0].Part);

            _Environment.RequestQueryString = "stringVALUE=Ab";
            var parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);

            Assert.IsTrue(parameters.IsValid);
            Assert.AreEqual(1, parameters.Parameters.Length);
            var model = parameters.Parameters[0] as StringModel;
            Assert.AreEqual("Ab", model.StringValue);
        }

        [TestMethod]
        public void BuildRouteParameters_Can_Be_Case_Sensitive_For_Building_Models_From_Query_String()
        {
            var route = Route_Tests.CreateRoute(typeof(QueryStringModelController), nameof(QueryStringModelController.StringModelFunc));
            _RouteMapper.AreQueryStringNamesCaseSensitive = true;
            _RouteMapper.Initialise(new Route[] { route });
            _Environment.SetRequestPath(route.PathParts[0].Part);

            _Environment.RequestQueryString = "stringVALUE=NotAb&StringValue=Ab";
            var parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);

            Assert.IsTrue(parameters.IsValid);
            Assert.AreEqual(1, parameters.Parameters.Length);
            var model = parameters.Parameters[0] as StringModel;
            Assert.AreEqual("Ab", model.StringValue);
        }

        [TestMethod]
        public void BuildRouteParameters_Passes_TypeParserResolver_To_Model_Builder_For_Query_String_Models()
        {
            var resolver = new TypeParserResolver(new ModelBuilder_Tests.String_Reverse_Parser());
            var route = Route_Tests.CreateRoute(typeof(QueryStringModelController), nameof(QueryStringModelController.StringModelFunc), parserResolver: resolver);
            _RouteMapper.Initialise(new Route[] { route });
            _Environment.SetRequestPath(route.PathParts[0].Part);

            _Environment.RequestQueryString = "StringValue=Ab";
            var parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);

            Assert.IsTrue(parameters.IsValid);
            Assert.AreEqual(1, parameters.Parameters.Length);
            var model = parameters.Parameters[0] as StringModel;
            Assert.AreEqual("bA", model.StringValue);
        }

        public class FormBodyModelController : Controller
        {
            [HttpPost, Route("x1")] public int StringModelFunc(StringModel model) { return 0; }
        }

        [TestMethod]
        public void BuildRouteParameters_Can_Build_Model_From_Form_Body()
        {
            var route = Route_Tests.CreateRoute(typeof(FormBodyModelController), nameof(FormBodyModelController.StringModelFunc));
            _RouteMapper.Initialise(new Route[] { route });
            _Environment.RequestMethod = "POST";
            _Environment.SetRequestPath(route.PathParts[0].Part);
            _Environment.SetRequestBody("StringValue=Ab", contentType: "application/x-www-form-urlencoded");

            var parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);

            Assert.IsTrue(parameters.IsValid);
            Assert.AreEqual(1, parameters.Parameters.Length);
            var model = parameters.Parameters[0] as StringModel;
            Assert.AreEqual("Ab", model.StringValue);
        }

        [TestMethod]
        public void BuildRouteParameters_Rejects_Attempt_To_Build_From_Form_Body_With_Bad_Encoding()
        {
            var route = Route_Tests.CreateRoute(typeof(FormBodyModelController), nameof(FormBodyModelController.StringModelFunc));
            _RouteMapper.Initialise(new Route[] { route });
            _Environment.RequestMethod = "POST";
            _Environment.SetRequestPath(route.PathParts[0].Part);
            _Environment.SetRequestBody("StringValue=Ab", contentType: "application/x-www-form-urlencoded; charset=who-knows");

            var parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);

            Assert.IsFalse(parameters.IsValid);
        }

        [TestMethod]
        public void BuildRouteParameters_Uses_Full_TypeParserResolver_When_Building_Model_From_Form_Body()
        {
            var resolver = new TypeParserResolver(new ModelBuilder_Tests.String_Reverse_Parser());
            var route = Route_Tests.CreateRoute(typeof(FormBodyModelController), nameof(FormBodyModelController.StringModelFunc), parserResolver: resolver);
            _RouteMapper.Initialise(new Route[] { route });
            _Environment.RequestMethod = "POST";
            _Environment.SetRequestPath(route.PathParts[0].Part);
            _Environment.SetRequestBody("StringValue=Ab", contentType: "application/x-www-form-urlencoded");

            var parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);

            Assert.IsTrue(parameters.IsValid);
            Assert.AreEqual(1, parameters.Parameters.Length);
            var model = parameters.Parameters[0] as StringModel;
            Assert.AreEqual("bA", model.StringValue);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void BuildRouteParameters_Uses_Case_Sensitivity_Flag_When_Building_Model_From_Form_Body(bool caseSensitiveKeys)
        {
            _RouteMapper.AreFormNamesCaseSensitive = caseSensitiveKeys;
            var route = Route_Tests.CreateRoute(typeof(FormBodyModelController), nameof(FormBodyModelController.StringModelFunc));
            _RouteMapper.Initialise(new Route[] { route });
            _Environment.RequestMethod = "POST";
            _Environment.SetRequestPath(route.PathParts[0].Part);
            _Environment.SetRequestBody("STRINGVALUE=Ab", contentType: "application/x-www-form-urlencoded");

            var parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);

            Assert.AreEqual(1, parameters.Parameters.Length);
            var model = parameters.Parameters[0] as StringModel;
            if(caseSensitiveKeys) {
                Assert.IsTrue(parameters.IsValid);
                Assert.IsNull(model.StringValue);
            } else {
                Assert.IsTrue(parameters.IsValid);
                Assert.AreEqual("Ab", model.StringValue);
            }
        }

        public class JsonBodyModelController : Controller
        {
            [HttpPost, Route("x1")] public int StringModelFunc(StringModel model) { return 0; }

            [HttpPost, Route("x2")] public long DateModelFunc(DateModel model) { return model?.DateValue.Ticks ?? 0L; }
        }

        [TestMethod]
        public void BuildRouteParameters_Can_Build_Model_From_Json_Body()
        {
            var route = Route_Tests.CreateRoute(typeof(JsonBodyModelController), nameof(JsonBodyModelController.StringModelFunc));
            _RouteMapper.Initialise(new Route[] { route });
            _Environment.RequestMethod = "POST";
            _Environment.SetRequestPath(route.PathParts[0].Part);
            _Environment.SetRequestBody("{ \"StringValue\": \"Ab\" }", contentType: "application/json");

            var parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);

            Assert.IsTrue(parameters.IsValid);
            Assert.AreEqual(1, parameters.Parameters.Length);
            var model = parameters.Parameters[0] as StringModel;
            Assert.AreEqual("Ab", model.StringValue);
        }

        [TestMethod]
        public void BuildRouteParameters_Defaults_To_Json_Bodies_When_ContentType_Missing()
        {
            var route = Route_Tests.CreateRoute(typeof(JsonBodyModelController), nameof(JsonBodyModelController.StringModelFunc));
            _RouteMapper.Initialise(new Route[] { route });
            _Environment.RequestMethod = "POST";
            _Environment.SetRequestPath(route.PathParts[0].Part);
            _Environment.SetRequestBody("{ \"StringValue\": \"Ab\" }");

            var parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);

            Assert.IsTrue(parameters.IsValid);
            Assert.AreEqual(1, parameters.Parameters.Length);
            var model = parameters.Parameters[0] as StringModel;
            Assert.AreEqual("Ab", model.StringValue);
        }

        [TestMethod]
        public void BuildRouteParameters_Uses_Limited_TypeParserResolver_When_Building_Bodies_From_Json()
        {
            // Only dates, byte arrays and guids go through the parser, everything else must be as per JSON spec

            var resolver = new TypeParserResolver(new JsonSerialiser_Tests.DateTime_JustDigits_Parser());
            var route = Route_Tests.CreateRoute(typeof(JsonBodyModelController), nameof(JsonBodyModelController.DateModelFunc), parserResolver: resolver);
            _RouteMapper.Initialise(new Route[] { route });
            _Environment.RequestMethod = "POST";
            _Environment.SetRequestPath(route.PathParts[0].Part);
            var expected = new DateTime(2019, 12, 17, 21, 52, 19, 123);
            _Environment.SetRequestBody($"{{ \"DateValue\": \"{expected.ToString("yyyyMMddHHmmssfff")}\" }}", contentType: "application/json");

            var parameters = _RouteMapper.BuildRouteParameters(route, _Environment.Environment);

            Assert.IsTrue(parameters.IsValid);
            Assert.AreEqual(1, parameters.Parameters.Length);
            var model = parameters.Parameters[0] as DateModel;
            Assert.AreEqual(expected, model.DateValue);
        }
    }
}
