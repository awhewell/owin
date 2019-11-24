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
    public class KeyValueParserTests
    {
        [TestMethod]
        public void Default_Ctor_Initialises_To_Sensible_Values()
        {
            var parser = new KeyValueParser();

            Assert.AreEqual('=', parser.Separator);
            Assert.AreEqual("",  parser.MissingValue);
        }

        [TestMethod]
        public void Custom_Parameters_Ctor_Initialises_Properties()
        {
            var parser = new KeyValueParser(':', "oops");

            Assert.AreEqual(':', parser.Separator);
            Assert.AreEqual("oops", parser.MissingValue);
        }

        [TestMethod]
        [DataRow(null,      "",     "")]
        [DataRow("",        "",     "")]
        [DataRow("a",       "a",    "")]
        [DataRow("a=b",     "a",    "b")]
        [DataRow("a1=b2",   "a1",   "b2")]
        [DataRow("=b",      "",     "b")]
        [DataRow("a = b",   "a ",   " b")]
        [DataRow("a==b",    "a",    "=b")]
        public void Parse_Splits_Key_And_Value_Correctly(string text, string expectedKey, string expectedValue)
        {
            var parser = new KeyValueParser();

            string key, value;
            parser.Parse(text, out key, out value);

            Assert.AreEqual(expectedKey,    key);
            Assert.AreEqual(expectedValue,  value);
        }

        [TestMethod]
        public void Parse_Uses_Separator()
        {
            var parser = new KeyValueParser(':', "");

            string key, value;
            parser.Parse("Z1: a=b", out key, out value);

            Assert.AreEqual("Z1",   key);
            Assert.AreEqual(" a=b", value);
        }

        [TestMethod]
        public void Parse_Uses_MissingValue()
        {
            var parser = new KeyValueParser('=', "ug");

            string key, value;
            parser.Parse("a", out key, out value);

            Assert.AreEqual("a",  key);
            Assert.AreEqual("ug", value);
        }
    }
}
