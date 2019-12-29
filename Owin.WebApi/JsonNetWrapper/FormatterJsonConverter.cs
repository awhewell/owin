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
using AWhewell.Owin.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AWhewell.Owin.WebApi.JsonNetWrapper
{
    class FormatterJsonConverter : JsonConverter
    {
        /// <summary>
        /// See base docs.
        /// </summary>
        public override bool CanRead => false;

        /// <summary>
        /// See base docs.
        /// </summary>
        public override bool CanWrite => true;

        /// <summary>
        /// The <see cref="TypeFormatterResolver"/> to use when formatting values.
        /// </summary>
        public TypeFormatterResolver TypeFormatterResolver { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="typeParserResolver"></param>
        public FormatterJsonConverter(TypeFormatterResolver typeFormatterResolver)
        {
            TypeFormatterResolver = typeFormatterResolver;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime) || objectType == typeof(DateTime?)
                || objectType == typeof(DateTimeOffset) || objectType == typeof(DateTimeOffset?)
                || objectType == typeof(byte[])
                || objectType == typeof(Guid) || objectType == typeof(Guid?);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var text = Formatter.FormatType(value, TypeFormatterResolver);
            writer.WriteValue(text);
        }
    }
}
