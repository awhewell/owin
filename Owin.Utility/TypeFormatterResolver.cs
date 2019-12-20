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
using System.Linq;
using System.Text;

namespace AWhewell.Owin.Utility
{
    /// <summary>
    /// Immutable class that keeps track of a set of <see cref="ITypeFormatter{T}"/> objects for different types.
    /// </summary>
    public class TypeFormatterResolver
    {
        class FormatterAndType
        {
            public Type             Type;
            public ITypeFormatter   Formatter;
        }

        /// <summary>
        /// The name of the <see cref="ITypeFormatter{T}"/> generic interface for type formatters.
        /// </summary>
        public const string ITypeFormatterGenericName = nameof(ITypeFormatter) + "`1";

        /// <summary>
        /// A list of resolver objects and their type.
        /// </summary>
        private List<FormatterAndType> _FormatterList = new List<FormatterAndType>();

        /// <summary>
        /// Gets the last assigned formatter for strings.
        /// </summary>
        public ITypeFormatter<string> StringFormatter { get; private set; }

        /// <summary>
        /// Gets the last assigned formatter for bools.
        /// </summary>
        public ITypeFormatter<bool> BoolFormatter { get; private set; }

        /// <summary>
        /// Gets the last assigned formatter for bytes.
        /// </summary>
        public ITypeFormatter<byte> ByteFormatter { get; private set; }

        /// <summary>
        /// Gets the last assigned formatter for chars
        /// </summary>
        public ITypeFormatter<char> CharFormatter { get; private set; }

        /// <summary>
        /// Gets the last assigned formatter for shorts.
        /// </summary>
        public ITypeFormatter<Int16> Int16Formatter { get; private set; }

        /// <summary>
        /// Gets the last assigned formatter for unsigned shorts.
        /// </summary>
        public ITypeFormatter<UInt16> UInt16Formatter { get; private set; }

        /// <summary>
        /// Gets the last assigned formatter for ints.
        /// </summary>
        public ITypeFormatter<Int32> Int32Formatter { get; private set; }

        /// <summary>
        /// Gets the last assigned formatter for unsigned ints.
        /// </summary>
        public ITypeFormatter<UInt32> UInt32Formatter { get; private set; }

        /// <summary>
        /// Gets the last assigned formatter for longs.
        /// </summary>
        public ITypeFormatter<Int64> Int64Formatter { get; private set; }

        /// <summary>
        /// Gets the last assigned formatter for unsigned longs.
        /// </summary>
        public ITypeFormatter<UInt64> UInt64Formatter { get; private set; }

        /// <summary>
        /// Gets the last assigned formatter for singles.
        /// </summary>
        public ITypeFormatter<float> FloatFormatter { get; private set; }

        /// <summary>
        /// Gets the last assigned formatter for doubles.
        /// </summary>
        public ITypeFormatter<double> DoubleFormatter { get; private set; }

        /// <summary>
        /// Gets the last assigned formatter for decimals.
        /// </summary>
        public ITypeFormatter<decimal> DecimalFormatter { get; private set; }

        /// <summary>
        /// Gets the last assigned formatter for date-times.
        /// </summary>
        public ITypeFormatter<DateTime> DateTimeFormatter { get; private set; }

        /// <summary>
        /// Gets the last assigned formatter for date-time offsets.
        /// </summary>
        public ITypeFormatter<DateTimeOffset> DateTimeOffsetFormatter { get; private set; }

        /// <summary>
        /// Gets the last assigned formatter for GUIDs.
        /// </summary>
        public ITypeFormatter<Guid> GuidFormatter { get; private set; }

