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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AWhewell.Owin.Interface.WebApi;

namespace Test.AWhewell.Owin.WebApi
{
    [TestClass]
    public class HttpMethodAttributeTests
    {
        private void CheckUsage<T>()
        {
            var usageAttribute = typeof(T)
                .GetCustomAttributes(inherit: false)
                .OfType<AttributeUsageAttribute>()
                .FirstOrDefault();

            Assert.IsNotNull(usageAttribute);
            Assert.AreEqual(AttributeTargets.Method, usageAttribute.ValidOn);
            Assert.AreEqual(true, usageAttribute.Inherited);
            Assert.AreEqual(false, usageAttribute.AllowMultiple);
        }

        [TestMethod]
        public void HttpDeleteAttribute_Has_Correct_Usage() => CheckUsage<HttpDeleteAttribute>();

        [TestMethod]
        public void HttpGetAttribute_Has_Correct_Usage() => CheckUsage<HttpGetAttribute>();

        [TestMethod]
        public void HttpHeadAttribute_Has_Correct_Usage() => CheckUsage<HttpHeadAttribute>();

        [TestMethod]
        public void HttpPatchAttribute_Has_Correct_Usage() => CheckUsage<HttpPatchAttribute>();

        [TestMethod]
        public void HttpPostAttribute_Has_Correct_Usage() => CheckUsage<HttpPostAttribute>();

        [TestMethod]
        public void HttpPutAttribute_Has_Correct_Usage() => CheckUsage<HttpPutAttribute>();
    }
}
