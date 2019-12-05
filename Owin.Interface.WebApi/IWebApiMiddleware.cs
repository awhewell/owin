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
using System.Threading.Tasks;
using AWhewell.Owin.Utility;

namespace AWhewell.Owin.Interface.WebApi
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// The interface for the OWIN middleware object that implements the web API.
    /// </summary>
    public interface IWebApiMiddleware
    {
        /// <summary>
        /// Gets or sets a value indicating that query string keys must match the case of the parameter
        /// names they are assigned to. Default is false.
        /// </summary>
        /// <remarks>
        /// The spec default is that they *ARE* case sensitive. However, Microsoft usually make them
        /// case insensitive so that is probably the less surprising choice.
        /// </remarks>
        bool AreQueryStringNamesCaseSensitive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that form string keys must match the case of the parameter
        /// names they are assigned to. Default is false.
        /// </summary>
        /// <remarks>
        /// The spec default is that they *ARE* case sensitive. However, Microsoft usually make them
        /// case insensitive so that is probably the less surprising choice. Note that this only applies
        /// to URL-encoded forms. Other body formats infer their own case sensitivity.
        /// </remarks>
        bool AreFormNamesCaseSensitive { get; set; }

        /// <summary>
        /// Gets the default list of parsers to use. If no parsers are specified then by default dates are
        /// invariant culture DateTime/DateTimeOffset.TryParse (no single format) and byte arrays are hex strings.
        /// </summary>
        /// <remarks>
        /// Any defaults established here can be overridden at the class, method and parameter levels with
        /// <see cref="UseParserAttribute"/> attributes.
        /// </remarks>
        IList<ITypeParser> DefaultParsers { get; }

        /// <summary>
        /// Creates the web API middleware.
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        AppFunc CreateMiddleware(AppFunc next);
    }
}
