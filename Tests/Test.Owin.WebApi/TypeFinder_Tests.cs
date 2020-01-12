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
using AWhewell.Owin.Interface.WebApi;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin.WebApi
{
    [TestClass]
    public class TypeFinder_Tests
    {
        [TestMethod]
        public void TypeFinder_Can_Find_Types()
        {
            var typeFinder = Factory.Resolve<ITypeFinder>();

            var types = typeFinder.GetAllTypes().ToArray();

            Assert.IsTrue(types.Contains(typeof(AssemblyInitialise)));
            Assert.IsTrue(types.Contains(typeof(String)));
            Assert.IsTrue(types.Contains(typeof(TestClassAttribute)));
            Assert.IsTrue(types.Contains(typeof(TypeFinder_Tests)));
        }

        [TestMethod]
        public void TypeFinder_Second_Call_Returns_Same_Values_As_First()
        {
            // It can take low-order hundreds of milliseconds to return all types. This
            // doesn't affect real-world use where searches for web API controllers are
            // seldom made but it hammers unit tests.
            //
            // To that end the type finder implementation is at liberty to cache results
            // for a short period of time. This just checks that two calls made quickly
            // after each other contain the same types.

            var typeFinder = Factory.Resolve<ITypeFinder>();

            var results1 = typeFinder.GetAllTypes().ToArray();
            var results2 = typeFinder.GetAllTypes().ToArray();

            Assertions.AreContentsSameUnordered(results1, results2);
        }
    }
}
