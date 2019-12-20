// Copyright © 2019 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using AWhewell.Owin.Utility;
using AWhewell.Owin.Utility.Formatters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin.Utility
{
    [TestClass]
    public class TypeFormatterResolverCache_Tests
    {
        [TestMethod]
        public void Find_FormatterList_Returns_Existing_Resolver()
        {
            var resolver1 = TypeFormatterResolverCache.Find(new DateTime_Iso8601_Formatter());
            var resolver2 = TypeFormatterResolverCache.Find(new DateTime_Iso8601_Formatter());

            Assert.IsNotNull(resolver1);
            Assert.AreSame(resolver1, resolver2);
        }

        [TestMethod]
        public void Find_FormatterList_Ignores_Order_Of_Types()
        {
            var resolver1 = TypeFormatterResolverCache.Find(new DateTime_Iso8601_Formatter(), new DateTimeOffset_JavaScriptTicks_Formatter());
            var resolver2 = TypeFormatterResolverCache.Find(new DateTimeOffset_JavaScriptTicks_Formatter(), new DateTime_Iso8601_Formatter());

            Assert.IsNotNull(resolver1);
            Assert.AreSame(resolver1, resolver2);
        }

        [TestMethod]
        public void Find_FormatterList_Does_Not_Ignores_Types()
        {
            var resolver1 = TypeFormatterResolverCache.Find(new DateTime_Iso8601_Formatter());
            var resolver2 = TypeFormatterResolverCache.Find(new DateTimeOffset_Iso8601_Formatter());

            Assert.AreNotSame(resolver1, resolver2);
        }

        [TestMethod]
        public void Find_FormatterList_Accepts_Null()
        {
            var resolver1 = TypeFormatterResolverCache.Find((ITypeFormatter[])null);
            var resolver2 = TypeFormatterResolverCache.Find((ITypeFormatter[])null);

            Assert.IsNotNull(resolver1);
            Assert.AreSame(resolver1, resolver2);
        }

        [TestMethod]
        public void Clear_Empties_Cache()
        {
            var resolver1 = TypeFormatterResolverCache.Find(new DateTime_Iso8601_Formatter());
            TypeFormatterResolverCache.Clear();
            var resolver2 = TypeFormatterResolverCache.Find(new DateTime_Iso8601_Formatter());

            Assert.AreNotSame(resolver1, resolver2);
        }
    }
}
