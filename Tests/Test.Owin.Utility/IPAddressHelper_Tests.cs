// Copyright © 2020 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Net;
using AWhewell.Owin.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin.Utility
{
    [TestClass]
    public class IPAddressHelper_Tests
    {
        [TestMethod]
        [DataRow("127.0.0.1",       true)]
        [DataRow("9.255.255.255",   false)]
        [DataRow("10.0.0.0",        true)]
        [DataRow("10.255.255.255",  true)]
        [DataRow("11.0.0.0",        false)]
        [DataRow("169.254.0.255",   false)]
        [DataRow("169.254.1.0",     true)]
        [DataRow("169.254.254.255", true)]
        [DataRow("169.254.255.0",   false)]
        [DataRow("172.15.255.255",  false)]
        [DataRow("172.16.0.0",      true)]
        [DataRow("172.31.255.255",  true)]
        [DataRow("172.32.0.0",      false)]
        [DataRow("192.167.255.255", false)]
        [DataRow("192.168.0.0",     true)]
        [DataRow("192.168.255.255", true)]
        [DataRow("192.169.0.0",     false)]

        public void IsLocalOrLandAddress_Returns_Correct_Value(string ipAddress, bool expected)
        {
            var address = IPAddress.Parse(ipAddress);

            var actual = IPAddressHelper.IsLocalOrLanAddress(address);

            Assert.AreEqual(expected, actual);
        }
    }
}
