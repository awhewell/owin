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
using System.Threading.Tasks;

namespace AWhewell.Owin.Interface
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// The interface for a stream manipulator that can compress the response if requested.
    /// </summary>
    /// <remarks>
    /// This is intended to be used as the last stream manipulator in the pipeline, or at
    /// least the last one that writes to the stream.
    /// </remarks>
    public interface ICompressResponseManipulator
    {
        /// <summary>
        /// Gets or sets a value indicating that the response will be compressed if the client accepts it.
        /// Defaults to true.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Creates the Task for the stream manipulator.
        /// </summary>
        /// <param name="next">The next task to chain to. Unused by stream manipulators.</param>
        /// <returns></returns>
        AppFunc AppFuncBuilder(AppFunc next);
    }
}
