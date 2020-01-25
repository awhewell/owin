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
using System.Globalization;
using System.Text;

namespace AWhewell.Owin.Utility
{
    /// <summary>
    /// Describes a weighted value as used in some headers.
    /// </summary>
    public class QualityValue
    {
        /// <summary>
        /// Gets the value that has a quality associated with it.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets the quality associated with the value or null if not specified.
        /// </summary>
        public double? Quality { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="value"></param>
        public QualityValue(string value) : this(value, null)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="quality"></param>
        public QualityValue(string value, double? quality)
        {
            Value = value;
            Quality = quality;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var result = new StringBuilder(Value);

            if(Quality != null) {
                result.Append(";q=");
                result.AppendFormat(CultureInfo.InvariantCulture, "{0:0.0##}", Quality);
            }

            return result.ToString();
        }

        /// <summary>
        /// Parses a header value into a value and a quality.
        /// </summary>
        /// <param name="headerValue"></param>
        /// <returns></returns>
        public static QualityValue Parse(string headerValue)
        {
            QualityValue result = null;

            if(headerValue != null) {
                var qValueIdx = headerValue.IndexOf(";q=", StringComparison.OrdinalIgnoreCase);

                double? quality = null;
                if(qValueIdx != -1
                    && double.TryParse(
                        headerValue.Substring(qValueIdx + 3),
                        NumberStyles.AllowDecimalPoint | NumberStyles.AllowTrailingWhite,
                        CultureInfo.InvariantCulture,
                        out var qualityParsed
                    )
                    && qualityParsed <= 1.0
                    && headerValue.Length - qValueIdx <= 8      // Cannot exceed 3 decimal places
                    && headerValue[qValueIdx + 3] != '.'        // Must start with a digit
                ) {
                    quality = qualityParsed;
                } else {
                    qValueIdx = -1;
                }

                var value = qValueIdx == -1 ? headerValue : headerValue.Substring(0, qValueIdx);

                result = new QualityValue(value, quality);
            }

            return result;
        }

        /// <summary>
        /// Parses a comma-separated list of q-value strings into a collection of <see cref="QualityValue"/>
        /// objects.
        /// </summary>
        /// <param name="headerValue"></param>
        /// <returns></returns>
        public static IList<QualityValue> ParseCommaSeparated(string headerValue)
        {
            var result = new List<QualityValue>();

            if(!String.IsNullOrWhiteSpace(headerValue)) {
                foreach(var chunk in headerValue.Split(',')) {
                    result.Add(QualityValue.Parse(chunk.Trim()));
                }
            }

            return result;
        }
    }
}
