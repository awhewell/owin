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
        /// Gets a value indicating that the <see cref="Method"/> has no result.
        /// </summary>
        public bool IsVoidMethod => Method?.ReturnType == typeof(void);

        /// <summary>
        /// Gets all of the parameters to the method in the order that they are passed.
        /// </summary>
        public MethodParameter[] MethodParameters { get; }

        /// <summary>
        /// Gets all of the filter attributes that apply to this route. These are a combination of the
        /// attributes from the parent controller and the route method. Includes authorisation filters.
        /// </summary>
        public IFilterAttribute[] FilterAttributes { get; }

        /// <summary>
        /// Gets the subset of <see cref="FilterAttributes"/> that implement <see cref="IAuthorizationFilter"/>.
        /// These filters are checked first.
        /// </summary>
        public IAuthorizationFilter[] AuthorizationFilters { get; }

        /// <summary>
        /// Gets the subset of <see cref="FilterAttributes"/> that does not implement <see cref="IAuthorizationFilter"/>.
        /// These will not be called if an authorisation filter rejects the request.
        /// </summary>
        public IFilterAttribute[] OtherFilters { get; }

        /// <summary>
        /// True if the method is flagged with <see cref="AllowAnonymousAttribute"/>.
        /// </summary>
        public bool HasAllowAnonymousAttribute { get; }

        /// <summary>
        /// Gets the default type parser resolver to use for all parameters on the method. This will be null if
        /// the default parsers are to be used.
        /// </summary>
        public TypeParserResolver TypeParserResolver { get; }

        /// <summary>
        /// Gets the type formatter resolver to use when formatting responses from the method. This will be
        /// null if the default formatters are to be used.
        /// </summary>
        public TypeFormatterResolver TypeFormatterResolver { get; }

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
        /// <param name="controllerType"></param>
        /// <param name="method"></param>
        /// <param name="routeAttribute"></param>
        public Route(ControllerType controllerType, MethodInfo method, RouteAttribute routeAttribute) : this()
        {
            ControllerType = controllerType;
            Method =         method;
            RouteAttribute = routeAttribute;

            FilterAttributes =              BuildFilterAttributes(controllerType, method);
            AuthorizationFilters =          FilterAttributes.OfType<IAuthorizationFilter>().ToArray();
            OtherFilters =                  BuildOtherFilters(FilterAttributes, AuthorizationFilters);
            HasAllowAnonymousAttribute =    method.GetCustomAttributes<AllowAnonymousAttribute>(inherit: false).Any();
            TypeParserResolver =            BuildTypeParserResolver(controllerType, method);
            TypeFormatterResolver =         BuildTypeFormatterResolver(controllerType, method);
            MethodParameters =              ExtractMethodParameters(method, TypeParserResolver);
            HttpMethod =                    ExtractHttpMethod(method);
            PathParts =                     ExtractPathParts(MethodParameters, routeAttribute);

            ValidatePathParts();

            ID = Interlocked.Increment(ref _NextID);
        }

        private IFilterAttribute[] BuildFilterAttributes(ControllerType controllerType, MethodInfo method)
        {
            return controllerType.FilterAttributes
                .Concat(
                    method.GetCustomAttributes(inherit: true)
                        .OfType<IFilterAttribute>()
                )
                .ToArray();
        }

        private IFilterAttribute[] BuildOtherFilters(IFilterAttribute[] filterAttributes, IAuthorizationFilter[] authorizationFilters)
        {
            // The Except() LINQ call will remove duplicates from filterAttributes - we need to preserve them
            return filterAttributes
                .Where(r => !authorizationFilters.Contains(r))
                .ToArray();
        }

        private TypeParserResolver BuildTypeParserResolver(ControllerType controllerType, MethodInfo method)
        {
            var useParserAttribute = method
                .GetCustomAttributes(inherit: true)
                .OfType<UseParserAttribute>()
                .LastOrDefault();

            return UseParserAttribute.ToTypeParserResolver(useParserAttribute, controllerType.TypeParserResolver);
        }

        private TypeFormatterResolver BuildTypeFormatterResolver(ControllerType controllerType, MethodInfo method)
        {
            var useFormatterAttribute = method
                .GetCustomAttributes(inherit: true)
                .OfType<UseFormatterAttribute>()
                .LastOrDefault();

            return UseFormatterAttribute.ToTypeFormatterResolver(useFormatterAttribute, controllerType.TypeFormatterResolver);
        }

        private MethodParameter[] ExtractMethodParameters(MethodInfo method, TypeParserResolver methodTypeParserResolver)
        {
            return method.GetParameters()
                .Select(r => new MethodParameter(r, methodTypeParserResolver))
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
                var unescaped = OwinConvert.UrlDecode(routeAttribute.Route);
                result = unescaped
                    .Split('/')
                    .Select(r => PathPart.Create(r, methodParameters))
                    .ToArray();
            }

            return result;
        }

        private void ValidatePathParts()
        {
            foreach(var textPart in (PathParts ?? new PathPart[0]).OfType<PathPartText>()) {
                // If the text part starts with { and finishes with } then it looks
                // like they were trying to specify a PathPartParameter but got the
                // name wrong. We need to flag those up otherwise the route will
                // never fill the parameter, and could even accept the parameter as text
                if(textPart.Part.StartsWith("{") && textPart.Part.EndsWith("}")) {
                    throw new InvalidRouteException($"Fatal error - route {RouteAttribute?.Route} on {ControllerType?.Type.FullName} method {Method?.Name} has path part {textPart.Part} that does not match a parameter");
                }
            }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => String.Join("/", (PathParts ?? new PathPart[0]).Select(r => r.ToString()));
    }
}
