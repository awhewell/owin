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
using System.IO;
using System.IO.Compression;
using System.Text;
using AWhewell.Owin.Interface;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin
{
    [TestClass]
    public class CompressResponseManipulator_Tests
    {
        private ICompressResponseManipulator    _Manipulator;
        private MockPipeline                    _Pipeline;
        private MockOwinEnvironment             _Environment;

        private const string                    _LongText = "This is a lump of text that should be long enough to produce a compressed " +
                                                            "stream that is longer than the original text. It needs to be fairly long, " +
                                                            "otherwise it might not be compressed by the GZipStream.";

        [TestInitialize]
        public void TestInitialise()
        {
            _Pipeline = new MockPipeline();
            _Environment = new MockOwinEnvironment();
            _Environment.AddRequiredFields();

            _Manipulator = Factory.Resolve<ICompressResponseManipulator>();
        }

        private byte[] ExpectedPlaintextBody(string textBody, Encoding bodyEncoding = null) => (bodyEncoding ?? Encoding.UTF8).GetBytes(textBody);

        private byte[] ExpectedGZipBody(string textBody, Encoding bodyEncoding = null)
        {
            var plainTextBytes = ExpectedPlaintextBody(textBody, bodyEncoding);

            using(var compressedStream = new MemoryStream()) {
                using(var gzipStream = new GZipStream(compressedStream, CompressionLevel.Optimal)) {
                    gzipStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    gzipStream.Flush();
                }
                return compressedStream.ToArray();
            }
        }

        private byte[] ExpectedDeflateBody(string textBody, Encoding bodyEncoding = null)
        {
            var plainTextBytes = ExpectedPlaintextBody(textBody, bodyEncoding);

            using(var compressedStream = new MemoryStream()) {
                using(var deflateStream = new DeflateStream(compressedStream, CompressionLevel.Optimal)) {
                    deflateStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    deflateStream.Flush();
                }
                return compressedStream.ToArray();
            }
        }

        [TestMethod]
        public void Enabled_Defaults_To_True()
        {
            Assert.IsTrue(_Manipulator.Enabled);
        }

        [TestMethod]
        [DataRow(true,  _LongText, "text/plain", "utf-7", null,                            null)]
        [DataRow(true,  _LongText, "text/plain", "utf-7", "gzip",                          "gzip")]
        [DataRow(true,  _LongText, "text/plain", "utf-7", "deflate",                       "deflate")]
        [DataRow(true,  _LongText, "text/plain", "utf-7", "br",                            null)]
        [DataRow(true,  _LongText, "text/plain", "utf-7", "*",                             null)]
        [DataRow(true,  _LongText, "text/plain", "utf-7", "gzip, deflate",                 "gzip")]
        [DataRow(true,  _LongText, "text/plain", "utf-7", "deflate, gzip",                 "deflate")]
        [DataRow(true,  _LongText, "text/plain", "utf-7", "deflate, gzip;q=1.0, *;q=0.5",  "deflate")]
        [DataRow(true,  _LongText, "text/plain", "utf-7", "deflate;q=0.5, gzip;q=0.55",    "gzip")]
        [DataRow(true,  _LongText, "text/plain", "utf-7", "deflate;q=0.55, gzip;q=0.5",    "deflate")]
        [DataRow(false, _LongText, "text/plain", "utf-7", "gzip",                          null)]
        public void AppFunc_Compresses_To_GZip_Body_If_Requested(bool enabled, string plaintext, string mimeType, string encodingName, string acceptEncoding, string expectedContentEncoding)
        {
            var encoding = Encoding.GetEncoding(encodingName);
            _Environment.AddResponseText(plaintext, mimeType, encoding);
            if(acceptEncoding != null) {
                _Environment.RequestHeaders["Accept-Encoding"] = acceptEncoding;
            }

            byte[] expectedBody;
            switch(expectedContentEncoding) {
                case null:      expectedBody = ExpectedPlaintextBody(plaintext, encoding); break;
                case "deflate": expectedBody = ExpectedDeflateBody(plaintext, encoding); break;
                case "gzip":    expectedBody = ExpectedGZipBody(plaintext, encoding); break;
                default:        throw new NotImplementedException();
            }

            _Manipulator.Enabled = enabled;
            _Pipeline.BuildAndCallMiddleware(_Manipulator.AppFuncBuilder, _Environment);

            Assertions.AreEqual(expectedBody,           _Environment.ResponseBodyBytes);
            Assert.AreEqual(mimeType,                   _Environment.ResponseHeadersDictionary.ContentTypeValue.MediaType);
            Assert.AreEqual(encoding.WebName,           _Environment.ResponseHeadersDictionary.ContentTypeValue.Charset);
            Assert.AreEqual(expectedBody.Length,        _Environment.ResponseHeadersDictionary.ContentLength);
            Assert.AreEqual(expectedContentEncoding,    _Environment.ResponseHeadersDictionary.ContentEncoding);
        }

        [TestMethod]
        [DataRow("application/x-7z-compressed",     false)]
        [DataRow("application/x-bzip",              false)]
        [DataRow("application/x-bzip2",             false)]
        [DataRow("application/x-rar-compressed",    false)]
        [DataRow("application/zip",                 false)]
        [DataRow("audio/mp4",                       false)]
        [DataRow("audio/almost-anything",           false)]
        [DataRow("audio/x-wav",                     true)]
        [DataRow("image/png",                       false)]
        [DataRow("image/almost-anything",           false)]
        [DataRow("image/bmp",                       true)]
        [DataRow("image/svg+xml",                   true)]
        [DataRow("video/anything",                  false)]
        public void AppFunc_Does_Not_Compress_Known_Compressed_Streams(string mimeType, bool expectCompression)
        {
            var plainText = _LongText;
            var expectedBody = expectCompression ? ExpectedGZipBody(plainText) : ExpectedPlaintextBody(plainText);
            _Environment.AddResponseText(plainText, mimeType, Encoding.UTF8);
            _Environment.RequestHeaders["Accept-Encoding"] = "gzip";

            _Pipeline.BuildAndCallMiddleware(_Manipulator.AppFuncBuilder, _Environment);

            Assertions.AreEqual(expectedBody, _Environment.ResponseBodyBytes);
        }
    }
}
