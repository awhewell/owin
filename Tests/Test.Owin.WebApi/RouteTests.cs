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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Owin.Interface.WebApi;

namespace Test.Owin.WebApi
{
    [TestClass]
    public class RouteTests
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
        }

        public static Route CreateRoute(Type controllerType, string methodName, string choosePath = null)
        {
            var method = controllerType.GetMethod(methodName);
            var routeAttributes = method.GetCustomAttributes(inherit: false).OfType<RouteAttribute>().ToArray();
            var routeAttribute = routeAttributes.Length == 1
                ? routeAttributes[0]
                : routeAttributes.Single(r => r.Route == choosePath);

            return new Route(controllerType, method, routeAttribute);
        }

        public static Route CreateRoute<T>(string methodName, string choosePath = null) => CreateRoute(typeof(T), methodName, choosePath);

        [TestMethod]
        public void Route_Ctor_Copies_Arguments_Properties()
        {
            var controller = typeof(Controller);
            var methodInfo = controller.GetMethod(nameof(Controller.Example));
            var routeAttribute = methodInfo.GetCustomAttributes(inherit: false).OfType<RouteAttribute>().Single();

            var route = new Route(controller, methodInfo, routeAttribute);
            Assert.AreSame(controller, route.Controller);
            Assert.AreSame(methodInfo, route.Method);
            Assert.AreSame(routeAttribute, route.RouteAttribute);
        }

        [TestMethod]
        public void Route_Ctor_Defaults_Method_To_Post()
        {
            var route = CreateRoute<Controller>(nameof(Controller.Example));

            Assert.AreEqual("POST", route.HttpMethod);
        }

        [TestMethod]
        [DataRow("Delete",  "DELETE")]
        [DataRow("Get",     "GET")]
        [DataRow("Head",    "HEAD")]
        [DataRow("Patch",   "PATCH")]
        [DataRow("Post",    "POST")]
        [DataRow("Put",     "PUT")]
        public void Route_Ctor_Sets_HttpMethod_From_Method_Attribute(string methodName, string expected)
        {
            var route = CreateRoute<Controller>(methodName);

            Assert.AreEqual(expected, route.HttpMethod);
        }

        [TestMethod]
        public void Route_Ctor_Sets_Empty_HttpMethod_If_Multiple_Attributes_Present()
        {
            var route = CreateRoute<Controller>(nameof(Controller.Duplicate_Methods_Not_Allowed));

            Assert.AreEqual("", route.HttpMethod);
        }

        public class NullRoute : Controller         { [HttpGet, Route(null)]                public int Method() { return 1; } }
        public class EmptyRoute : Controller        { [HttpGet, Route("")]                  public int Method() { return 1; } }
        public class SpaceRoute : Controller        { [HttpGet, Route(" ")]                 public int Method() { return 1; } }
        public class MultiPartRoute : Controller    { [HttpGet, Route("api/entity/{id}")]   public int Method() { return 1; } }
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
        public void Route_Ctor_Sets_Correct_PathParts(Type controllerType, string methodName, string[] expectedPathParts)
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
    }
}