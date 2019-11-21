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
using System.Text;
using AWhewell.Owin.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin.Utility
{
    [TestClass]
    public class OwinPathTests
    {
        [TestMethod]
        [DataRow(new string[] { },              null)]
        [DataRow(new string[] { },              "")]
        [DataRow(new string[] { },              "/")]
        [DataRow(new string[] { "part" },       "/part")]
        [DataRow(new string[] { "a", "b" },     "/a/b")]
        [DataRow(new string[] { "a", "", "b" }, "/a//b")]
        [DataRow(new string[] { "", "a" },      "//a")]
        [DataRow(new string[] { "a", "" },      "/a/")]
        // Path parts should start with a slash. However, if they don't then it would be nice if it could still cope
        [DataRow(new string[] { "part" },       "part")]
        [DataRow(new string[] { "a", "" },      "a/")]
        [DataRow(new string[] { "a", "b" },     "a/b")]
        public void RequestPathParts_Splits_RequestPath_Into_Chunks(string[] expected, string requestPath)
        {
            var actual = OwinPath.RequestPathParts(requestPath);

            if(expected == null) {
                Assert.IsNull(actual);
            } else {
                Assert.IsTrue(expected.SequenceEqual(actual));
            }
        }

        [TestMethod]
        [DataRow(new string[] { },              null)]
        [DataRow(new string[] { },              "")]
        [DataRow(new string[] { },              "/")]
        [DataRow(new string[] { "part" },       "/part")]
        [DataRow(new string[] { "a", "b" },     "/a/b")]
        [DataRow(new string[] { "a", "", "b" }, "/a//b")]
        [DataRow(new string[] { "", "a" },      "//a")]
        [DataRow(new string[] { "a", "" },      "/a/")]
        // Path parts should start with a slash. However, if they don't then it would be nice if it could still cope
        [DataRow(new string[] { "part" },       "part")]
        [DataRow(new string[] { "a", "" },      "a/")]
        [DataRow(new string[] { "a", "b" },     "a/b")]
        public void RequestPathParts_Extracts_RequestPath_From_Environment_And_Splits_Into_Chunks(string[] expected, string requestPath)
        {
            foreach(var useCache in new bool[] { true, false }) {
                var environment = new MockOwinEnvironment();
                if(requestPath != null) {
                    environment.RequestPath = requestPath;
                }

                var actual = OwinPath.RequestPathParts(environment.Environment, useCache);

                if(expected == null) {
                    Assert.IsNull(actual);
                } else {
                    Assert.IsTrue(expected.SequenceEqual(actual));
                }
            }
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void RequestPathParts_Can_Cache_Result_In_Environment(bool createCacheEntry)
        {
            var environment = new MockOwinEnvironment();
            environment.RequestPath = "/a";

            var actual = OwinPath.RequestPathParts(environment.Environment, createAndUseCachedResult: createCacheEntry);
            Assert.AreEqual(1, actual.Length);
            Assert.AreEqual("a", actual[0]);

            var cacheEntry = environment.Environment[CustomEnvironmentKey.OwinPathRequestPathParts] as IList<string>;
            if(createCacheEntry) {
                Assert.IsTrue(actual.SequenceEqual(cacheEntry));
            } else {
                Assert.IsNull(cacheEntry);
            }
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void RequestPathParts_Uses_Environment_Cache_Result_If_Present_And_Requested(bool useCacheEntry)
        {
            var environment = new MockOwinEnvironment();
            environment.RequestPath = "/a";
            environment.Environment[CustomEnvironmentKey.OwinPathRequestPathParts] = new string[] { "b" };

            var actual = OwinPath.RequestPathParts(environment.Environment, createAndUseCachedResult: useCacheEntry);

            if(!useCacheEntry) {
                Assert.IsTrue(new string[] { "a" }.SequenceEqual(actual));
            } else {
                Assert.IsTrue(new string[] { "b" }.SequenceEqual(actual));
            }
        }
    }
}
