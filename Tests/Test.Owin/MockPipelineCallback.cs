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
using AWhewell.Owin.Interface;

namespace Test.AWhewell.Owin
{
    /// <summary>
    /// An observable callback that can be used with <see cref="Owin.Interface.IPipelineBuilder"/>.
    /// </summary>
    public class MockPipelineCallback
    {
        /// <summary>
        /// The number of times the callback has been called.
        /// </summary>
        public int CallCount { get; set; }

        /// <summary>
        /// The environments passed to each call, earliest first.
        /// </summary>
        public List<IPipelineBuilderEnvironment> AllEnvironments { get; } = new List<IPipelineBuilderEnvironment>();

        /// <summary>
        /// The environment passed in the last call to the callback.
        /// </summary>
        public IPipelineBuilderEnvironment LastEnvironment => AllEnvironments.Count == 0 ? null : AllEnvironments[AllEnvironments.Count - 1];

        /// <summary>
        /// Optional, if supplied then the action will be called when the callback is called.
        /// </summary>
        public Action<IPipelineBuilderEnvironment> Action { get; set; }

        /// <summary>
        /// The callback. Pass this wherever a configuration callback is required.
        /// </summary>
        /// <param name="buildEnvironment"></param>
        public void Callback(IPipelineBuilderEnvironment buildEnvironment)
        {
            ++CallCount;
            AllEnvironments.Add(buildEnvironment);
            Action?.Invoke(buildEnvironment);
        }
    }
}
