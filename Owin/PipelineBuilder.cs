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
using System.Linq;
using InterfaceFactory;
using AWhewell.Owin.Interface;

namespace AWhewell.Owin
{
    /// <summary>
    /// Default implementation of <see cref="IPipelineBuilder"/>.
    /// </summary>
    class PipelineBuilder : IPipelineBuilder
    {
        /// <summary>
        /// The list of callbacks that will construct a pipeline for us.
        /// </summary>
        private List<PipelineBuilderCallbackHandle> _Callbacks = new List<PipelineBuilderCallbackHandle>();

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public IPipelineBuilderCallbackHandle RegisterCallback(Action<IPipelineBuilderEnvironment> callback, int priority)
        {
            if(callback == null) {
                throw new ArgumentNullException(nameof(callback));
            }

            var result = new PipelineBuilderCallbackHandle(callback, priority);
            _Callbacks.Add(result);

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="callbackHandle"></param>
        public void DeregisterCallback(IPipelineBuilderCallbackHandle callbackHandle)
        {
            if(callbackHandle == null) {
                throw new ArgumentNullException(nameof(callbackHandle));
            }

            if(callbackHandle is PipelineBuilderCallbackHandle pipelineBuilderCallbackHandle) {
                _Callbacks.Remove(pipelineBuilderCallbackHandle);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public IPipeline CreatePipeline(IPipelineBuilderEnvironment environment)
        {
            if(environment == null) {
                throw new ArgumentNullException(nameof(environment));
            }
            environment.Properties[ApplicationStartupKey.Version] = Constants.Version;

            foreach(var callback in _Callbacks.OrderBy(r => r.Priority)) {
                callback.Callback(environment);
            }

            var result = Factory.Resolve<IPipeline>();
            result.Construct(environment);

            return result;
        }
    }
}
