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
using System.Text;
using AWhewell.Owin.Interface.WebApi;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin.WebApi
{
    [TestClass]
    [DoNotParallelize]      // Makes use of statics to communicate between IApiController and unit tests
    public class RouteCaller_Tests
    {
        public class Controller : IApiController
        {
            public IDictionary<string, object> OwinEnvironment { get; set; }

            public Controller()
            {
                Assert.IsNull(RouteCaller_Tests._ControllerInstance);
                RouteCaller_Tests._ControllerInstance = this;
            }

            public int Route_1_CallCount { get; set; }
            public int Route_1_LastInput { get; set; }

            [HttpGet, Route("1")]
            public int Route_1(int input)
            {
                Assert.AreSame(RouteCaller_Tests._ExpectedEnvironment, OwinEnvironment);
                ++Route_1_CallCount;
                Route_1_LastInput = input;
                return input + 1;
            }

            public static int Route_2_CallCount { get; set; }
            public static int Route_2_LastInput { get; set; }

            [HttpGet, Route("2")]
            public static int Route_2(IDictionary<string, object> owinEnvironment, int input)
            {
                Assert.IsNull(RouteCaller_Tests._ControllerInstance);
                Assert.AreSame(owinEnvironment, RouteCaller_Tests._ExpectedEnvironment);
                ++Route_2_CallCount;
                Route_2_LastInput = input;
                return input * 2;
            }
        }

        private static Controller                   _ControllerInstance;
        private static IDictionary<string, object>  _ExpectedEnvironment;

        private IRouteCaller        _RouteCaller;
        private MockOwinEnvironment _Environment;


        [TestInitialize]
        public void TestInitialise()
        {
            // Initialise STATIC fields - these tests must not be run in parallel
            _ControllerInstance = null;
            _ExpectedEnvironment = null;
            Controller.Route_2_CallCount = 0;
            Controller.Route_2_LastInput = 0;

            _Environment = new MockOwinEnvironment();
            _ExpectedEnvironment = _Environment.Environment;

            _RouteCaller = Factory.Resolve<IRouteCaller>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CallRoute_Throws_If_Environment_Is_Null()
        {
            var route = Route_Tests.CreateRoute<Controller>(nameof(Controller.Route_1));
            var routeParameters = Route_Tests.CreateRouteParameters(41);

            _RouteCaller.CallRoute(null, route, routeParameters);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CallRoute_Throws_If_Route_Is_Null()
        {
            var routeParameters = Route_Tests.CreateRouteParameters(41);

            _RouteCaller.CallRoute(_Environment.Environment, null, routeParameters);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CallRoute_Throws_If_RouteParameters_Are_Null()
        {
            var route = Route_Tests.CreateRoute<Controller>(nameof(Controller.Route_1));

            _RouteCaller.CallRoute(_Environment.Environment, route, null);
        }

        [TestMethod]
        public void CallRoute_Creates_Instance_Of_Controller_And_Calls_Route()
        {
            var route = Route_Tests.CreateRoute<Controller>(nameof(Controller.Route_1));
            var routeParameters = Route_Tests.CreateRouteParameters(41);

            var outcome = _RouteCaller.CallRoute(_Environment.Environment, route, routeParameters);

            Assert.IsNotNull(_ControllerInstance);
            Assert.AreEqual(1, _ControllerInstance.Route_1_CallCount);
            Assert.AreEqual(41, _ControllerInstance.Route_1_LastInput);
            Assert.AreEqual(42, outcome);
        }

        [TestMethod]
        public void CallRoute_Can_Call_Static_Routes()
        {
            var route = Route_Tests.CreateRoute<Controller>(nameof(Controller.Route_2));
            var routeParameters = Route_Tests.CreateRouteParameters(_Environment.Environment, 32);

            var outcome = _RouteCaller.CallRoute(_Environment.Environment, route, routeParameters);

            Assert.IsNull(_ControllerInstance);
            Assert.AreEqual(1, Controller.Route_2_CallCount);
            Assert.AreEqual(32, Controller.Route_2_LastInput);
            Assert.AreEqual(64, outcome);
        }
    }
}
