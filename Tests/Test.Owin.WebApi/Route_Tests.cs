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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AWhewell.Owin.Interface.WebApi;
using AWhewell.Owin.Utility;
using AWhewell.Owin.Utility.Parsers;
using AWhewell.Owin.Utility.Formatters;
using System.Net;

namespace Test.AWhewell.Owin.WebApi
{
    [TestClass]
    public class Route_Tests
    {
        public class Controller : IApiController
        {
            public IDictionary<string, object> OwinEnvironment { get; set; }

            [Route("path-to-example")]
            public int Example() { return 200; }

            [Route("delete-example"), HttpDelete]
            public int Delete() { return 200; }

            [Route("get-example"), HttpGet]
            public int Get() { return 200; }

            [Route("head-example"), HttpHead]
            public int Head() { return 200; }

            [Route("patch-example"), HttpPatch]
            public int Patch() { return 200; }

            [Route("post-example"), HttpPost]
            public int Post() { return 200; }

            [Route("put-example"), HttpPut]
            public int Put() { return 200; }

            [Route("duplicate-methods-not-allowed"), HttpGet, HttpPost]
            public int Duplicate_Methods_Not_Allowed() { return 200; }

            [Route("void-method"), HttpGet]
            public void Void_Method() { ; }
        }

        public static Route CreateRoute(Type controllerNativeType, string methodName, string choosePath = null, TypeParserResolver parserResolver = null, TypeFormatterResolver formatterResolver = null)
        {
            var controllerType = new ControllerType(controllerNativeType, parserResolver, formatterResolver);
            var method = controllerNativeType.GetMethod(methodName);
            var routeAttributes = method.GetCustomAttributes(inherit: false).OfType<RouteAttribute>().ToArray();
            var routeAttribute = routeAttributes.Length == 1
                ? routeAttributes[0]
                : routeAttributes.Single(r => r.Route == choosePath);

            return new Route(controllerType, method, routeAttribute);
        }

        public static Route CreateRoute<T>(string methodName, string choosePath = null, TypeParserResolver parserResolver = null, TypeFormatterResolver formatterResolver = null)
        {
            return CreateRoute(typeof(T), methodName, choosePath, parserResolver, formatterResolver);
        }

        public static RouteParameters CreateRouteParameters(params object[] parameters)
        {
            return new RouteParameters(new string[0], parameters);
        }

        [TestMethod]
        public void Ctor_Copies_Arguments_Properties()
        {
            var controller = new ControllerType(typeof(Controller), null, null);
            var methodInfo = controller.Type.GetMethod(nameof(Controller.Example));
            var routeAttribute = methodInfo.GetCustomAttributes(inherit: false).OfType<RouteAttribute>().Single();

            var route = new Route(controller, methodInfo, routeAttribute);
            Assert.AreSame(controller, route.ControllerType);
            Assert.AreSame(methodInfo, route.Method);
            Assert.AreSame(routeAttribute, route.RouteAttribute);
        }

        [TestMethod]
        public void Ctor_Defaults_Method_To_Post()
        {
            var route = CreateRoute<Controller>(nameof(Controller.Example));

            Assert.AreEqual(HttpMethod.Post, route.HttpMethod);
        }

        [TestMethod]
        [DataRow("Delete",  HttpMethod.Delete)]
        [DataRow("Get",     HttpMethod.Get)]
        [DataRow("Head",    HttpMethod.Head)]
        [DataRow("Patch",   HttpMethod.Patch)]
        [DataRow("Post",    HttpMethod.Post)]
        [DataRow("Put",     HttpMethod.Put)]
        public void Ctor_Sets_HttpMethod_From_Method_Attribute(string methodName, HttpMethod expected)
        {
            var route = CreateRoute<Controller>(methodName);

            Assert.AreEqual(expected, route.HttpMethod);
        }

        [TestMethod]
        public void Ctor_Sets_Unknown_HttpMethod_If_Multiple_Attributes_Present()
        {
            var route = CreateRoute<Controller>(nameof(Controller.Duplicate_Methods_Not_Allowed));

            Assert.AreEqual(HttpMethod.Unknown, route.HttpMethod);
        }

        [TestMethod]
        public void Ctor_Sets_IsVoidMethod_For_Void_Methods()
        {
            var route = CreateRoute<Controller>(nameof(Controller.Void_Method));

            Assert.IsTrue(route.IsVoidMethod);
        }

