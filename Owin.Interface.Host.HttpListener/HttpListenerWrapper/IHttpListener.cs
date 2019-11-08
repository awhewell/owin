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

namespace AWhewell.Owin.Interface.Host.HttpListener.HttpListenerWrapper
{
    /// <summary>
    /// The interface for classes that wrap HttpListener. These are used by <see cref="IHostHttpListener"/>,
    /// they can be replaced by mocks to make the host testable.
    /// </summary>
    public interface IHttpListener : IDisposable
    {
        /// <summary>
        /// Gets or sets a value indicating whether write exceptions should be ignored.
        /// </summary>
        bool IgnoreWriteExceptions { get; set; }

        /// <summary>
        /// Gets a value indicating that the listener is accepting incoming requests.
        /// </summary>
        bool IsListening { get; }

        /// <summary>
        /// Gets a collection of URL prefixes that the listener will listen to.
        /// </summary>
        ICollection<string> Prefixes { get; }

        /// <summary>
        /// Starts listening for requests.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops listening for requests.
        /// </summary>
        void Stop();

        /// <summary>
        /// Waits in background for an incoming request and calls the callback when it arrives.
        /// </summary>
        /// <param name="asyncCallback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        IAsyncResult BeginGetContext(AsyncCallback asyncCallback, object state);

        /// <summary>
        /// Fetches the context for an async result returned by <see cref="BeginGetContext"/>.
        /// </summary>
        /// <param name="asyncResult"></param>
        /// <returns></returns>
        IHttpListenerContext EndGetContext(IAsyncResult asyncResult);
    }
}
