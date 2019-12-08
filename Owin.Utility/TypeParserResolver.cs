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
    /// Immutable class that keeps track of a set of <see cref="ITypeParser{T}"/> objects for different types.
    /// </summary>
    public class TypeParserResolver
    {
        class ParserAndType
        {
            public Type         Type;
            public ITypeParser  Parser;
        }

        /// <summary>
        /// The name of the <see cref="ITypeParser{T}"/> generic interface for type parsers.
        /// </summary>
        public const string ITypeParserGenericName = nameof(ITypeParser) + "`1";

        /// <summary>
        /// A list of resolver objects and their type.
        /// </summary>
        private List<ParserAndType> _ParserList = new List<ParserAndType>();

        /// <summary>
        /// Gets the last assigned parser for strings.
        /// </summary>
        public ITypeParser<string> StringParser { get; private set; }

        /// <summary>
        /// Gets the last assigned parser for bools.
        /// </summary>
        public ITypeParser<bool> BoolParser { get; private set; }

        /// <summary>
        /// Gets the last assigned parser for bytes.
        /// </summary>
        public ITypeParser<byte> ByteParser { get; private set; }

        /// <summary>
        /// Gets the last assigned parser for chars
        /// </summary>
        public ITypeParser<char> CharParser { get; private set; }

        /// <summary>
        /// Gets the last assigned parser for shorts.
        /// </summary>
        public ITypeParser<Int16> Int16Parser { get; private set; }

        /// <summary>
        /// Gets the last assigned parser for unsigned shorts.
        /// </summary>
        public ITypeParser<UInt16> UInt16Parser { get; private set; }

        /// <summary>
        /// Gets the last assigned parser for ints.
        /// </summary>
        public ITypeParser<Int32> Int32Parser { get; private set; }

        /// <summary>
        /// Gets the last assigned parser for unsigned ints.
        /// </summary>
        public ITypeParser<UInt32> UInt32Parser { get; private set; }

        /// <summary>
        /// Gets the last assigned parser for longs.
        /// </summary>
        public ITypeParser<Int64> Int64Parser { get; private set; }

        /// <summary>
        /// Gets the last assigned parser for unsigned longs.
        /// </summary>
        public ITypeParser<UInt64> UInt64Parser { get; private set; }

        /// <summary>
        /// Gets the last assigned parser for singles.
        /// </summary>
        public ITypeParser<float> FloatParser { get; private set; }

        /// <summary>
        /// Gets the last assigned parser for doubles.
        /// </summary>
        public ITypeParser<double> DoubleParser { get; private set; }

        /// <summary>
        /// Gets the last assigned parser for decimals.
        /// </summary>
        public ITypeParser<decimal> DecimalParser { get; private set; }

        /// <summary>
        /// Gets the last assigned parser for date-times.
        /// </summary>
        public ITypeParser<DateTime> DateTimeParser { get; private set; }

        /// <summary>
        /// Gets the last assigned parser for date-time offsets.
        /// </summary>
        public ITypeParser<DateTimeOffset> DateTimeOffsetParser { get; private set; }

        /// <summary>
        /// Gets the last assigned parser for GUIDs.
        /// </summary>
        public ITypeParser<Guid> GuidParser { get; private set; }

        /// <summary>
        /// Gets the last assigned parser for byte arrays.
        /// </summary>
        public ITypeParser<byte[]> ByteArrayParser { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public TypeParserResolver() : this((ITypeParser[])null)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="parsers">The parsers that the resolver will offer up.</param>
        public TypeParserResolver(params ITypeParser[] parsers)
        {
            if(parsers != null) {
                foreach(var parser in parsers) {
                    if(parser != null) {
                        var type = parser
                            .GetType()
                            .GetInterface(ITypeParserGenericName)
                            .GetGenericArguments()[0];
                        AddParser(_ParserList, type, parser);
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
            if(!result && obj is TypeParserResolver other) {
                result = ParsersEquals(other._ParserList.Select(r => r.Parser));
            }

            return result;
        }

        public static bool operator ==(TypeParserResolver lhs, TypeParserResolver rhs) => Object.Equals(lhs, rhs);

        public static bool operator !=(TypeParserResolver lhs, TypeParserResolver rhs) => !Object.Equals(lhs, rhs);

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            var result = 17;
            for(var i = 0;i < _ParserList.Count;++i) {
                unchecked {
                    result *= 31 + _ParserList[i].Parser.GetType().GetHashCode();
                }
            }

            return result;
        }

        /// <summary>
        /// Returns true if the parsers passed across match the parsers held by the resolver. Only types are significant,
        /// the instances and order in which they appear are not significant.
        /// </summary>
        /// <param name="parsers"></param>
        /// <returns></returns>
        public bool ParsersEquals(IEnumerable<ITypeParser> parsers)
        {
            var otherParsers = (parsers ?? new ITypeParser[0])
                .Where(r => r != null)
                .ToArray();

            var result = parsers != null && _ParserList.Count == otherParsers.Length;
            if(result) {
                var unmatchedOtherParsers = new LinkedList<Type>();
                for(var i = 0;i < otherParsers.Length;++i) {
                    unmatchedOtherParsers.AddLast(otherParsers[i].GetType());
                }

                for(var i = 0;i < _ParserList.Count;++i) {
                    var findParserType = _ParserList[i].Parser.GetType();
                    var matched = false;
                    for(var node = unmatchedOtherParsers.First;node != null;node = node.Next) {
                        matched = node.Value == findParserType;
                        if(matched) {
                            unmatchedOtherParsers.Remove(node);
                            break;
                        }
                    }
                    if(!matched) {
                        result = false;
                        break;
                    }
                }

                if(unmatchedOtherParsers.Count > 0) {
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns a collection of all parsers registered with the resolver.
        /// </summary>
        /// <returns></returns>
        public ITypeParser[] GetParsers()
        {
            return _ParserList
                .Select(r => r.Parser)
                .ToArray();
        }

        /// <summary>
        /// Returns a collection of all parsers registered with the resolver after applying overrides passed
        /// across.
        /// </summary>
        /// <param name="parsers"></param>
        /// <returns>
        /// A concatenation of registered parsers and <paramref name="parsers"/>. If there is a parser for the
        /// same Type in both sets then the parser in <paramref name="parsers"/> takes priority.
        /// </returns>
        public ITypeParser[] GetAugmentedParsers(params ITypeParser[] parsers)
        {
            var parserList = new List<ParserAndType>(_ParserList);

            if(parsers != null) {
                for(var i = 0;i < parsers.Length;++i) {
                    var parser = parsers[i];
                    if(parser != null) {
                        var type = parsers[i]
                            .GetType()
                            .GetInterface(ITypeParserGenericName)
                            .GetGenericArguments()[0];
                        AddParser(parserList, type, parser);
                    }
                }
            }

            return parserList
                .Select(r => r.Parser)
                .ToArray();
        }

        /// <summary>
        /// Returns the type parser assigned to the type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ITypeParser<T> Find<T>()
        {
            var type = typeof(T);
            for(var i = 0;i < _ParserList.Count;++i) {
                var parser = _ParserList[i];
                if(parser.Type == type) {
                    return (ITypeParser<T>)parser.Parser;
                }
            }

            return null;
        }

        private void AddParser(List<ParserAndType> parserList, Type type, ITypeParser typeParser)
        {
            var record = new ParserAndType() {
                Type =      type,
                Parser =    typeParser,
            };

            var idx = parserList.FindIndex(r => r.Type == type);
            if(idx != -1) {
                parserList[idx] = record;
            } else {
                parserList.Add(record);
            }

            if(parserList == _ParserList) {
                SetDirectAccessProperty(type, typeParser);
            }
        }

        private void SetDirectAccessProperty(Type type, ITypeParser typeParser)
        {
                 if(type == typeof(bool))           BoolParser = (ITypeParser<bool>)typeParser;
            else if(type == typeof(byte))           ByteParser = (ITypeParser<byte>)typeParser;
            else if(type == typeof(char))           CharParser = (ITypeParser<char>)typeParser;
            else if(type == typeof(Int16))          Int16Parser = (ITypeParser<Int16>)typeParser;
            else if(type == typeof(UInt16))         UInt16Parser = (ITypeParser<UInt16>)typeParser;
            else if(type == typeof(Int32))          Int32Parser = (ITypeParser<Int32>)typeParser;
            else if(type == typeof(UInt32))         UInt32Parser = (ITypeParser<UInt32>)typeParser;
            else if(type == typeof(Int64))          Int64Parser = (ITypeParser<Int64>)typeParser;
            else if(type == typeof(UInt64))         UInt64Parser = (ITypeParser<UInt64>)typeParser;
            else if(type == typeof(float))          FloatParser = (ITypeParser<float>)typeParser;
            else if(type == typeof(double))         DoubleParser = (ITypeParser<double>)typeParser;
            else if(type == typeof(decimal))        DecimalParser = (ITypeParser<decimal>)typeParser;
            else if(type == typeof(DateTime))       DateTimeParser = (ITypeParser<DateTime>)typeParser;
            else if(type == typeof(DateTimeOffset)) DateTimeOffsetParser = (ITypeParser<DateTimeOffset>)typeParser;
            else if(type == typeof(Guid))           GuidParser = (ITypeParser<Guid>)typeParser;
            else if(type == typeof(byte[]))         ByteArrayParser = (ITypeParser<byte[]>)typeParser;
            else if(type == typeof(string))         StringParser = (ITypeParser<string>)typeParser;
        }
    }
}
