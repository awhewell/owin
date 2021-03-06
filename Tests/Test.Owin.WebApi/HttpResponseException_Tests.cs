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
using System.Net;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AWhewell.Owin.Interface.WebApi;

namespace Test.AWhewell.Owin.WebApi
{
    [TestClass]
    public class HttpResponseException_Tests
    {
        [TestMethod]
        public void Default_Ctor_Sets_Properties_Correctly()
        {
            var exception = new HttpResponseException();

            Assert.AreEqual(HttpStatusCode.OK, exception.StatusCode);
            Assert.AreEqual("", exception.Message);     // Normally Exception puts a default message here, we want it to be empty as it is reported back to the anonymous caller
            Assert.IsNull(exception.InnerException);
        }

        [TestMethod]
        public void Message_Ctor_Sets_Properties_Correctly()
        {
            var exception = new HttpResponseException("message");

            Assert.AreEqual(HttpStatusCode.OK, exception.StatusCode);
            Assert.AreEqual("message", exception.Message);
            Assert.IsNull(exception.InnerException);
        }

        [TestMethod]
        public void InnerException_Ctor_Sets_Properties_Correctly()
        {
            var inner = new NotImplementedException();
            var exception = new HttpResponseException("message", inner);

            Assert.AreEqual(HttpStatusCode.OK, exception.StatusCode);
            Assert.AreEqual("message", exception.Message);
            Assert.AreSame(inner, exception.InnerException);
        }

        [TestMethod]
        public void StatusCode_Ctor_Sets_Properties_Correctly()
        {
            var exception = new HttpResponseException(HttpStatusCode.BadRequest);

            Assert.AreEqual(HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.AreEqual("", exception.Message);
            Assert.IsNull(exception.InnerException);
        }

        [TestMethod]
        public void StatusCode_And_Message_Ctor_Sets_Properties_Correctly()
        {
            var exception = new HttpResponseException(HttpStatusCode.Unauthorized, "Not allowed");

            Assert.AreEqual(HttpStatusCode.Unauthorized, exception.StatusCode);
            Assert.AreEqual("Not allowed", exception.Message);
            Assert.IsNull(exception.InnerException);
        }
    }
}
