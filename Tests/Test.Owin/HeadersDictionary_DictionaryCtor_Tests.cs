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
using AWhewell.Owin.Interface;

namespace Test.AWhewell.Owin
{
    [TestClass]
    public class HeadersDictionary_DictionaryCtor_Tests : HeadersDictionary_Agnostic_Tests
    {
        private Dictionary<string, string[]> _WrappedDictionary;

        [TestInitialize]
        public void TestInitialise()
        {
            _WrappedDictionary = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            _Headers = new HeadersDictionary(_WrappedDictionary);
        }

        protected override void Reset_To_RawStringArray()
        {
            _WrappedDictionary = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase) {
                { _Key, _RawStringArray },
            };
            _Headers = new HeadersDictionary(_WrappedDictionary);
        }

        [TestMethod]
        public void Wrapper_Ctor_Keys_Use_Whatever_Comparison_The_Original_Dictionary_Has()
        {
            var existingDictionary = new Dictionary<string, string[]>(StringComparer.Ordinal);
            _Headers = new HeadersDictionary(existingDictionary);

            _Headers.Add("one", new string[] { "value" });

            Assert.IsNull(_Headers["ONE"]);
        }

        [TestMethod]
        public void Wrapper_Ctor_Accepts_Null_Wrapped_Dictionary()
        {
            _Headers = new HeadersDictionary(null);
            Assert.AreEqual(0, _Headers.Count);
        }

        [TestMethod]
        public void Wrapper_Ctor_Converts_Null_Wrapped_Dictionary_To_Case_Insensitive_Dictionary()
        {
            _Headers = new HeadersDictionary(null);
            _Headers.Add("one", new string[] { "value" });

            Assert.AreEqual("value", _Headers["ONE"]);
        }
    }
}
