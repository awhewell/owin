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
using System.Text;
using AWhewell.Owin.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin.Utility
{
    [TestClass]
    public class CacheControlRequestValue_Tests
    {
        [TestMethod]
        [DataRow(1,     2,      true,   false,  false,  false)]
        [DataRow(null,  null,   false,  true,   false,  false)]
        [DataRow(null,  null,   false,  false,  true,   false)]
        [DataRow(null,  null,   false,  false,  false,  true)]
        public void Ctor_Fills_Properties_Correctly(int? maxAgeSeconds, int? maxStaleSeconds, bool noCache, bool noStore, bool noTransform, bool onlyIfCached)
        {
            var value = new CacheControlRequestValue(
                maxAgeSeconds:      maxAgeSeconds,
                maxStaleSeconds:    maxStaleSeconds,
                noCache:            noCache,
                noStore:            noStore,
                noTransform:        noTransform,
                onlyIfCached:       onlyIfCached
            );

            Assert.AreEqual(maxAgeSeconds,      value.MaxAgeSeconds);
            Assert.AreEqual(maxStaleSeconds,    value.MaxStaleSeconds);
            Assert.AreEqual(noCache,            value.NoCache);
            Assert.AreEqual(noStore,            value.NoStore);
            Assert.AreEqual(noTransform,        value.NoTransform);
            Assert.AreEqual(onlyIfCached,       value.OnlyIfCached);
        }

        [TestMethod]
        [DataRow(null,                      null,   null,       false,  false,  false,  false)]
        [DataRow("",                        null,   null,       false,  false,  false,  false)]
        [DataRow("max-age=1",               1,      null,       false,  false,  false,  false)]
        [DataRow("MAX-AGE=1",               1,      null,       false,  false,  false,  false)]
        [DataRow("max-stale",               null,   2147483647, false,  false,  false,  false)]
        [DataRow("max-stale=1",             null,   1,          false,  false,  false,  false)]
        [DataRow("MAX-STALE=1",             null,   1,          false,  false,  false,  false)]
        [DataRow("no-cache",                null,   null,       true,   false,  false,  false)]
        [DataRow("NO-CACHE",                null,   null,       true,   false,  false,  false)]
        [DataRow("no-store",                null,   null,       false,  true,   false,  false)]
        [DataRow("NO-STORE",                null,   null,       false,  true,   false,  false)]
        [DataRow("no-transform",            null,   null,       false,  false,  true,   false)]
        [DataRow("NO-TRANSFORM",            null,   null,       false,  false,  true,   false)]
        [DataRow("only-if-cached",          null,   null,       false,  false,  false,  true)]
        [DataRow("ONLY-IF-CACHED",          null,   null,       false,  false,  false,  true)]
        [DataRow("max-age=1,no-cache",      1,      null,       true,   false,  false,  false)]
        [DataRow("max-age=1 , no-cache",    1,      null,       true,   false,  false,  false)]
        public void Parse_Fills_Properties_Correctly(string header, int? maxAgeSeconds, int? maxStaleSeconds, bool noCache, bool noStore, bool noTransform, bool onlyIfCached)
        {
            var value = CacheControlRequestValue.Parse(header);

            if(header == null) {
                Assert.IsNull(value);
            } else {
                Assert.AreEqual(maxAgeSeconds,      value.MaxAgeSeconds);
                Assert.AreEqual(maxStaleSeconds,    value.MaxStaleSeconds);
                Assert.AreEqual(noCache,            value.NoCache);
                Assert.AreEqual(noStore,            value.NoStore);
                Assert.AreEqual(noTransform,        value.NoTransform);
                Assert.AreEqual(onlyIfCached,       value.OnlyIfCached);
            }
        }

        [TestMethod]
        [DataRow("",                        null,   null,       false,  false,  false,  false)]
        [DataRow("max-age=1",               1,      null,       false,  false,  false,  false)]
        [DataRow("max-stale",               null,   2147483647, false,  false,  false,  false)]
        [DataRow("max-stale=1",             null,   1,          false,  false,  false,  false)]
        [DataRow("no-cache",                null,   null,       true,   false,  false,  false)]
        [DataRow("no-store",                null,   null,       false,  true,   false,  false)]
        [DataRow("no-transform",            null,   null,       false,  false,  true,   false)]
        [DataRow("only-if-cached",          null,   null,       false,  false,  false,  true)]
        [DataRow("max-age=1,no-cache",      1,      null,       true,   false,  false,  false)]
        public void ToString_Returns_Correct_Value(string expected, int? maxAgeSeconds, int? maxStaleSeconds, bool noCache, bool noStore, bool noTransform, bool onlyIfCached)
        {
            var value = new CacheControlRequestValue(
                maxAgeSeconds:      maxAgeSeconds,
                maxStaleSeconds:    maxStaleSeconds,
                noCache:            noCache,
                noStore:            noStore,
                noTransform:        noTransform,
                onlyIfCached:       onlyIfCached
            );

            Assert.AreEqual(expected, value.ToString());
        }
    }
}
