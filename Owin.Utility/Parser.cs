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
using System.Text;

namespace AWhewell.Owin.Utility
{
    /// <summary>
    /// A string parser.
    /// </summary>
    /// <remarks>
    /// This only supports invariant culture values. It is intended to be faster than
    /// TypeDescriptor.GetConverter().ConvertFrom() for common types.
    /// </remarks>
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
                case "on":      return true;
                case "off":     return false;
                case "yes":     return true;
                case "no":      return false;
                case "1":       return true;
                case "0":       return false;
                default:        return null;
            }
        }

        /// <summary>
        /// Extracts an unsigned byte from the text or null if no byte could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static byte? ParseByte(string text)
        {
            if(byte.TryParse(text, out var result)) {
                return result;
            } else {
                return (byte?)null;
            }
        }

        /// <summary>
        /// Extracts a single character from the text or null if no char could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static char? ParseChar(string text)
        {
            if(char.TryParse(text, out var result)) {
                return result;
            } else {
                return (char?)null;
            }
        }

        /// <summary>
        /// Extracts a 16-bit integer from the text or null of no short could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static Int16? ParseInt16(string text)
        {
            if(Int16.TryParse(text, out var result)) {
                return result;
            } else {
                return (Int16?)null;
            }
        }

        /// <summary>
        /// Extracts an unsigned 16-bit integer from the text or null of no unsigned short could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static UInt16? ParseUInt16(string text)
        {
            if(UInt16.TryParse(text, out var result)) {
                return result;
            } else {
                return (UInt16?)null;
            }
        }

        /// <summary>
        /// Extracts an integer from the text or null if no int could be extracted.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Int32? ParseInt32(string text)
        {
            if(Int32.TryParse(text, out var result)) {
                return result;
            } else {
                return (Int32?)null;
            }
        }

        /// <summary>
        /// Extracts an unsigned integer from the text or null if no unsigned int could be extracted.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static UInt32? ParseUInt32(string text)
        {
            if(UInt32.TryParse(text, out var result)) {
                return result;
            } else {
                return (UInt32?)null;
            }
        }

        /// <summary>
        /// Extracts a long from the text or null if no long could be extracted.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Int64? ParseInt64(string text)
        {
            if(Int64.TryParse(text, out var result)) {
                return result;
            } else {
                return (Int64?)null;
            }
        }

        /// <summary>
        /// Extracts an unsigned long from the text or null if no ulong could be extracted.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static UInt64? ParseUInt64(string text)
        {
            if(UInt64.TryParse(text, out var result)) {
                return result;
            } else {
                return (UInt64?)null;
            }
        }

        /// <summary>
        /// Extracts a floating point number from the text or null if no float could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static float? ParseFloat(string text)
        {
            if(float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)) {
                return result;
            } else {
                return (float?)null;
            }
        }

        /// <summary>
        /// Extracts a double-precision floating point number from the text or null if no double could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static double? ParseDouble(string text)
        {
            if(double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)) {
                return result;
            } else {
                return (double?)null;
            }
        }

        /// <summary>
        /// Extracts a decimal floating point number from the text or null if no decimal could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static decimal? ParseDecimal(string text)
        {
            if(decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)) {
                return result;
            } else {
                return (decimal?)null;
            }
        }

        /// <summary>
        /// Extracts a date time from the text or null if no date time could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static DateTime? ParseDateTime(string text)
        {
            if(DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)) {
                return result;
            } else {
                return (DateTime?)null;
            }
        }

        /// <summary>
        /// Extracts a date time offset from the text or null if no date time offset could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static DateTimeOffset? ParseDateTimeOffset(string text)
        {
            if(DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)) {
                return result;
            } else {
                return (DateTimeOffset?)null;
            }
        }

        /// <summary>
        /// Extracts a GUID from the text or null if no GUID could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static Guid? ParseGuid(string text)
        {
            if(Guid.TryParse(text, out var result)) {
                return result;
            } else {
                return (Guid?)null;
            }
        }

        /// <summary>
        /// Extracts a byte array from the text or null if no byte array could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static byte[] ParseHexBytes(string text)
        {
            byte[] result = null;

            if(text != null && text.Length % 2 == 0) {
                var textStart = text.StartsWith("0x") ? 2 : 0;
                var textLength = text.Length - textStart;

                result = new byte[textLength / 2];
                try {
                    for(int arrayIdx = 0, textIdx = textStart;arrayIdx < result.Length;++arrayIdx, textIdx += 2) {
                        result[arrayIdx] = Convert.ToByte(text.Substring(textIdx, 2), 16);
                    }
                } catch(FormatException) {
                    result = null;
                }
            }

            return result;
        }

        /// <summary>
        /// Extracts a byte array from the MIME64 encoded string passed across or null if no byte array could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static byte[] ParseMime64Bytes(string text)
        {
            byte[] result = null;

            if(text != null) {
                try {
                    result = Convert.FromBase64String(text);
                } catch(FormatException) {
                    result = null;
                }
            }

            return result;
        }

        /// <summary>
        /// Parses the <paramref name="text"/> into an object of type <paramref name="type"/>. If the
        /// text cannot be parsed then null is returned.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="text"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <remarks>
        /// This should be faster than TypeDescriptor.GetConverter().ConvertFrom() for the stock types.
        /// </remarks>
        public static object ParseType(Type type, string text, ParserOptions options = null)
        {
            object result = null;

            if(type == typeof(string)) {
                result = text;
            } else if(!String.IsNullOrWhiteSpace(text)) {
                if(type == typeof(bool)) {
                    result = ParseBool(text);
                } else if(type == typeof(byte) || type == typeof(byte?)) {
                    result = ParseByte(text);
                } else if(type == typeof(char) || type == typeof(char?)) {
                    result = ParseChar(text);
                } else if(type == typeof(Int16) || type == typeof(Int16?)) {
                    result = ParseInt16(text);
                } else if(type == typeof(UInt16) || type == typeof(UInt16?)) {
                    result = ParseUInt16(text);
                } else if(type == typeof(Int32) || type == typeof(Int32?)) {
                    result = ParseInt32(text);
                } else if(type == typeof(UInt32) || type == typeof(UInt32?)) {
                    result = ParseUInt32(text);
                } else if(type == typeof(Int64) || type == typeof(Int64?)) {
                    result = ParseInt64(text);
                } else if(type == typeof(UInt64) || type == typeof(UInt64?)) {
                    result = ParseUInt64(text);
                } else if(type == typeof(float) || type == typeof(float?)) {
                    result = ParseFloat(text);
                } else if(type == typeof(double) || type == typeof(double?)) {
                    result = ParseDouble(text);
                } else if(type == typeof(decimal) || type == typeof(decimal?)) {
                    result = ParseDecimal(text);
                } else if(type == typeof(DateTime) || type == typeof(DateTime?)) {
                    result = ParseDateTime(text);
                } else if(type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?)) {
                    result = ParseDateTimeOffset(text);
                } else if(type == typeof(Guid) || type == typeof(Guid?)) {
                    result = ParseGuid(text);
                } else if(type == typeof(byte[])) {
                    if(options?.ByteArray == ParserOptions.ByteArrayFormat.HexString) {
                        result = ParseHexBytes(text);
                    } else {
                        result = ParseMime64Bytes(text);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the <see cref="HttpMethod"/> value corresponding with the text passed in.
        /// </summary>
        /// <param name="httpMethod"></param>
        /// <returns></returns>
        public static HttpMethod ParseHttpMethod(string httpMethod)
        {
            switch((httpMethod ?? "").Trim().ToUpper()) {
                case "CONNECT":     return HttpMethod.Connect;
                case "DELETE":      return HttpMethod.Delete;
                case "GET":         return HttpMethod.Get;
                case "HEAD":        return HttpMethod.Head;
                case "OPTIONS":     return HttpMethod.Options;
                case "PATCH":       return HttpMethod.Patch;
                case "POST":        return HttpMethod.Post;
                case "PUT":         return HttpMethod.Put;
                case "TRACE":       return HttpMethod.Trace;
                default:            return HttpMethod.Unknown;
            }
        }

        /// <summary>
        /// Returns the <see cref="HttpProtocol"/> value corresponding with the text passed in.
        /// </summary>
        /// <param name="httpProtocol"></param>
        /// <returns></returns>
        public static HttpProtocol ParseHttpProtocol(string httpProtocol)
        {
            switch((httpProtocol ?? "").Trim().ToUpper()) {
                case "HTTP/0.9":    return HttpProtocol.Http0_9;
                case "HTTP/1.0":    return HttpProtocol.Http1_0;
                case "HTTP/1.1":    return HttpProtocol.Http1_1;
                case "HTTP/2.0":    return HttpProtocol.Http2_0;
                case "HTTP/3.0":    return HttpProtocol.Http3_0;
                default:            return HttpProtocol.Unknown;
            }
        }

        /// <summary>
        /// Returns the correct <see cref="HttpScheme"/> value corresponding with the text passed in.
        /// </summary>
        /// <param name="httpScheme"></param>
        /// <returns></returns>
        public static HttpScheme ParseHttpScheme(string httpScheme)
        {
            switch((httpScheme ?? "").Trim().ToLower()) {
                case "http":    return HttpScheme.Http;
                case "https":   return HttpScheme.Https;
                default:        return HttpScheme.Unknown;
            }
        }
    }
}
