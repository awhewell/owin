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

namespace AWhewell.Owin.Interface
{
    /// <summary>
    /// The interface for OWIN hosts.
    /// </summary>
    public interface IHost : IDisposable
    {
        /// <summary>
        /// Gets or sets the site's path from root (excluding port and protocol).
        /// </summary>
        string Root { get; set; }

        /// <summary>
        /// Gets or sets the port that the host is listening to.
        /// </summary>
        int Port { get; set; }

        /// <summary>
        /// Gets a value indicating that the host is listening for requests.
        /// </summary>
        bool IsListening { get; }

        /// <summary>
        /// Raised after a request has finished passing through the pipeline and the response has been closed.
        /// </summary>
        /// <remarks>
        /// There are some situations where a request needs to result in a restart of the server, e.g. an API
        /// that configures the server. The application needs to defer the restart until after it is sure that
        /// a response has been sent for the request that triggered it. This event is intended to be raised
        /// after a request has been *completely* processed.
        /// </remarks>
        event EventHandler<RequestProcessedEventArgs> RequestProcessed;

        /// <summary>
        /// Initialises the host. At a minimum this should build the pipeline.
        /// </summary>
        /// <param name="builder">The builder that can be used to construct the pipeline for the host.</param>
        /// <param name="environment">The environment to use when constructing the pipeline.</param>
        /// <remarks></remarks>
        void Initialise(IPipelineBuilder builder, IPipelineBuilderEnvironment environment);

        /// <summary>
        /// Starts the host. Does nothing if called while already listening.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the host. Does nothing if called while not listening.
        /// </summary>
        void Stop();
    }
}
