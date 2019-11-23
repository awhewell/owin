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

namespace Test.AWhewell.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class MockMiddleware
    {
        public int CreateAppFuncCallCount { get; private set; }

        public bool ChainToNextMiddleware { get; set; } = true;

        public List<IDictionary<string, object>> Environments { get; } = new List<IDictionary<string, object>>();

        public IDictionary<string, object> LastEnvironment => Environments.Count > 0 ? Environments[Environments.Count - 1] : null;

        public int AppFuncCallCount => Environments.Count;

        public List<AppFunc> Nexts { get; } = new List<AppFunc>();

        public AppFunc LastNext => Nexts.Count > 0 ? Nexts[Nexts.Count - 1] : null;

        public Action Action { get; set; }

        public AppFunc CreateAppFunc(AppFunc next)
        {
            ++CreateAppFuncCallCount;

            return async(IDictionary<string, object> environment) => {
                Environments.Add(environment);
                Nexts.Add(next);

                Action?.Invoke();

                if(ChainToNextMiddleware) {
                    next.Invoke(environment).Wait();
                }
            };
        }

        public static Task Stub(IDictionary<string, object> environment)
        {
            return Task.FromResult(0);
        }

        public static void Call(AppFunc middleware, IDictionary<string, object> environment)
        {
            middleware.Invoke(environment).Wait();
        }
    }
}
