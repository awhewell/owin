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
using System.Text;

namespace Test.AWhewell.Owin
{
    /// <summary>
    /// A wrapped MemoryStream that can only move forward.
    /// </summary>
    /// <remarks>
    /// Useful for emulating forward-only host streams.
    /// </remarks>
    public class ForwardOnlyMemoryStream : Stream
    {
        public MemoryStream MemoryStream { get; }

        public override bool CanRead => MemoryStream.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => MemoryStream.CanWrite;

        public override long Length => MemoryStream.Length;

        public override long Position
        {
            get => MemoryStream.Position;
            set => throw new NotImplementedException();
        }

        public ForwardOnlyMemoryStream()
        {
            MemoryStream = new MemoryStream();
        }

        public ForwardOnlyMemoryStream(byte[] buffer)
        {
            MemoryStream = new MemoryStream(buffer);
        }

        public ForwardOnlyMemoryStream(byte[] buffer, bool writable)
        {
            MemoryStream = new MemoryStream(buffer, writable);
        }

        public override void Flush() => MemoryStream.Flush();

        public override int Read(byte[] buffer, int offset, int count) => MemoryStream.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count) => MemoryStream.Write(buffer, offset, count);
    }
}
