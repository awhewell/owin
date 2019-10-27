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
using Owin.Interface.WebApi;

namespace Owin.WebApi
{
    /// <summary>
    /// Default implementation of <see cref="IRouteMapper"/>.
    /// </summary>
    class RouteMapper : IRouteMapper
    {
        // The routes that the mapper was initialised with. These do not change over the lifetime of the object.
        private Route[] _Routes;

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

            _Routes = routes.ToArray();
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
            foreach(var route in _Routes) {
                if(route.HttpMethod == httpMethod && route.PathParts.Length >= requestPathParts.Length) {
                    var failedMatch = false;

                    for(var i = 0;i < route.PathParts.Length;++i) {
                        var routePathPart = route.PathParts[i];
                        var requestPathPart = requestPathParts.Length > i ? requestPathParts[i] : null;

                        if(requestPathPart == null) {
                            failedMatch = !routePathPart.IsOptional;
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
    }
}
