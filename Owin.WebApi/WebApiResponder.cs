﻿// Copyright © 2019 onwards, Andrew Whewell
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
using AWhewell.Owin.Interface.WebApi;
using AWhewell.Owin.Utility;
using InterfaceFactory;

namespace AWhewell.Owin.WebApi
{
    /// <summary>
    /// The default implementation of <see cref="IWebApiResponder"/>.
    /// </summary>
    class WebApiResponder : IWebApiResponder
    {
        /// <summary>
        /// The object that does all the serialisation to JSON for us.
        /// </summary>
        private IJsonSerialiser _JsonSerialiser;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public WebApiResponder()
        {
            _JsonSerialiser = Factory.Resolve<IJsonSerialiser>();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="owinEnvironment"></param>
        /// <param name="route"></param>
        /// <param name="obj"></param>
        public void ReturnJsonObject(IDictionary<string, object> owinEnvironment, Route route, object obj)
        {
            if(!route.IsVoidMethod) {
                var jsonText = _JsonSerialiser.Serialise(obj, route.TypeFormatterResolver);

                var context = OwinContext.Create(owinEnvironment);
                context.ReturnText(
                    jsonText,
                    Encoding.UTF8,
                    Formatter.FormatMediaType(MediaType.Json)
                );
            }
        }
    }
}
