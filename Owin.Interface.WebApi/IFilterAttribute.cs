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
using System.Text;

namespace AWhewell.Owin.Interface.WebApi
{
    /// <summary>
    /// The interface for attributes that are called before the method is called.
    /// </summary>
    public interface IFilterAttribute
    {
        /// <summary>
        /// Returns true if the request is allowed or false if it is to be suppressed.
        /// </summary>
        /// <param name="owinEnvironment"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// If the request is suppressed then the filter is expected to set up the body and status, if
        /// required. If no status is set then Web API will automatically set it to 403 (forbidden). If
        /// you want to return 200 (OK) on a suppressed request then you must explicitly set it.
        /// </para><para>
        /// If multiple filters are in force for a given route then each filter will be called in an
        /// undefined order until either all return true or one returns false. Once one filter fails
        /// no other filter will be called, the request is immediately abandoned.
        /// </para>
        /// </remarks>
        bool AllowRequest(IDictionary<string, object> owinEnvironment);
    }
}