        [TestMethod]
        public void Ctor_Clears_IsVoidMethod_For_Non_Void_Methods()
        {
            var route = CreateRoute<Controller>(nameof(Controller.Get));

            Assert.IsFalse(route.IsVoidMethod);
        }

        public class NullRoute : Controller         { [HttpGet, Route(null)]                public int Method() { return 1; } }
        public class EmptyRoute : Controller        { [HttpGet, Route("")]                  public int Method() { return 1; } }
        public class SpaceRoute : Controller        { [HttpGet, Route(" ")]                 public int Method() { return 1; } }
        public class MultiPartRoute : Controller    { [HttpGet, Route("api/entity/{id}")]   public int Method(int id) { return id + 1; } }
        public class BackslashRoute : Controller    { [HttpGet, Route("api\\entity")]       public int Method() { return 1; } }
        public class PercentRoute : Controller      { [HttpGet, Route("api%2Fentity")]      public int Method() { return 1; } }
        public class UpperCaseRoute : Controller    { [HttpGet, Route("API")]               public int Method() { return 1; } }
        public class SpacesRoute1 : Controller      { [HttpGet, Route("A B")]               public int Method() { return 1; } }
        public class SpacesRoute2 : Controller      { [HttpGet, Route("A%20B")]             public int Method() { return 1; } }
        public class EmptyPartRoute : Controller    { [HttpGet, Route("api//entity")]       public int Method() { return 1; } }
        public class LeadingSlashRoute : Controller { [HttpGet, Route("/api/entity")]       public int Method() { return 1; } }

        [TestMethod]
        [DataRow(typeof(NullRoute),         "Method", null)]
        [DataRow(typeof(EmptyRoute),        "Method", null)]
        [DataRow(typeof(SpaceRoute),        "Method", new string[] { " " })]
        [DataRow(typeof(MultiPartRoute),    "Method", new string[] { "api", "entity", "{id}" })]
        [DataRow(typeof(BackslashRoute),    "Method", new string[] { "api\\entity" })]
        [DataRow(typeof(PercentRoute),      "Method", new string[] { "api", "entity" })]
        [DataRow(typeof(UpperCaseRoute),    "Method", new string[] { "API" })]
        [DataRow(typeof(SpacesRoute1),      "Method", new string[] { "A B" })]
        [DataRow(typeof(SpacesRoute2),      "Method", new string[] { "A B" })]
        [DataRow(typeof(EmptyPartRoute),    "Method", new string[] { "api", "", "entity" })]
        [DataRow(typeof(LeadingSlashRoute), "Method", new string[] { "", "api", "entity" })]
        public void Ctor_Sets_Correct_PathParts(Type controllerType, string methodName, string[] expectedPathParts)
        {
            var route = CreateRoute(controllerType, methodName);

            if(expectedPathParts == null) {
                Assert.IsNull(route.PathParts);
            } else {
                Assert.AreEqual(expectedPathParts.Length, route.PathParts.Length);
                for(var i = 0;i < expectedPathParts.Length;++i) {
                    Assert.AreEqual(expectedPathParts[i], route.PathParts[i].Part);
                }
            }
        }

        public class BadPathParts : Controller
        {
            [Route("a/{b}")]
            public void Method1(int c) {;}

