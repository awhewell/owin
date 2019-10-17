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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Owin.Interface;
using Owin.Interface.HttpListenerWrapper;

namespace Test.Owin.Host.HttpListener
{
    [TestClass]
    public class Environment_ResponseHeaders_Tests : EnvironmentHeaders_Agnostic_Tests
    {
        private IClassFactory                       _Snapshot;
        private MockHttpListener                    _HttpListener;
        private IHostHttpListener                   _Host;
        private Mock<IPipelineBuilder>              _PipelineBuilder;
        private Mock<IPipelineBuilderEnvironment>   _PipelineBuilderEnvironment;
        private Mock<IPipeline>                     _Pipeline;
        private IDictionary<string, object>         _PipelineEnvironment;
        private Action<IDictionary<string, object>> _ProcessRequestAction;
        private NameValueCollection                 _ResponseHeaders;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _PipelineBuilder = MockHelper.FactorySingleton<IPipelineBuilder>();
            _PipelineBuilderEnvironment = MockHelper.FactoryImplementation<IPipelineBuilderEnvironment>();
            _Pipeline = MockHelper.FactoryImplementation<IPipeline>();
            _ProcessRequestAction = null;
            _Pipeline.Setup(r => r.ProcessRequest(It.IsAny<IDictionary<string, object>>()))
            .Callback((IDictionary<string, object> environment) => {
                _PipelineEnvironment = environment;
                _ProcessRequestAction?.Invoke(environment);
            });

            _PipelineBuilder.Setup(r => r.CreatePipeline(_PipelineBuilderEnvironment.Object)).Returns(() => _Pipeline.Object);

            _HttpListener = new MockHttpListener();
            Factory.RegisterInstance<IHttpListener>(_HttpListener.Object);

            _ResponseHeaders = _HttpListener.MockContext.MockResponse.Headers;

            _Host = Factory.Resolve<IHostHttpListener>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if(_Host != null) {
                _Host.Dispose();
            }
            Factory.RestoreSnapshot(_Snapshot);
        }

        protected override void SetNativeHeaderValue(string headerKey, string headerValue) => _ResponseHeaders[headerKey] = headerValue;

        protected override string GetNativeHeaderValue(string headerKey) => _ResponseHeaders[headerKey];

        protected override IDictionary<string, string[]> GetEnvironmentDictionary()
        {
            _Host.Initialise();
            _Host.Start();

            return (IDictionary<string, string[]>)_PipelineEnvironment[EnvironmentKey.ResponseHeaders];
        }

        private void RestrictedHeaderAssignmentTest(string headerKey, string validValue, Action<MockHttpListenerResponse> verifyResponseConfigured)
        {
            headerKey = headerKey.ToLower();
            RestrictedHeaderAssignmentTest_CheckAction(headerKey, validValue, (h,v) => h[headerKey] = v,                                        verifyResponseConfigured);
            RestrictedHeaderAssignmentTest_CheckAction(headerKey, validValue, (h,v) => h.Add(headerKey, v),                                     verifyResponseConfigured);
            RestrictedHeaderAssignmentTest_CheckAction(headerKey, validValue, (h,v) => h.Add(new KeyValuePair<string, string[]>(headerKey, v)), verifyResponseConfigured);

            headerKey = headerKey.ToUpper();
            RestrictedHeaderAssignmentTest_CheckAction(headerKey, validValue, (h,v) => h[headerKey] = v,                                        verifyResponseConfigured);
            RestrictedHeaderAssignmentTest_CheckAction(headerKey, validValue, (h,v) => h.Add(headerKey, v),                                     verifyResponseConfigured);
            RestrictedHeaderAssignmentTest_CheckAction(headerKey, validValue, (h,v) => h.Add(new KeyValuePair<string, string[]>(headerKey, v)), verifyResponseConfigured);
        }

        private void RestrictedHeaderAssignmentTest_CheckAction(string headerKey, string validValue, Action<IDictionary<string, string[]>, string[]> dictionaryAssign, Action<MockHttpListenerResponse> verifyResponseConfigured, bool caseSensitive = false)
        {
            if(!validValue.Any(r => Char.IsLetter(r))) {
                caseSensitive = true;
            }

            foreach(var useUpperCase in new bool?[] { null, true, false }) {
                TestCleanup();
                TestInitialise();

                var headers = GetEnvironmentDictionary();

                var caseAdjustedValue = validValue;
                switch(useUpperCase) {
                    case true:  caseAdjustedValue = caseAdjustedValue.ToUpper(); break;
                    case false: caseAdjustedValue = caseAdjustedValue.ToLower(); break;
                }

                dictionaryAssign(headers, new string[] { caseAdjustedValue });

                Assert.IsNull(GetNativeHeaderValue(headerKey));
                verifyResponseConfigured(_HttpListener.MockContext.MockResponse);

                if(caseSensitive) {
                    break;
                }
            }
        }

        [TestMethod]
        public void ContentLength_Writes_To_Response_Not_Dictionary()
        {
            RestrictedHeaderAssignmentTest(
                "Content-Length",
                "1",
                response => response.VerifySet(r => r.ContentLength64 = 1)
            );
        }

        [TestMethod]
        public void KeepAlive_Writes_To_Response_Not_Dictionary()
        {
            RestrictedHeaderAssignmentTest(
                "Keep-Alive",
                "False",
                response => response.VerifySet(r => r.KeepAlive = false)
            );
            RestrictedHeaderAssignmentTest(
                "Keep-Alive",
                "True",
                response => response.VerifySet(r => r.KeepAlive = true)
            );
        }

        [TestMethod]
        public void TransferEncoding_Writes_To_Response_Not_Dictionary()
        {
            RestrictedHeaderAssignmentTest(
                "Transfer-Encoding",
                "chunked",
                response => response.VerifySet(r => r.SendChunked = true)
            );
        }

        [TestMethod]
        public void TransferEncoding_Ignores_Other_Header_Values()
        {
            RestrictedHeaderAssignmentTest(
                "Transfer-Encoding",
                "gzip",
                response => {
                    response.VerifySet(r => r.SendChunked = true, Times.Never());
                    response.VerifySet(r => r.SendChunked = false, Times.Never());
                }
            );
        }

        [TestMethod]
        public void TransferEncoding_Chunked_Can_Be_Any_Element()
        {
            var headers = GetEnvironmentDictionary();

            headers["Transfer-Encoding"] = new string[] {
                "gzip",
                "chunked",
            };

            _HttpListener.MockContext.MockResponse.VerifySet(r => r.SendChunked = true);
            Assert.IsNull(GetNativeHeaderValue("Transfer-Encoding"));
        }
    }
}
