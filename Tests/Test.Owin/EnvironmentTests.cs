// Copyright © 2019 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Linq;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Owin.Interface;

namespace Test.Owin
{
    [TestClass]
    public class EnvironmentTests
    {
        private IEnvironment _Environment;

        [TestInitialize]
        public void TestInitialise()
        {
            _Environment = Factory.Resolve<IEnvironment>();
        }

        [TestMethod]
        public void Environment_Index_Operator_Returns_Null_For_Missing_Keys()
        {
            Assert.IsNull(_Environment["missing"]);
        }

        [TestMethod]
        public void Environment_Index_Operator_Supports_Assignment_For_New_Keys()
        {
            _Environment["a.b"] = 12;

            Assert.AreEqual(12, (int)_Environment["a.b"]);
        }

        [TestMethod]
        public void Environment_Index_Operator_Supports_Assignment_For_Existing_Keys()
        {
            _Environment["a.b"] = 12;
            _Environment["a.b"] = 24;

            Assert.AreEqual(24, (int)_Environment["a.b"]);
        }

        [TestMethod]
        public void Environment_Is_Case_Sensitive_When_Reading()
        {
            _Environment.Add("A", 1);

            Assert.IsNull(_Environment["a"]);
        }

        [TestMethod]
        public void Environment_Is_Case_Sensitive_When_Writing()
        {
            _Environment.Add("A", 1);
            _Environment.Add("a", 2);

            Assert.AreEqual(1, (int)_Environment["A"]);
            Assert.AreEqual(2, (int)_Environment["a"]);
        }
    }
}
