// Copyright © 2020 onwards, Andrew Whewell
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
using System.Security.Principal;
using AWhewell.Owin.Interface.WebApi;
using AWhewell.Owin.Utility;

namespace AWhewell.Owin.WebApi
{
    /// <summary>
    /// Default implementation of <see cref="IRouteFilter"/>.
    /// </summary>
    class RouteFilter : IRouteFilter
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="route"></param>
        /// <param name="owinEnvironment"></param>
        /// <returns></returns>
        public bool CanCallRoute(Route route, IDictionary<string, object> owinEnvironment)
        {
            var result = true;

            var isAnonymous = (owinEnvironment[EnvironmentKey.ServerUser] as IPrincipal) == null;
            if(!isAnonymous || !route.HasAllowAnonymousAttribute) {
                for(var i = 0;result && i < route.AuthorizationFilters.Length;++i) {
                    result = route.AuthorizationFilters[i].AllowRequest(owinEnvironment);
                }
            }

            if(result) {
                for(var i = 0;result && i < route.OtherFilters.Length;++i) {
                    result = route.OtherFilters[i].AllowRequest(owinEnvironment);
                }
            }

            return result;
        }
    }
}
