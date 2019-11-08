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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AWhewell.Owin.Interface;

namespace Test.AWhewell.Owin
{
    [TestClass]
    public class OwinDictionary_WrappedDictionary_Tests : OwinDictionary_Agnostic_Tests
    {
        [TestInitialize]
        public void TestInitialise()
        {
            _Dictionary = new OwinDictionary<object>();
        }

        [TestMethod]
        public void Ctor_Can_Accept_Existing_Dictionary_As_Backing_Store()
        {
            var existingDictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            _Dictionary = new OwinDictionary<object>(existingDictionary);

            _Dictionary["a"] = 1;

            Assert.AreEqual(1, _Dictionary["a"]);
            Assert.AreEqual(1, _Dictionary["A"]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_Throws_If_Existing_Dictionary_Is_Null()
        {
            _Dictionary = new OwinDictionary<object>((IDictionary<string, object>)null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Ctor_Throws_If_Existing_Dictionary_Is_ReadOnly()
        {
            // When the OWIN spec talks about dictionaries they are always mutable.
            var wrappedDictionary = new Dictionary<string, object>();
            var readonlyDictionary = new ReadOnlyDictionary<string, object>(wrappedDictionary);
            _Dictionary = new OwinDictionary<object>(readonlyDictionary);
        }
    }
}
