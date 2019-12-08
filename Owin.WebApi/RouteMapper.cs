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
using System.IO;
using System.Linq;
using System.Net;
using AWhewell.Owin.Interface.WebApi;
using AWhewell.Owin.Utility;
using InterfaceFactory;

namespace AWhewell.Owin.WebApi
{
    /// <summary>
    /// Default implementation of <see cref="IRouteMapper"/>.
    /// </summary>
    class RouteMapper : IRouteMapper
    {
        // The routes that the mapper was initialised with. These do not change over the lifetime of the object.
        private Dictionary<long, Route> _Routes;

        // The object that builds models for us.
        private IModelBuilder _ModelBuilder;

        /// <summary>
        /// True if the mapper has been initialised.
        /// </summary>
        private bool Initialised => _Routes != null;

        private bool _AreQueryStringNamesCaseSensitive = true;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool AreQueryStringNamesCaseSensitive
        {
            get => _AreQueryStringNamesCaseSensitive;
            set {
                if(_AreQueryStringNamesCaseSensitive != value) {
                    if(Initialised) {
                        throw new InvalidOperationException($"Cannot change {nameof(AreQueryStringNamesCaseSensitive)} after initialisation");
                    }
                    _AreQueryStringNamesCaseSensitive = value;
                }
            }
        }

