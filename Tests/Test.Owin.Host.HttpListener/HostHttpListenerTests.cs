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
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Owin.Interface;
using Owin.Interface.HttpListenerWrapper;

namespace Test.Owin.Host.HttpListener
{
    [TestClass]
    public class HostHttpListenerTests : CommonHostTests
    {
        private IClassFactory                       _Snapshot;
        private MockHttpListener                    _HttpListener;
        private IHostHttpListener                   _Host;
        private Mock<IPipelineBuilder>              _PipelineBuilder;
        private Mock<IPipelineBuilderEnvironment>   _PipelineBuilderEnvironment;
        private Mock<IPipeline>                     _Pipeline;
        private IDictionary<string, object>         _PipelineEnvironment;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _PipelineBuilder = MockHelper.FactorySingleton<IPipelineBuilder>();
            _PipelineBuilderEnvironment = MockHelper.FactoryImplementation<IPipelineBuilderEnvironment>();
            _Pipeline = MockHelper.FactoryImplementation<IPipeline>();
            _Pipeline.Setup(r => r.ProcessRequest(It.IsAny<IDictionary<string, object>>()))
                .Callback((IDictionary<string, object> environment) => _PipelineEnvironment = environment);

            _PipelineBuilder.Setup(r => r.CreatePipeline(_PipelineBuilderEnvironment.Object)).Returns(() => _Pipeline.Object);

            _HttpListener = new MockHttpListener();
            Factory.RegisterInstance<IHttpListener>(_HttpListener.Object);

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

        private void AssertEnvironment(Action<MockHttpListenerContext> setupContext, Action<IDictionary<string, object>> assertEnvironment)
        {
            setupContext?.Invoke(_HttpListener.MockContext);

            _Host.Initialise();
            _Host.Start();

            assertEnvironment(_PipelineEnvironment);
        }

        private void AssertEnvironment<T>(Action<MockHttpListenerContext> setupContext, T expected, Func<IDictionary<string, object>, T> getActual, Action<T,T> assertMethod = null)
        {
            AssertEnvironment(
                setupContext, 
                (env) => {
                    if(assertMethod == null) {
                        Assert.AreEqual(expected, getActual(env));
                    } else {
                        assertMethod(expected, getActual(env));
                    }
                }
            );
        }

        [TestMethod]
        public void Dispose_Disposes_Of_HttpListener()
        {
            _Host.Dispose();
            _Host = null;

            _HttpListener.Verify(r => r.Dispose(), Times.Once());
        }

        [TestMethod]
        public void Ctor_Sets_Prefix()
        {
            Assert.AreEqual("http://*:80/", _HttpListener.Object.Prefixes.Single());
        }

        [TestMethod]
        public void Ctor_Disables_Write_Exceptions()
        {
            Assert.AreEqual(true, _HttpListener.Object.IgnoreWriteExceptions);
        }

        [TestMethod]
        public void Initialise_Creates_Pipeline()
        {
            _Host.Initialise();

            _PipelineBuilder.Verify(r => r.CreatePipeline(_PipelineBuilderEnvironment.Object), Times.Once);
        }

