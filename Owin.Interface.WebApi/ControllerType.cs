﻿// Copyright © 2019 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Linq;
using AWhewell.Owin.Utility;

namespace AWhewell.Owin.Interface.WebApi
{
    /// <summary>
    /// Describes a controller type and information gleaned about it.
    /// </summary>
    public class ControllerType
    {
        /// <summary>
        /// Gets the controller's type.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets the type parser resolver to use for this controller. This is null if the controller does not
        /// have a <see cref="UseParserAttribute"/> attribute and there are no defaults in force.
        /// </summary>
        public TypeParserResolver TypeParserResolver { get; }

        /// <summary>
        /// Gets the type formatter resolver to use for this controller. This is null if the controller does not
        /// have a <see cref="UseFormatterAttribute"/> attribute and there are no defaults in force.
        /// </summary>
        public TypeFormatterResolver TypeFormatterResolver { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="defaultParserResolver"></param>
        /// <param name="defaultFormatterResolver"></param>
        public ControllerType(Type type, TypeParserResolver defaultParserResolver, TypeFormatterResolver defaultFormatterResolver)
        {
            Type = type;

            const bool useInheritedAttributes = true;

            var useParserAttr = Type.GetCustomAttributes(typeof(UseParserAttribute), useInheritedAttributes)
                .OfType<UseParserAttribute>()
                .FirstOrDefault();
            TypeParserResolver = UseParserAttribute.ToTypeParserResolver(useParserAttr, defaultParserResolver);

            var useFormatterAttr = Type.GetCustomAttributes(typeof(UseFormatterAttribute), useInheritedAttributes)
                .OfType<UseFormatterAttribute>()
                .FirstOrDefault();
            TypeFormatterResolver = UseFormatterAttribute.ToTypeFormatterResolver(useFormatterAttr, defaultFormatterResolver);
        }
    }
}
