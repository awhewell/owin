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
using System.Reflection;
using System.Text;
using AWhewell.Owin.Interface.WebApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin.WebApi
{
    [TestClass]
    public class MethodParameterTests
    {
        private void ExampleMethod(string stringParameter, [Expect(ExpectFormat.HexString)] byte[] byteArrayWithExpect, int optionalInt = 123) { ; }

        private ParameterInfo _ExampleMethod_stringParameter;
        private ParameterInfo _ExampleMethod_optionalInt;
        private ParameterInfo _ExampleMethod_byteArrayWithExpect;

        /// <summary>
        /// A utility function that returns method parameters for a method.
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public static MethodParameter[] CreateMethodParameters(MethodInfo methodInfo) => methodInfo.GetParameters().Select(r => new MethodParameter(r)).ToArray();

        [TestInitialize]
        public void TestInitialise()
        {
            var example1Method = GetType().GetMethod(nameof(ExampleMethod), BindingFlags.NonPublic | BindingFlags.Instance);
            _ExampleMethod_stringParameter =     example1Method.GetParameters().Single(r => r.Name == "stringParameter");
            _ExampleMethod_optionalInt =         example1Method.GetParameters().Single(r => r.Name == "optionalInt");
            _ExampleMethod_byteArrayWithExpect = example1Method.GetParameters().Single(r => r.Name == "byteArrayWithExpect");
        }

        [TestMethod]
        public void MethodParameter_Ctor_Fills_Mandatory_Parameter_Details_Correctly()
        {
            var param = new MethodParameter(_ExampleMethod_stringParameter);
            Assert.AreEqual("stringParameter", param.Name);
            Assert.AreEqual(PathPart.Normalise(param.Name), param.NormalisedName);
            Assert.AreEqual(typeof(string), param.ParameterType);
            Assert.AreEqual(false, param.IsOptional);
            Assert.AreEqual(System.DBNull.Value, param.DefaultValue);
            Assert.IsNull(param.Expect);
        }

        [TestMethod]
        public void MethodParameter_Ctor_Fills_Optional_Parameter_Details_Correctly()
        {
            var param = new MethodParameter(_ExampleMethod_optionalInt);
            Assert.AreEqual("optionalInt", param.Name);
            Assert.AreEqual(PathPart.Normalise(param.Name), param.NormalisedName);
            Assert.AreEqual(typeof(int), param.ParameterType);
            Assert.AreEqual(true, param.IsOptional);
            Assert.AreEqual(123, param.DefaultValue);
            Assert.IsNull(param.Expect);
        }

        [TestMethod]
        public void MethodParameter_Ctor_Fills_Expect_Property_Correctly()
        {
            var param = new MethodParameter(_ExampleMethod_byteArrayWithExpect);
            Assert.AreEqual("byteArrayWithExpect", param.Name);
            Assert.AreEqual(PathPart.Normalise(param.Name), param.NormalisedName);
            Assert.AreEqual(typeof(byte[]), param.ParameterType);
            Assert.AreEqual(false, param.IsOptional);
            Assert.AreEqual(System.DBNull.Value, param.DefaultValue);
            Assert.AreEqual(ExpectFormat.HexString, param.Expect.ExpectFormat);
        }
    }
}
