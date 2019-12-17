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
using AWhewell.Owin.Interface.WebApi;
using AWhewell.Owin.Utility;
using InterfaceFactory;

namespace AWhewell.Owin.WebApi
{
    /// <summary>
    /// Default implementation of <see cref="IModelBuilder"/>.
    /// </summary>
    class ModelBuilder : IModelBuilder
    {
        /// <summary>
        /// The object that maps JSON text to objects for us.
        /// </summary>
        private IJsonSerialiser _JsonSerialiser;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ModelBuilder()
        {
            _JsonSerialiser = Factory.Resolve<IJsonSerialiser>();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="modelType"></param>
        /// <param name="typeParserResolver"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public object BuildModel(Type modelType, TypeParserResolver typeParserResolver, QueryStringDictionary values)
        {
            if(modelType == null) {
                throw new ArgumentNullException(nameof(modelType));
            }
            if(values == null) {
                throw new ArgumentNullException(nameof(values));
            }

            var result = Activator.CreateInstance(modelType);

            foreach(var propertyInfo in modelType.GetProperties()) {
                if(values.ContainsKey(propertyInfo.Name)) {
                    var valueText = values.GetValue(propertyInfo.Name);
                    var parsedValue = Parser.ParseType(
                        propertyInfo.PropertyType,
                        valueText,
                        typeParserResolver
                    );
                    propertyInfo.SetValue(result, parsedValue);
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="modelType"></param>
        /// <param name="typeParserResolver"></param>
        /// <param name="jsonText"></param>
        /// <returns></returns>
        public object BuildModelFromJson(Type modelType, TypeParserResolver typeParserResolver, string jsonText)
        {
            return _JsonSerialiser.Deserialise(modelType, typeParserResolver, jsonText);
        }
    }
}
