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
using System.Linq;
using AWhewell.Owin.Utility.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin.Utility.Parsers
{
    [TestClass]
    public class ByteArray_HexString_ParserTests
    {
        [TestMethod]
        [DataRow("en-GB", null,     false,  null)]                          // Cannot parse null
        [DataRow("en-GB", "",       true,   new byte[0])]                   // Empty string is an empty byte array
        [DataRow("en-GB", "000B",   true,   new byte[] { 0x00, 0x0b })]     // Can parse byte array in upper-case without prefix
        [DataRow("en-GB", "abcd",   true,   new byte[] { 0xAB, 0xCD })]     // Can parse byte array in lower-case without prefix
        [DataRow("en-GB", "0",      false,  null)]                          // Single-digit hex not allowed
        [DataRow("en-GB", "0ba",    false,  null)]                          // Odd-length hex not allowed
        [DataRow("en-GB", "g1",     false,  null)]                          // Digits higher than base 16 not allowed
        [DataRow("en-GB", "0x0102", true,   new byte[] { 0x01, 0x02 })]     // 0x prefix is acceptable
        [DataRow("en-GB", "0x",     true,   new byte[0])]                   // 0x prefix on its own is acceptable
        [DataRow("en-GB", "0X0102", false,  null)]                          // 0x prefix is case sensitive
        [DataRow("en-GB", "010x",   false,  null)]                          // 0x prefix can only appear at start of string
        public void TryParse_Behaves_Correctly(string culture, string text, bool expectedResult, byte[] expectedValue)
        {
            using(new CultureSwap(culture)) {
                var parser = new ByteArray_HexString_Parser();

                var actualResult = parser.TryParse(text, out var actualValue);

                Assert.AreEqual(expectedResult, actualResult);
                if(expectedValue == null) {
                    Assert.IsNull(actualValue);
                } else {
                    Assert.IsTrue(expectedValue.SequenceEqual(actualValue));
                }
            }
        }
    }
}
