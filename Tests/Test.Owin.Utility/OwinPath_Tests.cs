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
    public class OwinPath_Tests
    {
        [TestMethod]
        [DataRow("http",    "1.2.3.4",      "/VirtualRadar",    "/poobah.html", "name=value",               "http://1.2.3.4/VirtualRadar/poobah.html?name=value")]
        [DataRow("http",    "1.2.3.4",      "",                 "/poobah.html", "name=value",               "http://1.2.3.4/poobah.html?name=value")]
        [DataRow("http",    "1.2.3.4",      "",                 "/",            "name=value",               "http://1.2.3.4/?name=value")]
        [DataRow("http",    "1.2.3.4",      "",                 "/",            "",                         "http://1.2.3.4/")]
        [DataRow("http",    "1.2.3.4",      "",                 "/",            null,                       "http://1.2.3.4/")]
        [DataRow("http",    "1.2.3.4",      "/Root",            "",             null,                       "http://1.2.3.4/Root")]
        [DataRow("http",    "1.2.3.4",      "/Root",            "/",            null,                       "http://1.2.3.4/Root/")]
        [DataRow(null,      "1.2.3.4",      "/VirtualRadar",    "/poobah.html", "name=value",               "http://1.2.3.4/VirtualRadar/poobah.html?name=value")]
        [DataRow("",        "1.2.3.4",      "/VirtualRadar",    "/poobah.html", "name=value",               "http://1.2.3.4/VirtualRadar/poobah.html?name=value")]
        [DataRow("https",   "1.2.3.4",      "/VirtualRadar",    "/poobah.html", "name=value",               "https://1.2.3.4/VirtualRadar/poobah.html?name=value")]
        [DataRow("http",    null,           "/VirtualRadar",    "/poobah.html", "name=value",               "http://127.0.0.1/VirtualRadar/poobah.html?name=value")]
        [DataRow("http",    "",             "/VirtualRadar",    "/poobah.html", "name=value",               "http://127.0.0.1/VirtualRadar/poobah.html?name=value")]
        [DataRow("http",    "1.2.3.4:80",   "/VirtualRadar",    "/poobah.html", "name=value",               "http://1.2.3.4:80/VirtualRadar/poobah.html?name=value")]
        [DataRow("http",    "1.2.3.4",      "/VirtualRadar",    "/poobah.html", "name=percent%2Dencoded",   "http://1.2.3.4/VirtualRadar/poobah.html?name=percent%2Dencoded")]
        [DataRow("http",    "1.2.3.4",      "/VirtualRadar",    "/poobah.html", "?name",                    "http://1.2.3.4/VirtualRadar/poobah.html??name")]   // Invalid, query string must not start with ?
        [DataRow("http",    "1.2.3.4",      "VirtualRadar",     "poobah.html",  "name",                     "http://1.2.3.4VirtualRadarpoobah.html?name")]      // Invalid, pathBase must start with a slash or be "", likewise path must start with a slash
        [DataRow("http",    "1.2.3.4",      "/VirtualRadar",    "/poobah.html", null,                       "http://1.2.3.4/VirtualRadar/poobah.html")]
        [DataRow("http",    "1.2.3.4",      "/VirtualRadar",    "/poobah.html", "",                         "http://1.2.3.4/VirtualRadar/poobah.html")]
        public void ConstructUrl_Returns_Correct_Values(string scheme, string host, string pathBase, string path, string queryString, string expected)
        {
            var url = OwinPath.ConstructUrl(scheme, host, pathBase, path, queryString);

            Assert.AreEqual(expected, url);
        }

        [TestMethod]
        [DataRow("/VirtualRadar",    "/poobah.html", "name=value",               "/VirtualRadar/poobah.html?name=value")]
        [DataRow("",                 "/poobah.html", "name=value",               "/poobah.html?name=value")]
        [DataRow("",                 "/",            "name=value",               "/?name=value")]
        [DataRow("",                 "/",            "",                         "/")]
        [DataRow("",                 "/",            null,                       "/")]
        [DataRow("/Root",            "",             null,                       "/Root")]
        [DataRow("/Root",            "/",            null,                       "/Root/")]
        [DataRow("/VirtualRadar",    "/poobah.html", "name=value",               "/VirtualRadar/poobah.html?name=value")]
        [DataRow("/VirtualRadar",    "/poobah.html", "name=value",               "/VirtualRadar/poobah.html?name=value")]
        [DataRow("/VirtualRadar",    "/poobah.html", "name=value",               "/VirtualRadar/poobah.html?name=value")]
        [DataRow("/VirtualRadar",    "/poobah.html", "name=value",               "/VirtualRadar/poobah.html?name=value")]
        [DataRow("/VirtualRadar",    "/poobah.html", "name=value",               "/VirtualRadar/poobah.html?name=value")]
        [DataRow("/VirtualRadar",    "/poobah.html", "name=value",               "/VirtualRadar/poobah.html?name=value")]
        [DataRow("/VirtualRadar",    "/poobah.html", "name=percent%2Dencoded",   "/VirtualRadar/poobah.html?name=percent%2Dencoded")]
        [DataRow("/VirtualRadar",    "/poobah.html", null,                       "/VirtualRadar/poobah.html")]
        [DataRow("/VirtualRadar",    "/poobah.html", "",                         "/VirtualRadar/poobah.html")]
        public void ConstructUrlFromRoot_Returns_Correct_Values(string pathBase, string path, string queryString, string expected)
        {
            var actual = OwinPath.ConstructUrlFromRoot(pathBase, path, queryString);

            Assert.AreEqual(expected, actual);
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

            var cacheEntry = environment.Environment[CustomEnvironmentKey.RequestPathParts] as IList<string>;
            if(createCacheEntry) {
                Assert.IsTrue(actual.SequenceEqual(cacheEntry));
            } else {
                Assert.IsNull(cacheEntry);
            }
        }

        [TestMethod]
        public void RequestPathParts_Rebuilds_Cache_If_Path_Changes()
        {
            var environment = new MockOwinEnvironment();
            environment.RequestPath = "/a";

            var pathParts1 = OwinPath.RequestPathParts(environment.Environment, createAndUseCachedResult: true);
            environment.RequestPath = "/b";
            var pathParts2 = OwinPath.RequestPathParts(environment.Environment, createAndUseCachedResult: true);

            Assert.AreNotSame(pathParts1, pathParts2);
            Assert.AreEqual(1, pathParts2.Length);
            Assert.AreEqual("b", pathParts2[0]);
        }
    }
}
