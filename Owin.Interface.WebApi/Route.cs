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

namespace AWhewell.Owin.Interface.WebApi
{
    /// <summary>
    /// Describes a route on a controller.
    /// </summary>
    public class Route
    {
        /// <summary>
        /// Gets the controller that exposes the route.
        /// </summary>
        public Type Controller { get; }

        /// <summary>
        /// Gets the method that handles the route.
        /// </summary>
        public MethodInfo Method { get; }

        /// <summary>
        /// Gets the attribute that denotes the controller and method as an API endpoint.
        /// </summary>
        public RouteAttribute RouteAttribute { get; }

        /// <summary>
        /// Gets the upper-case HTTP method associated with the function. Defaults to POST. You can only have
        /// one HTTP method per entry point, if you specify 2+ then <see cref="HttpMethod"/> is an empty string
        /// and will not match anything. Always in upper-case.
        /// </summary>
        public string HttpMethod { get; }

        /// <summary>
        /// Gets the path from the route attribute split into parts. If the route attribute is unusable then
        /// this will be null.
        /// </summary>
        public PathPart[] PathParts { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="method"></param>
        /// <param name="routeAttribute"></param>
        public Route(Type controller, MethodInfo method, RouteAttribute routeAttribute)
        {
            Controller = controller;
            Method = method;
            RouteAttribute = routeAttribute;

            HttpMethod = ExtractHttpMethod(method);
            PathParts = ExtractPathParts(method, routeAttribute);
        }

        /// <summary>
        /// See <see cref="HttpMethod"/>.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private string ExtractHttpMethod(MethodInfo method)
        {
            string result = null;

            foreach(var attr in method.GetCustomAttributes(inherit: true)) {
                var candidate = "";
                if(attr is HttpDeleteAttribute)     candidate = "DELETE";
                else if(attr is HttpGetAttribute)   candidate = "GET";
                else if(attr is HttpHeadAttribute)  candidate = "HEAD";
                else if(attr is HttpPatchAttribute) candidate = "PATCH";
                else if(attr is HttpPostAttribute)  candidate = "POST";
                else if(attr is HttpPutAttribute)   candidate = "PUT";

                if(candidate != "") {
                    if(result == null) {
                        result = candidate;
                    } else {
                        result = "";
                        break;
                    }
                }
            }

            return result ?? "POST";
        }

        /// <summary>
        /// See <see cref="PathParts"/>.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="routeAttribute"></param>
        /// <returns></returns>
        private PathPart[] ExtractPathParts(MethodInfo method, RouteAttribute routeAttribute)
        {
            PathPart[] result = null;

            if(!String.IsNullOrEmpty(routeAttribute.Route)) {
                var unescaped = Uri.UnescapeDataString(routeAttribute.Route);
                result = unescaped
                    .Split('/')
                    .Select(r => PathPart.Create(r, method))
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