        /// <summary>
        /// Gets the last assigned formatter for byte arrays.
        /// </summary>
        public ITypeFormatter<byte[]> ByteArrayFormatter { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public TypeFormatterResolver() : this((ITypeFormatter[])null)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="formatters">The formatters that the resolver will offer up.</param>
        public TypeFormatterResolver(params ITypeFormatter[] formatters)
        {
            if(formatters != null) {
                foreach(var formatter in formatters) {
                    if(formatter != null) {
                        var type = formatter
                            .GetType()
                            .GetInterface(ITypeFormatterGenericName)
                            .GetGenericArguments()[0];
                        AddFormatter(_FormatterList, type, formatter);
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if both resolvers have the same set of parsers by type (references
        /// can be different).
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var result = Object.ReferenceEquals(this, obj);
            if(!result && obj is TypeFormatterResolver other) {
                result = FormattersEquals(other._FormatterList.Select(r => r.Formatter));
            }

            return result;
        }

        public static bool operator ==(TypeFormatterResolver lhs, TypeFormatterResolver rhs) => Object.Equals(lhs, rhs);

        public static bool operator !=(TypeFormatterResolver lhs, TypeFormatterResolver rhs) => !Object.Equals(lhs, rhs);

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            var result = 17;
            for(var i = 0;i < _FormatterList.Count;++i) {
                unchecked {
                    result *= 31 + _FormatterList[i].Formatter.GetType().GetHashCode();
                }
            }

            return result;
        }

        /// <summary>
        /// Returns true if the formatters passed across match the formatters held by the resolver. Only types
        /// are significant, the instances and order in which they appear are not significant.
        /// </summary>
        /// <param name="formatters"></param>
        /// <returns></returns>
        public bool FormattersEquals(IEnumerable<ITypeFormatter> formatters)
        {
            var otherFormatters = (formatters ?? new ITypeFormatter[0])
                .Where(r => r != null)
                .ToArray();

            var result = formatters != null && _FormatterList.Count == otherFormatters.Length;
            if(result) {
                var unmatchedOtherFormatters = new LinkedList<Type>();
                for(var i = 0;i < otherFormatters.Length;++i) {
                    unmatchedOtherFormatters.AddLast(otherFormatters[i].GetType());
                }

                for(var i = 0;i < _FormatterList.Count;++i) {
                    var findFormatterType = _FormatterList[i].Formatter.GetType();
                    var matched = false;
                    for(var node = unmatchedOtherFormatters.First;node != null;node = node.Next) {
                        matched = node.Value == findFormatterType;
                        if(matched) {
                            unmatchedOtherFormatters.Remove(node);
                            break;
                        }
                    }
                    if(!matched) {
                        result = false;
                        break;
                    }
                }

                if(unmatchedOtherFormatters.Count > 0) {
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns a collection of all formatters registered with the resolver.
        /// </summary>
        /// <returns></returns>
        public ITypeFormatter[] GetFormatters()
        {
            return _FormatterList
                .Select(r => r.Formatter)
                .ToArray();
        }

        /// <summary>
        /// Returns a collection of all formatters registered with the resolver after applying overrides
        /// passed across.
        /// </summary>
        /// <param name="formatters"></param>
        /// <returns>
        /// A concatenation of registered formatter and <paramref name="formatters"/>. If there is a formatter
        /// for the same Type in both sets then the formatter in <paramref name="formatters"/> takes priority.
        /// </returns>
        public ITypeFormatter[] GetAugmentedFormatters(params ITypeFormatter[] formatters)
        {
            var formatterList = new List<FormatterAndType>(_FormatterList);

            if(formatters != null) {
                for(var i = 0;i < formatters.Length;++i) {
                    var formatter = formatters[i];
                    if(formatter != null) {
                        var type = formatters[i]
                            .GetType()
                            .GetInterface(ITypeFormatterGenericName)
                            .GetGenericArguments()[0];
                        AddFormatter(formatterList, type, formatter);
                    }
                }
            }

            return formatterList
                .Select(r => r.Formatter)
                .ToArray();
        }

        /// <summary>
        /// Returns the type formatter assigned to the type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ITypeFormatter<T> Find<T>()
        {
            var type = typeof(T);
            for(var i = 0;i < _FormatterList.Count;++i) {
                var parser = _FormatterList[i];
                if(parser.Type == type) {
                    return (ITypeFormatter<T>)parser.Formatter;
                }
            }

            return null;
        }

        private void AddFormatter(List<FormatterAndType> formatterList, Type type, ITypeFormatter typeFormatter)
        {
            var record = new FormatterAndType() {
                Type =      type,
                Formatter = typeFormatter,
            };

            var idx = formatterList.FindIndex(r => r.Type == type);
            if(idx != -1) {
                formatterList[idx] = record;
            } else {
                formatterList.Add(record);
            }

            if(formatterList == _FormatterList) {
                SetDirectAccessProperty(type, typeFormatter);
            }
        }

        private void SetDirectAccessProperty(Type type, ITypeFormatter typeFormatter)
        {
                 if(type == typeof(bool))           BoolFormatter = (ITypeFormatter<bool>)typeFormatter;
            else if(type == typeof(byte))           ByteFormatter = (ITypeFormatter<byte>)typeFormatter;
            else if(type == typeof(char))           CharFormatter = (ITypeFormatter<char>)typeFormatter;
            else if(type == typeof(Int16))          Int16Formatter = (ITypeFormatter<Int16>)typeFormatter;
            else if(type == typeof(UInt16))         UInt16Formatter = (ITypeFormatter<UInt16>)typeFormatter;
            else if(type == typeof(Int32))          Int32Formatter = (ITypeFormatter<Int32>)typeFormatter;
            else if(type == typeof(UInt32))         UInt32Formatter = (ITypeFormatter<UInt32>)typeFormatter;
            else if(type == typeof(Int64))          Int64Formatter = (ITypeFormatter<Int64>)typeFormatter;
            else if(type == typeof(UInt64))         UInt64Formatter = (ITypeFormatter<UInt64>)typeFormatter;
            else if(type == typeof(float))          FloatFormatter = (ITypeFormatter<float>)typeFormatter;
            else if(type == typeof(double))         DoubleFormatter = (ITypeFormatter<double>)typeFormatter;
            else if(type == typeof(decimal))        DecimalFormatter = (ITypeFormatter<decimal>)typeFormatter;
            else if(type == typeof(DateTime))       DateTimeFormatter = (ITypeFormatter<DateTime>)typeFormatter;
            else if(type == typeof(DateTimeOffset)) DateTimeOffsetFormatter = (ITypeFormatter<DateTimeOffset>)typeFormatter;
            else if(type == typeof(Guid))           GuidFormatter = (ITypeFormatter<Guid>)typeFormatter;
            else if(type == typeof(byte[]))         ByteArrayFormatter = (ITypeFormatter<byte[]>)typeFormatter;
            else if(type == typeof(string))         StringFormatter = (ITypeFormatter<string>)typeFormatter;
        }
    }
}
