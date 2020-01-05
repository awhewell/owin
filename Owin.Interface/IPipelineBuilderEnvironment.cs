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
        /// Gets a read-only collection of all standard middleware in the order in which they were added.
        /// </summary>
        IEnumerable<Func<AppFunc, AppFunc>> MiddlewareChain { get; }

        /// <summary>
        /// Gets a read-only collection of all stream manipulator middleware in the order in which they were added.
        /// </summary>
        IEnumerable<Func<AppFunc, AppFunc>> StreamManipulatorChain { get; }

        /// <summary>
        /// Adds standard OWIN middleware to the pipeline that is being constructed.
        /// </summary>
        /// <param name="middleware"></param>
        /// <remarks><para>
        /// OWIN middleware is a function that is passed a reference to the next OWIN middleware
        /// in the chain and returns an AppFunc that either invokes the next middleware to continue processing
        /// or returns without invoking the next middleware to stop processing the request.
        /// </para><para>
        /// If earlier middleware stops the processing early by not chaining onto the next peice
        /// of middleware then none of the middleware following it in the chain will be called.
        /// </para><para>
        /// The function you add here is only called once to create a pipeline. The AppFunc returned
        /// by this function will be called for every request that is sent through the pipeline.
        /// </para>
        /// </remarks>
        void UseMiddleware(Func<AppFunc, AppFunc> middleware);

        /// <summary>
        /// Adds stream manipulator middleware to the pipeline that is being constructed.
        /// </summary>
        /// <param name="middleware"></param>
        /// <remarks><para>
        /// Stream manipulators are standard OWIN middleware with two caveats.
        /// </para><para>
        /// First, while they are passed a reference to the next AppFunc in a chain they are not
        /// expected to call it. Calling it has no ill effect but they cannot break the chain by not
        /// calling it. Instead all stream manipulators are always called and no stream manipulator
        /// can stop that from happening.
        /// </para><para>
        /// Second, they are always executed after the standard OWIN middleware chain has finished
        /// processing. They will be executed even if the standard OWIN middleware chain was halted
        /// early.
        /// </para><para>
        /// The position of the response stream in the environment is always at the end of the
        /// stream when the manipulator is first called. Each manipulator is expected to ensure
        /// that the position is at the end of the stream content before returning.
        /// </para><para>
        /// To support manipulation of the response stream with hosts that use forward-only response
        /// streams the pipeline replaces the host response stream with a memory stream when stream
        /// manipulators are used. It will copy the content of the memory stream to the host stream
        /// once all stream manipulators have been called.
        /// </para>
        /// </remarks>
        void UseStreamManipulator(Func<AppFunc, AppFunc> middleware);
    }
}
