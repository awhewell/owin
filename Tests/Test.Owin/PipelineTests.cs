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
using System.Threading.Tasks;
using AWhewell.Owin.Interface;
using AWhewell.Owin.Utility;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Test.AWhewell.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    [TestClass]
    public class PipelineTests
    {
        private IPipeline                           _Pipeline;
        private Mock<IPipelineBuilderEnvironment>   _BuilderEnvironment;
        private List<Func<AppFunc, AppFunc>>        _MiddlewareChain;
        private List<Func<AppFunc, AppFunc>>        _StreamManipulatorChain;
        private Dictionary<string, object>          _Environment;

        [TestInitialize]
        public void TestInitialise()
        {
            _MiddlewareChain = new List<Func<AppFunc, AppFunc>>();
            _StreamManipulatorChain = new List<Func<AppFunc, AppFunc>>();
            _BuilderEnvironment = MockHelper.FactoryImplementation<IPipelineBuilderEnvironment>();
            _BuilderEnvironment.Setup(r => r.MiddlewareChain).Returns(_MiddlewareChain);
            _BuilderEnvironment.Setup(r => r.StreamManipulatorChain).Returns(_StreamManipulatorChain);

            _Environment = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            _Pipeline = Factory.Resolve<IPipeline>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Construct_Throws_If_Passed_Null()
        {
            _Pipeline.Construct(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Construct_Throws_If_Called_Twice()
        {
            _Pipeline.Construct(_BuilderEnvironment.Object);
            _Pipeline.Construct(_BuilderEnvironment.Object);
        }

        [TestMethod]
        public void Construct_Builds_Middleware_AppFunc_And_ProcessRequest_Uses_It()
        {
            var middleware = new MockMiddleware();
            _MiddlewareChain.Add(middleware.CreateAppFunc);
            _Pipeline.Construct(_BuilderEnvironment.Object);

            _Pipeline.ProcessRequest(_Environment).Wait();
            _Pipeline.ProcessRequest(_Environment).Wait();

            Assert.AreEqual(1, middleware.CreateAppFuncCallCount);
        }

        [TestMethod]
        public void HasStreamManipulators_Is_Clear_Before_Construct_Is_Called()
        {
            Assert.AreEqual(false, _Pipeline.HasStreamManipulators);
        }

        [TestMethod]
        public void HasStreamManipulators_Is_Set_When_Stream_Manipulators_Are_Added()
        {
            _StreamManipulatorChain.Add(new MockMiddleware().CreateAppFunc);

            _Pipeline.Construct(_BuilderEnvironment.Object);

            Assert.AreEqual(true, _Pipeline.HasStreamManipulators);
        }

        [TestMethod]
        public void HasStreamManipulators_Is_Clear_When_Stream_Manipulators_Are_Not_Added()
        {
            _Pipeline.Construct(_BuilderEnvironment.Object);

            Assert.AreEqual(false, _Pipeline.HasStreamManipulators);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProcessRequest_Throws_If_Passed_Null()
        {
            _Pipeline.Construct(_BuilderEnvironment.Object);

            try {
                _Pipeline.ProcessRequest(null).Wait();
            } catch(AggregateException ex) {
                throw ex.Flatten().InnerException;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ProcessRequest_Throws_If_Not_Yet_Constructed()
        {
            try {
                _Pipeline.ProcessRequest(_Environment).Wait();
            } catch(AggregateException ex) {
                throw ex.Flatten().InnerException;
            }
        }

        [TestMethod]
        public void ProcessRequest_Sets_404_Status_Code_When_Pipeline_Is_Empty()
        {
            _Pipeline.Construct(_BuilderEnvironment.Object);

            _Pipeline.ProcessRequest(_Environment).Wait();

            Assert.AreEqual(1, _Environment.Count);
            Assert.AreEqual(404, (int)_Environment[EnvironmentKey.ResponseStatusCode]);
        }

        [TestMethod]
        public void ProcessRequest_Calls_Middleware()
        {
            var middleware = new MockMiddleware();
            _MiddlewareChain.Add(middleware.CreateAppFunc);
            _Pipeline.Construct(_BuilderEnvironment.Object);

            _Pipeline.ProcessRequest(_Environment).Wait();

            Assert.AreEqual(1, middleware.Environments.Count);
            Assert.AreSame(_Environment, middleware.LastEnvironment);

            Assert.AreEqual(1, middleware.Nexts.Count);
            Assert.IsNotNull(middleware.LastNext);
        }

        [TestMethod]
        public void ProcessRequest_Calls_Two_Middleware()
        {
            var middleware_1 = new MockMiddleware();
            var middleware_2 = new MockMiddleware();
            middleware_1.Action = () => Assert.AreEqual(0, middleware_2.AppFuncCallCount);
            middleware_2.Action = () => Assert.AreEqual(1, middleware_1.AppFuncCallCount);

            _MiddlewareChain.Add(middleware_1.CreateAppFunc);
            _MiddlewareChain.Add(middleware_2.CreateAppFunc);
            _Pipeline.Construct(_BuilderEnvironment.Object);

            _Pipeline.ProcessRequest(_Environment).Wait();

            Assert.AreEqual(1, middleware_1.AppFuncCallCount);
            Assert.AreEqual(1, middleware_2.AppFuncCallCount);
        }

        [TestMethod]
        public void ProcessRequest_Middleware_Can_End_Processing_Early()
        {
            var middleware_1 = new MockMiddleware() { ChainToNextMiddleware = false, };
            var middleware_2 = new MockMiddleware();

            _MiddlewareChain.Add(middleware_1.CreateAppFunc);
            _MiddlewareChain.Add(middleware_2.CreateAppFunc);
            _Pipeline.Construct(_BuilderEnvironment.Object);

            _Pipeline.ProcessRequest(_Environment).Wait();

            Assert.AreEqual(1, middleware_1.AppFuncCallCount);
            Assert.AreEqual(0, middleware_2.AppFuncCallCount);
        }

        [TestMethod]
        public void ProcessRequest_Middleware_Can_End_Chain_In_One_Request_And_Not_End_Chain_In_Another()
        {
            var middleware_1 = new MockMiddleware() { ChainToNextMiddleware = false, };
            var middleware_2 = new MockMiddleware();

            _MiddlewareChain.Add(middleware_1.CreateAppFunc);
            _MiddlewareChain.Add(middleware_2.CreateAppFunc);
            _Pipeline.Construct(_BuilderEnvironment.Object);
            _Pipeline.ProcessRequest(_Environment).Wait();
            middleware_1.ChainToNextMiddleware = true;
            _Pipeline.ProcessRequest(_Environment).Wait();

            Assert.AreEqual(2, middleware_1.AppFuncCallCount);
            Assert.AreEqual(1, middleware_2.AppFuncCallCount);
        }

        [TestMethod]
        public void ProcessRequest_Calls_Stream_Manipulators()
        {
            var middleware = new MockMiddleware();
            _StreamManipulatorChain.Add(middleware.CreateAppFunc);
            _Pipeline.Construct(_BuilderEnvironment.Object);

            _Pipeline.ProcessRequest(_Environment).Wait();

            Assert.AreEqual(1, middleware.Environments.Count);
            Assert.AreSame(_Environment, middleware.LastEnvironment);

            Assert.AreEqual(1, middleware.Nexts.Count);
            Assert.IsNotNull(middleware.LastNext);
        }

        [TestMethod]
        public void ProcessRequest_Calls_Stream_Manipulators_After_Middleware()
        {
            var middleware = new MockMiddleware();
            var streamManipulator = new MockMiddleware();
            middleware.Action = () => Assert.AreEqual(0, streamManipulator.AppFuncCallCount);
            streamManipulator.Action = () => Assert.AreEqual(1, middleware.AppFuncCallCount);

            _MiddlewareChain.Add(middleware.CreateAppFunc);
            _StreamManipulatorChain.Add(streamManipulator.CreateAppFunc);
            _Pipeline.Construct(_BuilderEnvironment.Object);

            _Pipeline.ProcessRequest(_Environment).Wait();

            Assert.AreEqual(1, middleware.AppFuncCallCount);
            Assert.AreEqual(1, streamManipulator.AppFuncCallCount);
        }

        [TestMethod]
        public void ProcessRequest_Does_Not_Let_Stream_Manipulators_Break_The_Chain()
        {
            var middleware_1 = new MockMiddleware() { ChainToNextMiddleware = false, };
            var middleware_2 = new MockMiddleware();

            _StreamManipulatorChain.Add(middleware_1.CreateAppFunc);
            _StreamManipulatorChain.Add(middleware_2.CreateAppFunc);
            _Pipeline.Construct(_BuilderEnvironment.Object);

            _Pipeline.ProcessRequest(_Environment).Wait();

            Assert.AreEqual(1, middleware_1.AppFuncCallCount);
            Assert.AreEqual(1, middleware_2.AppFuncCallCount);
        }

        [TestMethod]
        public void ProcessRequest_Calls_Stream_Manipulators_Even_If_Middleware_Breaks_Chain()
        {
            var middleware_1 = new MockMiddleware() { ChainToNextMiddleware = false, };
            var middleware_2 = new MockMiddleware();
            var streamManipulator = new MockMiddleware();

            _MiddlewareChain.Add(middleware_1.CreateAppFunc);
            _MiddlewareChain.Add(middleware_2.CreateAppFunc);
            _StreamManipulatorChain.Add(streamManipulator.CreateAppFunc);
            _Pipeline.Construct(_BuilderEnvironment.Object);

            _Pipeline.ProcessRequest(_Environment).Wait();

            Assert.AreEqual(1, middleware_1.AppFuncCallCount);
            Assert.AreEqual(0, middleware_2.AppFuncCallCount);
            Assert.AreEqual(1, streamManipulator.AppFuncCallCount);
        }
    }
}
