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
using InterfaceFactory;

namespace Owin.Interface
{
    /// <summary>
    /// An object that different parts of the application can use to be notified when a new OWIN pipeline is being
    /// constructed. The notification lets them add middleware to the new pipeline.
    /// </summary>
    public interface IPipelineBuilder
    {
        /// <summary>
        /// Records a function that will be called to create an OWIN middleware function.
        /// </summary>
        /// <param name="callback">A method that is called when the middleware needs to be built.</param>
        /// <param name="priority">A value indicating the order in which callbacks are called, lowest first.</param>
        /// <returns>A handle that can be passed to <see cref="DeregisterMiddlewareBuilder"/>.</returns>
        IMiddlewareBuilderCallbackHandle RegisterMiddlewareBuilder(Action<IPipelineBuilderEnvironment> callback, int priority);

        /// <summary>
        /// Deregisters a callback that was previously registered with <see cref="RegisterMiddlewareBuilder"/>.
        /// </summary>
        /// <param name="handle"></param>
        void DeregisterMiddlewareBuilder(IMiddlewareBuilderCallbackHandle handle);

        /// <summary>
        /// Creates a new pipeline by calling the section callbacks in order of priority.
        /// </summary>
        /// <param name="environment">The environment to use when building the pipeline.</param>
        /// <returns></returns>
        IPipeline CreatePipeline(IPipelineBuilderEnvironment environment);
    }
}
