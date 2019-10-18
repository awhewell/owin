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
using Owin.Interface.Host.HttpListener.HttpListenerWrapper;

namespace Owin.Host.HttpListener.HttpListenerWrapper
{
    class HttpListener : IHttpListener
    {
        /// <summary>
        /// The class that this is wrapping.
        /// </summary>
        private System.Net.HttpListener _HttpListener;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public ICollection<string> Prefixes => _HttpListener.Prefixes;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IgnoreWriteExceptions
        {
            get => _HttpListener.IgnoreWriteExceptions;
            set => _HttpListener.IgnoreWriteExceptions = value;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsListening => _HttpListener.IsListening;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public HttpListener()
        {
            _HttpListener = new System.Net.HttpListener();
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~HttpListener()
        {
            Dispose(false);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                ((IDisposable)_HttpListener).Dispose();
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Start() => _HttpListener.Start();

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Stop() => _HttpListener.Stop();

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="asyncCallback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IAsyncResult BeginGetContext(AsyncCallback asyncCallback, object state) => _HttpListener.BeginGetContext(asyncCallback, state);

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="asyncResult"></param>
        /// <returns></returns>
        public IHttpListenerContext EndGetContext(IAsyncResult asyncResult)
        {
            return new HttpListenerContextWrapper(
                _HttpListener.EndGetContext(asyncResult)
            );
        }
    }
}
