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
using AWhewell.Owin.Utility.Formatters;

namespace AWhewell.Owin.Utility
{
    /// <summary>
    /// A type formatter.
    /// </summary>
    public static class Formatter
    {
        private static readonly ByteArray_HexString_Formatter       _ByteArray_HexString_Formatter = new ByteArray_HexString_Formatter();
        private static readonly ByteArray_Mime64_Formatter          _ByteArray_Mime64_Formatter = new ByteArray_Mime64_Formatter();
        private static readonly DateTime_Iso8601_Formatter          _DateTime_Iso8601_Formatter = new DateTime_Iso8601_Formatter();
        private static readonly DateTimeOffset_Iso8601_Formatter    _DateTimeOffset_Iso8601_Formatter = new DateTimeOffset_Iso8601_Formatter();

        /// <summary>
        /// Returns text describing the bool.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FormatBool(bool? value)
        {
            return value == null
                ? null
                : value.Value
                    ? "true"
                    : "false";
        }

        /// <summary>
        /// Returns text describing the bool.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static string FormatBool(bool? value, TypeFormatterResolver resolver)
        {
            var formatter = resolver?.BoolFormatter;
            return formatter == null || value == null
                ? FormatBool(value)
                : formatter.Format(value.Value);
        }

        /// <summary>
        /// Returns the text passed across.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string FormatString(string text) => text;

        /// <summary>
        /// Returns the text passed across unless <paramref name="resolver"/> is supplied, in which case
        /// it is passed through the resolver.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static string FormatString(string text, TypeFormatterResolver resolver)
        {
            var formatter = resolver?.StringFormatter;
            return formatter == null || text == null
                ? text
                : formatter.Format(text);
        }

        /// <summary>
        /// Returns text describing the byte.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FormatByte(byte? value) => value?.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Returns text describing the byte.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static string FormatByte(byte? value, TypeFormatterResolver resolver)
        {
            var formatter = resolver?.ByteFormatter;
            return formatter == null || value == null
                ? FormatByte(value)
                : formatter.Format(value.Value);
        }

        /// <summary>
        /// Returns text describing the char.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FormatChar(char? value) => value?.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Returns text describing the char.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static string FormatChar(char? value, TypeFormatterResolver resolver)
        {
            var formatter = resolver?.CharFormatter;
            return formatter == null || value == null
                ? FormatChar(value)
                : formatter.Format(value.Value);
        }

        /// <summary>
        /// Returns text describing the short int.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FormatInt16(Int16? value) => value?.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Returns text describing the short int.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static string FormatInt16(Int16? value, TypeFormatterResolver resolver)
        {
            var formatter = resolver?.Int16Formatter;
            return formatter == null || value == null
                ? FormatInt16(value)
                : formatter.Format(value.Value);
        }

        /// <summary>
        /// Returns text describing the unsigned short int.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FormatUInt16(UInt16? value) => value?.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Returns text describing the unsigned short int.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static string FormatUInt16(UInt16? value, TypeFormatterResolver resolver)
        {
            var formatter = resolver?.UInt16Formatter;
            return formatter == null || value == null
                ? FormatUInt16(value)
                : formatter.Format(value.Value);
        }

        /// <summary>
        /// Returns text describing the int.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FormatInt32(Int32? value) => value?.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Returns text describing the int.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static string FormatInt32(Int32? value, TypeFormatterResolver resolver)
        {
            var formatter = resolver?.Int32Formatter;
            return formatter == null || value == null
                ? FormatInt32(value)
                : formatter.Format(value.Value);
        }

        /// <summary>
        /// Returns text describing the unsigned int.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FormatUInt32(UInt32? value) => value?.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Returns text describing the unsigned int.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static string FormatUInt32(UInt32? value, TypeFormatterResolver resolver)
        {
            var formatter = resolver?.UInt32Formatter;
            return formatter == null || value == null
                ? FormatUInt32(value)
                : formatter.Format(value.Value);
        }

        /// <summary>
        /// Returns text describing the long int.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FormatInt64(Int64? value) => value?.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Returns text describing the long int.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static string FormatInt64(Int64? value, TypeFormatterResolver resolver)
        {
            var formatter = resolver?.Int64Formatter;
            return formatter == null || value == null
                ? FormatInt64(value)
                : formatter.Format(value.Value);
        }

        /// <summary>
        /// Returns text describing the unsigned long int.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FormatUInt64(UInt64? value) => value?.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Returns text describing the unsigned long int.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static string FormatUInt64(UInt64? value, TypeFormatterResolver resolver)
        {
            var formatter = resolver?.UInt64Formatter;
            return formatter == null || value == null
                ? FormatUInt64(value)
                : formatter.Format(value.Value);
        }

        /// <summary>
        /// Returns text describing the float.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FormatFloat(float? value) => value?.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Returns text describing the float.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static string FormatFloat(float? value, TypeFormatterResolver resolver)
        {
            var formatter = resolver?.FloatFormatter;
            return formatter == null || value == null
                ? FormatFloat(value)
                : formatter.Format(value.Value);
        }

