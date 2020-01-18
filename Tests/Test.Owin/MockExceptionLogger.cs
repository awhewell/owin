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
using AWhewell.Owin.Interface;

namespace Test.AWhewell.Owin
{
    /// <summary>
    /// A mock exception logger.
    /// </summary>
    public class MockExceptionLogger : IExceptionLogger
    {
        public Action<Exception> LogExceptionCallback { get; set; }

        public int CallCount => Exceptions.Count;

        public List<Exception> Exceptions { get; } = new List<Exception>();

        public List<string> RequestUrls { get; } = new List<string>();

        public Exception LastExceptionLogged => Exceptions.Count == 0 ? null : Exceptions[Exceptions.Count - 1];

        public void LogException(Exception ex)
        {
            Exceptions.Add(ex);
            LogExceptionCallback?.Invoke(ex);
        }

        public void LogException(string requestUrl, Exception ex)
        {
            RequestUrls.Add(requestUrl);
            LogException(ex);
        }

        public void Reset()
        {
            Exceptions.Clear();
        }

        public override string ToString() => CallCount == 0 ? "none logged" : $"[CallCount - 1] {LastExceptionLogged?.GetType().Name}";
    }
}
