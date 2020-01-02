// Copyright © 2020 onwards, Andrew Whewell
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
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin.WebApi
{
    [TestClass]
    public class RouteFilter_Tests
    {
        [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
        class BaseFilterAttribute : Attribute, IFilterAttribute
        {
            public string Name { get; set; } = "";

            public bool Allow { get; set; } = true;

            public IDictionary<string, object> OwinEnvironment { get; set; }

            public bool AllowRequest(IDictionary<string, object> owinEnvironment)
            {
                OwinEnvironment = owinEnvironment;
                return Allow;
            }
        }

        class AuthFilterAttribute : BaseFilterAttribute, IAuthorizationFilter
        {
        }

        class OtherFilterAttribute : BaseFilterAttribute
        {
        }

        class Controller : IApiController
        {
            public IDictionary<string, object> OwinEnvironment { get; set; }

            [Route("no-auth-filter")]
            public void NoAuthFilter() {;}

            [AuthFilter]
            [Route("one-auth-filter")]
            public void OneAuthFilter() {;}

            [AuthFilter(Name = "1")]
            [AuthFilter(Name = "2")]
            [Route("two-auth-filters")]
            public void TwoAuthFilters() {;}

            [AuthFilter]
            [AllowAnonymous]
            [Route("one-auth-allow-anon")]
            public void OneAuthFilterAllowAnonymous() {;}

            [AuthFilter(Name = "A1")]
            [OtherFilter(Name = "O1")]
            [AllowAnonymous]
            [Route("one-auth-one-other-allow-anon")]
            public void OneAuthOneOtherFilterAllowAnonymous() {;}

            [OtherFilter]
            [Route("one-other-filter")]
            public void OneOtherFilter() {;}

            [OtherFilter(Name = "O1")]
            [AuthFilter(Name = "A1")]
            [OtherFilter(Name = "O2")]
            [AuthFilter(Name = "A2")]
            [Route("auth-and-other-filters")]
            public void TwoAuthTwoOther() {;}
        }

        private IRouteFilter            _RouteFilter;
        private MockOwinEnvironment     _Environment;

        [TestInitialize]
        public void TestInitialise()
        {
            _RouteFilter = Factory.Resolve<IRouteFilter>();

            _Environment = new MockOwinEnvironment();
            _Environment.SetRequestPrincipal("test", "Basic");
        }

        private BaseFilterAttribute ConfigureBaseFilter(Route route, string baseFilterName, bool allowRequest)
        {
            var baseFilter = route.FilterAttributes
                .OfType<BaseFilterAttribute>()
                .Single(r => r.Name == baseFilterName);
            baseFilter.Allow = allowRequest;
            baseFilter.OwinEnvironment = null;

            return baseFilter;
        }

        [TestMethod]
        public void CanCallRoute_Returns_True_If_Route_Has_No_Authorisation_Filter()
        {
            var route = Route_Tests.CreateRoute<Controller>(nameof(Controller.NoAuthFilter));

            var actual = _RouteFilter.CanCallRoute(route, _Environment.Environment);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void CanCallRoute_Returns_False_If_Singleton_Auth_Filter_Returns_False()
        {
            var route = Route_Tests.CreateRoute<Controller>(nameof(Controller.OneAuthFilter));
            ConfigureBaseFilter(route, "", false);

            var actual = _RouteFilter.CanCallRoute(route, _Environment.Environment);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void CanCallRoute_Returns_True_If_Singleton_Auth_Filter_Returns_True()
        {
            var route = Route_Tests.CreateRoute<Controller>(nameof(Controller.OneAuthFilter));
            ConfigureBaseFilter(route, "", true);

            var actual = _RouteFilter.CanCallRoute(route, _Environment.Environment);

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void CanCallRoute_Skips_Authorisation_Filters_If_Anonymous_And_AllowAnonymous_Used()
        {
            var route = Route_Tests.CreateRoute<Controller>(nameof(Controller.OneAuthFilterAllowAnonymous));
            var authFilter = ConfigureBaseFilter(route, "", false);
            _Environment.SetRequestPrincipal(null);

            var actual = _RouteFilter.CanCallRoute(route, _Environment.Environment);

            Assert.IsTrue(actual);
            Assert.IsNull(authFilter.OwinEnvironment);
        }

        [TestMethod]
        public void CanCallRoute_Does_Not_Skip_Authorisation_Filters_If_AllowAnonymous_Used_And_Request_Is_Not_Anonymous()
        {
            var route = Route_Tests.CreateRoute<Controller>(nameof(Controller.OneAuthFilterAllowAnonymous));
            ConfigureBaseFilter(route, "", false);

            var actual = _RouteFilter.CanCallRoute(route, _Environment.Environment);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void CanCallRoute_Always_Calls_Other_Filter_If_Authentication_Skipped()
        {
            var route = Route_Tests.CreateRoute<Controller>(nameof(Controller.OneAuthOneOtherFilterAllowAnonymous));
            var authFilter = ConfigureBaseFilter(route, "A1", false);
            var otherFilter = ConfigureBaseFilter(route, "O1", false);
            _Environment.SetRequestPrincipal(null);

            var actual = _RouteFilter.CanCallRoute(route, _Environment.Environment);

            Assert.IsFalse(actual);
            Assert.IsNull(authFilter.OwinEnvironment);
            Assert.IsNotNull(otherFilter.OwinEnvironment);
        }

        [TestMethod]
        public void CanCallRoute_Passes_Environment_To_Auth_Filter()
        {
            var route = Route_Tests.CreateRoute<Controller>(nameof(Controller.OneAuthFilter));
            var authFilter = ConfigureBaseFilter(route, "", true);

            _RouteFilter.CanCallRoute(route, _Environment.Environment);

            Assert.AreSame(_Environment.Environment, authFilter.OwinEnvironment);
        }

        [TestMethod]
        [DataRow(false, false,  false)]
        [DataRow(false, true,   false)]
        [DataRow(true,  false,  false)]
        [DataRow(true,  true,   true)]
        public void CanCallRoute_Returns_Correct_Result_When_More_Than_One_Auth_Filter_Used(bool auth1Result, bool auth2Result, bool expected)
        {
            var route = Route_Tests.CreateRoute<Controller>(nameof(Controller.TwoAuthFilters));
            ConfigureBaseFilter(route, "1", auth1Result);
            ConfigureBaseFilter(route, "2", auth2Result);

            var actual = _RouteFilter.CanCallRoute(route, _Environment.Environment);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CanCallRoute_Returns_False_If_Other_Filter_Returns_False()
        {
            var route = Route_Tests.CreateRoute<Controller>(nameof(Controller.OneOtherFilter));
            ConfigureBaseFilter(route, "", false);

            var actual = _RouteFilter.CanCallRoute(route, _Environment.Environment);

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void CanCallRoute_Passes_Environment_To_Other_Filter()
        {
            var route = Route_Tests.CreateRoute<Controller>(nameof(Controller.OneOtherFilter));
            var otherFilter = ConfigureBaseFilter(route, "", true);

            _RouteFilter.CanCallRoute(route, _Environment.Environment);

            Assert.AreSame(_Environment.Environment, otherFilter.OwinEnvironment);
        }

        [TestMethod]
        //       -- Auth --     -- Other --     -- Called --    Expected
        [DataRow(false, false,  true,   true,   true,   false,  false)]
        [DataRow(false, true,   true,   true,   true,   false,  false)]
        [DataRow(true,  false,  true,   true,   true,   false,  false)]
        [DataRow(true,  true,   true,   true,   true,   true,   true)]
        [DataRow(true,  true,   false,  false,  true,   true,   false)]
        [DataRow(true,  true,   false,  true,   true,   true,   false)]
        [DataRow(true,  true,   true,   false,  true,   true,   false)]
        public void CanCallRoute_Calls_Combinations_Of_Auth_And_Other_Filters_Correctly(bool auth1Result, bool auth2Result, bool other1Result, bool other2Result, bool expectAuthCall, bool expectOtherCall, bool expected)
        {
            var route = Route_Tests.CreateRoute<Controller>(nameof(Controller.TwoAuthTwoOther));
            var auth1 =  ConfigureBaseFilter(route, "A1", auth1Result);
            var auth2 =  ConfigureBaseFilter(route, "A2", auth2Result);
            var other1 = ConfigureBaseFilter(route, "O1", other1Result);
            var other2 = ConfigureBaseFilter(route, "O2", other2Result);

            var actual = _RouteFilter.CanCallRoute(route, _Environment.Environment);
            var anyAuthCalled =  auth1.OwinEnvironment != null || auth2.OwinEnvironment != null;
            var anyOtherCalled = other1.OwinEnvironment != null || other2.OwinEnvironment != null;

            Assert.AreEqual(expectAuthCall, anyAuthCalled);
            Assert.AreEqual(expectOtherCall, anyOtherCalled);
            Assert.AreEqual(expected, actual);
        }
    }
}
