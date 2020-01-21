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
using System.Text;
using System.Threading.Tasks;
using AWhewell.Owin.Interface;
using AWhewell.Owin.Interface.Host.Ram;
using AWhewell.Owin.Utility;

namespace AWhewell.Owin.Host.Ram
{
    /// <summary>
    /// Default implementation of <see cref="IHostRam"/>.
    /// </summary>
    class HostRam : IHostRam
    {
        /// <summary>
        /// The pipeline that requests will be sent down.
        /// </summary>
        private IPipeline _Pipeline;

        private string _Root = "/";
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Root
        {
            get => _Root;
            set {
                var root = value ?? "/";
                if(!root.StartsWith("/")) {
                    root = $"/{root}";
                }
                if(root.EndsWith("/") && root.Length > 1) {
                    root = root.Substring(0, root.Length - 1);
                }
                _Root = root;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int Port { get; set; } = 80;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsListening { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<RequestProcessedEventArgs> RequestProcessed;

        /// <summary>
        /// Raises <see cref="RequestProcessed"/>.
        /// </summary>
        /// <param name="requestID"></param>
        protected virtual void OnRequestProcessed(long requestID)
        {
            if(RequestProcessed != null) {
                var args = new RequestProcessedEventArgs(requestID);
                RequestProcessed?.Invoke(this, args);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public Action<IDictionary<string, object>> BeforeProcessingRequest { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="environment"></param>
        public void Initialise(IPipelineBuilder builder, IPipelineBuilderEnvironment environment)
        {
            if(builder == null) {
                throw new ArgumentNullException(nameof(builder));
            }
            if(environment == null) {
                throw new ArgumentNullException(nameof(environment));
            }
            if(_Pipeline != null) {
                throw new InvalidOperationException("You cannot initialise a host twice");
            }

            environment.Properties[ApplicationStartupKey.HostType] = "AWhewell.Owin.Host.Ram";
            environment.Properties[ApplicationStartupKey.Version] =  Constants.Version;
            _Pipeline = builder.CreatePipeline(environment);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Start()
        {
            if(_Pipeline == null) {
                throw new InvalidOperationException($"{nameof(Initialise)} must be called before {nameof(Start)} is called");
            }
            IsListening = true;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Stop()
        {
            IsListening = false;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public void ProcessRequest(IDictionary<string, object> environment)
        {
            if(environment == null) {
                throw new ArgumentNullException(nameof(environment));
            }
            if(_Pipeline == null) {
                throw new InvalidOperationException($"{nameof(Initialise)} must be called before {nameof(ProcessRequest)}");
            }
            if(IsListening) {
                BeforeProcessingRequest?.Invoke(environment);
                _Pipeline.ProcessRequest(environment).Wait();

                if(environment.TryGetValue(CustomEnvironmentKey.RequestID, out object requestIDBoxed)) {
                    var requestID = requestIDBoxed as long?;
                    if(requestID != null) {
                        OnRequestProcessed(requestID.Value);
                    }
                }
            }
        }
    }
}
