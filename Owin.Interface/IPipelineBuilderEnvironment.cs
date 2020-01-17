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
using AWhewell.Owin.Utility;

namespace AWhewell.Owin.Interface
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// The environment used to construct a new instance of the pipeline. You get one of these for every
    /// new pipeline that is constructed.
    /// </summary>
    public interface IPipelineBuilderEnvironment
    {
        /// <summary>
        /// Gets a dictionary of properties set by the host and middleware that describes the environment
        /// that the pipeline is being built for.
        /// </summary>
        OwinDictionary<object> Properties { get; }

        /// <summary>
        /// Gets a read-only collection of all middleware builders added via <see cref="UseMiddlewareBuilder"/> thus far.
        /// </summary>
        IReadOnlyList<Func<AppFunc, AppFunc>> MiddlewareBuilders { get; }

        /// <summary>
        /// Gets a read-only collection of all stream manipulator middleware builders added via <see cref="UseStreamManipulatorBuilder"/> thus far.
        /// </summary>
        IReadOnlyList<Func<AppFunc, AppFunc>> StreamManipulatorChain { get; }

        /// <summary>
        /// Gets a read-only collection of all exception loggers added via <see cref="UseExceptionLogger"/> thus far.
        /// </summary>
        IReadOnlyList<IExceptionLogger> ExceptionLoggers { get; }

        /// <summary>
        /// Registers a function that will be called to create a middleware AppFunc. Middleware AppFuncs are expected
        /// to chain to the next AppFunc if the request passes through the middleware, or not call the next AppFunc
        /// if the request is to stop being processed.
        /// </summary>
        /// <param name="appFuncBuilder"></param>
        void UseMiddlewareBuilder(Func<AppFunc, AppFunc> appFuncBuilder);

        /// <summary>
        /// Registers a function that will be called to create a stream manipulator AppFunc. Stream
        /// manipulators are always called even if a middleware AppFunc stopped the pipeline early. Stream
        /// manipulators do not have to call the next AppFunc. A stream manipulator cannot stop any other
        /// stream manipulator from being called.
        /// </summary>
        /// <param name="appFuncBuilder"></param>
        /// <remarks>
        /// <para>
        /// To support manipulation of the response stream with hosts that use forward-only response streams
        /// the pipeline replaces the host response stream with a memory stream when stream manipulators are
        /// used. The pipeline will copy the content of the memory stream to the host response stream once all
        /// stream manipulators have been called.
        /// </para>
        /// </remarks>
        void UseStreamManipulatorBuilder(Func<AppFunc, AppFunc> appFuncBuilder);

        /// <summary>
        /// Registers an exception logger.
        /// </summary>
        /// <param name="exceptionLogger"></param>
        void UseExceptionLogger(IExceptionLogger exceptionLogger);
    }
}
