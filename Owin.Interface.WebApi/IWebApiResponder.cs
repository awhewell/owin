// Copyright © 2019 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Collections.Generic;
using System.Text;
using AWhewell.Owin.Utility;

namespace AWhewell.Owin.Interface.WebApi
{
    /// <summary>
    /// The interface for type-safe objects that send web API responses back to the client.
    /// </summary>
    public interface IWebApiResponder
    {
        /// <summary>
        /// Sets up the environment to return the object passed across. The format of the response is always
        /// JSON. The formatters attached to the route stored in the environment are used to format dates etc.
        /// </summary>
        /// <param name="owinEnvironment">The OWIN environment to set up with the response.</param>
        /// <param name="obj">The object to return.</param>
        void ReturnJsonObject(IDictionary<string, object> owinEnvironment, object obj);

        /// <summary>
        /// Sets up the environment to return the object passed across. The format of the response is always
        /// JSON. The formatters attached to the route stored in the environment are used to format dates etc.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="obj"></param>
        void ReturnJsonObject(OwinContext context, object obj);

        /// <summary>
        /// Sets up the environment to return the object passed across.
        /// </summary>
        /// <param name="owinEnvironment">The OWIN environment to set up with the response.</param>
        /// <param name="obj">The object to return.</param>
        /// <param name="resolver">
        /// The resolver to use when formatting the object. Formatters associated with the route in the
        /// environment (if any) are ignored.
        /// </param>
        /// <param name="encoding">The encoding to use when formatting the JSON.</param>
        /// <param name="mimeType">
        /// The mime type to use. If this is null or empty then it defaults to application/json.
        /// </param>
        void ReturnJsonObject(IDictionary<string, object> owinEnvironment, object obj, TypeFormatterResolver resolver, Encoding encoding, string mimeType);

        /// <summary>
        /// Sets up the environment to return the object passed across.
        /// </summary>
        /// <param name="context">The OWIN environment to set up with the response.</param>
        /// <param name="obj">The object to return.</param>
        /// <param name="resolver">
        /// The resolver to use when formatting the object. Formatters associated with the route in the
        /// environment (if any) are ignored.
        /// </param>
        /// <param name="encoding">The encoding to use when formatting the JSON.</param>
        /// <param name="mimeType">
        /// The mime type to use. If this is null or empty then it defaults to application/json.
        /// </param>
        void ReturnJsonObject(OwinContext context, object obj, TypeFormatterResolver resolver, Encoding encoding, string mimeType);
    }
}
