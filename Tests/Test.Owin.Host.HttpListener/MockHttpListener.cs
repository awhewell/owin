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
using Moq;
using AWhewell.Owin.Interface.Host.HttpListener.HttpListenerWrapper;

namespace Test.AWhewell.Owin.Host.HttpListener
{
    class MockHttpListener : Mock<IHttpListener>
    {
        public List<string> Prefixes { get; } = new List<string>();

        public MockHttpListenerContext MockContext { get; set; } = new MockHttpListenerContext();

        public Mock<IAsyncResult> MockAsyncResult { get; } = new Mock<IAsyncResult>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();

        public bool BeginContextTriggersCallback { get; set; } = true;

        public Action BeginContextAction { get; set; }

        public Action EndGetContextAction { get; set; }

        public int BeginContextCallCount { get; private set; }

        public MockHttpListener() : base()
        {
            DefaultValue = DefaultValue.Mock;
            SetupAllProperties();

            Setup(r => r.Prefixes).Returns(Prefixes);
            Setup(r => r.Start()).Callback(() => SetupGet(r => r.IsListening).Returns(true));
            Setup(r => r.Stop()).Callback(() =>  SetupGet(r => r.IsListening).Returns(false));

            Setup(r => r.BeginGetContext(It.IsAny<AsyncCallback>(), It.IsAny<object>()))
                .Callback((AsyncCallback callback, object state) => {
                    ++BeginContextCallCount;
                    BeginContextAction?.Invoke();
                    if(BeginContextTriggersCallback) {
                        BeginContextTriggersCallback = false;
                        callback(MockAsyncResult.Object);
                    }
                });

            Setup(r => r.EndGetContext(MockAsyncResult.Object))
                .Callback((IAsyncResult unused) => EndGetContextAction?.Invoke())
                .Returns((IAsyncResult unused) => MockContext.Object);
        }
    }
}
