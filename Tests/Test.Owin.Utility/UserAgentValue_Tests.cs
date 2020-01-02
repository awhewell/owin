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
using System.Text;
using AWhewell.Owin.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin.Utility
{
    [TestClass]
    public class UserAgentValue_Tests
    {
        public const string Samsung_Galaxy_S6 = "Mozilla/5.0 (Linux; Android 6.0.1; SM-G920V Build/MMB29K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.98 Mobile Safari/537.36";

        [TestMethod]
        public void Ctor_Defaults_UserAgent()
        {
            var value = new UserAgentValue();

            Assert.AreEqual("", value.UserAgent);
        }

        [TestMethod]
        public void Ctor_Sets_UserAgent()
        {
            var value = new UserAgentValue(Samsung_Galaxy_S6);

            Assert.AreEqual(Samsung_Galaxy_S6, value.UserAgent);
        }

        [TestMethod]
        public void Ctor_Handles_Null_UserAgent()
        {
            var value = new UserAgentValue(null);

            Assert.AreEqual("", value.UserAgent);
        }

        [TestMethod]
        [DataRow("",                                                                                                                                                                    false)]
        [DataRow("Mozilla/5.0 (Linux; Android 6.0.1; SM-G920V Build/MMB29K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.98 Mobile Safari/537.36",                           true)]      // Samsung Galaxy S6
        [DataRow("Mozilla/5.0 (Windows Phone 10.0; Android 4.2.1; Microsoft; Lumia 950) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Mobile Safari/537.36 Edge/13.10586",  true)]      // Microsoft Lumia 950
        [DataRow("Mozilla/5.0 (Linux; Android 6.0.1; Nexus 6P Build/MMB29P) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.83 Mobile Safari/537.36",                           true)]      // Nexus 6P
        [DataRow("Mozilla/5.0 (iPhone; CPU iPhone OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5376e Safari/8536.25",                             true)]      // iOS 6 iPhone
        [DataRow("Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5376e Safari/8536.25",                                      true)]      // iOS 6 iPad
        [DataRow("AppleTV5,3/9.1.1",                                                                                                                                                    true)]      // Apple TV 4th Gen
        [DataRow("Mozilla/5.0 (Linux; Android 4.2.2; AFTB Build/JDQ39) AppleWebKit/537.22 (KHTML, like Gecko) Chrome/25.0.1364.173 Mobile Safari/537.22",                               true)]      // Amazon Fire TV
        [DataRow("Mozilla/5.0 (Linux; Android 7.0; Pixel C Build/NRD90M; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/52.0.2743.98 Safari/537.36",                     true)]      // Google Pixel C
        [DataRow("Mozilla/5.0 (Linux; Android 5.0.2; SAMSUNG SM-T550 Build/LRX22G) AppleWebKit/537.36 (KHTML, like Gecko) SamsungBrowser/3.3 Chrome/38.0.2125.102 Safari/537.36",       true)]      // Samsung Galaxy Tab A
        [DataRow("Mozilla/5.0 (Linux; Android 4.4.3; KFTHWI Build/KTU84M) AppleWebKit/537.36 (KHTML, like Gecko) Silk/47.1.79 like Chrome/47.0.2526.80 Safari/537.36",                  true)]      // Amazon Kindle Fire HDX 7
        [DataRow("Mozilla/5.0 (X11; U; Linux armv7l like Android; en-us) AppleWebKit/531.2+ (KHTML, like Gecko) Version/5.0 Safari/533.2+ Kindle/3.0+",                                 true)]      // Amazon Kindle 4
        [DataRow("Mozilla/5.0 (Windows Phone 10.0; Android 4.2.1; Xbox; Xbox One) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Mobile Safari/537.36 Edge/13.10586",        true)]      // Xbox One
        [DataRow("Mozilla/5.0 (PlayStation 4 3.11) AppleWebKit/537.73 (KHTML, like Gecko)",                                                                                             true)]      // PS4
        [DataRow("Mozilla/5.0 (PlayStation Vita 3.61) AppleWebKit/537.73 (KHTML, like Gecko) Silk/3.2",                                                                                 true)]      // PS Vita
        [DataRow("Mozilla/5.0 (Nintendo 3DS; U; ; en) Version/1.7412.EU",                                                                                                               true)]      // Nintendo 3DS
        [DataRow("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246",                                     false)]     // Windows 10-based PC using Edge browser
        [DataRow("Mozilla/5.0 (X11; CrOS x86_64 8172.45.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.64 Safari/537.36",                                                   false)]     // Chromebook
        [DataRow("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_2) AppleWebKit/601.3.9 (KHTML, like Gecko) Version/9.0.2 Safari/601.3.9",                                                false)]     // Mac OS X-based computer using a Safari browser
        [DataRow("Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/47.0.2526.111 Safari/537.36",                                                       false)]     // Windows 7-based PC using a Chrome browser
        [DataRow("Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:15.0) Gecko/20100101 Firefox/15.0.1",                                                                                      false)]     // Linux-based PC using a Firefox browser
        public void IsMobileUserAgentString_Decodes_Common_Mobile_User_Agent_Strings(string userAgent, bool expected)
        {
            var value = new UserAgentValue(userAgent);

            var actual = value.IsMobileUserAgentString;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("",                                                                                                                                        false)]
        [DataRow("Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5376e Safari/8536.25",          true)]  // iPad
        [DataRow("Mozilla/5.0 (iPhone; CPU iPhone OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5376e Safari/8536.25", false)] // iPhone 6
        public void IsTabletUserAgentString_Decodes_Common_Tablet_User_Agent_String(string userAgent, bool expected)
        {
            var value = new UserAgentValue(userAgent);

            var actual = value.IsTabletUserAgentString;

            Assert.AreEqual(expected, actual);
        }
    }
}