        private bool _AreFormNamesCaseSensitive = true;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool AreFormNamesCaseSensitive
        {
            get => _AreFormNamesCaseSensitive;
            set {
                if(_AreFormNamesCaseSensitive != value) {
                    if(Initialised) {
                        throw new InvalidOperationException($"Cannot change {nameof(AreFormNamesCaseSensitive)} after initialisation");
                    }
                    _AreFormNamesCaseSensitive = value;
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="routes"></param>
        public void Initialise(IEnumerable<Route> routes)
        {
            if(routes == null) {
                throw new ArgumentNullException();
            }
            if(Initialised) {
                throw new InvalidOperationException($"You cannot call {nameof(Initialise)} twice");
            }

            _ModelBuilder = Factory.Resolve<IModelBuilder>();

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
        /// <param name="owinEnvironment"></param>
        /// <returns></returns>
        public Route FindRouteForRequest(IDictionary<string, object> owinEnvironment)
        {
            if(owinEnvironment == null) {
                throw new ArgumentNullException(nameof(owinEnvironment));
            }

            var context = OwinContext.Create(owinEnvironment);

            var httpMethod = context.RequestHttpMethod;
            var pathParts = context.RequestPathParts;

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
        /// <param name="owinEnvironment"></param>
        /// <returns></returns>
        public RouteParameters BuildRouteParameters(Route route, IDictionary<string, object> owinEnvironment)
        {
            if(route == null) {
                throw new ArgumentNullException(nameof(route));
            }
            if(owinEnvironment == null) {
                throw new ArgumentNullException(nameof(owinEnvironment));
            }

            var context = OwinContext.Create(owinEnvironment);

            string failedValidationMessage = null;
            object[] resultParameters = null;

            if(!_Routes.ContainsKey(route.ID)) {
                failedValidationMessage = "Cannot build parameters for routes that have not been initialised";
            }

            if(failedValidationMessage == null) {
                var methodParameters = route.MethodParameters;
                resultParameters = new object[methodParameters.Length];

                var pathParts = context.RequestPathParts;

                for(var paramIdx = 0;paramIdx < methodParameters.Length && failedValidationMessage == null;++paramIdx) {
                    var methodParameter = methodParameters[paramIdx];
                    object parameterValue = null;
                    var filledParameter = false;

                    if(methodParameter.IsObject) {
                        parameterValue = _ModelBuilder.BuildModel(
                            methodParameter.ParameterType,
                            null,
                            context.RequestQueryStringDictionary(false)
                        );
                        filledParameter = true;
                    } else {
                        filledParameter = UseInjectedValueForParameter(ref parameterValue, ref failedValidationMessage, methodParameter, route, context);

                        if(!filledParameter && failedValidationMessage == null) {
                            filledParameter = ExtractParameterFromRequestPathParts(ref parameterValue, ref failedValidationMessage, methodParameter, route, pathParts);
                        }

                        if(!filledParameter && failedValidationMessage == null) {
                            filledParameter = ExtractParameterFromQueryString(ref parameterValue, methodParameter, context);
                        }
                        if(!filledParameter && failedValidationMessage == null) {
                            filledParameter = ExtractParameterFromRequestBody(ref parameterValue, methodParameter, context);
                        }
                    }

                    if(filledParameter && failedValidationMessage == null) {
                        resultParameters[paramIdx] = parameterValue;
                    } else if(failedValidationMessage == null) {
                        failedValidationMessage = $"No value was passed for the {methodParameter.Name} parameter";
                    }
                }
            }

            return new RouteParameters(
                failedValidationMessage == null ? null : new string[] { failedValidationMessage },
                resultParameters
            );
        }

        private bool UseInjectedValueForParameter(ref object parameterValue, ref string failedValidationMessage, MethodParameter methodParameter, Route route, OwinContext context)
        {
            var filled = false;

            if(methodParameter.ParameterType == typeof(IDictionary<string, object>)) {
                if(!route.Method.IsStatic) {
                    failedValidationMessage = $"Cannot inject OWIN environment into parameters to {methodParameter.Name}, it is not static. Use the OWIN environment property instead.";
                } else {
                    filled = true;
                    parameterValue = context.Environment;
                }
            }

            return filled;
        }

        private bool ExtractParameterFromRequestPathParts(ref object parameterValue, ref string failedValidationMessage, MethodParameter methodParameter, Route route, string[] pathParts)
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
                    var pathPart = pathParts[pathPartIdx];
                    parameterValue = Parser.ParseType(
                        methodParameter.ParameterType,
                        pathPart,
                        methodParameter.TypeParserResolver
                    );

                    if(parameterValue == null) {
                        failedValidationMessage = $"Could not parse '{pathPart}' as type {methodParameter.ParameterType}";
                    } else {
                        filled = true;
                    }
                }
            }

            return filled;
        }

        private bool ExtractParameterFromQueryString(ref object parameterValue, MethodParameter methodParameter, OwinContext context)
        {
            var queryStringDictionary = context.RequestQueryStringDictionary(AreQueryStringNamesCaseSensitive);

            return ExtractParameterFromQueryStringDictionary(ref parameterValue, methodParameter, queryStringDictionary);
        }

        private bool ExtractParameterFromRequestBody(ref object parameterValue, MethodParameter methodParameter, OwinContext context)
        {
            using(var streamReader = new StreamReader(context.RequestBody)) {
                var queryStringDictionary = new QueryStringDictionary(
                    streamReader.ReadToEnd(),
                    AreFormNamesCaseSensitive
                );
                return ExtractParameterFromQueryStringDictionary(ref parameterValue, methodParameter, queryStringDictionary);
            }
        }

        private bool ExtractParameterFromQueryStringDictionary(ref object parameterValue, MethodParameter methodParameter, QueryStringDictionary queryStringDictionary)
        {
            var filled = false;

            if(queryStringDictionary.TryGetValue(methodParameter.Name, out var queryStringArray)) {
                var parseSingleValue = !methodParameter.IsArray || methodParameter.IsArrayPassedAsSingleValue;

                if(parseSingleValue) {
                    parameterValue = Parser.ParseType(
                        methodParameter.ParameterType,
                        QueryStringDictionary.JoinValue(queryStringArray),
                        methodParameter.TypeParserResolver
                    );
                    filled = true;
                } else {
                    var array = Array.CreateInstance(methodParameter.ElementType, queryStringArray.Length);
                    for(var idx = 0;idx < queryStringArray.Length;++idx) {
                        array.SetValue(
                            Parser.ParseType(
                                methodParameter.ElementType,
                                queryStringArray[idx],
                                (TypeParserResolver)null
                            ),
                            idx
                        );
                    }
                    parameterValue = array;
                    filled = true;
                }
            }

            return filled;
        }
    }
}
