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
using System.Threading.Tasks;
using AWhewell.Owin.Interface;
using AWhewell.Owin.Interface.Host.Ram;
using AWhewell.Owin.Utility;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Test.AWhewell.Owin.Host.Ram
{
    [TestClass]
    public class HostRam_Tests : CommonHostTests
    {
        private IClassFactory                       _Snapshot;
        private IHostRam                            _Host;
        private Mock<IPipelineBuilder>              _PipelineBuilder;
        private Mock<IPipelineBuilderEnvironment>   _PipelineBuilderEnvironment;
        private Mock<IPipeline>                     _Pipeline;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _PipelineBuilder = MockHelper.FactoryImplementation<IPipelineBuilder>();
            _PipelineBuilderEnvironment = MockHelper.FactoryImplementation<IPipelineBuilderEnvironment>();
            _Pipeline = MockHelper.FactoryImplementation<IPipeline>();
            _PipelineBuilder.Setup(r => r.CreatePipeline(_PipelineBuilderEnvironment.Object)).Returns(() => _Pipeline.Object);

            _Host = Factory.Resolve<IHostRam>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if(_Host != null) {
                _Host.Dispose();
            }
            Factory.RestoreSnapshot(_Snapshot);
        }

        [TestMethod]
        public void Initialise_Creates_Pipeline()
        {
            _Host.Initialise(_PipelineBuilder.Object, _PipelineBuilderEnvironment.Object);

            _PipelineBuilder.Verify(r => r.CreatePipeline(_PipelineBuilderEnvironment.Object), Times.Once);
        }

        [TestMethod]
        public void Initialise_Sets_Environment_Properties()
        {
            _Host.Initialise(_PipelineBuilder.Object, _PipelineBuilderEnvironment.Object);

            var properties = _PipelineBuilderEnvironment.Object.Properties;
            Assert.AreEqual(Constants.Version,          properties[ApplicationStartupKey.Version]);
            Assert.AreEqual("AWhewell.Owin.Host.Ram",   properties[ApplicationStartupKey.HostType]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Initialise_Throws_If_Called_Twice() => All_Hosts_Initialise_Throws_If_Called_Twice(_Host);

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Initialise_Throws_If_Builder_Is_Null() => All_Hosts_Initialise_Throws_If_Builder_Is_Null(_Host);

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Initialise_Throws_If_Environment_Is_Null() => All_Hosts_Initialise_Throws_If_Environment_Is_Null(_Host);

        [TestMethod]
        public void Root_Defaults_To_Slash() => All_Hosts_Root_Defaults_To_Slash(_Host);

        [TestMethod]
        public void Root_Coalesces_Null_To_Slash() => All_Hosts_Root_Coalesces_Null_To_Slash(_Host);

        [TestMethod]
        public void Root_Replaces_Empty_String_With_Slash() => All_Hosts_Root_Replaces_Empty_String_With_Slash(_Host);

        [TestMethod]
        public void Root_Always_Prefixed_With_Slash() => All_Hosts_Root_Always_Prefixed_With_Slash(_Host);

        [TestMethod]
        public void Root_Does_Not_Prefix_With_Slash_When_Already_Starts_With_Slash() => All_Hosts_Root_Does_Not_Prefix_With_Slash_When_Already_Starts_With_Slash(_Host);

        [TestMethod]
        public void Root_Strips_Trailing_Slash() => All_Hosts_Root_Strips_Trailing_Slash(_Host);

        [TestMethod]
        public void Port_Defaults_To_80() => All_Hosts_Port_Defaults_To_80(_Host);

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Start_Throws_If_Not_Initialised() => All_Hosts_Start_Throws_If_Not_Initialised(_Host);

        [TestMethod]
        public void Start_Sets_IsListening()
        {
            _Host.Initialise(_PipelineBuilder.Object, _PipelineBuilderEnvironment.Object);
            _Host.Start();

            Assert.IsTrue(_Host.IsListening);
        }

        [TestMethod]
        public void Start_Silently_Ignores_Double_Calls()
        {
            _Host.Initialise(_PipelineBuilder.Object, _PipelineBuilderEnvironment.Object);
            _Host.Start();
            _Host.Start();

            Assert.IsTrue(_Host.IsListening);
        }

        [TestMethod]
        public void Stop_Clears_IsListening()
        {
            _Host.Initialise(_PipelineBuilder.Object, _PipelineBuilderEnvironment.Object);
            _Host.Start();
            _Host.Stop();

            Assert.IsFalse(_Host.IsListening);
        }

        [TestMethod]
        public void ProcessRequest_Sends_Environment_Down_Pipeline()
        {
            _Host.Initialise(_PipelineBuilder.Object, _PipelineBuilderEnvironment.Object);
            _Host.Start();

            var env = new OwinDictionary<object>();
            _Host.ProcessRequest(env);

            _Pipeline.Verify(r => r.ProcessRequest(env), Times.Once());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProcessRequest_Throws_If_Environment_Is_Null()
        {
            _Host.Initialise(_PipelineBuilder.Object, _PipelineBuilderEnvironment.Object);
            _Host.Start();

            _Host.ProcessRequest(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ProcessRequest_Throws_If_Not_Initialised()
        {
            var env = new OwinDictionary<object>();
            _Host.ProcessRequest(env);
        }

        [TestMethod]
        public void ProcessRequest_Does_Nothing_If_Host_Not_Listening()
        {
            _Host.Initialise(_PipelineBuilder.Object, _PipelineBuilderEnvironment.Object);

            var env = new OwinDictionary<object>();
            _Host.ProcessRequest(env);

            _Pipeline.Verify(r => r.ProcessRequest(env), Times.Never());
        }
    }
}
