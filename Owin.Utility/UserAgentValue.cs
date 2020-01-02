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
using System.Linq;
using System.Text;

namespace AWhewell.Owin.Utility
{
    /// <summary>
    /// A helper class for user agent string sniffing. Use with caution.
    /// </summary>
    public class UserAgentValue
    {
        private string[] _Tokens;
        private bool?    _IsMobileUserAgentString;
        private bool?    _IsTabletUserAgentString;

        /// <summary>
        /// Gets the user agent string.
        /// </summary>
        public string UserAgent { get; }

        /// <summary>
        /// Gets a value indicating that the caller *MIGHT* be a mobile device. Do not rely on this.
        /// </summary>
        /// <returns></returns>
        public bool IsMobileUserAgentString
        {
            get {
                if(_IsMobileUserAgentString == null) {
                    _IsMobileUserAgentString = Tokens.Any(r => 
                        String.Equals("mobile", r, StringComparison.OrdinalIgnoreCase) ||
                        String.Equals("iemobile", r, StringComparison.OrdinalIgnoreCase) ||
                        String.Equals("android", r, StringComparison.OrdinalIgnoreCase) ||
                        String.Equals("playstation", r, StringComparison.OrdinalIgnoreCase) ||
                        String.Equals("nintendo", r, StringComparison.OrdinalIgnoreCase) ||
                        r.StartsWith("appletv", StringComparison.OrdinalIgnoreCase)
                    );
                }
                return _IsMobileUserAgentString.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating that the caller *MIGHT* be a tablet device. Do not rely on this.
        /// </summary>
        public bool IsTabletUserAgentString
        {
            get {
                if(_IsTabletUserAgentString == null) {
                    _IsTabletUserAgentString = Tokens.Any(r =>
                        String.Equals("ipad", r, StringComparison.OrdinalIgnoreCase)
                    );
                }
                return _IsTabletUserAgentString.Value;
            }
        }

        /// <summary>
        /// Gets the <see cref="UserAgent"/> split into tokens.
        /// </summary>
        private string[] Tokens
        {
            get {
                if(_Tokens == null) {
                    _Tokens = UserAgent.Split(' ', '/', '(', ')', ';');
                }

                return _Tokens;
            }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public UserAgentValue() : this("")
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="userAgent"></param>
        public UserAgentValue(string userAgent)
        {
            UserAgent = userAgent ?? "";
        }
    }
}
