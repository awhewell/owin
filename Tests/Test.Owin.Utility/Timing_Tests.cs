// Copyright © 2019 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#if !DEBUG
    #define RUN_TIMING_TESTS
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using AWhewell.Owin.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin.Utility
{
    [TestClass]
    public class Timing_Tests
    {
        private const int Parser_ParseInt32_Iterations = 10000000;

        [TestMethod]
        public void Parser_ParseInt32_No_Resolver()
        {
            #if RUN_TIMING_TESTS
                var stopWatch = Stopwatch.StartNew();
                for(var i = 0;i < Parser_ParseInt32_Iterations;++i) {
                    Parser.ParseInt32("32019");
                }
                stopWatch.Stop();

                Assert.Inconclusive($"{Parser_ParseInt32_Iterations:N0} calls to {nameof(Parser)}.{nameof(Parser.ParseInt32)}(string) took {stopWatch.ElapsedMilliseconds} ms");
            #endif
        }

        [TestMethod]
        public void Parser_ParseInt32_With_Null_Resolver()
        {
            #if RUN_TIMING_TESTS
                var stopWatch = Stopwatch.StartNew();
                for(var i = 0;i < Parser_ParseInt32_Iterations;++i) {
                    Parser.ParseInt32("32019", null);
                }
                stopWatch.Stop();

                Assert.Inconclusive($"{Parser_ParseInt32_Iterations:N0} calls to {nameof(Parser)}.{nameof(Parser.ParseInt32)}(string, {nameof(TypeParserResolver)}) with null resolver took {stopWatch.ElapsedMilliseconds} ms");
            #endif
        }

        [TestMethod]
        public void Parser_ParseInt32_With_Resolver_And_No_Parser()
        {
            #if RUN_TIMING_TESTS
                var resolver = new TypeParserResolver();

                var stopWatch = Stopwatch.StartNew();
                for(var i = 0;i < Parser_ParseInt32_Iterations;++i) {
                    Parser.ParseInt32("32019", resolver);
                }
                stopWatch.Stop();

                Assert.Inconclusive($"{Parser_ParseInt32_Iterations:N0} calls to {nameof(Parser)}.{nameof(Parser.ParseInt32)}(string, {nameof(TypeParserResolver)}) took {stopWatch.ElapsedMilliseconds} ms");
            #endif
        }

        class Int32Parser : ITypeParser<int>
        {
            public bool TryParse(string text, out int value) => int.TryParse(text, out value);
        }

        [TestMethod]
        public void Parser_ParseInt32_With_Resolver_And_Parser()
        {
            #if RUN_TIMING_TESTS
                var resolver = new TypeParserResolver(new Int32Parser());

                var stopWatch = Stopwatch.StartNew();
                for(var i = 0;i < Parser_ParseInt32_Iterations;++i) {
                    Parser.ParseInt32("32019", resolver);
                }
                stopWatch.Stop();

                Assert.Inconclusive($"{Parser_ParseInt32_Iterations:N0} calls to {nameof(Parser)}.{nameof(Parser.ParseInt32)}(string, {nameof(TypeParserResolver)}) took {stopWatch.ElapsedMilliseconds} ms");
            #endif
        }
    }
}
