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
using Owin.Interface;

namespace Test.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    [TestClass]
    public class PipelineBuilderEnvironmentTests
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
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UseMiddleware_Throws_If_Passed_Null()
        {
            _Environment.UseMiddleware(null);
        }

        [TestMethod]
        public void UseMiddleware_Adds_AppFunc_To_Middlewares_Collection()
        {
            var link = (Func<AppFunc, AppFunc>)AppFuncChainLink_1;

            _Environment.UseMiddleware(link);

            var chain = _Environment.MiddlewareChain.ToArray();
            Assert.AreEqual(1, chain.Length);
            Assert.AreSame(link, chain[0]);
        }

        [TestMethod]
        public void UseMiddleware_Has_Cumulative_Effect()
        {
            var link_1 = (Func<AppFunc, AppFunc>)AppFuncChainLink_1;
            var link_2 = (Func<AppFunc, AppFunc>)AppFuncChainLink_2;

            _Environment.UseMiddleware(link_1);
            _Environment.UseMiddleware(link_2);

            var chain = _Environment.MiddlewareChain.ToArray();
            Assert.AreEqual(2, chain.Length);
            Assert.AreSame(link_1, chain[0]);
            Assert.AreSame(link_2, chain[1]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UseStreamManipulator_Throws_If_Passed_Null()
        {
            _Environment.UseStreamManipulator(null);
        }

        [TestMethod]
        public void UseStreamManipulator_Adds_AppFunc_To_Manipulators_Collection()
        {
            var link = (Func<AppFunc, AppFunc>)AppFuncChainLink_1;

            _Environment.UseStreamManipulator(link);

            var chain = _Environment.StreamManipulatorChain.ToArray();
            Assert.AreEqual(1, chain.Length);
            Assert.AreSame(link, chain[0]);
        }

        [TestMethod]
        public void UseStreamManipulator_Has_Cumulative_Effect()
        {
            var link_1 = (Func<AppFunc, AppFunc>)AppFuncChainLink_1;
            var link_2 = (Func<AppFunc, AppFunc>)AppFuncChainLink_2;

            _Environment.UseStreamManipulator(link_1);
            _Environment.UseStreamManipulator(link_2);

            var chain = _Environment.StreamManipulatorChain.ToArray();
            Assert.AreEqual(2, chain.Length);
            Assert.AreSame(link_1, chain[0]);
            Assert.AreSame(link_2, chain[1]);
        }

        [TestMethod]
        public void Middleware_And_Manipulators_Are_Isolated_From_Each_Other()
        {
            var link_1 = (Func<AppFunc, AppFunc>)AppFuncChainLink_1;
            var link_2 = (Func<AppFunc, AppFunc>)AppFuncChainLink_2;

            _Environment.UseMiddleware(link_1);
            _Environment.UseStreamManipulator(link_2);

            var middlewareChain = _Environment.MiddlewareChain.ToArray();
            Assert.AreEqual(1, middlewareChain.Length);
            Assert.AreSame(link_1, middlewareChain[0]);

            var manipulatorsChain = _Environment.StreamManipulatorChain.ToArray();
            Assert.AreEqual(1, manipulatorsChain.Length);
            Assert.AreSame(link_2, manipulatorsChain[0]);
        }
    }
}
