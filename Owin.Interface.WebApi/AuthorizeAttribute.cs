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
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using AWhewell.Owin.Utility;

namespace AWhewell.Owin.Interface.WebApi
{
    /// <summary>
    /// Can be set at the controller or method level to control access to specific
    /// principals or roles.
    /// </summary>
    /// <remarks><para>
    /// If both user and roles are set then the request will only be allowed if the
    /// request meets both conditions - i.e. the request user and role matches.
    /// </para><para>
    /// If neither role nor user are specified then anonymous requests are rejected
    /// but any authenticated user is accepted.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private string[] _Roles = new string[0];
        /// <summary>
        /// Gets the roles that are allowed access. Multiple roles can be separated with commas. Role names
        /// are not case sensitive.
        /// </summary>
        public string Roles
        {
            get => String.Join(",", _Roles);
            set => _Roles = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private string[] _Users = new string[0];
        /// <summary>
        /// Gets the users that are allowed access. Multiple users can be separated with commas. User names
        /// are not case sensitive.
        /// </summary>
        public string Users
        {
            get => String.Join(",", _Users);
            set {
                _Users = value
                    .Split(',')
                    .Select(r => r.Trim())
                    .Where(r => r != "")
                    .ToArray();
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="owinEnvironment"></param>
        /// <returns></returns>
        public bool AllowRequest(IDictionary<string, object> owinEnvironment)
        {
            var principal = owinEnvironment[CustomEnvironmentKey.Principal] as IPrincipal;
            var result = principal?.Identity?.IsAuthenticated ?? false;
            if(result) {
                if(_Users.Length > 0) {
                    result = _Users.Any(r => String.Equals(principal.Identity.Name, r, StringComparison.OrdinalIgnoreCase));
                }

                if(result && _Roles.Length > 0) {
                    result = _Roles.Any(r => principal.IsInRole(r));
                }
            }

            if(!result) {
                owinEnvironment[EnvironmentKey.ResponseStatusCode] = (int)HttpStatusCode.Unauthorized;
            }

            return result;
        }
    }
}
