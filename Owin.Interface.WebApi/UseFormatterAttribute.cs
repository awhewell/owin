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
    /// Indicates which formatters(s) to use for a web API method or class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class UseFormatterAttribute : Attribute
    {
        /// <summary>
        /// Gets a flag indicating that the formatters in <see cref="Formatters"/> all exist, there are no
        /// duplicates and so on.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Gets the formatters to use for the tagged method or class.
        /// </summary>
        public ITypeFormatter[] Formatters { get; }

        /// <summary>
        /// Set if <see cref="IsValid"/> is false, indicates the issue found at runtime when constructing the attribute.
        /// </summary>
        public string CtorErrorMessage { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="formatterTypes"></param>
        public UseFormatterAttribute(params Type[] formatterTypes)
        {
            var formattersList = formatterTypes
                .Select(r => {
                    ITypeFormatter formatterInstance = null;
                    try {
                        formatterInstance = Activator.CreateInstance(r) as ITypeFormatter;
                    } catch {
                    }
                    return formatterInstance;
                })
                .ToArray();

            if(formattersList.Any(r => r == null)) {
                CtorErrorMessage = $"At least one of the formatter types does not implement {nameof(ITypeFormatter)}";
            }
            
            var countDistinctTypes = formattersList
                .Where(r => r != null)
                .Select(r => r
                    .GetType()
                    .GetInterface(TypeFormatterResolver.ITypeFormatterGenericName)
                    .GetGenericArguments()[0]
                )
                .Distinct();
            if(CtorErrorMessage == null && countDistinctTypes.Count() != formattersList.Length) {
                CtorErrorMessage = $"At least two of the formatters are for the same type";
            }

            CtorErrorMessage = CtorErrorMessage ?? "";
            IsValid = CtorErrorMessage == "";
            Formatters = IsValid ? formattersList : new ITypeFormatter[0];
        }

        /// <summary>
        /// Returns a <see cref="TypeParserResolver"/> filled with the parsers from the <see cref="UseParserAttribute"/> attribute.
        /// </summary>
        /// <param name="useFormatter"></param>
        /// <param name="defaultResolver"></param>
        /// <returns></returns>
        public static TypeFormatterResolver ToTypeFormatterResolver(UseFormatterAttribute useFormatter, TypeFormatterResolver defaultResolver)
        {
            var result = defaultResolver;

            if(useFormatter?.IsValid ?? false) {
                result = result == null
                    ? TypeFormatterResolverCache.Find(useFormatter.Formatters)
                    : TypeFormatterResolverCache.Find(result.GetAugmentedFormatters(useFormatter.Formatters));
            }

            return result;
        }
    }
}
