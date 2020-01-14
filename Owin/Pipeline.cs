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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AWhewell.Owin.Interface;
using AWhewell.Owin.Utility;

namespace AWhewell.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Default implementation of <see cref="IPipeline"/>.
    /// </summary>
    class Pipeline : IPipeline
    {
        // A reference to the first function in a chain of middleware functions.
        private AppFunc _MiddlewareChain;

        // An array of stream manipulator functions. Unlike middleware these do not chain on to each other.
        private AppFunc[] _StreamManipulatorChain;

        private IExceptionLogger[] _ExceptionLoggers;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IReadOnlyList<IExceptionLogger> ExceptionLoggers => _ExceptionLoggers;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool HasStreamManipulators => _StreamManipulatorChain?.Length > 0;

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="buildEnvironment"></param>
        public void Construct(IPipelineBuilderEnvironment buildEnvironment)
        {
            if(buildEnvironment == null) {
                throw new ArgumentNullException(nameof(buildEnvironment));
            }
            if(_MiddlewareChain != null) {
                throw new InvalidOperationException($"You cannot call {nameof(Construct)} twice");
            }

            _ExceptionLoggers = buildEnvironment.ExceptionLoggers.ToArray();

            _MiddlewareChain = ChainTerminatorAppFunc;
            foreach(var chainLink in buildEnvironment.MiddlewareChain.Reverse()) {
                _MiddlewareChain = chainLink.Invoke(_MiddlewareChain);
            }

            var streamManipulators = new List<AppFunc>();
            foreach(var chainLink in buildEnvironment.StreamManipulatorChain) {
                streamManipulators.Add(chainLink.Invoke(ChainTerminatorAppFunc));
            }
            _StreamManipulatorChain = streamManipulators.ToArray();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="environment"></param>
        public async Task ProcessRequest(IDictionary<string, object> environment)
        {
            if(environment == null) {
                throw new ArgumentNullException(nameof(environment));
            }
            if(_MiddlewareChain == null) {
                throw new InvalidOperationException($"You cannot call {nameof(ProcessRequest)} before calling {nameof(Construct)}");
            }

            if(!HasStreamManipulators) {
                await _MiddlewareChain(environment);
            } else {
                environment.TryGetValue(EnvironmentKey.ResponseBody, out var originalResponseBodyObj);
                var originalResponseBody = originalResponseBodyObj as Stream;
                var responseBodyWrapper = originalResponseBody != null && originalResponseBody != Stream.Null ? new MemoryStream() : null;
                if(responseBodyWrapper != null) {
                    environment[EnvironmentKey.ResponseBody] = responseBodyWrapper;
                }

                try {
                    await _MiddlewareChain(environment);

                    foreach(var streamManipulatorAppFunc in _StreamManipulatorChain) {
                        await streamManipulatorAppFunc(environment);
                    }
                } finally {
                    if(responseBodyWrapper != null) {
                        CopyStream(
                            environment,
                            responseBodyWrapper,
                            originalResponseBody
                        );
                        responseBodyWrapper.Dispose();
                    }
                }
            }
        }

        private void CopyStream(IDictionary<string, object> environment, MemoryStream source, Stream destination)
        {
            environment[EnvironmentKey.ResponseBody] = destination;

            var sourceBytes = source.ToArray();
            var length = sourceBytes.Length;

            if(length < (long)int.MaxValue) {
                if(   environment.TryGetValue(EnvironmentKey.ResponseHeaders, out var rawResponseHeaders)
                   && rawResponseHeaders is IDictionary<string, string[]> responseHeaders
                ) {
                    responseHeaders["Content-Length"] = new string[] { length.ToString(CultureInfo.InvariantCulture) };
                }

                destination.Write(sourceBytes, 0, (int)length);
            }
        }

        /// <summary>
        /// An AppFunc that sets the status to 404 not found if no other status has been set.
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        private static Task ChainTerminatorAppFunc(IDictionary<string, object> environment)
        {
            if(!environment.ContainsKey(EnvironmentKey.ResponseStatusCode)) {
                environment[EnvironmentKey.ResponseStatusCode] = (int)HttpStatusCode.NotFound;
            }
            return Task.FromResult(0);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="ex"></param>
        public void LogException(Exception ex)
        {
            if(ex != null && _ExceptionLoggers != null) {
                foreach(var logger in _ExceptionLoggers) {
                    try {
                        logger.LogException(ex);
                    } catch {
                        ; // Exceptions within the log exception methods are documented as swallowed
                    }
                }
            }
        }
    }
}
