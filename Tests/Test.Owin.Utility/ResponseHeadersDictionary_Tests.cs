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
using System.Globalization;
using System.Linq;
using AWhewell.Owin.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin.Utility
{
    [TestClass]
    public class ResponseHeadersDictionary_Tests
    {
        IDictionary<string, string[]>   _UnderlyingDictionary;
        ResponseHeadersDictionary       _ResponseHeaders;

        [TestInitialize]
        public void TestInitialise()
        {
            _UnderlyingDictionary = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            _ResponseHeaders = new ResponseHeadersDictionary(_UnderlyingDictionary);
        }

        [TestMethod]
        [DataRow(nameof(ResponseHeadersDictionary.AccessControlAllowOrigin),    "Access-Control-Allow-Origin")]
        [DataRow(nameof(ResponseHeadersDictionary.CacheControl),                "Cache-Control")]
        [DataRow(nameof(ResponseHeadersDictionary.ContentType),                 "Content-Type")]
        [DataRow(nameof(ResponseHeadersDictionary.Location),                    "Location")]
        public void String_Values_Exposed_Correctly(string propertyName, string headerKey)
        {
            var propertyInfo = typeof(ResponseHeadersDictionary).GetProperty(propertyName);

            propertyInfo.SetValue(_ResponseHeaders, "Ab");
            var actual = propertyInfo.GetValue(_ResponseHeaders, null);

            Assert.AreEqual("Ab", actual);
            Assert.IsTrue(new string[] { "Ab" }.SequenceEqual(_UnderlyingDictionary[headerKey]));
        }

        [TestMethod]
        [DataRow(nameof(ResponseHeadersDictionary.ContentLength), "Content-Length")]
        public void Long_Values_Exposed_Correctly(string propertyName, string headerKey)
        {
            var propertyInfo = typeof(ResponseHeadersDictionary).GetProperty(propertyName);

            propertyInfo.SetValue(_ResponseHeaders, long.MaxValue);
            var actual = propertyInfo.GetValue(_ResponseHeaders, null);

            Assert.AreEqual(long.MaxValue, actual);
            Assert.IsTrue(new string[] { long.MaxValue.ToString(CultureInfo.InvariantCulture) }.SequenceEqual(_UnderlyingDictionary[headerKey]));

            propertyInfo.SetValue(_ResponseHeaders, null);
            actual = propertyInfo.GetValue(_ResponseHeaders, null);

            Assert.AreEqual(null, actual);
            Assert.IsFalse(_UnderlyingDictionary.ContainsKey(headerKey));

            _UnderlyingDictionary[headerKey] = new string[] { "not a long" };
            actual = propertyInfo.GetValue(_ResponseHeaders, null);

            Assert.AreEqual(null, actual);
        }

        [TestMethod]
        public void ContentTypeValue_Gets_And_Sets_Correctly()
        {
            _UnderlyingDictionary["Content-Type"] = new string[] { "text/plain" };

            var value = _ResponseHeaders.ContentTypeValue;
            Assert.AreEqual("text/plain", value.MediaType);

            _ResponseHeaders.ContentTypeValue = null;
            Assert.IsFalse(_UnderlyingDictionary.ContainsKey("Content-Type"));

            _ResponseHeaders.ContentTypeValue = new ContentTypeValue("application/json");
            Assert.IsTrue(new string[] { "application/json" }.SequenceEqual(_UnderlyingDictionary["Content-Type"]));
        }

        [TestMethod]
        public void ContentTypeValue_Returns_Empty_Object_Even_If_ContentType_Is_Not_Set()
        {
            // This keeps things consistent with RequestHeadersDictionary where empty values are shown even when the
            // request does not specify a content type. If one dictionary never returns null and the other dictionary
            // sometimes returns null then people (especially me) are going to forget which one needs a null check
            // and which does not.

            Assert.IsNotNull(_ResponseHeaders.ContentTypeValue);
        }

        [TestMethod]
        public void CacheControlValue_Gets_And_Sets_Correctly()
        {
            _UnderlyingDictionary["Cache-Control"] = new string[] { "no-cache" };

            var value = _ResponseHeaders.CacheControlValue;
            Assert.AreEqual(true, value.NoCache);

            _ResponseHeaders.CacheControlValue = null;
            Assert.IsFalse(_UnderlyingDictionary.ContainsKey("Cache-Control"));

            _ResponseHeaders.CacheControlValue = new CacheControlResponseValue(maxAgeSeconds: 10, isPublic: true);
            Assert.IsTrue(new string[] { "max-age=10,public" }.SequenceEqual(_UnderlyingDictionary["Cache-Control"]));
        }

        [TestMethod]
        public void CacheControlValue_Returns_Empty_Object_Even_If_CacheControl_Is_Not_Set()
        {
            // See ContentTypeValue tests for reasoning behind this

            Assert.IsNotNull(_ResponseHeaders.CacheControlValue);
        }
    }
}
