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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Owin.Interface.WebApi;

namespace Owin.WebApi
{
    /// <summary>
    /// Default implementation of <see cref="IRouteManager"/>.
    /// </summary>
    class RouteManager : IRouteManager
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="controllerTypes"></param>
        public IEnumerable<Route> DiscoverRoutes(IEnumerable<Type> controllerTypes)
        {
            if(controllerTypes == null) {
                throw new ArgumentNullException(nameof(controllerTypes));
            }

            var result = new List<Route>();

            foreach(var controllerType in controllerTypes) {
                foreach(var methodInfo in controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)) {
                    var routeAttribute = methodInfo.GetCustomAttributes().OfType<RouteAttribute>().FirstOrDefault();
                    if(routeAttribute != null) {
                        result.Add(new Route(
                            controllerType,
                            methodInfo,
                            routeAttribute
                        ));
                    }
                }
            }

            return result;
        }
    }
}