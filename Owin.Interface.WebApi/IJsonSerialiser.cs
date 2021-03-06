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
using AWhewell.Owin.Utility;

namespace AWhewell.Owin.Interface.WebApi
{
    /// <summary>
    /// Converts between JSON and objects.
    /// </summary>
    public interface IJsonSerialiser
    {
        /// <summary>
        /// Serialises an object hierarchy into JSON. The resolver, if supplied, is only used for
        /// types not specified in the JSON spec (dates, byte arrays and GUIDs).
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        string Serialise(object obj, TypeFormatterResolver resolver);

        /// <summary>
        /// Deserialises JSON text into an object hierarchy using the resolver passed across. The resolver
        /// is only used for types not specified in the JSON spec (dates, byte arrays and GUIDs).
        /// </summary>
        /// <param name="modelType"></param>
        /// <param name="resolver"></param>
        /// <param name="jsonText"></param>
        /// <returns></returns>
        object Deserialise(Type modelType, TypeParserResolver resolver, string jsonText);
    }
}
