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
using System.Text.RegularExpressions;

namespace Test.AWhewell.Owin
{
    /// <summary>
    /// Parses values in test attributes for types that .NET forbids in attributes.
    /// </summary>
    public static class DataRowParser
    {
        /// <summary>
        /// The regex used to parse date times.
        /// </summary>
        static Regex DateTimeRegex = new Regex(
            @"^" +
            @"(" +
                @"((?<year>\d\d\d\d)-(?<month>\d\d)-(?<day>\d\d))" +
                @"|" +
                @"(?<named>today)" +
            @")" +
            @"( " +
                @"(?<hour>\d\d):(?<minute>\d\d):(?<second>\d\d)" +
                @"(.(?<ms>\d+))?" +
            @")?" +
            @"( " +
                @"(?<kind>local|utc|unspecified)" +
            @")?" +
            @"$",
            RegexOptions.IgnoreCase
        );

        /// <summary>
        /// The regex used to parse date time offsets.
        /// </summary>
        static Regex DateTimeOffsetRegex = new Regex(
            @"^" +
            @"(" +
                @"((?<year>\d\d\d\d)-(?<month>\d\d)-(?<day>\d\d))" +
                @"|" +
                @"(?<named>today)" +
            @")" +
            @"( " +
                @"(?<hour>\d\d):(?<minute>\d\d):(?<second>\d\d)" +
                @"(.(?<ms>\d+))?" +
            @")?" +
            @"( " +
                @"(?<offset>[+|-]\d+)" +
            @")?" +
            @"$",
            RegexOptions.IgnoreCase
        );

