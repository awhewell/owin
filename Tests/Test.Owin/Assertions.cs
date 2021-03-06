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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin
{
    /// <summary>
    /// Extra assertions.
    /// </summary>
    public static class Assertions
    {
        /// <summary>
        /// As per Assert.AreEqual except this can also compare lists for equality.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        public static void AreEqual(object expected, object actual)
        {
            var expectedCollection = expected as IList;
            var actualCollection = actual as IList;

            if(expectedCollection == null && actualCollection == null) {
                Assert.AreEqual(expected, actual);
            } else {
                if(expectedCollection == null) {
                    Assert.IsNull(actual);   // <-- this will always fail if it gets this far
                } else {
                    Assert.AreEqual(expectedCollection.Count, actualCollection.Count);
                    for(var i = 0;i < expectedCollection.Count;++i) {
                        Assert.AreEqual(expectedCollection[i], actualCollection[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Asserts that the two lists have the same contents by reference. The contents do not have to be in
        /// the same order in each list.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        public static void AreContentsSameUnordered<T>(IList<T> lhs, IList<T> rhs)
        {
            if(lhs != null || rhs != null) {
                Assert.IsNotNull(lhs);
                Assert.IsNotNull(rhs);
                Assert.AreEqual(lhs.Count, rhs.Count);

                var notInLhs = new LinkedList<T>();
                for(var i = 0;i < rhs.Count;++i) {
                    notInLhs.AddLast(rhs[i]);
                }

                for(var i = 0;i < lhs.Count;++i) {
                    var lhsValue = lhs[i];
                    var found = false;

                    for(var node = notInLhs.First;!found && node != null;node = node.Next) {
                        if(Object.ReferenceEquals(lhsValue, node.Value)) {
                            found = true;
                            notInLhs.Remove(node);
                        }
                    }

                    Assert.IsTrue(found, $"Could not find LHS value {lhsValue} in RHS collection");
                }

                Assert.AreEqual(0, notInLhs.Count, $"The RHS collection has {notInLhs.Count} item(s) that are not in LHS");
            }
        }
    }
}
