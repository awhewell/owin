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
using System.Threading.Tasks;

namespace AWhewell.Owin.Interface
{
    /// <summary>
    /// Describes a pipeline that can process web requests.
    /// </summary>
    public interface IPipeline
    {
        /// <summary>
        /// Gets an array of objects that can log exceptions.
        /// </summary>
        IReadOnlyList<IExceptionLogger> ExceptionLoggers { get; }

        /// <summary>
        /// True if the pipeline contains stream manipulators, i.e. middleware functions
        /// that always run after normal middleware has finished.
        /// </summary>
        bool HasStreamManipulators { get; }

        /// <summary>
        /// Builds the pipeline. This can only be called once.
        /// </summary>
        /// <param name="buildEnvironment"></param>
        void Construct(IPipelineBuilderEnvironment buildEnvironment);

        /// <summary>
        /// Processes a request. Note that exceptions thrown within the pipeline are not logged, they are
        /// allowed to bubble up out of the function.
        /// </summary>
        /// <param name="environment"></param>
        Task ProcessRequest(IDictionary<string, object> environment);

        /// <summary>
        /// Logs an exception.
        /// </summary>
        /// <param name="ex"></param>
        /// <remarks>
        /// Each logger is guaranteed to be called even if previous loggers themselves threw exceptions.
        /// Exceptions thrown by a logger do not bubble out of this method.
        /// </remarks>
        void LogException(Exception ex);

        /// <summary>
        /// Logs an exception when the request URL is known.
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="ex"></param>
        /// <remarks>
        /// Each logger is guaranteed to be called even if previous loggers themselves threw exceptions.
        /// Exceptions thrown by a logger do not bubble out of this method.
        /// </remarks>
        void LogException(string requestUrl, Exception ex);
    }
}
