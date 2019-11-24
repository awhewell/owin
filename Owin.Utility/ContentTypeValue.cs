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

namespace AWhewell.Owin.Utility
{
    /// <summary>
    /// Gets or sets the decoded value of a Content-Type header.
    /// </summary>
    public class ContentTypeValue
    {
        /// <summary>
        /// The object that will split key-value pairs for us.
        /// </summary>
        private static KeyValueParser _KeyValueParser = new KeyValueParser('=', "");

        /// <summary>
        /// Gets the MIME type of the content.
        /// </summary>
        public string MediaType { get; }

        /// <summary>
        /// Gets the charset of the content.
        /// </summary>
        public string Charset { get; }

        /// <summary>
        /// Gets the bounday for multipart content.
        /// </summary>
        public string Boundary { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="mediaType"></param>
        /// <param name="charset"></param>
        /// <param name="boundary"></param>
        public ContentTypeValue(string mediaType, string charset = null, string boundary = null)
        {
            MediaType = mediaType;
            Charset = charset;
            Boundary = boundary;
        }

        /// <summary>
        /// Returns a <see cref="ContentTypeValue"/> parsed from a header.
        /// </summary>
        /// <param name="headerValue"></param>
        /// <returns></returns>
        public static ContentTypeValue Parse(string headerValue)
        {
            ContentTypeValue result = null;

            if(headerValue != null) {
                string mediaType = null;
                string charset = null;
                string boundary = null;

                var semiColonIdx = headerValue.IndexOf(';');

                var lhs = semiColonIdx == -1 ? headerValue : headerValue.Substring(0, semiColonIdx).Trim();
                var rhs = semiColonIdx == -1 ? null        : headerValue.Substring(semiColonIdx + 1).Trim();

                mediaType = lhs.Trim();

                ExtractCharset(rhs, ref charset, ref boundary);

                result = new ContentTypeValue(mediaType, charset, boundary);
            }

            return result;
        }

        private static void ExtractCharset(string chunk, ref string charset, ref string boundary)
        {
            _KeyValueParser.Parse(chunk, out var key, out var value);

            switch(key.Trim().ToLower()) {
                case "boundary":    boundary = value.Trim(); break;
                case "charset":     charset = value.Trim(); break;
            }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if(MediaType == null) {
                return null;
            }

            var buffer = new StringBuilder(MediaType);
            if(!String.IsNullOrEmpty(Charset)) {
                buffer.Append($"; charset={Charset}");
            }
            if(!String.IsNullOrEmpty(Boundary)) {
                buffer.Append($"; boundary={Boundary}");
            }

            return buffer.ToString();
        }
    }
}
