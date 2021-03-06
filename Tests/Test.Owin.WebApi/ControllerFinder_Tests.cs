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
using AWhewell.Owin.Interface.WebApi;
using AWhewell.Owin.Utility;
using AWhewell.Owin.Utility.Formatters;
using AWhewell.Owin.Utility.Parsers;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Test.AWhewell.Owin.WebApi
{
    [TestClass]
    public class ControllerFinder_Tests
    {
        class MockController1 : IApiController
        {
            public IDictionary<string, object> OwinEnvironment { get; set; }
        }

        private IClassFactory           _Snapshot;
        private Mock<ITypeFinder> _AppDomainWrapper;
        private List<Type>              _AllTypes;
        private IControllerFinder       _ControllerFinder;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _AppDomainWrapper = MockHelper.FactoryImplementation<ITypeFinder>();
            _AllTypes = new List<Type>();
            _AppDomainWrapper.Setup(r => r.GetAllTypes()).Returns(_AllTypes);

            _ControllerFinder = Factory.Resolve<IControllerFinder>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        [TestMethod]
        public void DiscoverControllers_Finds_Controllers_Using_AppDomainWrapper()
        {
            _AllTypes.Add(typeof(ControllerFinder_Tests));
            _AllTypes.Add(typeof(MockController1));
            _AllTypes.Add(typeof(string));

            var controllerTypes = _ControllerFinder.DiscoverControllers().ToArray();

            _AppDomainWrapper.Verify(r => r.GetAllTypes(), Times.Once());
            Assert.AreEqual(1, controllerTypes.Length);

            Assert.AreEqual(typeof(MockController1), controllerTypes[0].Type);
            Assert.IsNull(controllerTypes[0].TypeParserResolver);
        }

        [TestMethod]
        public void DiscoverControllers_Uses_Default_TypeParserResolver()
        {
            _AllTypes.Add(typeof(MockController1));
            var resolver = new TypeParserResolver(new DateTime_Local_Parser());
            _ControllerFinder.DefaultTypeParserResolver = resolver;

            var controllerTypes = _ControllerFinder.DiscoverControllers().ToArray();

            _AppDomainWrapper.Verify(r => r.GetAllTypes(), Times.Once());
            Assert.AreEqual(resolver, controllerTypes[0].TypeParserResolver);
        }

        [TestMethod]
        public void DiscoverControllers_Uses_Default_TypeFormatterResolver()
        {
            _AllTypes.Add(typeof(MockController1));
            var resolver = new TypeFormatterResolver(new DateTime_MicrosoftJson_Formatter());
            _ControllerFinder.DefaultTypeFormatterResolver = resolver;

            var controllerTypes = _ControllerFinder.DiscoverControllers().ToArray();

            _AppDomainWrapper.Verify(r => r.GetAllTypes(), Times.Once());
            Assert.AreEqual(resolver, controllerTypes[0].TypeFormatterResolver);
        }
    }
}
