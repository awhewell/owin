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
using AWhewell.Owin.Utility;

namespace AWhewell.Owin.Interface.WebApi
{
    /// <summary>
    /// Indicates which parser(s) to use for a web API parameter, method or class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter)]
    public class UseParserAttribute : Attribute
    {
        /// <summary>
        /// Gets a flag indicating that the parsers in <see cref="Parsers"/> all exist, there are no
        /// duplicates and so on.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Gets the parsers to use for the tagged parameter, method or class.
        /// </summary>
        public ITypeParser[] Parsers { get; }

        /// <summary>
        /// Set if <see cref="IsValid"/> is false, indicates the issue found at runtime when constructing the attribute.
        /// </summary>
        public string CtorErrorMessage { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="parserType"></param>
        public UseParserAttribute(params Type[] parserTypes)
        {
            var parsersList = parserTypes
                .Select(r => {
                    ITypeParser parserInstance = null;
                    try {
                        parserInstance = Activator.CreateInstance(r) as ITypeParser;
                    } catch {
                    }
                    return parserInstance;
                })
                .ToArray();

            if(parsersList.Any(r => r == null)) {
                CtorErrorMessage = $"At least one of the parser types does not implement {nameof(ITypeParser)}";
            }

            var countDistinctTypes = parsersList
                .Where(r => r != null)
                .Select(r => r
                    .GetType()
                    .GetInterface(TypeParserResolver.ITypeParserGenericName)
                    .GetGenericArguments()[0]
                )
                .Distinct();
            if(CtorErrorMessage == null && countDistinctTypes.Count() != parsersList.Length) {
                CtorErrorMessage = $"At least two of the parsers are for the same type";
            }

            CtorErrorMessage = CtorErrorMessage ?? "";
            IsValid = CtorErrorMessage == "";
            Parsers = IsValid ? parsersList : new ITypeParser[0];
        }

        /// <summary>
        /// Returns a <see cref="TypeParserResolver"/> filled with the parsers instantiated by the ctor.
        /// </summary>
        /// <returns></returns>
        public TypeParserResolver ToTypeParserResolver()
        {
            return IsValid
                ? new TypeParserResolver(Parsers)
                : null;
        }
    }
}
