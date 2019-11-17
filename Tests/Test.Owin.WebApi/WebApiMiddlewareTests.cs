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
using System.Threading.Tasks;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using AWhewell.Owin.Interface;
using AWhewell.Owin.Interface.WebApi;

namespace Test.AWhewell.Owin.WebApi
{
    [TestClass]
    public class WebApiMiddlewareTests
    {
        private IClassFactory               _Snapshot;
        private IWebApiMiddleware           _WebApi;
        private MockPipeline                _Pipeline;
        private MockOwinEnvironment         _Environment;
        private Mock<IControllerManager>    _ControllerManager;
        private List<Type>                  _ControllerTypes;
        private Mock<IRouteManager>         _RouteManager;
        private List<Route>                 _Routes;
        private Mock<IRouteMapper>          _RouteMapper;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _ControllerManager = MockHelper.FactoryImplementation<IControllerManager>();
            _ControllerTypes = new List<Type>();
            _ControllerManager.Setup(r => r.DiscoverControllers()).Returns(_ControllerTypes);

            _RouteManager = MockHelper.FactoryImplementation<IRouteManager>();
            _Routes = new List<Route>();
            _RouteManager.Setup(r => r.DiscoverRoutes(It.IsAny<IEnumerable<Type>>())).Returns(_Routes);

            _RouteMapper = MockHelper.FactoryImplementation<IRouteMapper>();

            _Pipeline = new MockPipeline();
            _Environment = new MockOwinEnvironment();

            _WebApi = Factory.Resolve<IWebApiMiddleware>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        private void CallMiddleware() => _Pipeline.CallMiddleware(_WebApi.CreateMiddleware, _Environment.Environment);

        [TestMethod]
        public void Middleware_Calls_Next_Delegate()
        {
            CallMiddleware();
            Assert.IsTrue(_Pipeline.NextMiddlewareCalled);
        }

        [TestMethod]
        public void CreateMiddleware_Finds_Controller_Types()
        {
            _WebApi.CreateMiddleware(null);

            _ControllerManager.Verify(r => r.DiscoverControllers(), Times.Once());
        }

        [TestMethod]
        public void CreateMiddleware_Finds_Routes()
        {
            _WebApi.CreateMiddleware(null);

            _RouteManager.Verify(r => r.DiscoverRoutes(_ControllerTypes), Times.Once());
        }

        [TestMethod]
        public void CreateMiddleware_Initialises_Route_Mapper()
        {
            _WebApi.CreateMiddleware(null);

            _RouteMapper.Verify(r => r.Initialise(_Routes), Times.Once());
        }

        [TestMethod]
        [DataRow(false, false)]
        [DataRow(false, true)]
        [DataRow(true,  false)]
        [DataRow(true,  true)]
        public void CreateMiddleware_Sets_Properties_On_Route_Mapper_Before_Initialisation(bool queryStringCaseSensitive, bool formBodyCaseSensitive)
        {
            bool? actualQueryStringCaseSensitive = null;
            bool? actualFormBodyCaseSensitive = null;
            _RouteMapper.Setup(r => r.Initialise(It.IsAny<IEnumerable<Route>>())).Callback(() => {
                actualFormBodyCaseSensitive =    _RouteMapper.Object.AreFormNamesCaseSensitive;
                actualQueryStringCaseSensitive = _RouteMapper.Object.AreQueryStringNamesCaseSensitive;
            });

            _WebApi.AreFormNamesCaseSensitive =         formBodyCaseSensitive;
            _WebApi.AreQueryStringNamesCaseSensitive =  queryStringCaseSensitive;

            _WebApi.CreateMiddleware(null);

            Assert.AreEqual(queryStringCaseSensitive, actualQueryStringCaseSensitive);
            Assert.AreEqual(formBodyCaseSensitive,    actualFormBodyCaseSensitive);
        }
    }
}
