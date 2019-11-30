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

namespace AWhewell.Owin.Utility.Parsers
{
    /// <summary>
    /// Parses a string of hex digits with two digits per byte, no separators between the digits
    /// or bytes and with an optional case-sensitive '0x' prefix.
    /// </summary>
    public class ByteArray_HexString_Parser : ITypeParser<byte[]>
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryParse(string text, out byte[] value)
        {
            var result = false;
            value = null;

            if(text != null && text.Length % 2 == 0) {
                var textStart = text.StartsWith("0x") ? 2 : 0;
                var textLength = text.Length - textStart;

                value = new byte[textLength / 2];
                try {
                    for(int arrayIdx = 0, textIdx = textStart;arrayIdx < value.Length;++arrayIdx, textIdx += 2) {
                        value[arrayIdx] = Convert.ToByte(text.Substring(textIdx, 2), 16);
                    }
                    result = true;
                } catch(FormatException) {
                    value = null;
                }
            }

            return result;
        }
    }
}
