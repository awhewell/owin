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
using System.Net;
using AWhewell.Owin.Interface.WebApi;
using AWhewell.Owin.Utility;

namespace AWhewell.Owin.WebApi
{
    /// <summary>
    /// Default implementation of <see cref="IRouteMapper"/>.
    /// </summary>
    class RouteMapper : IRouteMapper
    {
        // The routes that the mapper was initialised with. These do not change over the lifetime of the object.
        private Dictionary<long, Route> _Routes;

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="routes"></param>
        public void Initialise(IEnumerable<Route> routes)
        {
            if(routes == null) {
                throw new ArgumentNullException();
            }
            if(_Routes != null) {
                throw new InvalidOperationException($"You cannot call {nameof(Initialise)} twice");
            }

            _Routes = new Dictionary<long, Route>();
            foreach(var route in routes) {
                if(!_Routes.ContainsKey(route.ID)) {
                    _Routes.Add(route.ID, route);
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="httpMethod"></param>
        /// <param name="pathParts"></param>
        /// <returns></returns>
        public Route FindRouteForPath(string httpMethod, string[] pathParts)
        {
            if(httpMethod == null) {
                throw new ArgumentNullException(nameof(httpMethod));
            }
            if(pathParts == null) {
                throw new ArgumentNullException(nameof(pathParts));
            }

            httpMethod = httpMethod.ToUpper();
            var requestPathParts = new string[pathParts.Length];
            for(var i = 0;i < pathParts.Length;++i) {
                requestPathParts[i] = PathPart.Normalise(pathParts[i]);
            }

            Route result = null;
            foreach(var route in _Routes.Values) {
                if(route.HttpMethod == httpMethod && route.PathParts.Length >= requestPathParts.Length) {
                    var failedMatch = false;

                    for(var i = 0;i < route.PathParts.Length;++i) {
                        var routePathPart = route.PathParts[i];
                        var pathPartParameter = routePathPart as PathPartParameter;
                        var requestPathPart = requestPathParts.Length > i ? requestPathParts[i] : null;

                        if(requestPathPart == null) {
                            failedMatch = !(pathPartParameter?.MethodParameter.IsOptional ?? false);
                        } else {
                            if(!routePathPart.MatchesRequestPathPart(requestPathPart)) {
                                failedMatch = true;
                            }
                        }
                    }

                    if(!failedMatch) {
                        result = route;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="route"></param>
        /// <param name="pathParts"></param>
        /// <param name="owinEnvironment"></param>
        /// <returns></returns>
        public object[] BuildRouteParameters(Route route, string[] pathParts, IDictionary<string, object> owinEnvironment)
        {
            if(route == null) {
                throw new ArgumentNullException(nameof(route));
            }
            if(pathParts == null) {
                throw new ArgumentNullException(nameof(pathParts));
            }
            if(owinEnvironment == null) {
                throw new ArgumentNullException(nameof(owinEnvironment));
            }

            CheckRouteIsValid(route);

            var methodParameters = route.MethodParameters;;

            var result = new object[methodParameters.Length];
            object parameterValue;
            bool filledParameter;

            for(var paramIdx = 0;paramIdx < methodParameters.Length;++paramIdx) {
                var methodParameter = methodParameters[paramIdx];
                parameterValue = null;

                filledParameter = UseInjectedValueForParameter(ref parameterValue, methodParameter, route, owinEnvironment);
                if(!filledParameter) {
                    filledParameter = ExtractParameterFromRequestPathParts(ref parameterValue, methodParameter, route, pathParts);
                }
                if(!filledParameter) {
                    filledParameter = ExtractParameterFromQueryString(ref parameterValue, methodParameter, owinEnvironment);
                }

                if(filledParameter) {
                    result[paramIdx] = parameterValue;
                }
            }

            return result;
        }

        private void CheckRouteIsValid(Route route)
        {
            if(!_Routes.ContainsKey(route.ID)) {
                throw new HttpResponseException(
                    HttpStatusCode.BadRequest,
                    $"The {route.HttpMethod} {String.Join("/", route.PathParts.Select(r => r.Part))} route is no longer being serviced"
                );
            }
        }

        private bool UseInjectedValueForParameter(ref object parameterValue, MethodParameter methodParameter, Route route, IDictionary<string, object> owinEnvironment)
        {
            var filled = false;

            if(route.Method.IsStatic && methodParameter.ParameterType == typeof(IDictionary<string, object>)) {
                filled = true;
                parameterValue = owinEnvironment;
            }

            return filled;
        }

        private bool ExtractParameterFromRequestPathParts(ref object parameterValue, MethodParameter methodParameter, Route route, string[] pathParts)
        {
            var filled = false;

            var pathPartIdx = 0;
            PathPartParameter pathPartParameter = null;
            for(;pathPartIdx < route.PathParts.Length;++pathPartIdx) {
                if(route.PathParts[pathPartIdx] is PathPartParameter candidate) {
                    if(Object.ReferenceEquals(candidate.MethodParameter, methodParameter)) {
                        pathPartParameter = candidate;
                        break;
                    }
                }
            }

            if(pathPartParameter != null) {
                if(pathParts.Length <= pathPartIdx) {
                    if(methodParameter.IsOptional) {
                        filled = true;
                        parameterValue = methodParameter.DefaultValue;
                    }
                } else {
                    filled = true;
                    var pathPart = pathParts[pathPartIdx];
                    parameterValue =
                        Parser.ParseType(
                            methodParameter.ParameterType,
                            pathPart,
                            ExpectFormatConverter.ToParserOptions(methodParameter.Expect?.ExpectFormat)
                        )
                        ??
                        throw new HttpResponseException(
                            HttpStatusCode.BadRequest,
                            $"Cannot convert from \"{pathPart}\" to {methodParameter.ParameterType}"
                        )
                    ;
                }
            }

            return filled;
        }

        private bool ExtractParameterFromQueryString(ref object parameterValue, MethodParameter methodParameter, IDictionary<string, object> owinEnvironment)
        {
            var filled = false;

            var queryStringDictionary = new QueryStringDictionary(owinEnvironment[EnvironmentKey.RequestQueryString] as string);
            var parseSingleValue = !methodParameter.IsArray
                                || (methodParameter.ElementType == typeof(byte) && methodParameter.Expect.ExpectFormat != ExpectFormat.Array);

            if(parseSingleValue) {
                parameterValue = Parser.ParseType(
                    methodParameter.ParameterType,
                    queryStringDictionary.GetValue(methodParameter.Name),
                    ExpectFormatConverter.ToParserOptions(methodParameter.Expect?.ExpectFormat)
                );
                filled = true;
            } else {
                var strings = queryStringDictionary[methodParameter.Name];
                var array = Array.CreateInstance(methodParameter.ElementType, strings.Length);
                for(var idx = 0;idx < strings.Length;++idx) {
                    array.SetValue(
                        Parser.ParseType(
                            methodParameter.ElementType,
                            strings[idx],
                            null
                        ),
                        idx
                    );
                }
                parameterValue = array;
                filled = true;
            }


            return filled;
        }
    }
}
