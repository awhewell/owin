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
    public class CacheControlResponseValue_Tests
    {
        [TestMethod]
        [DataRow(1,     2,      true,   false,  false,  false,  false,  false,  false)]
        [DataRow(null,  null,   false,  true,   false,  false,  false,  false,  false)]
        [DataRow(null,  null,   false,  false,  true,   false,  false,  false,  false)]
        [DataRow(null,  null,   false,  false,  false,  true,   false,  false,  false)]
        [DataRow(null,  null,   false,  false,  false,  false,  true,   false,  false)]
        [DataRow(null,  null,   false,  false,  false,  false,  false,  true,   false)]
        [DataRow(null,  null,   false,  false,  false,  false,  false,  false,  true)]
        public void Ctor_Fills_Properties_Correctly(int? maxAgeSeconds, int? sMaxAgeSeconds, bool mustRevalidate, bool noCache, bool noStore, bool noTransform, bool isPublic, bool isPrivate, bool proxyRevalidate)
        {
            var value = new CacheControlResponseValue(
                maxAgeSeconds:      maxAgeSeconds,
                sMaxAgeSeconds:     sMaxAgeSeconds,
                mustRevalidate:     mustRevalidate,
                noCache:            noCache,
                noStore:            noStore,
                noTransform:        noTransform,
                isPublic:           isPublic,
                isPrivate:          isPrivate,
                proxyRevalidate:    proxyRevalidate
            );

            Assert.AreEqual(maxAgeSeconds,      value.MaxAgeSeconds);
            Assert.AreEqual(sMaxAgeSeconds,     value.SMaxAgeSeconds);
            Assert.AreEqual(noCache,            value.NoCache);
            Assert.AreEqual(noStore,            value.NoStore);
            Assert.AreEqual(noTransform,        value.NoTransform);
            Assert.AreEqual(isPublic,           value.IsPublic);
            Assert.AreEqual(isPrivate,          value.IsPrivate);
            Assert.AreEqual(proxyRevalidate,    value.ProxyRevalidate);
        }

        [TestMethod]
        [DataRow(null,                      null,   null,   false,  false,  false,  false,  false,  false,  false)]
        [DataRow("",                        null,   null,   false,  false,  false,  false,  false,  false,  false)]
        [DataRow("max-age=1",               1,      null,   false,  false,  false,  false,  false,  false,  false)]
        [DataRow("MAX-AGE=1",               1,      null,   false,  false,  false,  false,  false,  false,  false)]
        [DataRow("s-maxage=1",              null,   1,      false,  false,  false,  false,  false,  false,  false)]
        [DataRow("S-MAXAGE=1",              null,   1,      false,  false,  false,  false,  false,  false,  false)]
        [DataRow("must-revalidate",         null,   null,   true,   false,  false,  false,  false,  false,  false)]
        [DataRow("MUST-REVALIDATE",         null,   null,   true,   false,  false,  false,  false,  false,  false)]
        [DataRow("no-cache",                null,   null,   false,  true,   false,  false,  false,  false,  false)]
        [DataRow("NO-CACHE",                null,   null,   false,  true,   false,  false,  false,  false,  false)]
        [DataRow("no-store",                null,   null,   false,  false,  true,   false,  false,  false,  false)]
        [DataRow("NO-STORE",                null,   null,   false,  false,  true,   false,  false,  false,  false)]
        [DataRow("no-transform",            null,   null,   false,  false,  false,  true,   false,  false,  false)]
        [DataRow("NO-TRANSFORM",            null,   null,   false,  false,  false,  true,   false,  false,  false)]
        [DataRow("public",                  null,   null,   false,  false,  false,  false,  true,   false,  false)]
        [DataRow("PUBLIC",                  null,   null,   false,  false,  false,  false,  true,   false,  false)]
        [DataRow("private",                 null,   null,   false,  false,  false,  false,  false,  true,   false)]
        [DataRow("PRIVATE",                 null,   null,   false,  false,  false,  false,  false,  true,   false)]
        [DataRow("proxy-revalidate",        null,   null,   false,  false,  false,  false,  false,  false,  true)]
        [DataRow("PROXY-REVALIDATE",        null,   null,   false,  false,  false,  false,  false,  false,  true)]
        [DataRow("max-age=1,no-cache",      1,      null,   false,  true,   false,  false,  false,  false,  false)]
        [DataRow("max-age=1 , no-cache",    1,      null,   false,  true,   false,  false,  false,  false,  false)]
        public void Parse_Returns_Correct_Object(string header, int? maxAgeSeconds, int? sMaxAgeSeconds, bool mustRevalidate, bool noCache, bool noStore, bool noTransform, bool isPublic, bool isPrivate, bool proxyRevalidate)
        {
            var value = CacheControlResponseValue.Parse(header);

            if(header == null) {
                Assert.IsNull(value);
            } else {
                Assert.AreEqual(maxAgeSeconds,      value.MaxAgeSeconds);
                Assert.AreEqual(sMaxAgeSeconds,     value.SMaxAgeSeconds);
                Assert.AreEqual(noCache,            value.NoCache);
                Assert.AreEqual(noStore,            value.NoStore);
                Assert.AreEqual(noTransform,        value.NoTransform);
                Assert.AreEqual(isPublic,           value.IsPublic);
                Assert.AreEqual(isPrivate,          value.IsPrivate);
                Assert.AreEqual(proxyRevalidate,    value.ProxyRevalidate);
            }
        }

        [TestMethod]
        [DataRow("",                        null,   null,   false,  false,  false,  false,  false,  false,  false)]
        [DataRow("max-age=1",               1,      null,   false,  false,  false,  false,  false,  false,  false)]
        [DataRow("s-maxage=1",              null,   1,      false,  false,  false,  false,  false,  false,  false)]
        [DataRow("must-revalidate",         null,   null,   true,   false,  false,  false,  false,  false,  false)]
        [DataRow("no-cache",                null,   null,   false,  true,   false,  false,  false,  false,  false)]
        [DataRow("no-store",                null,   null,   false,  false,  true,   false,  false,  false,  false)]
        [DataRow("no-transform",            null,   null,   false,  false,  false,  true,   false,  false,  false)]
        [DataRow("public",                  null,   null,   false,  false,  false,  false,  true,   false,  false)]
        [DataRow("private",                 null,   null,   false,  false,  false,  false,  false,  true,   false)]
        [DataRow("proxy-revalidate",        null,   null,   false,  false,  false,  false,  false,  false,  true)]
        [DataRow("max-age=1,no-cache",      1,      null,   false,  true,   false,  false,  false,  false,  false)]
        public void ToString_Returns_Correct_Header_String(string expected, int? maxAgeSeconds, int? sMaxAgeSeconds, bool mustRevalidate, bool noCache, bool noStore, bool noTransform, bool isPublic, bool isPrivate, bool proxyRevalidate)
        {
            var value = new CacheControlResponseValue(
                maxAgeSeconds:      maxAgeSeconds,
                sMaxAgeSeconds:     sMaxAgeSeconds,
                mustRevalidate:     mustRevalidate,
                noCache:            noCache,
                noStore:            noStore,
                noTransform:        noTransform,
                isPublic:           isPublic,
                isPrivate:          isPrivate,
                proxyRevalidate:    proxyRevalidate
            );

            Assert.AreEqual(expected, value.ToString());
        }
    }
}
