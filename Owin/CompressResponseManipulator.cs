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
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using AWhewell.Owin.Interface;
using AWhewell.Owin.Utility;

namespace AWhewell.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// The default implementation of <see cref="ICompressResponseManipulator"/>.
    /// </summary>
    class CompressResponseManipulator : ICompressResponseManipulator
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool Enabled { get; set;  } = true;

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public AppFunc AppFuncBuilder(AppFunc next)
        {
            return async(IDictionary<string, object> environment) => {
                if(Enabled) {
                    var context = new OwinContext(environment);
                    var acceptEncodings = context.RequestHeadersDictionary.AcceptEncodingValues;

                    if(acceptEncodings.Any(r => r.Value == "gzip")) {
                        CompressUsingGZip(context);
                    }
                }

                await next(environment);
            };
        }

        private static void CompressUsingGZip(OwinContext context)
        {
            var responseStream = context.ResponseBody;

            using(var compressedStream = new MemoryStream()) {
                using(var gzipStream = new GZipStream(compressedStream, CompressionLevel.Optimal, leaveOpen: true)) {
                    responseStream.Position = 0;
                    responseStream.CopyTo(gzipStream);
                    gzipStream.Flush();
                }

                context.ResponseHeadersDictionary.ContentLength = compressedStream.Position;
                context.ResponseHeadersDictionary.ContentEncoding = "gzip";

                responseStream.SetLength(0);
                compressedStream.Position = 0;
                compressedStream.CopyTo(responseStream);
            }
        }
    }
}
