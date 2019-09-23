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
using System.Reflection;
using InterfaceFactory;
using Moq;

namespace Test.Owin
{
    /// <summary>
    /// Mock helpers.
    /// </summary>
    static class MockHelper
    {
        /// <summary>
        /// Creates and registers a Moq mock object for an interface tagged as Singleton.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Mock<T> FactorySingleton<T>()
            where T: class
        {
            if(typeof(T).GetCustomAttribute<SingletonAttribute>() != null) {
                var result = CreateMock<T>();
                Factory.RegisterInstance<T>(result.Object);

                return result;
            } else {
                throw new InvalidOperationException($"{nameof(T)} is not tagged as a Singleton");
            }
        }

        /// <summary>
        /// Creates a Moq stub for an object and registers it with the class factory. The instance returned by the method
        /// is the one the factory will always return when asked for an instantiation of the interface.
        /// </summary>
        /// <returns></returns>
        public static Mock<T> FactoryImplementation<T>()
            where T: class
        {
            if(typeof(T).GetCustomAttribute<SingletonAttribute>() != null) {
                throw new InvalidOperationException($"{typeof(T).Name} is tagged as a Singleton, use CreateMockSingleton instead");
            }

            var result = CreateMock<T>();
            Factory.RegisterInstance<T>(result.Object);

            return result;
        }

        /// <summary>
        /// Creates a Moq stub for an object but does not register it with the class factory.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Mock<T> CreateMock<T>()
            where T: class
        {
            return new Mock<T>() {
                DefaultValue = DefaultValue.Mock
            }
            .SetupAllProperties();
        }
    }
}
