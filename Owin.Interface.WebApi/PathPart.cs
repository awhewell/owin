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
using System.Text.RegularExpressions;

namespace AWhewell.Owin.Interface.WebApi
{
    /// <summary>
    /// Describes a part of a route path.
    /// </summary>
    public abstract class PathPart
    {
        /// <summary>
        /// The regular expression that extracts parameter details out of a path part.
        /// </summary>
        private static Regex ParameterRegex = new Regex(@"^{(?<name>[a-zA-Z_][a-zA-Z_0-9]*)(?<optional>\?)?}$");

        /// <summary>
        /// Gets the part without normalisation.
        /// </summary>
        public string Part { get; }

        /// <summary>
        /// Gets the part after normalisation.
        /// </summary>
        public string NormalisedPart { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="normalisedPart"></param>
        protected PathPart(string part, string normalisedPart)
        {
            Part = part;
            NormalisedPart = normalisedPart;
        }

        /// <summary>
        /// Creates a new path part.
        /// </summary>
        /// <param name="pathPart"></param>
        /// <param name="methodParameters"></param>
        /// <returns></returns>
        public static PathPart Create(string pathPart, IEnumerable<MethodParameter> methodParameters)
        {
            if(methodParameters == null) {
                throw new ArgumentNullException(nameof(methodParameters));
            }

            var part = pathPart ?? "";
            var match = ParameterRegex.Match(part);

            var parameterName = match.Success ? Normalise(match.Groups["name"].Value) : null;
            var methodParameter = parameterName == null ? null : methodParameters.FirstOrDefault(r => r.NormalisedName == parameterName);

            if(match.Success && match.Groups["optional"].Value == "?") {
                if(methodParameter != null && !methodParameter.IsOptional) {
                    // The Microsoft Web API ? suffix is only there to make it easier to port Web API
                    // code across. However, it can only be applied against optional parameters. In
                    // our world optional parameters are always optional and are the only way of marking
                    // optional parameters. If they use ? against a non-optional parameter then we cannot
                    // allow it.
                    methodParameter = null;
                }
            }

            if(parameterName == null || methodParameter == null) {
                return new PathPartText(part, Normalise(part));
            } else {
                return new PathPartParameter(
                    part,
                    Normalise(match.Groups["name"].Value),
                    methodParameter
                );
            }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Part ?? "";

        /// <summary>
        /// Normalises a path part using the same technique as used for <see cref="NormalisedPart"/>.
        /// </summary>
        /// <param name="pathPart"></param>
        /// <returns></returns>
        public static string Normalise(string pathPart)
        {
            return (pathPart ?? "").ToLower();
        }

        /// <summary>
        /// Returns true if this path part matches a request's path part.
        /// </summary>
        /// <param name="pathPart"></param>
        /// <returns></returns>
        public abstract bool MatchesRequestPathPart(string pathPart);
    }
}
