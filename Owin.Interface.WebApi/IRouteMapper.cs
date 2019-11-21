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
using System.Text;

namespace AWhewell.Owin.Interface.WebApi
{
    /// <summary>
    /// The interface for objects that can map incoming requests to controllers, methods and method parameters.
    /// </summary>
    public interface IRouteMapper
    {
        /// <summary>
        /// Gets or sets a value indicating that query string keys must match the case of the parameter
        /// names they are assigned to.
        /// </summary>
        /// <remarks>
        /// Changes to this property after <see cref="Initialise"/> has been called will throw an exception.
        /// </remarks>
        bool AreQueryStringNamesCaseSensitive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that form string keys must match the case of the parameter
        /// names they are assigned to.
        /// </summary>
        /// <remarks>
        /// Changes to this property after <see cref="Initialise"/> has been called will throw an exception.
        /// </remarks>
        bool AreFormNamesCaseSensitive { get; set; }

        /// <summary>
        /// Initialises the mapper. This can only be called once.
        /// </summary>
        /// <param name="routes"></param>
        void Initialise(IEnumerable<Route> routes);

        /// <summary>
        /// Returns the single route that matches the method and path parts supplied.
        /// </summary>
        /// <param name="httpMethod"></param>
        /// <param name="pathParts"></param>
        /// <returns></returns>
        Route FindRouteForPath(string httpMethod, string[] pathParts);

        /// <summary>
        /// Returns the parameters to pass to a route method.
        /// </summary>
        /// <param name="route"></param>
        /// <param name="owinEnvironment"></param>
        /// <returns></returns>
        RouteParameters BuildRouteParameters(Route route, IDictionary<string, object> owinEnvironment);
    }
}
