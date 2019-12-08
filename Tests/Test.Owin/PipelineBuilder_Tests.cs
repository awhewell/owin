// Copyright � 2019 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using AWhewell.Owin.Interface;

namespace Test.AWhewell.Owin
{
    [TestClass]
    public class PipelineBuilder_Tests
    {
        private IClassFactory                       _Snapshot;
        private IPipelineBuilder                    _PipelineBuilder;
        private Mock<IPipelineBuilderEnvironment>   _Environment;
        private Mock<IPipeline>                     _Pipeline;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _Environment = MockHelper.FactoryImplementation<IPipelineBuilderEnvironment>();
            _Pipeline = MockHelper.FactoryImplementation<IPipeline>();

            _PipelineBuilder = Factory.Resolve<IPipelineBuilder>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegisterMiddlewareBuilder_Throws_If_Passed_Null()
        {
            _PipelineBuilder.RegisterMiddlewareBuilder(null, 0);
        }

        [TestMethod]
        public void RegisterMiddlewareBuilder_Returns_Handle()
        {
            var callback = new MockPipelineCallback();
            Assert.IsNotNull(_PipelineBuilder.RegisterMiddlewareBuilder(callback.Callback, 0));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DeregisterMiddlewareBuilder_Throws_If_Passed_Null()
        {
            _PipelineBuilder.DeregisterMiddlewareBuilder(null);
        }

        [TestMethod]
        public void DeregisterMiddlewareBuilder_Does_Nothing_If_Passed_Same_Handle_Twice()
        {
            var callback = new MockPipelineCallback();
            var handle = _PipelineBuilder.RegisterMiddlewareBuilder(callback.Callback, 0);

            _PipelineBuilder.DeregisterMiddlewareBuilder(handle);
            _PipelineBuilder.DeregisterMiddlewareBuilder(handle);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreatePipeline_Throws_If_Passed_Null()
        {
            _PipelineBuilder.CreatePipeline(null);
        }

        [TestMethod]
        public void CreatePipeline_Calls_Registered_Callback()
        {
            var callback = new MockPipelineCallback();
            _PipelineBuilder.RegisterMiddlewareBuilder(callback.Callback, 0);

            _PipelineBuilder.CreatePipeline(_Environment.Object);

            Assert.AreEqual(1, callback.CallCount);
            Assert.AreEqual(1, callback.AllEnvironments.Count);
            Assert.AreSame(_Environment.Object, callback.LastEnvironment);
        }

        [TestMethod]
        public void Sets_Mandatory_Keys_In_Properties()
        {
            _PipelineBuilder.CreatePipeline(_Environment.Object);

            Assert.AreEqual(Constants.Version, _Environment.Object.Properties["owin.Version"]);
        }

        [TestMethod]
        public void CreatePipeline_Passes_Environment_To_New_Instance_Of_Pipeline()
        {
            var callback = new MockPipelineCallback();
            callback.Action = (env) => _Pipeline.Verify(r => r.Construct(_Environment.Object), Times.Never);
            _PipelineBuilder.RegisterMiddlewareBuilder(callback.Callback, 0);

            _PipelineBuilder.CreatePipeline(_Environment.Object);

            _Pipeline.Verify(r => r.Construct(_Environment.Object), Times.Once);
        }

        [TestMethod]
        public void CreatePipeline_Returns_Pipeline()
        {
            var result = _PipelineBuilder.CreatePipeline(_Environment.Object);

            Assert.AreSame(_Pipeline.Object, result);
        }

        [TestMethod]
        public void CreatePipeline_Does_Not_Call_Callbacks_That_Have_Been_Removed()
        {
            var callback = new MockPipelineCallback();
            var handle = _PipelineBuilder.RegisterMiddlewareBuilder(callback.Callback, 0);
            _PipelineBuilder.DeregisterMiddlewareBuilder(handle);

            _PipelineBuilder.CreatePipeline(_Environment.Object);

            Assert.AreEqual(0, callback.CallCount);
        }

        [TestMethod]
        public void CreatePipeline_Calls_Registered_Callbacks_In_Correct_Order()
        {
            var callback_1 = new MockPipelineCallback();
            var callback_2 = new MockPipelineCallback();
            var callback_3 = new MockPipelineCallback();

            _PipelineBuilder.RegisterMiddlewareBuilder(callback_2.Callback, 0);
            _PipelineBuilder.RegisterMiddlewareBuilder(callback_3.Callback,  1);
            _PipelineBuilder.RegisterMiddlewareBuilder(callback_1.Callback,  -1);

            callback_1.Action = r => {
                Assert.AreEqual(0, callback_2.CallCount, "2nd callback called before 1st");
                Assert.AreEqual(0, callback_3.CallCount, "3rd callback called before 1st");
            };
            callback_2.Action = r => {
                Assert.AreEqual(1, callback_1.CallCount, "1st callback not called before 2nd");
                Assert.AreEqual(0, callback_3.CallCount, "3rd callback called before 2nd");
            };
            callback_3.Action = r => {
                Assert.AreEqual(1, callback_1.CallCount, "1st callback not called before 3rd");
                Assert.AreEqual(1, callback_2.CallCount, "2nd callback not called before 3rd");
            };

            _PipelineBuilder.CreatePipeline(_Environment.Object);

            Assert.AreEqual(1, callback_1.CallCount);
            Assert.AreEqual(1, callback_2.CallCount);
            Assert.AreEqual(1, callback_3.CallCount);
        }
    }
}