        /// <summary>
        /// Returns text describing the double.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FormatDouble(double? value) => value?.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Returns text describing the double.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static string FormatDouble(double? value, TypeFormatterResolver resolver)
        {
            var formatter = resolver?.DoubleFormatter;
            return formatter == null || value == null
                ? FormatDouble(value)
                : formatter.Format(value.Value);
        }

        /// <summary>
        /// Returns text describing the decimal.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FormatDecimal(decimal? value) => value?.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Returns text describing the decimal.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static string FormatDecimal(decimal? value, TypeFormatterResolver resolver)
        {
            var formatter = resolver?.DecimalFormatter;
            return formatter == null || value == null
                ? FormatDecimal(value)
                : formatter.Format(value.Value);
        }

        /// <summary>
        /// Returns text describing the DateTime.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FormatDateTime(DateTime? value) => value == null ? null : _DateTime_Iso8601_Formatter.Format(value.Value);

        /// <summary>
        /// Returns text describing the DateTime.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static string FormatDateTime(DateTime? value, TypeFormatterResolver resolver)
        {
            if(value == null) {
                return null;
            } else {
                var formatter = resolver?.DateTimeFormatter;
                return formatter == null
                    ? _DateTime_Iso8601_Formatter.Format(value.Value)
                    : formatter.Format(value.Value);
            }
        }

        /// <summary>
        /// Returns text describing the DateTimeOffset.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FormatDateTimeOffset(DateTimeOffset? value) => value == null ? null : _DateTimeOffset_Iso8601_Formatter.Format(value.Value);

        /// <summary>
        /// Returns text describing the DateTimeOffset.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static string FormatDateTimeOffset(DateTimeOffset? value, TypeFormatterResolver resolver)
        {
            if(value == null) {
                return null;
            } else {
                var formatter = resolver?.DateTimeOffsetFormatter;
                return formatter == null
                    ? _DateTimeOffset_Iso8601_Formatter.Format(value.Value)
                    : formatter.Format(value.Value);
            }
        }

        /// <summary>
        /// Returns text describing the GUID.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FormatGuid(Guid? value) => value?.ToString();

        /// <summary>
        /// Returns text describing the GUID.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static string FormatGuid(Guid? value, TypeFormatterResolver resolver)
        {
            var formatter = resolver?.GuidFormatter;
            return formatter == null || value == null
                ? FormatGuid(value)
                : formatter.Format(value.Value);
        }

        /// <summary>
        /// Returns text describing the byte array.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FormatByteArray(byte[] value) => _ByteArray_Mime64_Formatter.Format(value);

        /// <summary>
        /// Returns text describing the byte array.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public static string FormatByteArray(byte[] value, TypeFormatterResolver resolver)
        {
            var formatter = resolver?.ByteArrayFormatter;
            return formatter == null
                ? _ByteArray_Mime64_Formatter.Format(value)
                : formatter.Format(value);
        }

        /// <summary>
        /// Returns text describing the byte array as a hex string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FormatHexBytes(byte[] value) => _ByteArray_HexString_Formatter.Format(value);

        /// <summary>
        /// Returns text describing the byte array as a MIME64 string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FormatMime64Bytes(byte[] value) => FormatByteArray(value);

        /// <summary>
        /// Returns text describing the <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FormatType(object value) => FormatType(value, null);

