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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AWhewell.Owin.Interface.WebApi;
using System.Linq;

namespace Test.AWhewell.Owin.WebApi
{
    [TestClass]
    public class PathPartTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Create_Throws_If_MethodParameters_Is_Null()
        {
            PathPart.Create("whatever", null);
        }

        [TestMethod]
        [DataRow(null,          typeof(PathPartText),       "",             "")]
        [DataRow("",            typeof(PathPartText),       "",             "")]
        [DataRow("api",         typeof(PathPartText),       "api",          "api")]
        [DataRow("ID",          typeof(PathPartText),       "ID",           "id")]
        [DataRow("a",           typeof(PathPartText),       "a",            "a")]
        [DataRow("{a}",         typeof(PathPartParameter),  "{a}",          "a")]
        [DataRow("{A}",         typeof(PathPartParameter),  "{A}",          "a")]
        [DataRow(" {a}",        typeof(PathPartText),       " {a}",         " {a}")]
        [DataRow("{a} ",        typeof(PathPartText),       "{a} ",         "{a} ")]
        [DataRow("{ a}",        typeof(PathPartText),       "{ a}",         "{ a}")]
        [DataRow("{a }",        typeof(PathPartText),       "{a }",         "{a }")]
        [DataRow("{ a }",       typeof(PathPartText),       "{ a }",        "{ a }")]
        [DataRow("{1}",         typeof(PathPartText),       "{1}",          "{1}")]
        [DataRow("{a1}",        typeof(PathPartParameter),  "{a1}",         "a1")]
        [DataRow("{a-1}",       typeof(PathPartText),       "{a-1}",        "{a-1}")]
        [DataRow("{a 1}",       typeof(PathPartText),       "{a 1}",        "{a 1}")]
        [DataRow("{_}",         typeof(PathPartParameter),  "{_}",          "_")]
        [DataRow("{_abc123}",   typeof(PathPartParameter),  "{_abc123}",    "_abc123")]
        [DataRow("{ABC_123}",   typeof(PathPartParameter),  "{ABC_123}",    "abc_123")]
        [DataRow("{no}",        typeof(PathPartText),       "{no}",         "{no}")]        // The method has no parameter called "no", therefore it must be a text parameter

        public void Create_Fills_Properties(string inputText, Type expectedType, string part, string normalisedPart)
        {
            var methodInfo = GetType().GetMethod(nameof(ValidExampleParameterNames));
            var methodParameters = MethodParameterTests.CreateMethodParameters(methodInfo);

            var actual = PathPart.Create(inputText, methodParameters);

            Assert.IsInstanceOfType(actual, expectedType);      // they got the parameters the wrong way round... all other calls are expected then actual
            Assert.AreEqual(part, actual.Part);
            Assert.AreEqual(normalisedPart, actual.NormalisedPart);

            if(expectedType == typeof(PathPartParameter)) {
                var expectedMethodParameter = methodParameters.First(r => r.NormalisedName == normalisedPart);
                var actualPathPartParameter = (PathPartParameter)actual;
                Assert.AreSame(expectedMethodParameter, actualPathPartParameter.MethodParameter);
            }
        }

        public void ValidExampleParameterNames(string a, string a1, string _, string _abc123, string ABC_123)
        {
        }

        [TestMethod]
        [DataRow(null,  "")]
        [DataRow("",    "")]
        [DataRow("api", "api")]
        [DataRow("API", "api")]
        public void Normalise_Returns_Normalised_String(string pathPart, string expected)
        {
            var actual = PathPart.Normalise(pathPart);

            if(expected == null) {
                Assert.IsNull(actual);
            } else {
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        [DataRow("api",     "api",      true)]
        [DataRow("API",     "api",      true)]
        [DataRow("api",     "not-api",  false)]
        [DataRow("api",     "API",      false)]      // Incoming requests need to be normalised first
        [DataRow("API",     "API",      false)]      // Incoming requests need to be normalised first
        [DataRow(null,      null,       false)]      // Incoming requests need to be normalised first
        [DataRow(null,      "",         true)]
        [DataRow("{id}",    "1",        true)]
        [DataRow("{id}",    "anything", true)]
        [DataRow("{id}",    "",         true)]
        [DataRow("{id}",    null,       false)]      // Normalisation coalesces nulls, they do not match anything
        public void MatchesRequestPathPart_Returns_Correct_Response(string ctorPathPart, string matchPathPart, bool expected)
        {
            var methodInfo = GetType().GetMethod(nameof(MatchesRequestPathPartExampleParameters));
            var methodParameters = MethodParameterTests.CreateMethodParameters(methodInfo);
            var pathPart = PathPart.Create(ctorPathPart, methodParameters);

            var actual = pathPart.MatchesRequestPathPart(matchPathPart);

            Assert.AreEqual(expected, actual);
        }

        public void MatchesRequestPathPartExampleParameters(string api, string id)
        {
        }
    }
}
