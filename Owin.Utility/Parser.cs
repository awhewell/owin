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

namespace AWhewell.Owin.Utility
{
    /// <summary>
    /// A group of handy parser methods.
    /// </summary>
    public static class Parser
    {
        /// <summary>
        /// Extracts a bool from the text or null if no bool could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool? ParseBool(string text)
        {
            switch((text ?? "").ToLower().Trim()) {
                case "true":    return true;
                case "false":   return false;
                default:        return null;
            }
        }

        /// <summary>
        /// Extracts a long from the text or null if no long could be extracted.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static long? ParseInt64(string text)
        {
            if(long.TryParse(text, out var result)) {
                return result;
            } else {
                return (long?)null;
            }
        }

        /// <summary>
        /// Extracts an integer from the text or null if no long could be extracted.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int? ParseInt32(string text)
        {
            if(int.TryParse(text, out var result)) {
                return result;
            } else {
                return (int?)null;
            }
        }
    }
}
