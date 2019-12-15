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
    public class RequestHeadersDictionary_Tests
    {
        IDictionary<string, string[]>   _UnderlyingDictionary;
        RequestHeadersDictionary        _RequestHeaders;

        [TestInitialize]
        public void TestInitialise()
        {
            _UnderlyingDictionary = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            _RequestHeaders = new RequestHeadersDictionary(_UnderlyingDictionary);
        }

        [TestMethod]
        [DataRow(nameof(RequestHeadersDictionary.Authorization),    "Authorization")]
        [DataRow(nameof(RequestHeadersDictionary.CacheControl),     "Cache-Control")]
        [DataRow(nameof(RequestHeadersDictionary.ContentType),      "Content-Type")]
        [DataRow(nameof(RequestHeadersDictionary.Origin),           "Origin")]
        [DataRow(nameof(RequestHeadersDictionary.Referer),          "Referer")]
        [DataRow(nameof(RequestHeadersDictionary.UserAgent),        "User-Agent")]
        public void ReadOnly_String_Values_Exposed_Correctly(string propertyName, string headerKey)
        {
            var propertyInfo = typeof(RequestHeadersDictionary).GetProperty(propertyName);
            Assert.IsTrue(propertyInfo.CanRead);
            Assert.IsFalse(propertyInfo.CanWrite);

            Assert.IsNull(propertyInfo.GetValue(_RequestHeaders, null));

            _UnderlyingDictionary.Add(headerKey, new string[] { "Ab" });
            Assert.AreEqual("Ab", propertyInfo.GetValue(_RequestHeaders, null));
        }

        [TestMethod]
        [DataRow(nameof(RequestHeadersDictionary.Accept), "Accept")]
        public void ReadOnly_String_Array_Values_Exposed_Correctly(string propertyName, string headerKey)
        {
            var propertyInfo = typeof(RequestHeadersDictionary).GetProperty(propertyName);
            Assert.IsTrue(propertyInfo.CanRead);
            Assert.IsFalse(propertyInfo.CanWrite);

            Assert.AreEqual(0, ((IList<string>)propertyInfo.GetValue(_RequestHeaders, null)).Count);

            _UnderlyingDictionary.Add(headerKey, new string[] { "Ab", "Cd" });
            var result = (IList<string>)propertyInfo.GetValue(_RequestHeaders, null);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Ab", result[0]);
            Assert.AreEqual("Cd", result[1]);
        }

        [TestMethod]
        [DataRow(null,                          "",             null,       null)]
        [DataRow("",                            "",             null,       null)]
        [DataRow("text/plain",                  "text/plain",   null,       null)]
        [DataRow("text/plain; charset=utf-8",   "text/plain",   "utf-8",    null)]
        [DataRow("text/plain; boundary=cut",    "text/plain",   null,       "cut")]
        public void ContentTypeValue_Returns_ContentType_Parsed_Into_ContentTypeValue(string contentType, string expectedMediaType, string expectedCharset, string expectedBoundary)
        {
            if(contentType != null) {
                _RequestHeaders["Content-Type"] = contentType;
            }

            var contentTypeValue = _RequestHeaders.ContentTypeValue;

            Assert.AreEqual(expectedMediaType,  contentTypeValue.MediaType);
            Assert.AreEqual(expectedCharset,    contentTypeValue.Charset);
            Assert.AreEqual(expectedBoundary,   contentTypeValue.Boundary);
        }
    }
}
