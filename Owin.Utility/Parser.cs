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
using AWhewell.Owin.Utility.Parsers;

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
        private static readonly ByteArray_HexString_Parser      _ByteArray_HexString_Parser = new ByteArray_HexString_Parser();
        private static readonly ByteArray_Mime64_Parser         _ByteArray_Mime64_Parser = new ByteArray_Mime64_Parser();
        private static readonly DateTime_Invariant_Parser       _DateTime_Invariant_Parser = new DateTime_Invariant_Parser();
        private static readonly DateTimeOffset_Invariant_Parser _DateTimeOffset_Invariant_Parser = new DateTimeOffset_Invariant_Parser();

        /// <summary>
        /// Extracts a bool from the text or null if no bool could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool? ParseBool(string text)
        {
            if(text != null) {
                switch(text.ToLower().Trim()) {
                    case "on":
                    case "yes":
                    case "1":
                    case "true":    return true;
                    case "off":
                    case "no":
                    case "0":
                    case "false":   return false;
                }
            }

            return null;
        }

        /// <summary>
        /// Extracts a bool from the text or null if no bool could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static bool? ParseBool(string text, TypeParserResolver resolver)
        {
            var parser = resolver?.BoolParser;
            return parser == null
                ? ParseBool(text)
                : parser.TryParse(text, out var result)
                    ? result
                    : (bool?)null;
        }

        /// <summary>
        /// Extracts an unsigned byte from the text or null if no byte could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static byte? ParseByte(string text) => byte.TryParse(text, out var result) ? result : (byte?)null;

        /// <summary>
        /// Extracts an unsigned byte from the text or null if no byte could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static byte? ParseByte(string text, TypeParserResolver resolver)
        {
            var parser = resolver?.ByteParser;
            return parser == null
                ? ParseByte(text)
                : parser.TryParse(text, out var result)
                    ? result
                    : (byte?)null;
        }

        /// <summary>
        /// Extracts a single character from the text or null if no char could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static char? ParseChar(string text) => char.TryParse(text, out var result) ? result : (char?)null;

        /// <summary>
        /// Extracts a single character from the text or null if no char could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static char? ParseChar(string text, TypeParserResolver resolver)
        {
            var parser = resolver?.CharParser;
            return parser == null
                ? ParseChar(text)
                : parser.TryParse(text, out var result)
                    ? result
                    : (char?)null;
        }

        /// <summary>
        /// Extracts a 16-bit integer from the text or null of no short could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static Int16? ParseInt16(string text) => Int16.TryParse(text, out var result) ? result : (Int16?)null;

        /// <summary>
        /// Extracts a 16-bit integer from the text or null of no short could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static Int16? ParseInt16(string text, TypeParserResolver resolver)
        {
            var parser = resolver?.Int16Parser;
            return parser == null
                ? ParseInt16(text)
                : parser.TryParse(text, out var result)
                    ? result
                    : (Int16?)null;
        }

        /// <summary>
        /// Extracts an unsigned 16-bit integer from the text or null of no unsigned short could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static UInt16? ParseUInt16(string text) => UInt16.TryParse(text, out var result) ? result : (UInt16?)null;

        /// <summary>
        /// Extracts an unsigned 16-bit integer from the text or null of no unsigned short could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static UInt16? ParseUInt16(string text, TypeParserResolver resolver)
        {
            var parser = resolver?.UInt16Parser;
            return parser == null
                ? ParseUInt16(text)
                : parser.TryParse(text, out var result)
                    ? result
                    : (UInt16?)null;
        }

        /// <summary>
        /// Extracts an integer from the text or null if no int could be extracted.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Int32? ParseInt32(string text) => Int32.TryParse(text, out var result) ? result : (Int32?)null;

        /// <summary>
        /// Extracts an integer from the text or null if no int could be extracted.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static Int32? ParseInt32(string text, TypeParserResolver resolver)
        {
            var parser = resolver?.Int32Parser;
            return parser == null
                ? ParseInt32(text)
                : parser.TryParse(text, out var result)
                    ? result
                    : (Int32?)null;
        }

        /// <summary>
        /// Extracts an unsigned integer from the text or null if no unsigned int could be extracted.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static UInt32? ParseUInt32(string text) => UInt32.TryParse(text, out var result) ? result : (UInt32?)null;

        /// <summary>
        /// Extracts an unsigned integer from the text or null if no unsigned int could be extracted.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static UInt32? ParseUInt32(string text, TypeParserResolver resolver)
        {
            var parser = resolver?.UInt32Parser;
            return parser == null
                ? ParseUInt32(text)
                : parser.TryParse(text, out var result)
                    ? result
                    : (UInt32?)null;
        }

        /// <summary>
        /// Extracts a long from the text or null if no long could be extracted.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Int64? ParseInt64(string text) => Int64.TryParse(text, out var result) ? result : (Int64?)null;

        /// <summary>
        /// Extracts a long from the text or null if no long could be extracted.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static Int64? ParseInt64(string text, TypeParserResolver resolver)
        {
            var parser = resolver?.Int64Parser;
            return parser == null
                ? ParseInt64(text)
                : parser.TryParse(text, out var result)
                    ? result
                    : (Int64?)null;
        }

        /// <summary>
        /// Extracts an unsigned long from the text or null if no ulong could be extracted.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static UInt64? ParseUInt64(string text) => UInt64.TryParse(text, out var result) ? result : (UInt64?)null;

        /// <summary>
        /// Extracts an unsigned long from the text or null if no ulong could be extracted.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static UInt64? ParseUInt64(string text, TypeParserResolver resolver)
        {
            var parser = resolver?.UInt64Parser;
            return parser == null
                ? ParseUInt64(text)
                : parser.TryParse(text, out var result)
                    ? result
                    : (UInt64?)null;
        }

        /// <summary>
        /// Extracts a floating point number from the text or null if no float could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static float? ParseFloat(string text) => float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : (float?)null;

        /// <summary>
        /// Extracts a floating point number from the text or null if no float could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static float? ParseFloat(string text, TypeParserResolver resolver)
        {
            var parser = resolver?.FloatParser;
            return parser == null
                ? ParseFloat(text)
                : parser.TryParse(text, out var result)
                    ? result
                    : (float?)null;
        }

        /// <summary>
        /// Extracts a double-precision floating point number from the text or null if no double could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static double? ParseDouble(string text) => double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : (double?)null;

        /// <summary>
        /// Extracts a double-precision floating point number from the text or null if no double could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static double? ParseDouble(string text, TypeParserResolver resolver)
        {
            var parser = resolver?.DoubleParser;
            return parser == null
                ? ParseDouble(text)
                : parser.TryParse(text, out var result)
                    ? result
                    : (double?)null;
        }

        /// <summary>
        /// Extracts a decimal floating point number from the text or null if no decimal could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static decimal? ParseDecimal(string text) => decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : (decimal?)null;

        /// <summary>
        /// Extracts a decimal floating point number from the text or null if no decimal could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static decimal? ParseDecimal(string text, TypeParserResolver resolver)
        {
            var parser = resolver?.DecimalParser;
            return parser == null
                ? ParseDecimal(text)
                : parser.TryParse(text, out var result)
                    ? result
                    : (decimal?)null;
        }

        /// <summary>
        /// Extracts a date time from the text or null if no date time could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static DateTime? ParseDateTime(string text) => _DateTime_Invariant_Parser.TryParse(text, out var result) ? result : (DateTime?)null;

        /// <summary>
        /// Extracts a date time from the text or null if no date time could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static DateTime? ParseDateTime(string text, TypeParserResolver resolver)
        {
            var parser = resolver?.DateTimeParser;
            return parser == null
                ? ParseDateTime(text)
                : parser.TryParse(text, out var result)
                    ? result
                    : (DateTime?)null;
        }

        /// <summary>
        /// Extracts a date time offset from the text or null if no date time offset could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static DateTimeOffset? ParseDateTimeOffset(string text) => _DateTimeOffset_Invariant_Parser.TryParse(text, out var result) ? result : (DateTimeOffset?)null;

        /// <summary>
        /// Extracts a date time offset from the text or null if no date time offset could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static DateTimeOffset? ParseDateTimeOffset(string text, TypeParserResolver resolver)
        {
            var parser = resolver?.DateTimeOffsetParser;
            return parser == null
                ? ParseDateTimeOffset(text)
                : parser.TryParse(text, out var result)
                    ? result
                    : (DateTimeOffset?)null;
        }

        /// <summary>
        /// Extracts a GUID from the text or null if no GUID could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static Guid? ParseGuid(string text) => Guid.TryParse(text, out var result) ? result : (Guid?)null;

        /// <summary>
        /// Extracts a GUID from the text or null if no GUID could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static Guid? ParseGuid(string text, TypeParserResolver resolver)
        {
            var parser = resolver?.GuidParser;
            return parser == null
                ? ParseGuid(text)
                : parser.TryParse(text, out var result)
                    ? result
                    : (Guid?)null;
        }

        /// <summary>
        /// Extracts a byte array from a MIME64 encoded string.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static byte[] ParseByteArray(string text) => ParseMime64Bytes(text);

        /// <summary>
        /// Extracts a byte array from text with an optional type parser resolver.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static byte[] ParseByteArray(string text, TypeParserResolver resolver)
        {
            var parser = resolver?.ByteArrayParser;
            return parser == null
                ? ParseByteArray(text)
                : parser.TryParse(text, out var result)
                    ? result
                    : (byte[])null;
        }

        /// <summary>
        /// Extracts a byte array from the text or null if no byte array could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static byte[] ParseHexBytes(string text)
        {
            if(_ByteArray_HexString_Parser.TryParse(text, out var result)) {
                return result;
            } else {
                return (byte[])null;
            }
        }

        /// <summary>
        /// Extracts a byte array from the MIME64 encoded string passed across or null if no byte array could be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static byte[] ParseMime64Bytes(string text)
        {
            if(_ByteArray_Mime64_Parser.TryParse(text, out var result)) {
                return result;
            } else {
                return (byte[])null;
            }
        }

        /// <summary>
        /// Parses the <paramref name="text"/> into an object of type <paramref name="type"/>. If the
        /// text cannot be parsed then null is returned.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <remarks>
        /// This should be faster than TypeDescriptor.GetConverter().ConvertFrom() for the stock types.
        /// </remarks>
        public static object ParseType(Type type, string text) => ParseType(type, text, null);

        /// <summary>
        /// Parses the <paramref name="text"/> into an object of type <paramref name="type"/>. If the
        /// text cannot be parsed then null is returned.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="text"></param>
        /// <param name="typeParserResolver"></param>
        /// <returns></returns>
        /// <remarks>
        /// This should be faster than TypeDescriptor.GetConverter().ConvertFrom() for the stock types.
        /// </remarks>
        public static object ParseType(Type type, string text, TypeParserResolver typeParserResolver)
        {
            object result = null;

            if(type == typeof(string)) {
                result = text;
            } else if(text != null) {
                if(type == typeof(bool) || type == typeof(bool?)) {
                    result = typeParserResolver == null
                        ? ParseBool(text)
                        : ParseBool(text, typeParserResolver);
                } else if(type == typeof(byte) || type == typeof(byte?)) {
                    result = typeParserResolver == null
                        ? ParseByte(text)
                        : ParseByte(text, typeParserResolver);
                } else if(type == typeof(char) || type == typeof(char?)) {
                    result = typeParserResolver == null
                        ? ParseChar(text)
                        : ParseChar(text, typeParserResolver);
                } else if(type == typeof(Int16) || type == typeof(Int16?)) {
                    result = typeParserResolver == null
                        ? ParseInt16(text)
                        : ParseInt16(text, typeParserResolver);
                } else if(type == typeof(UInt16) || type == typeof(UInt16?)) {
                    result = typeParserResolver == null
                        ? ParseUInt16(text)
                        : ParseUInt16(text, typeParserResolver);
                } else if(type == typeof(Int32) || type == typeof(Int32?)) {
                    result = typeParserResolver == null
                        ? ParseInt32(text)
                        : ParseInt32(text, typeParserResolver);
                } else if(type == typeof(UInt32) || type == typeof(UInt32?)) {
                    result = typeParserResolver == null
                        ? ParseUInt32(text)
                        : ParseUInt32(text, typeParserResolver);
                } else if(type == typeof(Int64) || type == typeof(Int64?)) {
                    result = typeParserResolver == null
                        ? ParseInt64(text)
                        : ParseInt64(text, typeParserResolver);
                } else if(type == typeof(UInt64) || type == typeof(UInt64?)) {
                    result = typeParserResolver == null
                        ? ParseUInt64(text)
                        : ParseUInt64(text, typeParserResolver);
                } else if(type == typeof(float) || type == typeof(float?)) {
                    result = typeParserResolver == null
                        ? ParseFloat(text)
                        : ParseFloat(text, typeParserResolver);
                } else if(type == typeof(double) || type == typeof(double?)) {
                    result = typeParserResolver == null
                        ? ParseDouble(text)
                        : ParseDouble(text, typeParserResolver);
                } else if(type == typeof(decimal) || type == typeof(decimal?)) {
                    result = typeParserResolver == null
                        ? ParseDecimal(text)
                        : ParseDecimal(text, typeParserResolver);
                } else if(type == typeof(DateTime) || type == typeof(DateTime?)) {
                    result = typeParserResolver == null
                        ? ParseDateTime(text)
                        : ParseDateTime(text, typeParserResolver);
                } else if(type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?)) {
                    result = typeParserResolver == null
                        ? ParseDateTimeOffset(text)
                        : ParseDateTimeOffset(text, typeParserResolver);
                } else if(type == typeof(Guid) || type == typeof(Guid?)) {
                    result = typeParserResolver == null
                        ? ParseGuid(text)
                        : ParseGuid(text, typeParserResolver);
                } else if(type == typeof(byte[])) {
                    result = typeParserResolver == null
                        ? ParseByteArray(text)
                        : ParseByteArray(text, typeParserResolver);
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