        [TestMethod]
        public void Initialise_Sets_Environment_Properties()
        {
            _Host.Initialise();

            var properties = _PipelineBuilderEnvironment.Object.Properties;
            Assert.AreEqual(Constants.Version,          properties[ApplicationStartupKey.Version]);
            Assert.AreEqual("Owin.Host.HttpListener",   properties[ApplicationStartupKey.HostType]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Initialise_Throws_If_Called_Twice() => All_Hosts_Initialise_Throws_If_Called_Twice(_Host);

        [TestMethod]
        public void Root_Defaults_To_Slash() => All_Hosts_Root_Defaults_To_Slash(_Host);

        [TestMethod]
        public void Root_Coalesces_Null_To_Slash() => All_Hosts_Root_Coalesces_Null_To_Slash(_Host);

        [TestMethod]
        public void Root_Replaces_Empty_String_With_Slash() => All_Hosts_Root_Replaces_Empty_String_With_Slash(_Host);

        [TestMethod]
        public void Root_Always_Prefixed_With_Slash() => All_Hosts_Root_Always_Prefixed_With_Slash(_Host);

        [TestMethod]
        public void Root_Does_Not_Prefix_With_Slash_When_Already_Starts_With_Slash() => All_Hosts_Root_Does_Not_Prefix_With_Slash_When_Already_Starts_With_Slash(_Host);

        [TestMethod]
        public void Root_Strips_Trailing_Slash() => All_Hosts_Root_Strips_Trailing_Slash(_Host);

        [TestMethod]
        public void Root_Changing_Root_Changes_HttpListener_Prefix()
        {
            _Host.Root = "/NewRoot";
            Assert.AreEqual("http://*:80/NewRoot/", _HttpListener.Object.Prefixes.Single());
        }

        [TestMethod]
        public void Port_Defaults_To_80() => All_Hosts_Port_Defaults_To_80(_Host);

        [TestMethod]
        public void Port_Changing_Port_Changes_HttpListener_Prefix()
        {
            _Host.Port = 8080;
            Assert.AreEqual("http://*:8080/", _HttpListener.Object.Prefixes.Single());
        }

        [TestMethod]
        public void UseStrongWildcard_Changes_HttpListener_Prefix()
        {
            _Host.UseStrongWildcard = true;
            Assert.AreEqual("http://+:80/", _HttpListener.Object.Prefixes.Single());
        }

        [TestMethod]
        public void BindToLocalhost_Changes_HttpListener_Prefix()
        {
            _Host.BindToLocalhost = true;
            Assert.AreEqual("http://localhost:80/", _HttpListener.Object.Prefixes.Single());
        }

        [TestMethod]
        public void IsListening_Passes_Through_To_HttpListener()
        {
            var unused = _Host.IsListening;
            _HttpListener.VerifyGet(r => r.IsListening, Times.Once());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Start_Throws_If_Not_Initialised() => All_Hosts_Start_Throws_If_Not_Initialised(_Host);

        [TestMethod]
        public void Start_Passes_Through_To_HttpListener()
        {
            _Host.Initialise();
            _Host.Start();

            _HttpListener.Verify(r => r.Start(), Times.Once());
        }

        [TestMethod]
        public void Start_Will_Not_Pass_Through_If_Already_Listening()
        {
            _Host.Initialise();
            _Host.Start();
            _Host.Start();

            _HttpListener.Verify(r => r.Start(), Times.Once());
        }

        [TestMethod]
        public void Start_Calls_BeginGetContext()
        {
            _HttpListener.BeginContextTriggersCallback = false;
            _Host.Initialise();
            _Host.Start();

            _HttpListener.Verify(r => r.BeginGetContext(It.IsAny<AsyncCallback>(), It.IsAny<object>()), Times.Once());
        }

        [TestMethod]
        public void Stop_Passes_Through_To_HttpListener()
        {
            _Host.Initialise();
            _Host.Start();
            _Host.Stop();

            _HttpListener.Verify(r => r.Stop(), Times.Once());
        }

        [TestMethod]
        public void Stop_Will_Not_Pass_Through_If_Not_Already_Listening()
        {
            _Host.Initialise();
            _Host.Start();
            _Host.Stop();
            _Host.Stop();

            _HttpListener.Verify(r => r.Stop(), Times.Once());
        }

        [TestMethod]
        public void GetContext_Calls_EndGetContext_When_Context_Is_Received()
        {
            _Host.Initialise();
            _Host.Start();

            _HttpListener.Verify(r => r.EndGetContext(_HttpListener.MockAsyncResult.Object), Times.Once());
        }

        [TestMethod]
        public void GetContext_Does_Not_Call_EndGetContext_When_Listener_Has_Stopped_Listening()
        {
            _HttpListener.BeginContextAction = () => _Host.Stop();

            _Host.Initialise();
            _Host.Start();

            _HttpListener.Verify(r => r.EndGetContext(_HttpListener.MockAsyncResult.Object), Times.Never());
        }

        [TestMethod]
        public void GetContext_Discards_Exceptions_In_EndGetContext()
        {
            _HttpListener.EndGetContextAction = () => throw new InvalidOperationException();

            _Host.Initialise();
            _Host.Start();
        }

        [TestMethod]
        public void GetContext_Calls_BeginGetContext()
        {
            _HttpListener.EndGetContextAction = () => _HttpListener.Verify(r => r.BeginGetContext(It.IsAny<AsyncCallback>(), It.IsAny<object>()), Times.Once());

            _Host.Initialise();
            _Host.Start();

            // The first call is the initial BeginGetContext from the Start() call
            // The second call is the second BeginGetContext in the callback
            _HttpListener.Verify(r => r.BeginGetContext(It.IsAny<AsyncCallback>(), It.IsAny<object>()), Times.Exactly(2));
        }

        [TestMethod]
        public void GetContext_Does_Not_Call_BeginGetContext_If_It_Has_Stopped_Listening()
        {
            _HttpListener.BeginContextAction = () => _Host.Stop();

            _Host.Initialise();
            _Host.Start();

            // The first call is the initial BeginGetContext from the Start() call
            _HttpListener.Verify(r => r.BeginGetContext(It.IsAny<AsyncCallback>(), It.IsAny<object>()), Times.Once());
        }

        [TestMethod]
        public void GetContext_Does_Not_Call_BeginGetContext_If_EndGetContext_Threw_Exception()
        {
            _HttpListener.EndGetContextAction = () => throw new InvalidOperationException();

            _Host.Initialise();
            _Host.Start();

            // The first call is the initial BeginGetContext from the Start() call
            _HttpListener.Verify(r => r.BeginGetContext(It.IsAny<AsyncCallback>(), It.IsAny<object>()), Times.Once());
        }

        [TestMethod]
        public void GetContext_Calls_BeginGetContext_If_EndGetContext_Threw_HttpListenerException()
        {
            _HttpListener.EndGetContextAction = () => throw new HttpListenerException();

            _Host.Initialise();
            _Host.Start();

            // The first call is the initial BeginGetContext from the Start() call
            // The second call is the second BeginGetContext in the callback
            _HttpListener.Verify(r => r.BeginGetContext(It.IsAny<AsyncCallback>(), It.IsAny<object>()), Times.Exactly(2));
        }

        [TestMethod]
        public void GetContext_Ignores_Exceptions_In_BeginGetContext()
        {
            _HttpListener.BeginContextAction = () => {
                if(_HttpListener.BeginContextCallCount == 2) {
                    throw new InvalidOperationException();
                }
            };

            _Host.Initialise();
            _Host.Start();
        }

        [TestMethod]
        public void GetContext_Sends_Request_Down_Pipeline()
        {
            _Host.Initialise();
            _Host.Start();

            _Pipeline.Verify(r => r.ProcessRequest(It.IsAny<IDictionary<string, object>>()), Times.Once());
        }

        [TestMethod]
        [DataRow("/Root", "/NotRoot", false)]   // Request URL does not start with root
        [DataRow("/Root", "/root",    true)]    // Match should not be case sensitive
        [DataRow("/a",    "/ab",      false)]   // Request URL cannot start with a segment that is root plus more characters
        [DataRow("/a",    "/a/",      true)]    // Request URL can be the root plus a slash
        [DataRow("/a",    "/%61",     true)]    // Request URL can be percent-encoded
        public void GetContext_Ignores_Requests_That_Do_Not_Match_Current_Root(string hostRoot, string requestUrl, bool shouldAccept)
        {
            foreach(var unescaped in new bool[] { true, false }) {
                TestCleanup();
                TestInitialise();

                _Host.Root = hostRoot;
                _Host.Initialise();

                _HttpListener.MockContext.MockRequest.SetUrl(requestUrl, httpListenerUrlUnescapesPath: unescaped);
                _Host.Start();

                if(shouldAccept) {
                    _Pipeline.Verify(r => r.ProcessRequest(It.IsAny<IDictionary<string, object>>()), Times.Once());
                } else {
                    _Pipeline.Verify(r => r.ProcessRequest(It.IsAny<IDictionary<string, object>>()), Times.Never());
                }
            }
        }

        [TestMethod]
        public void GetContext_Does_Not_Call_Pipeline_If_EndGetContext_Threw_Exception()
        {
            _HttpListener.EndGetContextAction = () => throw new HttpListenerException();

            _Host.Initialise();
            _Host.Start();

            _Pipeline.Verify(r => r.ProcessRequest(It.IsAny<OwinDictionary<object>>()), Times.Never());
        }

        [TestMethod]
        public void GetContext_Does_Not_Call_Pipeline_If_BeginGetContext_Threw_Exception()
        {
            _HttpListener.BeginContextAction = () => {
                if(_HttpListener.BeginContextCallCount == 2) {
                    throw new InvalidOperationException();
                }
            };

            _Host.Initialise();
            _Host.Start();

            _Pipeline.Verify(r => r.ProcessRequest(It.IsAny<OwinDictionary<object>>()), Times.Never());
        }

        [TestMethod]
        public void GetContext_Environment_Is_Case_Sensitive()
        {
            _Host.Initialise();
            _Host.Start();

            Assert.IsNotNull(_PipelineEnvironment["owin.Version"]);
            Assert.IsNull(_PipelineEnvironment["OWIN.VERSION"]);
        }

        [TestMethod]
        public void GetContext_Environment_CallCancelled_Is_Set()
        {
            AssertEnvironment(
                null,
                env => {
                    var cancellationToken = (CancellationToken)env[EnvironmentKey.CallCancelled];
                    Assert.IsFalse(cancellationToken.IsCancellationRequested);
                }
            );
        }

        [TestMethod]
        public void GetContext_Environment_Version_Is_Set()
        {
            AssertEnvironment(
                null,
                Constants.Version,
                env => env[EnvironmentKey.Version]
            );
        }

        [TestMethod]
        public void GetContext_Environment_RequestMethod_Is_Set()
        {
            AssertEnvironment(
                context => context.MockRequest.SetupGet(r => r.HttpMethod).Returns("POST"),
                "POST",
                env => env[EnvironmentKey.RequestMethod]
            );
        }

        [TestMethod]
        //       method     has-body    expect Stream.Null
        [DataRow("CONNECT", null,       true)]
        [DataRow("DELETE",  false,      true)]
        [DataRow("DELETE",  true,       false)]
        [DataRow("GET",     null,       true)]
        [DataRow("HEAD",    null,       true)]
        [DataRow("OPTIONS", null,       true)]
        [DataRow("PATCH",   null,       false)]
        [DataRow("POST",    null,       false)]
        [DataRow("PUT",     null,       false)]
        [DataRow("TRACE",   null,       true)]
        public void GetContext_Environment_RequestBody_Has_Stream_For_All_Methods(string method, bool? hasEntityBody, bool expectStreamNull)
        {
            using(var requestBody = new MemoryStream()) {
                AssertEnvironment(
                    context => {
                        context.MockRequest.SetMethodAndStream(method, requestBody);
                        if(hasEntityBody != null) {
                            context.MockRequest.OptionalBodyPresent = hasEntityBody.Value;
                        }
                    },
                    expectStreamNull ? Stream.Null : requestBody,
                    env => env[EnvironmentKey.RequestBody],
                    assertMethod: (expected, actual) => Assert.AreSame(expected, actual)
                );
            }
        }

        [TestMethod]
        public void GetContext_Environment_RequestHeaders_Is_Case_Insensitive()
        {
            AssertEnvironment(
                context => {
                    context.MockRequest.Headers["one"] = "value-one";
                },
                env => {
                    Assert.AreEqual("value-one", ((IDictionary<string, string[]>)env[EnvironmentKey.RequestHeaders])["one"][0]);
                    Assert.AreEqual("value-one", ((IDictionary<string, string[]>)env[EnvironmentKey.RequestHeaders])["ONE"][0]);
                }
            );
        }

        [TestMethod]
        //       host.Root  request URL         path base
        [DataRow("/",       "/",                "")]        // Path base is empty if root is slash
        [DataRow("/",       "/a",               "")]        // Path base is empty if root is slash
        [DataRow("/Root",   "/Root/a",          "/Root")]   // Path base is root if root is not slash
        [DataRow("/A",      "/%41/File.txt",    "/A")]      // Path base expands percent-encoding
        public void GetContext_Environment_RequestPathBase_Filled_Correctly(string hostRoot, string requestUrl, string expectedPathBase)
        {
            foreach(var unescaped in new bool[] { true, false }) {
                TestCleanup();
                TestInitialise();

                _Host.Root = hostRoot;

                AssertEnvironment(
                    context => context.MockRequest.SetUrl(requestUrl, httpListenerUrlUnescapesPath: unescaped),
                    expectedPathBase,
                    env => env[EnvironmentKey.RequestPathBase]
                );
            }
        }

        [TestMethod]
        //       host.Root  request URL                 path
        [DataRow("/",       "/",                        "/")]       // Path is slash if root is slash
        [DataRow("/Root",   "/Root",                    "")]        // Path is empty if raw URL is exactly root
        [DataRow("/Root",   "/root",                    "")]        // Path is empty if raw URL is exactly root - ignores case
        [DataRow("/",       "/a",                       "/a")]      // Path must start with a slash if the root is empty
        [DataRow("/Root",   "/Root/",                   "/")]       // Path is slash if raw URL is exactly root plus a slash
        [DataRow("/Root",   "/Root/a",                  "/a")]      // Path contains everything after root
        [DataRow("/Root",   "/ROOT/a",                  "/a")]      // Path contains everything after root - still ignores case
        [DataRow("/Root",   "/Root/%41%62%63",          "/Abc")]    // Path expands escaped characters
        [DataRow("/Root",   "/Root%2fA",                "/A")]      // Path expands escaped slashes - this seems a bit odd but 5.5 Percent encoding in the spec says the path must be expanded
        [DataRow("/Root",   "/Root/a?q=1",              "/a")]      // Path does not include query string
        [DataRow("/Root",   "/Root/a%3Fid=1?id=2?id=3", "/a?id=1")] // Path includes an escaped question mark
        [DataRow("/",       "/%25%36%31",               "/%61")]    // Path unescapes once.
                                                                    // .NET HttpListener will double-unescape this to /a in the Url property and leave it as received in RawUrl
                                                                    // DotNet Core will safe unescape it in the Url property, leaving it as %2561, and leave it as received in RawUrl
                                                                    // Microsoft.Owin.Host.HttpListener sets an owin.RequestPath of %61 for this request.
                                                                    // When in doubt we are following the Microsoft.Owin behaviour, therefore we want to see %61.

        public void GetContext_Environment_RequestPath_Filled_Correctly(string hostRoot, string requestUrl, string expectedPath)
        {
            foreach(var unescaped in new bool[] { true, false }) {
                TestCleanup();
                TestInitialise();

                _Host.Root = hostRoot;

                AssertEnvironment(
                    context => context.MockRequest.SetUrl(requestUrl, httpListenerUrlUnescapesPath: unescaped),
                    expectedPath,
                    env => env[EnvironmentKey.RequestPath]
                );
            }
        }

        [TestMethod]
        //       host.Root  request URL                 query string
        [DataRow("/",       "/",                        "")]            // Query string is empty if not present
        [DataRow("/Root",   "/Root?a=1",                "a=1")]         // Query string does not include leading question mark
        [DataRow("/Root",   "/Root?a=1%3F",             "a=1%3F")]      // Query string does not unescape percent-encoded characters (see 5.5 Percent encoding in spec)
        [DataRow("/Root",   "/Root/a?id=1?id=2",        "id=1?id=2")]   // Double-question mark behaves as per Microsoft.Owin - it starts query string at first instance
        [DataRow("/Root",   "/Root/a%3Fid=1?id=2?id=3", "id=2?id=3")]   // Escaped question mark is part of path, not start of query string
        [DataRow("/",       "/?foo",                    "foo")]         // Sanity check - query string when no root specified
        [DataRow("/",       "/?",                       "")]            // Sanity check - query string with no parameters
        [DataRow("/",       "/?",                       "")]            // Sanity check - query string with ampersand in it
        [DataRow("/Root",   "/root?a=1&b=2",            "a=1&b=2")]     // Sanity check - query string with ampersand in it
        [DataRow("/Root",   "/root?a=1&amp;b=2",        "a=1&amp;b=2")] // Sanity check - query string with HTTP-encoded ampersand in it
        public void GetContext_Environment_RequestQueryString_Filled_Correctly(string hostRoot, string requestUrl, string expectedQueryString)
        {
            foreach(var unescaped in new bool[] { true, false }) {
                TestCleanup();
                TestInitialise();

                _Host.Root = hostRoot;

                AssertEnvironment(
                    context => context.MockRequest.SetUrl(requestUrl, httpListenerUrlUnescapesPath: unescaped),
                    expectedQueryString,
                    env => env[EnvironmentKey.RequestQueryString]
                );
            }
        }

        [TestMethod]
        [DataRow("1.0", "HTTP/1.0")]
        [DataRow("1.1", "HTTP/1.1")]
        [DataRow("2.0", "HTTP/2.0")]
        public void GetContext_Environment_RequestProtocol_Filled_Correctly(string protocolVersion, string expectedProtocol)
        {
            AssertEnvironment(
                context => context.MockRequest.SetupGet(r => r.ProtocolVersion).Returns(Version.Parse(protocolVersion)),
                expectedProtocol,
                env => env[EnvironmentKey.RequestProtocol]
            );
        }

        [TestMethod]
        [DataRow("http",  "http")]
        [DataRow("https", "https")]
        public void GetContext_Environment_RequestScheme_Filled_Correctly(string scheme, string expectedScheme)
        {
            AssertEnvironment(
                context => context.MockRequest.SetUrl("/", scheme: scheme, httpListenerUrlUnescapesPath: false),
                expectedScheme,
                env => env[EnvironmentKey.RequestScheme]
            );
        }
    }
}
