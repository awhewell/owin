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
using AWhewell.Owin.Interface;
using AWhewell.Owin.Utility;

namespace AWhewell.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Default implementation of <see cref="IPipelineBuilderEnvironment"/>.
    /// </summary>
    class PipelineBuilderEnvironment : IPipelineBuilderEnvironment
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        public OwinDictionary<object> Properties => new OwinDictionary<object>();

        private readonly List<Func<AppFunc, AppFunc>> _MiddlewareChain = new List<Func<AppFunc, AppFunc>>();
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IReadOnlyList<Func<AppFunc, AppFunc>> MiddlewareChain => _MiddlewareChain;

        private readonly List<Func<AppFunc, AppFunc>> _StreamManipulatorChain = new List<Func<AppFunc, AppFunc>>();
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IReadOnlyList<Func<AppFunc, AppFunc>> StreamManipulatorChain => _StreamManipulatorChain;

        private readonly List<IExceptionLogger> _ExceptionLoggers = new List<IExceptionLogger>();
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IReadOnlyList<IExceptionLogger> ExceptionLoggers => _ExceptionLoggers;

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="middleware"></param>
        public void UseMiddleware(Func<AppFunc, AppFunc> middleware)
        {
            if(middleware == null) {
                throw new ArgumentNullException(nameof(middleware));
            }
            _MiddlewareChain.Add(middleware);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="middleware"></param>
        public void UseStreamManipulator(Func<AppFunc, AppFunc> middleware)
        {
            if(middleware == null) {
                throw new ArgumentNullException(nameof(middleware));
            }
            _StreamManipulatorChain.Add(middleware);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="exceptionLogger"></param>
        public void UseExceptionLogger(IExceptionLogger exceptionLogger)
        {
            if(exceptionLogger == null) {
                throw new ArgumentNullException(nameof(exceptionLogger));
            }
            _ExceptionLoggers.Add(exceptionLogger);
        }
    }
}
