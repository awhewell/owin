﻿// Copyright © 2019 onwards, Andrew Whewell
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
using System.Globalization;
using System.Text;
using System.Threading;

namespace Test.Owin
{
    /// <summary>
    /// Switches in a culture in the ctor, restores the current culture in the dtor.
    /// </summary>
    public class CultureSwap : IDisposable
    {
        /// <summary>
        /// The value of Thread.CurrentThread.CurrentCulture when the object was built.
        /// </summary>
        public CultureInfo OriginalCulture { get; }

        /// <summary>
        /// The value of Thread.CurrentThread.CurrentUICulture when the object was built.
        /// </summary>
        public CultureInfo OriginalUICulture { get; }

        /// <summary>
        /// The culture that CurrentCulture and CurrentUICulture were set to by the constructor.
        /// </summary>
        public CultureInfo CurrentCulture { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="newCulture"></param>
        public CultureSwap(string newCulture)
        {
            OriginalCulture =   Thread.CurrentThread.CurrentCulture;
            OriginalUICulture = Thread.CurrentThread.CurrentUICulture;

            CurrentCulture = new CultureInfo(newCulture);

            Thread.CurrentThread.CurrentCulture = CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = CurrentCulture;
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~CultureSwap()
        {
            Dispose(false);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                Thread.CurrentThread.CurrentCulture = OriginalCulture;
                Thread.CurrentThread.CurrentUICulture = OriginalUICulture;
            }
        }
    }
}
