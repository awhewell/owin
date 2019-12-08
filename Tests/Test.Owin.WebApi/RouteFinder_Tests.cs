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
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AWhewell.Owin.Interface.WebApi;

namespace Test.AWhewell.Owin.WebApi
{
    [TestClass]
    public class RouteFinder_Tests
    {
        class ValidRoutes : IApiController
        {
            public IDictionary<string, object> OwinEnvironment { get; set; }

            [Route("path-to-example1")]
            public void Example1()
            {
            }

            public void Example2()
            {
            }
        }

        class PrivateMethods : IApiController
        {
            public IDictionary<string, object> OwinEnvironment { get; set; }

            [Route("private-route")]
            private void PrivateRoute()
            {
            }
        }

        class StaticMethods : IApiController
        {
            public IDictionary<string, object> OwinEnvironment { get; set; }

            [Route("static-route")]
            public static void StaticRoute()
            {
            }
        }

        private IClassFactory   _Snapshot;
        private IRouteFinder    _RouteFinder;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _RouteFinder = Factory.Resolve<IRouteFinder>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DiscoverRoutes_Throws_If_Passed_Null()
        {
            _RouteFinder.DiscoverRoutes(null);
        }

        [TestMethod]
        public void DiscoverRoutes_Returns_Methods_Tagged_With_Route_Attribute()
        {
            var routes = _RouteFinder.DiscoverRoutes(new ControllerType[] { new ControllerType(typeof(ValidRoutes), null) });

            var route = routes.Single();
            Assert.AreEqual("path-to-example1", route.RouteAttribute.Route);
            Assert.AreSame(typeof(ValidRoutes).GetMethod(nameof(ValidRoutes.Example1)), route.Method);
            Assert.AreSame(typeof(ValidRoutes), route.ControllerType.Type);
        }

        [TestMethod]
        public void DiscoverRoutes_Ignores_Static_Methods()
        {
            var routes = _RouteFinder.DiscoverRoutes(new ControllerType[] { new ControllerType(typeof(StaticMethods), null) });

            Assert.AreEqual(0, routes.Count());
        }
    }
}