        /// <summary>
        /// Parses a string of the form "YYYY-MM-DD|today[ HH:MM:SS[.SSS]][ +|-zzzz]" into a
        /// nullable date-time.
        /// </summary>
        /// <param name="expected">
        /// Null or a string of either YYYY-MM-DD or "today", then an optional space and the time of HH:MM:SS
        /// with optional three digit milliseconds and then an optional space and DateTimeKind of local, utc
        /// or unspecified.
        /// </param>
        /// <param name="expectedIsUtc">
        /// An optional method that is called if the DateTime parsed out of <paramref name="expected"/> is UTC.
        /// </param>
        /// <returns></returns>
        public static DateTime? DateTime(string expected, Action expectedIsUtc = null)
        {
            DateTime? result = null;

            if(expected != null) {
                var match = DateTimeRegex.Match(expected);
                if(match.Success) {
                    int groupToInt(string group, int missingValue)
                    {
                        var text = match.Groups[group].Value;
                        return text == "" ? missingValue : int.Parse(text);
                    }

                    var year =   groupToInt("year", 1);
                    var month =  groupToInt("month", 1);
                    var day =    groupToInt("day", 1);
                    var hour =   groupToInt("hour", 0);
                    var minute = groupToInt("minute", 0);
                    var second = groupToInt("second", 0);
                    var ms =     groupToInt("ms", 0);

                    switch(match.Groups["named"].Value.ToLower()) {
                        case "today":
                            var today = System.DateTime.Today;
                            year = today.Year;
                            month = today.Month;
                            day = today.Day;
                            break;
                    }

                    var kind = DateTimeKind.Unspecified;
                    switch(match.Groups["kind"].Value.ToLower()) {
                        case "local":   kind = DateTimeKind.Local; break;
                        case "utc":     kind = DateTimeKind.Utc; break;
                    }

                    result = new DateTime(year, month, day, hour, minute, second, ms, kind);

                    if(result.Value.Kind == DateTimeKind.Utc) {
                        expectedIsUtc?.Invoke();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Parses a string of the form "YYYY-MM-DD|today[ HH:MM:SS[.SSS]][ local|utc|unspecified]" into a
        /// nullable date-time offset.
        /// </summary>
        /// <param name="expected">
        /// Null or a string of either YYYY-MM-DD or "today", then an optional space and the time of HH:MM:SS
        /// with optional three digit milliseconds and then an optional space and +/- offset minutes.
        /// </param>
        /// <returns></returns>
        public static DateTimeOffset? DateTimeOffset(string expected)
        {
            DateTimeOffset? result = null;

            if(expected != null) {
                var match = DateTimeOffsetRegex.Match(expected);
                if(match.Success) {
                    int groupToInt(string group, int missingValue)
                    {
                        var text = match.Groups[group].Value;
                        return text == "" ? missingValue : int.Parse(text);
                    }

                    var year =   groupToInt("year", 1);
                    var month =  groupToInt("month", 1);
                    var day =    groupToInt("day", 1);
                    var hour =   groupToInt("hour", 0);
                    var minute = groupToInt("minute", 0);
                    var second = groupToInt("second", 0);
                    var ms =     groupToInt("ms", 0);
                    var offset = groupToInt("offset", 0);

                    var offsetHours =   offset / 100;
                    var offsetMinutes = offset % 100;

                    switch(match.Groups["named"].Value.ToLower()) {
                        case "today":
                            var today = System.DateTime.Today;
                            year = today.Year;
                            month = today.Month;
                            day = today.Day;
                            break;
                    }

                    result = new DateTimeOffset(year, month, day, hour, minute, second, ms, new TimeSpan(offsetHours, offsetMinutes, 0));
                }
            }

            return result;
        }

        /// <summary>
        /// Parses the culture invariant text into a nullable decimal.
        /// </summary>
        /// <param name="expected"></param>
        /// <returns></returns>
        public static decimal? Decimal(string expected)
        {
            decimal? result = null;

            if(!String.IsNullOrEmpty(expected)) {
                if(decimal.TryParse(expected, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)) {
                    result = parsed;
                }
            }

            return result;
        }

        /// <summary>
        /// Parses the encoding from an encoding's web name.
        /// </summary>
        /// <param name="webName"></param>
        /// <returns></returns>
        public static Encoding Encoding(string webName)
        {
            Encoding result = null;

            if(!String.IsNullOrEmpty(webName)) {
                result = System.Text.Encoding.GetEncoding(webName);
            }

            return result;
        }

        /// <summary>
        /// Parses the text into a nullable GUID.
        /// </summary>
        /// <param name="expected"></param>
        /// <returns></returns>
        public static Guid? Guid(string expected)
        {
            Guid? result = null;

            if(!String.IsNullOrEmpty(expected)) {
                if(System.Guid.TryParse(expected, out var parsed)) {
                    result = parsed;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the <paramref name="expected"/> object unchanged unless <paramref name="type"/> is one of
        /// the types that you can't use in attributes, in which case <paramref name="expected"/> is expected
        /// to be a string and it is parsed into an object of type <paramref name="type"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="expected"></param>
        /// <returns></returns>
        public static object ConvertExpected(Type type, object expected)
        {
            var result = expected;

            if(type == typeof(decimal) || type == typeof(decimal?)) {
                result = Decimal((string)expected);
            } else if(type == typeof(DateTime) || type == typeof(DateTime?)) {
                result = DateTime((string)expected);
            } else if(type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?)) {
                result = DateTimeOffset((string)expected);
            } else if(type == typeof(Encoding)) {
                result = Encoding((string)expected);
            } else if(type == typeof(Guid) || type == typeof(Guid?)) {
                result = Guid((string)expected);
            } else if(type != typeof(string) && expected?.GetType() == typeof(string)) {
                var text = (string)expected;
                if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                    type = type.GetGenericArguments()[0];
                }
                if(type != typeof(byte[])) {
                    result = Convert.ChangeType(text, type, CultureInfo.InvariantCulture);
                } else {
                    var bytes = new List<byte>();
                    for(var i = 0;i < text.Length;i += 2) {
                        bytes.Add(
                            Convert.ToByte(
                                text.Substring(i, 2),
                                16
                            )
                        );
                    }
                    result = bytes.ToArray();
                }
            }

            return result;
        }
    }
}
