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
using System.Globalization;
using System.Text;
using AWhewell.Owin.Utility.Parsers;

namespace AWhewell.Owin.Utility.Formatters
{
    /// <summary>
    /// Emits a Microsoft JSON format date time offset.
    /// </summary>
    public class DateTimeOffset_MicrosoftJson_Formatter : ITypeFormatter<DateTimeOffset>
    {
        //internal static readonly DateTimeOffset UnixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string Format(DateTimeOffset value)
        {
            var result = new StringBuilder("/Date(");

            result.Append(
                (value.UtcDateTime - DateTime_JavaScriptTicks_Parser.UnixEpoch)
                .TotalMilliseconds
                .ToString(CultureInfo.InvariantCulture)
            );

            if(value.Offset != TimeSpan.Zero) {
                result.Append(value.Offset.TotalMilliseconds > 0 ? '+' : '-');
                result.Append(Math.Abs(value.Offset.Hours).ToString("00"));
                result.Append(Math.Abs(value.Offset.Minutes).ToString("00"));
            }

            result.Append(")/");

            return result.ToString();
        }
    }
}
