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
using System.Threading.Tasks;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AWhewell.Owin.Interface;

namespace Test.AWhewell.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    [TestClass]
    public class PipelineBuilderEnvironment_Tests
    {
        private IPipelineBuilderEnvironment _Environment;

        [TestInitialize]
        public void TestInitialise()
        {
            _Environment = Factory.Resolve<IPipelineBuilderEnvironment>();
        }

        private AppFunc AppFuncChainLink_1(AppFunc next)  => throw new NotImplementedException();

        private AppFunc AppFuncChainLink_2(AppFunc next)  => throw new NotImplementedException();

        [TestMethod]
        public void Properties_Defaults_To_Known_State()
        {
            Assert.AreEqual(0, _Environment.Properties.Count);
            Assert.IsTrue(_Environment.PipelineLogsExceptions);
            Assert.IsTrue(_Environment.PipelineSwallowsExceptions);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UseMiddlewareBuilder_Throws_If_Passed_Null()
        {
            _Environment.UseMiddlewareBuilder(null);
        }

        [TestMethod]
        public void UseMiddlewareBuilder_Adds_AppFunc_To_Middlewares_Collection()
        {
            var link = (Func<AppFunc, AppFunc>)AppFuncChainLink_1;

            _Environment.UseMiddlewareBuilder(link);

            var chain = _Environment.MiddlewareBuilders.ToArray();
            Assert.AreEqual(1, chain.Length);
            Assert.AreSame(link, chain[0]);
        }

        [TestMethod]
        public void UseMiddlewareBuilder_Has_Cumulative_Effect()
        {
            var link_1 = (Func<AppFunc, AppFunc>)AppFuncChainLink_1;
            var link_2 = (Func<AppFunc, AppFunc>)AppFuncChainLink_2;

            _Environment.UseMiddlewareBuilder(link_1);
            _Environment.UseMiddlewareBuilder(link_2);

            var chain = _Environment.MiddlewareBuilders.ToArray();
            Assert.AreEqual(2, chain.Length);
            Assert.AreSame(link_1, chain[0]);
            Assert.AreSame(link_2, chain[1]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UseStreamManipulatorBuilder_Throws_If_Passed_Null()
        {
            _Environment.UseStreamManipulatorBuilder(null);
        }

        [TestMethod]
        public void UseStreamManipulatorBuilder_Adds_AppFunc_To_Manipulators_Collection()
        {
            var link = (Func<AppFunc, AppFunc>)AppFuncChainLink_1;

            _Environment.UseStreamManipulatorBuilder(link);

            var chain = _Environment.StreamManipulatorBuilders.ToArray();
            Assert.AreEqual(1, chain.Length);
            Assert.AreSame(link, chain[0]);
        }

        [TestMethod]
        public void UseStreamManipulatorBuilder_Has_Cumulative_Effect()
        {
            var link_1 = (Func<AppFunc, AppFunc>)AppFuncChainLink_1;
            var link_2 = (Func<AppFunc, AppFunc>)AppFuncChainLink_2;

            _Environment.UseStreamManipulatorBuilder(link_1);
            _Environment.UseStreamManipulatorBuilder(link_2);

            var chain = _Environment.StreamManipulatorBuilders.ToArray();
            Assert.AreEqual(2, chain.Length);
            Assert.AreSame(link_1, chain[0]);
            Assert.AreSame(link_2, chain[1]);
        }

        [TestMethod]
        public void MiddlewareBuilders_And_StreamManipulatorBuilders_Are_Isolated_From_Each_Other()
        {
            var link_1 = (Func<AppFunc, AppFunc>)AppFuncChainLink_1;
            var link_2 = (Func<AppFunc, AppFunc>)AppFuncChainLink_2;

            _Environment.UseMiddlewareBuilder(link_1);
            _Environment.UseStreamManipulatorBuilder(link_2);

            var middlewareChain = _Environment.MiddlewareBuilders.ToArray();
            Assert.AreEqual(1, middlewareChain.Length);
            Assert.AreSame(link_1, middlewareChain[0]);

            var manipulatorsChain = _Environment.StreamManipulatorBuilders.ToArray();
            Assert.AreEqual(1, manipulatorsChain.Length);
            Assert.AreSame(link_2, manipulatorsChain[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UseExceptionLogger_Throws_If_Passed_Null()
        {
            _Environment.UseExceptionLogger(null);
        }

        [TestMethod]
        public void UseExceptionLogger_Records_Exception_Logger()
        {
            Assert.AreEqual(0, _Environment.ExceptionLoggers.Count);

            var logger1 = new MockExceptionLogger();
            _Environment.UseExceptionLogger(logger1);
            Assert.AreEqual(1, _Environment.ExceptionLoggers.Count);
            Assert.AreSame(logger1, _Environment.ExceptionLoggers[0]);

            var logger2 = new MockExceptionLogger();
            _Environment.UseExceptionLogger(logger2);
            Assert.AreEqual(2, _Environment.ExceptionLoggers.Count);
            Assert.IsTrue(_Environment.ExceptionLoggers.Contains(logger2));
        }
    }
}
