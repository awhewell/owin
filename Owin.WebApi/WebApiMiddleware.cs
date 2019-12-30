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
using System.Threading.Tasks;
using InterfaceFactory;
using AWhewell.Owin.Interface.WebApi;
using AWhewell.Owin.Utility;
using System.Linq;

namespace AWhewell.Owin.WebApi
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// The default implementation of <see cref="IWebApiMiddleware"/>.
    /// </summary>
    class WebApiMiddleware : IWebApiMiddleware
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool AreQueryStringNamesCaseSensitive { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool AreFormNamesCaseSensitive { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IList<ITypeParser> DefaultParsers { get; } = new List<ITypeParser>();

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IList<ITypeFormatter> DefaultFormatters { get; } = new List<ITypeFormatter>();

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public AppFunc CreateMiddleware(AppFunc next)
        {
            var controllerFinder = Factory.Resolve<IControllerFinder>();
            controllerFinder.DefaultTypeParserResolver =    TypeParserResolverCache.Find(DefaultParsers.ToArray());
            controllerFinder.DefaultTypeFormatterResolver = TypeFormatterResolverCache.Find(DefaultFormatters.ToArray());
            var controllerTypes = controllerFinder.DiscoverControllers();

            var routeFinder = Factory.Resolve<IRouteFinder>();
            var routes = routeFinder.DiscoverRoutes(controllerTypes);

            var routeMapper = Factory.Resolve<IRouteMapper>();
            routeMapper.AreFormNamesCaseSensitive =         AreFormNamesCaseSensitive;
            routeMapper.AreQueryStringNamesCaseSensitive =  AreQueryStringNamesCaseSensitive;
            routeMapper.Initialise(routes);

            var routeCaller = Factory.Resolve<IRouteCaller>();
            var responder = Factory.Resolve<IWebApiResponder>();

            // TODO: Think about / implement exception handling
            // TODO: Permissions
            // TODO: Route object should be available to route method via environment so that route methods can use IWebApiResponder

            return async(IDictionary<string, object> environment) =>
            {
                var route = routeMapper.FindRouteForRequest(environment);
                if(route != null) {
                    var parameters = routeMapper.BuildRouteParameters(route, environment);
                    environment[EnvironmentKey.ResponseStatusCode] = 400;
                    if(parameters.IsValid) {
                        var result = routeCaller.CallRoute(environment, route, parameters);
                        responder.ReturnJsonObject(environment, route, result);
                    }
                }

                await next(environment);
            };
        }
    }
}
