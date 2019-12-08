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
using System.Linq;
using System.Reflection;
using AWhewell.Owin.Interface.WebApi;
using AWhewell.Owin.Utility;
using AWhewell.Owin.Utility.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin.WebApi
{
    [TestClass]
    public class MethodParameterTests
    {
        private void ExampleMethod(
            string stringParameter,

            [UseParser(typeof(ByteArray_Mime64_Parser))]
            byte[] byteArrayWithExpect,

            [ExpectArray]
            byte[] byteArrayPassedAsBytes,

            int[] intArray,

            int optionalInt = 123
        ) { ; }

        private ParameterInfo _ExampleMethod_string;
        private ParameterInfo _ExampleMethod_optionalInt;
        private ParameterInfo _ExampleMethod_intArray;
        private ParameterInfo _ExampleMethod_byteArrayWithExpect;
        private ParameterInfo _ExampleMethod_byteArrayPassedAsBytes;

        /// <summary>
        /// A utility function that returns method parameters for a method.
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="defaultResolver"></param>
        /// <returns></returns>
        public static MethodParameter[] CreateMethodParameters(MethodInfo methodInfo, TypeParserResolver defaultResolver)
        {
            return methodInfo
                .GetParameters()
                .Select(r => new MethodParameter(r, defaultResolver))
                .ToArray();
        }

        [TestInitialize]
        public void TestInitialise()
        {
            var example1Method = GetType().GetMethod(nameof(ExampleMethod), BindingFlags.NonPublic | BindingFlags.Instance);
            _ExampleMethod_string =                 example1Method.GetParameters().Single(r => r.Name == "stringParameter");
            _ExampleMethod_optionalInt =            example1Method.GetParameters().Single(r => r.Name == "optionalInt");
            _ExampleMethod_intArray =               example1Method.GetParameters().Single(r => r.Name == "intArray");
            _ExampleMethod_byteArrayWithExpect =    example1Method.GetParameters().Single(r => r.Name == "byteArrayWithExpect");
            _ExampleMethod_byteArrayPassedAsBytes = example1Method.GetParameters().Single(r => r.Name == "byteArrayPassedAsBytes");
        }

        [TestMethod]
        public void MethodParameter_Ctor_Fills_Mandatory_Parameter_Details_Correctly()
        {
            var param = new MethodParameter(_ExampleMethod_string, null);
            Assert.AreEqual("stringParameter", param.Name);
            Assert.AreEqual(PathPart.Normalise(param.Name), param.NormalisedName);
            Assert.AreEqual(typeof(string), param.ParameterType);
            Assert.AreEqual(false, param.IsOptional);
            Assert.AreEqual(System.DBNull.Value, param.DefaultValue);
            Assert.AreEqual(null, param.TypeParserResolver);
            Assert.IsNull(param.ElementType);
            Assert.IsFalse(param.IsArray);
        }

        [TestMethod]
        public void MethodParameter_Ctor_Fills_Optional_Parameter_Details_Correctly()
        {
            var param = new MethodParameter(_ExampleMethod_optionalInt, null);
            Assert.AreEqual("optionalInt", param.Name);
            Assert.AreEqual(PathPart.Normalise(param.Name), param.NormalisedName);
            Assert.AreEqual(typeof(int), param.ParameterType);
            Assert.AreEqual(true, param.IsOptional);
            Assert.AreEqual(123, param.DefaultValue);
            Assert.AreEqual(null, param.TypeParserResolver);
            Assert.IsNull(param.ElementType);
            Assert.IsFalse(param.IsArray);
        }

        [TestMethod]
        public void MethodParameter_Ctor_Fills_TypeParserResolver_Property_When_No_Default_Passed()
        {
            var param = new MethodParameter(_ExampleMethod_byteArrayWithExpect, null);
            Assert.AreEqual("byteArrayWithExpect", param.Name);
            Assert.AreEqual(PathPart.Normalise(param.Name), param.NormalisedName);
            Assert.AreEqual(typeof(byte[]), param.ParameterType);
            Assert.AreEqual(false, param.IsOptional);
            Assert.AreEqual(System.DBNull.Value, param.DefaultValue);
            Assert.AreEqual(typeof(byte), param.ElementType);
            Assert.IsTrue(param.IsArray);

            Assert.IsNotNull(param.TypeParserResolver);
            var parsers = param.TypeParserResolver.GetParsers();
            Assert.AreEqual(1, parsers.Length);
            Assert.IsInstanceOfType(parsers[0], typeof(ByteArray_Mime64_Parser));
        }

        [TestMethod]
        public void MethodParameter_Ctor_Fills_TypeParserResolver_Property_When_Default_Passed_And_Not_Overridden()
        {
            var resolver = new TypeParserResolver(new DateTime_Local_Parser());
            var param = new MethodParameter(_ExampleMethod_optionalInt, resolver);

            Assert.IsNotNull(param.TypeParserResolver);
            var parsers = param.TypeParserResolver.GetParsers();
            Assert.AreEqual(1, parsers.Length);
            Assert.IsInstanceOfType(parsers[0], typeof(DateTime_Local_Parser));
        }

        [TestMethod]
        public void MethodParameter_Ctor_Fills_TypeParserResolver_Property_When_Default_Passed_And_Overridden()
        {
            var resolver = new TypeParserResolver(new DateTime_Local_Parser(), new ByteArray_HexString_Parser());
            var param = new MethodParameter(_ExampleMethod_byteArrayWithExpect, resolver);

            Assert.IsNotNull(param.TypeParserResolver);
            var parsers = param.TypeParserResolver.GetParsers();
            Assert.AreEqual(2, parsers.Length);
            Assert.IsTrue(parsers.Any(r => r.GetType() == typeof(DateTime_Local_Parser)));
            Assert.IsTrue(parsers.Any(r => r.GetType() == typeof(ByteArray_Mime64_Parser)));
        }

        [TestMethod]
        public void MethodParameter_Ctor_Clears_IsArrayPassedAsSingleValue_For_Non_Array()
        {
            var param = new MethodParameter(_ExampleMethod_string, null);

            Assert.IsFalse(param.IsArray);
            Assert.IsFalse(param.IsArrayPassedAsSingleValue);
        }

        [TestMethod]
        public void MethodParameter_Ctor_Sets_IsArrayPassedAsSingleValue_For_Byte_Array()
        {
            var param = new MethodParameter(_ExampleMethod_byteArrayWithExpect, null);

            Assert.IsTrue(param.IsArray);
            Assert.IsTrue(param.IsArrayPassedAsSingleValue);
        }

        [TestMethod]
        public void MethodParameter_Ctor_Clears_IsArrayPassedAsSingleValue_For_Other_Array_Types()
        {
            var param = new MethodParameter(_ExampleMethod_intArray, null);

            Assert.IsTrue(param.IsArray);
            Assert.IsFalse(param.IsArrayPassedAsSingleValue);
        }

        [TestMethod]
        public void MethodParameter_Ctor_Clears_IsArrayPassedAsSingleValue_If_ExpectArrayAttribute_Used()
        {
            var param = new MethodParameter(_ExampleMethod_byteArrayPassedAsBytes, null);

            Assert.IsTrue(param.IsArray);
            Assert.IsFalse(param.IsArrayPassedAsSingleValue);
        }
    }
}
