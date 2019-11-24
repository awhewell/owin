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
using AWhewell.Owin.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin.Utility
{
    [TestClass]
    public class ContentTypeValueTests
    {
        [TestMethod]
        public void Ctor_Fills_Properties_Correctly()
        {
            var value = new ContentTypeValue("Mime", "Abc", "Bounds");

            Assert.AreEqual("Mime", value.MediaType);
            Assert.AreEqual("Abc", value.Charset);
            Assert.AreEqual("Bounds", value.Boundary);
        }

        [TestMethod]
        [DataRow(null,                                      "--unused--",           "--unused--",   "--unused--")]
        [DataRow("",                                        "",                     null,           null)]
        [DataRow(" ",                                       "",                     null,           null)]
        [DataRow("text/html",                               "text/html",            null,           null)]
        [DataRow("text/html ",                              "text/html",            null,           null)]
        [DataRow(" text/html",                              "text/html",            null,           null)]
        [DataRow("text/html; charset=UTF-8",                "text/html",            "UTF-8",        null)]
        [DataRow("text/html ; charset=UTF-8",               "text/html",            "UTF-8",        null)]
        [DataRow("text/html; CHARSET=UTF-8",                "text/html",            "UTF-8",        null)]
        [DataRow("text/html; charset =UTF-8",               "text/html",            "UTF-8",        null)]
        [DataRow("text/html; charset= UTF-8",               "text/html",            "UTF-8",        null)]
        [DataRow("text/html; charseto=UTF-8",               "text/html",            null,           null)]
        [DataRow("text/html; charseto=UTF-8",               "text/html",            null,           null)]
        [DataRow("multipart/form-data; boundary=something", "multipart/form-data",  null,           "something")]
        [DataRow("application/json; boundary = a",          "application/json",     null,           "a")]
        public void Parse_Returns_Correct_ContentTypeValue(string headerValue, string mediaType, string charsetValue, string boundary)
        {
            var value = ContentTypeValue.Parse(headerValue);

            if(headerValue == null) {
                Assert.IsNull(value);
            } else {
                Assert.AreEqual(mediaType, value.MediaType);
                Assert.AreEqual(charsetValue, value.Charset);
                Assert.AreEqual(boundary, value.Boundary);
            }
        }

        [TestMethod]
        [DataRow(null,                                      null,                   null,      null)]
        [DataRow("",                                        "",                     null,      null)]
        [DataRow("text/html",                               "text/html",            null,      null)]
        [DataRow("text/html; charset=UTF-8",                "text/html",            "UTF-8",   null)]
        [DataRow("multipart/form-data; boundary=something", "multipart/form-data",  null,      "something")]
        public void ToString_Formats_Content_Correctly(string expected, string mediaType, string charsetValue, string boundary)
        {
            var value = new ContentTypeValue(mediaType, charsetValue, boundary);

            Assert.AreEqual(mediaType, value.MediaType);
            Assert.AreEqual(charsetValue, value.Charset);
            Assert.AreEqual(boundary, value.Boundary);

            Assert.AreEqual(expected, value.ToString());
        }
    }
}
