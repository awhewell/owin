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
using System.Reflection;
using System.Text;
using AWhewell.Owin.Utility;

namespace AWhewell.Owin.Interface.WebApi
{
    /// <summary>
    /// A cache of information about a method's parameter.
    /// </summary>
    public class MethodParameter
    {
        /// <summary>
        /// Get the parameters name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the parameter's name normalised as per <see cref="PathPart.Normalise(string)"/>.
        /// </summary>
        public string NormalisedName { get; }

        /// <summary>
        /// Gets the parameter's type.
        /// </summary>
        public Type ParameterType { get; }

        /// <summary>
        /// Gets the type of element when <see cref="ParameterType"/> is an array.
        /// </summary>
        public Type ElementType { get; }

        /// <summary>
        /// Gets a value indicating that <see cref="ParameterType"/> is an array.
        /// </summary>
        public bool IsArray { get; }

        /// <summary>
        /// Gets a value indicating that <see cref="IsArray"/> is set and the array is
        /// sent as a single string (e.g. a byte array passed as a MIME64 string). This can be overridden
        /// by tagging the parameter with <see cref="ExpectArrayAttribute"/>.
        /// </summary>
        public bool IsArrayPassedAsSingleValue { get; }

        /// <summary>
        /// Gets a value indicating that the parameter is optional.
        /// </summary>
        public bool IsOptional { get; }

        /// <summary>
        /// Gets the parameter's default value.
        /// </summary>
        public object DefaultValue { get; }

        /// <summary>
        /// Gets the <see cref="TypeParserResolver"/> to use when parsing values for this type. This
        /// is null if the default parser is to be used.
        /// </summary>
        public TypeParserResolver TypeParserResolver { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="parameterInfo"></param>
        /// <param name="defaultParserResolver"></param>
        public MethodParameter(ParameterInfo parameterInfo, TypeParserResolver defaultParserResolver)
        {
            Name =                       parameterInfo.Name;
            NormalisedName =             PathPart.Normalise(parameterInfo.Name);
            ParameterType =              parameterInfo.ParameterType;
            ElementType =                parameterInfo.ParameterType.IsArray ? parameterInfo.ParameterType.GetElementType() : null;
            IsArray =                    ElementType != null;
            IsOptional =                 parameterInfo.IsOptional;
            DefaultValue =               parameterInfo.DefaultValue;
            TypeParserResolver =         BuildTypeParserResolver(parameterInfo, defaultParserResolver);
            IsArrayPassedAsSingleValue = IsArray
                                      && ElementType == typeof(byte)
                                      && parameterInfo.GetCustomAttribute(typeof(ExpectArrayAttribute), inherit: false) == null;
        }

        private TypeParserResolver BuildTypeParserResolver(ParameterInfo parameterInfo, TypeParserResolver defaultParserResolver)
        {
            var useParserAttribute = parameterInfo
                .GetCustomAttributes(inherit: false)
                .OfType<UseParserAttribute>()
                .FirstOrDefault();

            return UseParserAttribute.ToTypeParserResolver(useParserAttribute, defaultParserResolver);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => !IsOptional ? $"{ParameterType} {Name}" : $"{ParameterType} {Name} = {DefaultValue}";
    }
}
