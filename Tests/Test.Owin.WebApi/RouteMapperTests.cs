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

        [TestInitialize]
        public void TestInitialise()
        {
            _RouteMapper = Factory.Resolve<IRouteMapper>();
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
        public void BuildRouteParameters_Extracts_Values_From_Path_Parts()
        {
            var route = RouteTests.CreateRoute(typeof(ApiEntityWithIDController), nameof(ApiEntityWithIDController.Method));
            _RouteMapper.Initialise(new Route[] { route });

            var parameters = _RouteMapper.BuildRouteParameters(route, new string[] { "api", "entity", "12" });

            Assert.AreEqual(1, parameters.Length);
            Assert.IsInstanceOfType(parameters[0], typeof(int));
            Assert.AreEqual(12, (int)parameters[0]);
        }

        [TestMethod]
        public void BuildRouteParameters_Uses_Default_If_Optional_Parameter_Missing()
        {
            var route = RouteTests.CreateRoute(typeof(ApiEntityWithOptionalIDController), nameof(ApiEntityWithOptionalIDController.Method));
            _RouteMapper.Initialise(new Route[] { route });

            var parameters = _RouteMapper.BuildRouteParameters(route, new string[] { "api", "entity" });

            Assert.AreEqual(1, parameters.Length);
            Assert.IsInstanceOfType(parameters[0], typeof(int));
            Assert.AreEqual(-1, (int)parameters[0]);
        }

        [TestMethod]
        public void BuildRouteParameters_Throws_HttpResponseException_When_Parameter_Cannot_Be_Parsed()
        {
            var route = RouteTests.CreateRoute(typeof(ApiEntityWithIDController), nameof(ApiEntityWithIDController.Method));
            _RouteMapper.Initialise(new Route[] { route });

            HttpResponseException exception = null;
            try {
                _RouteMapper.BuildRouteParameters(route, new string[] { "api", "entity", "not-an-int" });
            } catch(HttpResponseException ex) {
                exception = ex;
            }

            Assert.IsNotNull(exception);
            Assert.AreEqual(HttpStatusCode.BadRequest, exception.StatusCode);
        }

        public class PathPartTypeController : Controller
        {
            [HttpGet, Route("string/{param}")]          public int StringPP(string param)                 { return 0; }
            [HttpGet, Route("int/{param}")]             public int IntPP(int param)                       { return 0; }
            [HttpGet, Route("n-int/{param}")]           public int NIntPP(int? param)                     { return 0; }
            [HttpGet, Route("double/{param}")]          public int DoublePP(double param)                 { return 0; }
            [HttpGet, Route("date/{param}")]            public int DatePP(DateTime param)                 { return 0; }
            [HttpGet, Route("datetime/{param}")]        public int DateTimePP(DateTime param)             { return 0; }
            [HttpGet, Route("datetimeoffset/{param}")]  public int DateTimeOffsetPP(DateTimeOffset param) { return 0; }
        }

        [DataRow(nameof(PathPartTypeController.StringPP),         "",                             "en-GB", "")]
        [DataRow(nameof(PathPartTypeController.IntPP),            "1",                            "en-GB", "1")]
        [DataRow(nameof(PathPartTypeController.NIntPP),           "1",                            "en-GB", "1")]
        [DataRow(nameof(PathPartTypeController.DoublePP),         "1.2",                          "en-GB", "1.2")]
        [DataRow(nameof(PathPartTypeController.DoublePP),         "1.2",                          "de-DE", "1.2")]
        [DataRow(nameof(PathPartTypeController.DatePP),           "2019-01-30",                   "en-GB", "2019-01-30")]
        [DataRow(nameof(PathPartTypeController.DatePP),           "2019-01-02",                   "en-US", "2019-01-02")]
        [DataRow(nameof(PathPartTypeController.DateTimePP),       "2019-07-01T22:53:47+01:00",    "en-GB", "2019-07-01 21:53:47")]
        [DataRow(nameof(PathPartTypeController.DateTimePP),       "2019-07-01 22:53:47",          "en-GB", "2019-07-01 22:53:47")]
        [DataRow(nameof(PathPartTypeController.DateTimeOffsetPP), "2019-07-01T22:53:47+01:00",    "en-GB", "2019-07-01 21:53:47")]
        [DataRow(nameof(PathPartTypeController.DateTimeOffsetPP), "2019-07-01 22:53:47",          "en-GB", "2019-07-01 22:53:47")]
        [TestMethod]
        public void BuildRouteParameters_Parses_Different_Types_From_Path_Parts(string methodName, string pathPart, string culture, string expectedText)
        {
            using(new CultureSwap(culture)) {
                var route = RouteTests.CreateRoute(typeof(PathPartTypeController), methodName);
                _RouteMapper.Initialise(new Route[] { route });

                var parameters = _RouteMapper.BuildRouteParameters(route, new string[] { route.PathParts[0].Part, pathPart });

                object expected = expectedText;
                switch(methodName) {
                    case nameof(PathPartTypeController.IntPP):
                    case nameof(PathPartTypeController.NIntPP):
                        expected = int.Parse(expectedText, NumberStyles.Integer, CultureInfo.InvariantCulture);
                        break;
                    case nameof(PathPartTypeController.DoublePP):
                        expected = double.Parse(expectedText, NumberStyles.Float, CultureInfo.InvariantCulture);
                        break;
                    case nameof(PathPartTypeController.DatePP):
                        expected = DateTime.Parse(expectedText, CultureInfo.InvariantCulture);
                        break;
                    case nameof(PathPartTypeController.DateTimePP):
                        expected = DateTime.Parse(expectedText, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                        break;
                    case nameof(PathPartTypeController.DateTimeOffsetPP):
                        expected = DateTimeOffset.Parse(expectedText, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                        break;
                }

                Assert.AreEqual(1, parameters.Length);
                Assert.AreEqual(expected, parameters[0]);
            }
        }
    }
}