            [Route("a/{b?}")]
            public void Method2(int b) {;}
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidRouteException))]
        public void Ctor_Throws_If_PathParts_Do_Not_Match_Method()
        {
            var route = CreateRoute(typeof(BadPathParts), nameof(BadPathParts.Method1));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidRouteException))]
        public void Ctor_Throws_If_Microsoft_Web_API_Question_Mark_Suffix_Used_On_Non_Optional_Parameter()
        {
            var route = CreateRoute(typeof(BadPathParts), nameof(BadPathParts.Method2));
        }

        [TestMethod]
        public void Ctor_Assigns_A_New_ID_For_Each_New_Object()
        {
            var route1 = CreateRoute(typeof(NullRoute), nameof(NullRoute.Method));
            var route2 = CreateRoute(typeof(EmptyRoute), nameof(EmptyRoute.Method));

            Assert.AreNotEqual(0, route1.ID);
            Assert.AreNotEqual(0, route2.ID);
            Assert.AreNotEqual(route1.ID, route2.ID);
        }

        [TestMethod]
        public void Ctor_Assigns_Different_ID_Even_If_Routes_Are_For_Same_Method()
        {
            var route1 = CreateRoute(typeof(NullRoute), nameof(NullRoute.Method));
            var route2 = CreateRoute(typeof(NullRoute), nameof(NullRoute.Method));

            Assert.AreNotEqual(route1.ID, route2.ID);
        }

        public class MethodParametersController : Controller
        {
            [HttpGet, Route("no-params")] public int NoParams() { return 1; }

            [HttpGet, Route("three-params")] public int ThreeParams(string p1, int p2, byte p3) { return 1; }

            [HttpGet, Route("abc/{p1}")] public int OnePathPart(string p1) { return 1; }
        }

        [TestMethod]
        public void Ctor_Fills_MethodParameters_Correctly_For_Routes_With_No_Parameters()
        {
            var route = CreateRoute(typeof(MethodParametersController), nameof(MethodParametersController.NoParams));

            Assert.AreEqual(0, route.MethodParameters.Length);
        }

        [TestMethod]
        public void Ctor_Fills_MethodParameters_Correctly_For_Routes_With_Parameters()
        {
            var route = CreateRoute(typeof(MethodParametersController), nameof(MethodParametersController.ThreeParams));

            Assert.AreEqual(3, route.MethodParameters.Length);
            Assert.AreEqual("p1", route.MethodParameters[0].Name);
            Assert.AreEqual("p2", route.MethodParameters[1].Name);
            Assert.AreEqual("p3", route.MethodParameters[2].Name);
        }

        [TestMethod]
        public void Ctor_Uses_Same_MethodParameters_For_MethodParameters_Property_And_PathPartParameter_Objects()
        {
            var route = CreateRoute(typeof(MethodParametersController), nameof(MethodParametersController.OnePathPart));

            Assert.AreEqual(1, route.MethodParameters.Length);
            Assert.AreEqual(2, route.PathParts.Length);

            var pathPartParameter = (PathPartParameter)route.PathParts[1];
            Assert.AreSame(route.MethodParameters[0], pathPartParameter.MethodParameter);
        }

        public class MethodFilterController : Controller
        {
            [ControllerType_Tests.Filter1]
            [ControllerType_Tests.Filter2]
            [Authorize]
            [Route("filters")]
            public void MethodFilters() { ; }

            [ControllerType_Tests.Filter1]
            [ControllerType_Tests.Filter1]
            [Authorize]
            [Authorize]
            [Route("dupe-filters")]
            public void DuplicateFilters() { ; }

            [Authorize]
            [AllowAnonymous]
            [Route("allow-anon")]
            public void AllowAnonymous() {;}
        }

        [ControllerType_Tests.Filter1]
        [ControllerType_Tests.Filter2]
        public class ControllerFilterController : Controller
        {
            [Route("no-filter")]
            public void NoMethodFilter() {;}

            [Authorize]
            [Route("new-filters")]
            public void ExtendFilters() {;}

            [ControllerType_Tests.Filter2]
            [Route("duplicate-filters")]
            public void DuplicateFilters() {;}
        }

        [TestMethod]
        public void Ctor_Fills_FilterAttributes_Correctly_When_There_Are_No_Filters()
        {
            var route = CreateRoute(typeof(Controller), nameof(Controller.Get));

            Assert.AreEqual(0, route.FilterAttributes.Length);
        }

        [TestMethod]
        public void Ctor_Fills_Method_FilterAttributes_Correctly()
        {
            var route = CreateRoute(typeof(MethodFilterController), nameof(MethodFilterController.MethodFilters));

            Assert.AreEqual(3, route.FilterAttributes.Length);
            Assert.IsTrue(route.FilterAttributes.Any(r => r is ControllerType_Tests.Filter1Attribute));
            Assert.IsTrue(route.FilterAttributes.Any(r => r is ControllerType_Tests.Filter2Attribute));
            Assert.IsTrue(route.FilterAttributes.Any(r => r is AuthorizeAttribute));
        }

        [TestMethod]
        public void Ctor_FilterAttributes_Allows_Duplicate_Filters()
        {
            var route = CreateRoute(typeof(MethodFilterController), nameof(MethodFilterController.DuplicateFilters));

            Assert.AreEqual(4, route.FilterAttributes.Length);

            var filter1s = route.FilterAttributes.OfType<ControllerType_Tests.Filter1Attribute>().ToArray();
            Assert.AreEqual(2, filter1s.Length);
            Assert.AreNotSame(filter1s[0], filter1s[1]);

            var authorises = route.FilterAttributes.OfType<AuthorizeAttribute>().ToArray();
            Assert.AreEqual(2, authorises.Length);
            Assert.AreNotSame(authorises[0], authorises[1]);
        }

        [TestMethod]
        public void Ctor_FilterAttributes_Inherits_Filters_From_Controller()
        {
            var route = CreateRoute(typeof(ControllerFilterController), nameof(ControllerFilterController.NoMethodFilter));

            Assert.AreEqual(2, route.FilterAttributes.Length);
            Assert.IsTrue(route.FilterAttributes.Any(r => r is ControllerType_Tests.Filter1Attribute));
            Assert.IsTrue(route.FilterAttributes.Any(r => r is ControllerType_Tests.Filter2Attribute));
        }

        [TestMethod]
        public void Ctor_FilterAttributes_Extends_Filters_From_Controller()
        {
            var route = CreateRoute(typeof(ControllerFilterController), nameof(ControllerFilterController.ExtendFilters));

            Assert.AreEqual(3, route.FilterAttributes.Length);
            Assert.IsTrue(route.FilterAttributes.Any(r => r is ControllerType_Tests.Filter1Attribute));
            Assert.IsTrue(route.FilterAttributes.Any(r => r is ControllerType_Tests.Filter2Attribute));
            Assert.IsTrue(route.FilterAttributes.Any(r => r is AuthorizeAttribute));
        }

        [TestMethod]
        public void Ctor_FilterAttributes_Adds_Duplicate_Filters_From_Controller()
        {
            var route = CreateRoute(typeof(ControllerFilterController), nameof(ControllerFilterController.DuplicateFilters));

            Assert.AreEqual(3, route.FilterAttributes.Length);

            var filter1s = route.FilterAttributes.OfType<ControllerType_Tests.Filter1Attribute>().ToArray();
            Assert.AreEqual(1, filter1s.Length);

            var filter2s = route.FilterAttributes.OfType<ControllerType_Tests.Filter2Attribute>().ToArray();
            Assert.AreEqual(2, filter2s.Length);
            Assert.AreNotSame(filter2s[0], filter2s[1]);
        }

        [TestMethod]
        public void Ctor_AuthorizationFilters_Filled_Correctly_When_Method_Has_No_Authorisation_Filters()
        {
            var route = CreateRoute(typeof(Controller), nameof(Controller.Get));

            Assert.AreEqual(0, route.AuthorizationFilters.Length);
        }

        [TestMethod]
        public void Ctor_AuthorizationFilters_Filled_Correctly_When_Method_Has_Authorisation_Filters()
        {
            var route = CreateRoute(typeof(MethodFilterController), nameof(MethodFilterController.DuplicateFilters));

            Assert.AreEqual(2, route.AuthorizationFilters.Length);
            Assert.AreNotSame(route.AuthorizationFilters[0], route.AuthorizationFilters[1]);
        }

        [TestMethod]
        public void Ctor_HasAllowAnonymousAttribute_Clear_When_Attribute_Not_Present()
        {
            var route = CreateRoute(typeof(Controller), nameof(Controller.Get));

            Assert.IsFalse(route.HasAllowAnonymousAttribute);
        }

        [TestMethod]
        public void Ctor_HasAllowAnonymousAttribute_Set_When_Attribute_Present()
        {
            var route = CreateRoute(typeof(MethodFilterController), nameof(MethodFilterController.AllowAnonymous));

            Assert.IsTrue(route.HasAllowAnonymousAttribute);
        }

        [TestMethod]
        public void Ctor_OtherFilters_Filled_Correctly_When_Method_Has_No_Other_Filters()
        {
            var route = CreateRoute(typeof(Controller), nameof(Controller.Get));

            Assert.AreEqual(0, route.OtherFilters.Length);
        }

        [TestMethod]
        public void Ctor_OtherFilters_Filled_Correctly_When_Method_Has_None_Authorisation_Filters()
        {
            var route = CreateRoute(typeof(MethodFilterController), nameof(MethodFilterController.DuplicateFilters));

            Assert.AreEqual(2, route.OtherFilters.Length);

            var filter1s = route.FilterAttributes.OfType<ControllerType_Tests.Filter1Attribute>().ToArray();
            Assert.AreEqual(2, filter1s.Length);
            Assert.AreNotSame(filter1s[0], filter1s[1]);
        }

        public class NoDefaultResolverController : Controller
        {
            [HttpGet, Route("a1"), UseParser(typeof(DateTime_Local_Parser))] public int A1(int x) { return x + 1; }

            [HttpGet, Route("b1"), UseFormatter(typeof(DateTime_MicrosoftJson_Formatter))] public DateTime B1(int x) { return DateTime.UtcNow.AddSeconds(x); }
        }

        [UseParser(   typeof(DateTime_Iso8601_Parser),    typeof(ByteArray_Mime64_Parser))]
        [UseFormatter(typeof(DateTime_Iso8601_Formatter), typeof(ByteArray_Mime64_Formatter))]
        public class DefaultResolverController : Controller
        {
            [HttpGet, Route("a1"), UseParser(typeof(DateTime_Local_Parser), typeof(DateTimeOffset_Local_Parser))] public int A1() { return 1; }

            [HttpGet, Route("b1"), UseFormatter(typeof(DateTime_MicrosoftJson_Formatter), typeof(DateTimeOffset_MicrosoftJson_Formatter))] public DateTime B1() { return DateTime.UtcNow; }
        }

        [TestMethod]
        public void Ctor_Builds_TypeParserResolver_From_UseParserAttribute_On_Method()
        {
            var route = CreateRoute(typeof(NoDefaultResolverController), nameof(NoDefaultResolverController.A1));

            Assert.IsNotNull(route.TypeParserResolver);
            var parsers = route.TypeParserResolver.GetParsers();
            Assert.AreEqual(1, parsers.Length);
            Assert.IsInstanceOfType(parsers[0], typeof(DateTime_Local_Parser));
        }

        [TestMethod]
        public void Ctor_Builds_TypeFormatterResolver_From_UseFormatterAttribute_On_Method()
        {
            var route = CreateRoute(typeof(NoDefaultResolverController), nameof(NoDefaultResolverController.B1));

            Assert.IsNotNull(route.TypeFormatterResolver);
            var formatters = route.TypeFormatterResolver.GetFormatters();
            Assert.AreEqual(1, formatters.Length);
            Assert.IsInstanceOfType(formatters[0], typeof(DateTime_MicrosoftJson_Formatter));
        }

        [TestMethod]
        public void Ctor_Uses_Default_TypeParserResolver_From_Controller()
        {
            var route = CreateRoute(typeof(DefaultResolverController), nameof(DefaultResolverController.A1));

            Assert.IsNotNull(route.TypeParserResolver);
            var parsers = route.TypeParserResolver.GetParsers();
            Assert.AreEqual(3, parsers.Length);
            Assert.IsInstanceOfType(route.TypeParserResolver.DateTimeParser,        typeof(DateTime_Local_Parser));
            Assert.IsInstanceOfType(route.TypeParserResolver.DateTimeOffsetParser,  typeof(DateTimeOffset_Local_Parser));
            Assert.IsInstanceOfType(route.TypeParserResolver.ByteArrayParser,       typeof(ByteArray_Mime64_Parser));
        }

        [TestMethod]
        public void Ctor_Uses_Default_TypeFormatterResolver_From_Controller()
        {
            var route = CreateRoute(typeof(DefaultResolverController), nameof(DefaultResolverController.B1));

            Assert.IsNotNull(route.TypeFormatterResolver);
            var formatters = route.TypeFormatterResolver.GetFormatters();
            Assert.AreEqual(3, formatters.Length);
            Assert.IsInstanceOfType(route.TypeFormatterResolver.DateTimeFormatter,        typeof(DateTime_MicrosoftJson_Formatter));
            Assert.IsInstanceOfType(route.TypeFormatterResolver.DateTimeOffsetFormatter,  typeof(DateTimeOffset_MicrosoftJson_Formatter));
            Assert.IsInstanceOfType(route.TypeFormatterResolver.ByteArrayFormatter,       typeof(ByteArray_Mime64_Formatter));
        }

        [TestMethod]
        public void Ctor_Passes_Method_TypeParserResolver_To_MethodParameter_Ctor()
        {
            var route = CreateRoute(typeof(NoDefaultResolverController), nameof(NoDefaultResolverController.A1));
            var parameter = route.MethodParameters.Single();

            Assert.IsNotNull(parameter.TypeParserResolver);
            Assert.AreEqual(parameter.TypeParserResolver, route.TypeParserResolver);
        }
    }
}
