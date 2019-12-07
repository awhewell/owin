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
using System.Linq;
using System.Reflection;
using System.Threading;
using AWhewell.Owin.Utility;

namespace AWhewell.Owin.Interface.WebApi
{
    /// <summary>
    /// Describes a route on a controller.
    /// </summary>
    public class Route
    {
        /// <summary>
        /// The next value to assign to <see cref="ID"/>.
        /// </summary>
        private static long _NextID;

        /// <summary>
        /// Gets an internal identifier for the route. These stay constant over the lifetime of the route object.
        /// </summary>
        public long ID { get; }

        /// <summary>
        /// Gets the controller type that handles the route.
        /// </summary>
        public ControllerType ControllerType { get; }

        /// <summary>
        /// Gets the method that handles the route.
        /// </summary>
        public MethodInfo Method { get; }

        /// <summary>
        /// Gets all of the parameters to the method in the order that they are passed.
        /// </summary>
        public MethodParameter[] MethodParameters { get; }

        /// <summary>
        /// Gets the attribute that denotes the controller and method as an API endpoint.
        /// </summary>
        public RouteAttribute RouteAttribute { get; }

        /// <summary>
        /// Gets the HTTP method associated with the function. Defaults to POST. You can only have
        /// one HTTP method per entry point, if you specify 2+ then <see cref="HttpMethod"/> is Unknown
        /// and will not match anything.
        /// </summary>
        public HttpMethod HttpMethod { get; }

        /// <summary>
        /// Gets the path from the route attribute split into parts. If the route attribute is unusable then
        /// this will be null.
        /// </summary>
        public PathPart[] PathParts { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Route()
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="method"></param>
        /// <param name="routeAttribute"></param>
        public Route(ControllerType controller, MethodInfo method, RouteAttribute routeAttribute) : this()
        {
            ControllerType = controller;
            Method = method;
            RouteAttribute = routeAttribute;

            MethodParameters = ExtractMethodParameters(method);
            HttpMethod = ExtractHttpMethod(method);
            PathParts = ExtractPathParts(MethodParameters, routeAttribute);

            ID = Interlocked.Increment(ref _NextID);
        }

        private MethodParameter[] ExtractMethodParameters(MethodInfo method)
        {
            return method.GetParameters()
                .Select(r => new MethodParameter(r))
                .ToArray();
        }

        private HttpMethod ExtractHttpMethod(MethodInfo method)
        {
            HttpMethod? result = null;

            foreach(var attr in method.GetCustomAttributes(inherit: true)) {
                if(attr is HttpMethodAttribute methodAttribute) {
                    if(result == null) {
                        result = methodAttribute.Method;
                    } else {
                        result = HttpMethod.Unknown;
                        break;
                    }
                }
            }

            return result ?? HttpMethod.Post;
        }

        private PathPart[] ExtractPathParts(MethodParameter[] methodParameters, RouteAttribute routeAttribute)
        {
            PathPart[] result = null;

            if(!String.IsNullOrEmpty(routeAttribute.Route)) {
                var unescaped = Uri.UnescapeDataString(routeAttribute.Route);
                result = unescaped
                    .Split('/')
                    .Select(r => PathPart.Create(r, methodParameters))
                    .ToArray();
            }

            return result;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => String.Join("/", (PathParts ?? new PathPart[0]).Select(r => r.ToString()));
    }
}