        /// <summary>
        /// Returns text describing the <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="typeFormatterResolver"></param>
        /// <returns></returns>
        public static string FormatType(object value, TypeFormatterResolver typeFormatterResolver)
        {
            string result = null;

            if(value != null) {
                var type = value.GetType();
                if(type == typeof(string)) {
                    result = typeFormatterResolver == null
                        ? FormatString((string)value)
                        : FormatString((string)value, typeFormatterResolver);
                } else if(value != null) {
                    if(type == typeof(bool) || type == typeof(bool?)) {
                        result = typeFormatterResolver == null
                            ? FormatBool((bool?)value)
                            : FormatBool((bool?)value, typeFormatterResolver);
                    } else if(type == typeof(byte) || type == typeof(byte?)) {
                        result = typeFormatterResolver == null
                            ? FormatByte((byte?)value)
                            : FormatByte((byte?)value, typeFormatterResolver);
                    } else if(type == typeof(char) || type == typeof(char?)) {
                        result = typeFormatterResolver == null
                            ? FormatChar((char?)value)
                            : FormatChar((char?)value, typeFormatterResolver);
                    } else if(type == typeof(Int16) || type == typeof(Int16?)) {
                        result = typeFormatterResolver == null
                            ? FormatInt16((Int16?)value)
                            : FormatInt16((Int16?)value, typeFormatterResolver);
                    } else if(type == typeof(UInt16) || type == typeof(UInt16?)) {
                        result = typeFormatterResolver == null
                            ? FormatUInt16((UInt16?)value)
                            : FormatUInt16((UInt16?)value, typeFormatterResolver);
                    } else if(type == typeof(Int32) || type == typeof(Int32?)) {
                        result = typeFormatterResolver == null
                            ? FormatInt32((Int32?)value)
                            : FormatInt32((Int32?)value, typeFormatterResolver);
                    } else if(type == typeof(UInt32) || type == typeof(UInt32?)) {
                        result = typeFormatterResolver == null
                            ? FormatUInt32((UInt32?)value)
                            : FormatUInt32((UInt32?)value, typeFormatterResolver);
                    } else if(type == typeof(Int64) || type == typeof(Int64?)) {
                        result = typeFormatterResolver == null
                            ? FormatInt64((Int64?)value)
                            : FormatInt64((Int64?)value, typeFormatterResolver);
                    } else if(type == typeof(UInt64) || type == typeof(UInt64?)) {
                        result = typeFormatterResolver == null
                            ? FormatUInt64((UInt64?)value)
                            : FormatUInt64((UInt64?)value, typeFormatterResolver);
                    } else if(type == typeof(float) || type == typeof(float?)) {
                        result = typeFormatterResolver == null
                            ? FormatFloat((float?)value)
                            : FormatFloat((float?)value, typeFormatterResolver);
                    } else if(type == typeof(double) || type == typeof(double?)) {
                        result = typeFormatterResolver == null
                            ? FormatDouble((double?)value)
                            : FormatDouble((double?)value, typeFormatterResolver);
                    } else if(type == typeof(decimal) || type == typeof(decimal?)) {
                        result = typeFormatterResolver == null
                            ? FormatDecimal((decimal?)value)
                            : FormatDecimal((decimal?)value, typeFormatterResolver);
                    } else if(type == typeof(DateTime) || type == typeof(DateTime?)) {
                        result = typeFormatterResolver == null
                            ? FormatDateTime((DateTime?)value)
                            : FormatDateTime((DateTime?)value, typeFormatterResolver);
                    } else if(type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?)) {
                        result = typeFormatterResolver == null
                            ? FormatDateTimeOffset((DateTimeOffset?)value)
                            : FormatDateTimeOffset((DateTimeOffset?)value, typeFormatterResolver);
                    } else if(type == typeof(Guid) || type == typeof(Guid?)) {
                        result = typeFormatterResolver == null
                            ? FormatGuid((Guid?)value)
                            : FormatGuid((Guid?)value, typeFormatterResolver);
                    } else if(type == typeof(byte[])) {
                        result = typeFormatterResolver == null
                            ? FormatByteArray((byte[])value)
                            : FormatByteArray((byte[])value, typeFormatterResolver);
                    } else {
                        // TODO: need test for this
                        result = value.ToString();
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
        public static HttpMethod FormatHttpMethod(string httpMethod)
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
        public static HttpProtocol FormatHttpProtocol(string httpProtocol)
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
        public static HttpScheme FormatHttpScheme(string httpScheme)
        {
            switch((httpScheme ?? "").Trim().ToLower()) {
                case "http":    return HttpScheme.Http;
                case "https":   return HttpScheme.Https;
                default:        return HttpScheme.Unknown;
            }
        }

        /// <summary>
        /// Returns the enum associated with the media type passed across.
        /// </summary>
        /// <param name="mediaType"></param>
        /// <returns></returns>
        public static MediaType FormatMediaType(string mediaType)
        {
            switch((mediaType ?? "").Trim().ToLower()) {
                case "application/javascript":              return MediaType.JavaScript;
                case "application/json":                    return MediaType.Json;
                case "multipart/form-data":                 return MediaType.MultipartForm;
                case "text/plain":                          return MediaType.PlainText;
                case "application/x-www-form-urlencoded":   return MediaType.UrlEncodedForm;
                case "application/xml":
                case "text/xml":                            return MediaType.Xml;
                default:                                    return MediaType.Unknown;
            }
        }

        /// <summary>
        /// Returns the .NET encoding for a charset's name or null if the charset is not recognised. If the
        /// charset is null or empty then UTF-8 is returned.
        /// </summary>
        /// <param name="charset"></param>
        /// <returns></returns>
        public static Encoding FormatCharset(string charset)
        {
            Encoding result = Encoding.UTF8;

            if(!String.IsNullOrEmpty(charset)) {
                try {
                    switch(charset.ToLower()) {
                        case "csutf7":      charset = "utf-7"; break;
                        case "csutf8":      charset = "utf-8"; break;
                        case "csutf16":     charset = "utf-16"; break;
                        case "csutf16be":   charset = "utf-16be"; break;
                        case "csutf16le":   charset = "utf-16le"; break;
                        case "csutf32":     charset = "utf-32"; break;
                        case "csutf32be":   charset = "utf-32be"; break;
                        case "csutf32le":   charset = "utf-32le"; break;
                    }

                    result = Encoding.GetEncoding(charset);
                } catch(ArgumentException) {
                    result = null;
                }
            }

            return result;
        }
    }
}
