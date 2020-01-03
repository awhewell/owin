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
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading;
using AWhewell.Owin.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin.Utility
{
    [TestClass]
    public class OwinContext_Tests
    {
        private OwinDictionary<object>  _Environment;
        private OwinContext             _Context;

        [TestInitialize]
        public void TestInitialise()
        {
            _Environment = new OwinDictionary<object>();
            _Context = new OwinContext(_Environment);
        }

        private MockOwinEnvironment UseEnvironmentWithRequiredFields()
        {
            var result = new MockOwinEnvironment();
            result.AddRequiredFields();

            _Context = new OwinContext(result.Environment);

            return result;
        }

        private void AssertExceptionThrownOnAction(Action action, Type exceptionType = null)
        {
            exceptionType = exceptionType ?? typeof(InvalidOperationException);

            var exceptionThrown = false;
            try {
                action();
            } catch(TargetInvocationException ex) {
                exceptionThrown = ex.InnerException.GetType() == exceptionType;
            } catch(Exception ex) {
                exceptionThrown = ex.GetType() == exceptionType;
            }

            Assert.IsTrue(exceptionThrown);
        }

        [TestMethod]
        public void Ctor_Tracks_Environment()
        {
            Assert.AreSame(_Environment, _Context.Environment.WrappedDictionary);
        }

        [TestMethod]
        public void Ctor_Generated_Environments_Behave_In_Civilised_Manner()
        {
            foreach(var ctor in new string[] { "default-ctor", "null-dictionary" }) {
                OwinContext context;
                switch(ctor) {
                    case "default-ctor":    context = new OwinContext(); break;
                    case "null-dictionary": context = new OwinContext(null); break;
                    default:                throw new NotImplementedException();
                }
                var errorMessage = $"{nameof(OwinContext)} created using {ctor} method";

                // Will not throw if key is missing
                Assert.IsNull(context.Environment["This.Is.A.Missing.Key"], errorMessage);

                // Is case sensitive
                context.Environment["My.Key"] = "1";
                Assert.AreEqual("1", context.Environment["My.Key"], errorMessage);
                Assert.IsNull(context.Environment["my.key"], errorMessage);
            }
        }

        [TestMethod]
        public void Create_Returns_Correct_Context_And_Caches_Result_For_Future_Calls()
        {
            var context = OwinContext.Create(_Environment);
            Assert.IsNotNull(context);
            Assert.AreSame(context, _Environment[CustomEnvironmentKey.Context]);

            var secondContext = OwinContext.Create(_Environment);
            Assert.AreSame(context, secondContext);
            Assert.AreSame(context, _Environment[CustomEnvironmentKey.Context]);
        }

        [TestMethod]
        public void Create_Works_When_Environment_Is_Null()
        {
            var context = OwinContext.Create(null);
            Assert.IsNotNull(context);
            Assert.AreSame(context, context.Environment[CustomEnvironmentKey.Context]);
        }

        [TestMethod]
        [DataRow(nameof(OwinContext.RequestBody),   EnvironmentKey.RequestBody)]
        [DataRow(nameof(OwinContext.ResponseBody),  EnvironmentKey.ResponseBody)]
        public void Stream_Properties_Expose_Underlying_Stream_Correctly(string propertyName, string environmentKey)
        {
            var propertyInfo = typeof(OwinContext).GetProperty(propertyName);

            // Get and set works correctly...
            using(var stream = new MemoryStream()) {
                // Setter lets you overwrite null
                propertyInfo.SetValue(_Context, stream);
                Assert.AreSame(stream, _Environment[environmentKey]);

                // Getter exposes current content, not what was last set through setter
                _Environment[environmentKey] = Stream.Null;
                Assert.AreSame(Stream.Null, propertyInfo.GetValue(_Context, null));

                // Setter lets you overwrite Stream.Null
                propertyInfo.SetValue(_Context, stream);
                Assert.AreSame(stream, _Environment[environmentKey]);

                // Setter lets you overwrite self
                propertyInfo.SetValue(_Context, stream);
                Assert.AreSame(stream, _Environment[environmentKey]);

                // Setter throws if you overwrite existing stream with new stream
                using(var newStream = new MemoryStream()) {
                    AssertExceptionThrownOnAction(() => propertyInfo.SetValue(_Context, newStream));
                }
            }
        }

        [TestMethod]
        [DataRow(nameof(OwinContext.RequestHeaders),    EnvironmentKey.RequestHeaders)]
        [DataRow(nameof(OwinContext.ResponseHeaders),   EnvironmentKey.ResponseHeaders)]
        public void Headers_Properties_Expose_Underlying_Headers_Correctly(string propertyName, string environmentKey)
        {
            var propertyInfo = typeof(OwinContext).GetProperty(propertyName);
            var dictionary = new Dictionary<string, string[]>();

            // Setter lets you overwrite null
            propertyInfo.SetValue(_Context, dictionary);
            Assert.AreSame(dictionary, propertyInfo.GetValue(_Context, null));

            // Setter lets you overwrite self
            propertyInfo.SetValue(_Context, dictionary);
            Assert.AreSame(dictionary, propertyInfo.GetValue(_Context, null));

            // Setter throws if you overwrite existing dictionary
            AssertExceptionThrownOnAction(() => propertyInfo.SetValue(_Context, new Dictionary<string, string[]>()));
        }

        [TestMethod]
        [DataRow(nameof(OwinContext.RequestHeadersDictionary),  nameof(OwinContext.RequestHeaders),     EnvironmentKey.RequestHeaders,  typeof(RequestHeadersDictionary))]
        [DataRow(nameof(OwinContext.ResponseHeadersDictionary), nameof(OwinContext.ResponseHeaders),    EnvironmentKey.ResponseHeaders, typeof(ResponseHeadersDictionary))]
        public void Dictionary_Wrapper_Properties_Expose_Wrappers_Correctly(string wrapperPropertyName, string headersPropertyName, string environmentKey, Type wrapperType)
        {
            var wrapperPropertyInfo = typeof(OwinContext).GetProperty(wrapperPropertyName);
            var headersPropertyInfo = typeof(OwinContext).GetProperty(headersPropertyName);

            // If the headers are null then the wrapper is null (cannot expose a stub, it would not reflect environment)
            Assert.IsNull(wrapperPropertyInfo.GetValue(_Context, null));

            // Can assign wrapper to underlying headers property and wrapper property will expose same object, it will
            // not create a new wrapper for it.
            var wrapper = Activator.CreateInstance(wrapperType);
            headersPropertyInfo.SetValue(_Context, wrapper);
            Assert.AreSame(wrapper, wrapperPropertyInfo.GetValue(_Context, null));
            Assert.AreSame(wrapper, wrapperPropertyInfo.GetValue(_Context, null));

            TestInitialise();

            // If the headers property is a simple dictionary then a wrapper will be created for it and subsequent
            // fetches return the same wrapper
            var d1 = new HeadersDictionary();
            _Environment[environmentKey] = d1;
            var w1 = wrapperPropertyInfo.GetValue(_Context, null);
            Assert.AreSame(wrapperType, w1.GetType());
            Assert.AreSame(w1, wrapperPropertyInfo.GetValue(_Context, null));

            // If the underlying headers are changed then a new wrapper is created
            var d2 = new HeadersDictionary();
            _Environment[environmentKey] = d2;
            var w2 = wrapperPropertyInfo.GetValue(_Context, null);
            Assert.AreSame(wrapperType, w2.GetType());
            Assert.AreSame(w2, wrapperPropertyInfo.GetValue(_Context, null));
        }

        [TestMethod]
        [DataRow(nameof(OwinContext.RequestMethod),         EnvironmentKey.RequestMethod)]
        [DataRow(nameof(OwinContext.RequestPath),           EnvironmentKey.RequestPath)]
        [DataRow(nameof(OwinContext.RequestPathBase),       EnvironmentKey.RequestPathBase)]
        [DataRow(nameof(OwinContext.RequestProtocol),       EnvironmentKey.RequestProtocol)]
        [DataRow(nameof(OwinContext.RequestQueryString),    EnvironmentKey.RequestQueryString)]
        [DataRow(nameof(OwinContext.RequestScheme),         EnvironmentKey.RequestScheme)]
        [DataRow(nameof(OwinContext.ServerLocalIpAddress),  EnvironmentKey.ServerLocalIpAddress)]
        [DataRow(nameof(OwinContext.ServerLocalPort),       EnvironmentKey.ServerLocalPort)]
        [DataRow(nameof(OwinContext.ServerRemoteIpAddress), EnvironmentKey.ServerRemoteIpAddress)]
        [DataRow(nameof(OwinContext.ServerRemotePort),      EnvironmentKey.ServerRemotePort)]
        [DataRow(nameof(OwinContext.Version),               EnvironmentKey.Version)]
        public void Unchangeable_String_Properties_Expose_Underlying_String_Values_Correctly(string propertyName, string environmentKey)
        {
            var propertyInfo = typeof(OwinContext).GetProperty(propertyName);
            var text = "Abc123";

            // Setter lets you overwrite null
            propertyInfo.SetValue(_Context, text);
            Assert.AreEqual(text, propertyInfo.GetValue(_Context, null));
            Assert.AreEqual(text, _Environment[environmentKey]);

            // Setter lets you overwrite self
            propertyInfo.SetValue(_Context, text);
            Assert.AreEqual(text, propertyInfo.GetValue(_Context, null));

            // Setter throws if you overwrite existing value
            AssertExceptionThrownOnAction(() => propertyInfo.SetValue(_Context, "different text"));
        }

        [TestMethod]
        [DataRow(nameof(OwinContext.ServerIsLocal), EnvironmentKey.ServerIsLocal)]
        public void Unchangeable_Boolean_Properties_Expose_Underlying_Bool_Values_Correctly(string propertyName, string environmentKey)
        {
            var propertyInfo = typeof(OwinContext).GetProperty(propertyName);
            var boolValue = true;

            // Setter lets you overwrite null
            propertyInfo.SetValue(_Context, boolValue);
            Assert.AreEqual(boolValue, propertyInfo.GetValue(_Context, null));
            Assert.AreEqual(boolValue, _Environment[environmentKey]);

            // Setter lets you overwrite self
            propertyInfo.SetValue(_Context, boolValue);
            Assert.AreEqual(boolValue, propertyInfo.GetValue(_Context, null));

            // Setter throws if you overwrite existing value
            AssertExceptionThrownOnAction(() => propertyInfo.SetValue(_Context, !boolValue));
        }

        [TestMethod]
        public void RequestHttpMethod_Is_Parsed_RequestMethod()
        {
            // The implementation is expected to call Parser.ParseHttpMethod so the full range of
            // methods are not tested here. Rather we're just testing that the call happens at
            // the right time.

            Assert.AreEqual(HttpMethod.Unknown, _Context.RequestHttpMethod);

            _Environment[EnvironmentKey.RequestMethod] = "GET";
            Assert.AreEqual(HttpMethod.Get, _Context.RequestHttpMethod);

            _Environment[EnvironmentKey.RequestMethod] = "POST";
            Assert.AreEqual(HttpMethod.Post, _Context.RequestHttpMethod);
        }

        [TestMethod]
        [DataRow(@"/file.txt",                      "/file.txt")]       // Simple case
        [DataRow(null,                              "/")]               // NULL returns root
        [DataRow(@"",                               "/")]               // Empty string returns root
        [DataRow(@"/1/../2",                        "/2")]              // Processes directory traveersal parts
        [DataRow(@"/1/2/3/4/5/6/../../../../3.1",   "/1/2/3.1")]        // Can traverse multiple levels
        [DataRow(@"/1/2/3/../a/b/../../X/y/z",      "/1/2/X/y/z")]      // Can traverse more than one group
        [DataRow(@"/1/2/..",                        "/1/")]             // Handles case when last part is directory up
        [DataRow(@"/1/2/../",                       "/1/")]             // Handles case when last part is folder
        [DataRow(@"/../../../1",                    "/1")]              // Cannot traverse out of root
        [DataRow(@"/1/..",                          "/")]               // Can traverse to root
        [DataRow(@"/..",                            "/")]               // Cannot traverse out of root from root
        [DataRow(@"/1/./2",                         "/1/2")]            // Removes current directory parts
        [DataRow(@"/././1",                         "/1")]              // Removes run of current directory parts
        [DataRow(@"/1/.",                           "/1/")]             // Handles current directory as last part filename
        [DataRow(@"/1/./",                          "/1/")]             // Handles current directory as last part folder
        [DataRow(@"/.",                             "/")]               // Handles current directory part in root
        [DataRow(@"/1//2",                          "/1/2")]            // Removes empty path parts
        [DataRow(@"/1//",                           "/1/")]             // Handles trailing empty path parts
        [DataRow(@"//",                             "/")]               // Removes empty path parts from root
        [DataRow(@"/1/",                            "/1/")]             // Leaves trailing slash intact
        [DataRow(@"/a\b",                           "/a/b")]            // Backslashes are transformed to forward slashes
        [DataRow(@"/..\../a",                       "/a")]              // Cannot use backslashes to traverse out of root
        public void RequestPathFlattened_Returns_Expected_Value(string requestPath, string expected)
        {
            _Environment[EnvironmentKey.RequestPath] = requestPath;

            Assert.AreEqual(expected, _Context.RequestPathFlattened);
        }

        [TestMethod]
        [DataRow(null,      "/")]
        [DataRow(@"",       "/")]
        [DataRow(@"/a",     "/a")]
        [DataRow(@"/a\b",   "/a/b")]
        public void RequestPathNormalised_Returns_Expected_Value(string requestPath, string expected)
        {
            _Environment[EnvironmentKey.RequestPath] = requestPath;

            Assert.AreEqual(expected, _Context.RequestPathNormalised);
        }

        [TestMethod]
        public void RequestPathParts_Responds_To_Changes_In_Underlying_RequestPath()
        {
            var pathParts = _Context.RequestPathParts;
            Assert.AreEqual(0, pathParts.Length);

            _Environment[EnvironmentKey.RequestPath] = "/a/b";

            pathParts = _Context.RequestPathParts;
            Assert.AreSame(pathParts, _Context.RequestPathParts);   // Doesn't keep creating new instances
            Assert.AreEqual(2, pathParts.Length);
            Assert.AreEqual("a", pathParts[0]);
            Assert.AreEqual("b", pathParts[1]);

            // Can cope with change to underlying path
            _Environment[EnvironmentKey.RequestPath] = "/c";

            pathParts = _Context.RequestPathParts;
            Assert.AreEqual(1, pathParts.Length);
            Assert.AreEqual("c", pathParts[0]);
        }

        [TestMethod]
        public void RequestHttpProtocol_Is_Parsed_RequestProtocol()
        {
            // The implementation is expected to call Parser.ParseHttpProtocol so the full range of
            // methods are not tested here. Rather we're just testing that the call happens at
            // the right time.

            Assert.AreEqual(HttpProtocol.Unknown, _Context.RequestHttpProtocol);

            _Environment[EnvironmentKey.RequestProtocol] = "HTTP/1.0";
            Assert.AreEqual(HttpProtocol.Http1_0, _Context.RequestHttpProtocol);

            _Environment[EnvironmentKey.RequestProtocol] = "HTTP/1.1";
            Assert.AreEqual(HttpProtocol.Http1_1, _Context.RequestHttpProtocol);
        }

        [TestMethod]
        public void RequestQueryStringDictionary_Exposes_Wrapper_Around_RequestQueryString()
        {
            foreach(var caseSensitive in new bool[] { true, false }) {
                TestInitialise();

                var initial = _Context.RequestQueryStringDictionary(caseSensitive);
                Assert.AreEqual(0, initial.Count);
                Assert.AreSame(initial, _Context.RequestQueryStringDictionary(caseSensitive));

                _Environment[EnvironmentKey.RequestQueryString] = "a=1&A=2";

                var changed = _Context.RequestQueryStringDictionary(caseSensitive);
                Assert.AreNotSame(initial, changed);
                Assert.AreSame(changed, _Context.RequestQueryStringDictionary(caseSensitive));
                if(!caseSensitive) {
                    Assert.AreEqual(1, changed.Count);
                    Assert.AreEqual("1,2", changed.GetValue("a"));
                    Assert.AreEqual("1,2", changed.GetValue("A"));
                } else {
                    Assert.AreEqual(2, changed.Count);
                    Assert.AreEqual("1", changed.GetValue("a"));
                    Assert.AreEqual("2", changed.GetValue("A"));
                }

                var flippedIsCaseSensitive = !caseSensitive;
                var flipped = _Context.RequestQueryStringDictionary(flippedIsCaseSensitive);
                Assert.AreNotSame(changed, flipped);
                Assert.AreSame(flipped, _Context.RequestQueryStringDictionary(flippedIsCaseSensitive));
                Assert.AreEqual(flippedIsCaseSensitive ? 2 : 1, flipped.Count);
            }
        }

        [TestMethod]
        public void RequestHttpScheme_Is_Parsed_RequestScheme()
        {
            Assert.AreEqual(HttpScheme.Unknown, _Context.RequestHttpScheme);

            _Environment[EnvironmentKey.RequestScheme] = "http";
            Assert.AreEqual(HttpScheme.Http, _Context.RequestHttpScheme);

            _Environment[EnvironmentKey.RequestScheme] = "https";
            Assert.AreEqual(HttpScheme.Https, _Context.RequestHttpScheme);
        }

        [TestMethod]
        public void RequestPrincipal_Is_Null_By_Default()
        {
            Assert.IsNull(_Context.RequestPrincipal);
        }

        [TestMethod]
        public void RequestPrincipal_Can_Be_Assigned()
        {
            var user = new GenericPrincipal(
                new GenericIdentity("foo", "basic"),
                new string[] { "Admin" }
            );

            // Can overwrite principal when it's null
            _Context.RequestPrincipal = user;
            Assert.AreSame(user, _Context.RequestPrincipal);
            Assert.AreSame(user, _Environment[CustomEnvironmentKey.Principal]);

            // Can overwrite principal when it's the same value
            _Context.RequestPrincipal = user;
            Assert.AreSame(user, _Context.RequestPrincipal);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RequestPrincipal_Cannot_Be_Changed_Once_Assigned()
        {
            _Context.RequestPrincipal = new GenericPrincipal(new GenericIdentity("foo", "basic"), new string[] {});
            _Context.RequestPrincipal = new GenericPrincipal(new GenericIdentity("foo2", "basic"), new string[] {});
        }

        [TestMethod]
        public void ResponseStatusCode_Exposes_Underlying_Status_Code()
        {
            Assert.IsNull(_Context.ResponseStatusCode);

            _Environment[EnvironmentKey.ResponseStatusCode] = 1;
            Assert.AreEqual(1, _Context.ResponseStatusCode);

            _Context.ResponseStatusCode = 2;
            Assert.AreEqual(2, _Environment[EnvironmentKey.ResponseStatusCode]);
        }

        [TestMethod]
        public void ResponseHttpStatusCode_Exposes_Underlying_Status_Code()
        {
            Assert.IsNull(_Context.ResponseHttpStatusCode);

            _Environment[EnvironmentKey.ResponseStatusCode] = 404;
            Assert.AreEqual(HttpStatusCode.NotFound, _Context.ResponseHttpStatusCode);

            _Context.ResponseHttpStatusCode = HttpStatusCode.Forbidden;
            Assert.AreEqual(403, _Environment[EnvironmentKey.ResponseStatusCode]);
        }

        [TestMethod]
        public void ResponseReasonPhrase_Exposes_Underlying_Reason_Phrase()
        {
            Assert.IsNull(_Context.ResponseReasonPhrase);

            _Environment[EnvironmentKey.ResponseReasonPhrase] = "a";
            Assert.AreEqual("a", _Context.ResponseReasonPhrase);

            _Context.ResponseReasonPhrase = "b";
            Assert.AreEqual("b", _Environment[EnvironmentKey.ResponseReasonPhrase]);
        }

        [TestMethod]
        public void ResponseProtocol_Exposes_Underlying_Protocol()
        {
            Assert.IsNull(_Context.ResponseProtocol);

            _Environment[EnvironmentKey.ResponseProtocol] = "a";
            Assert.AreEqual("a", _Context.ResponseProtocol);

            _Context.ResponseProtocol = "b";
            Assert.AreEqual("b", _Environment[EnvironmentKey.ResponseProtocol]);
        }

        [TestMethod]
        public void CallCancelled_Exposes_Underlying_Cancellation_Token_Correctly()
        {
            Assert.IsNull(_Context.CallCancelled);

            // Can set when null
            var token = new CancellationToken();        // <-- note this is a struct...
            _Context.CallCancelled = token;
            Assert.AreEqual(token, _Environment[EnvironmentKey.CallCancelled]);
            Assert.AreEqual(token, _Context.CallCancelled);

            // Can overwrite with self
            _Context.CallCancelled = token;
            Assert.AreEqual(token, _Environment[EnvironmentKey.CallCancelled]);

            // Cannot change
            AssertExceptionThrownOnAction(() => _Context.CallCancelled = new CancellationToken(true));
        }

        [TestMethod]
        public void SslClientCertificate_Exposes_Underlying_SslClientCertificate()
        {
            using(var c1 = new X509Certificate()) {
                Assert.IsNull(_Context.SslClientCertificate);

                // Can change null value
                _Context.SslClientCertificate = c1;
                Assert.AreSame(c1, _Environment[EnvironmentKey.SslClientCertificate]);
                Assert.AreSame(c1, _Context.SslClientCertificate);

                // Can overwrite with self
                _Context.SslClientCertificate = c1;
                Assert.AreSame(c1, _Environment[EnvironmentKey.SslClientCertificate]);
                Assert.AreSame(c1, _Context.SslClientCertificate);

                // Cannot change to new value
                //
                // This is a bit tricky to unit test because X509Certificate has a custom Equals
                // that compares properties so two default ctor certificates compare as equal and
                // the exception isn't triggered. Creating certificates that compare as unequal
                // involves creating self-signed X509 certificates either within the test (bit
                // tricky on the face of it) or shipping them with the tests.
                //
                // I'm not *too* fussed if the "throw exception if they try to overwrite" code
                // isn't working, it's just a nicety - you can always defeat it by just writing
                // directly to the underlying OWIN environment dictionary. As long as we can show
                // that we can overwrite the null / empty state and assign with self then that
                // will be OK.
                //
                // This will not throw the expected exception:
                // using(var c2 = new X509Certificate()) {
                //     AssertExceptionThrownOnAction(() => _Context.SslClientCertificate = c2);
                // }
            }
        }

        [TestMethod]
        [DataRow(null,      "UTF-8",    "text/plain")]
        [DataRow("",        "UTF-32",   "text/plain")]
        [DataRow("1",       "UTF-8",    "text/plain")]
        [DataRow("Hello",   "UTF-7",    "application/json")]
        public void ReturnText_Returns_Text_In_Body(string text, string encodingName, string mimeType)
        {
            var mockEnvironment = UseEnvironmentWithRequiredFields();
            var encoding = Encoding.GetEncoding(encodingName);

            _Context.ReturnText(text, encoding, mimeType);

            var actualBody = encoding.GetString(mockEnvironment.ResponseBodyBytes);
            var headers = mockEnvironment.ResponseHeadersDictionary;
            var actualContentLength = (int)(headers.ContentLength ?? -1);
            var actualContentType = headers.ContentTypeValue;
            var actualStatusCode = (int?)mockEnvironment.Environment[EnvironmentKey.ResponseStatusCode];

            Assert.AreEqual(text ?? "",                             actualBody);
            Assert.AreEqual(encoding.GetBytes(text ?? "").Length,   actualContentLength);
            Assert.AreEqual(mimeType,                               actualContentType.MediaType);
            Assert.AreEqual(encoding.WebName,                       actualContentType.Charset, ignoreCase: true);

            // We should not be setting status code here, this should just be about setting up the body
            Assert.IsNull(actualStatusCode);
        }

        [TestMethod]
        public void ReturnText_Silently_Fails_If_Environment_Not_Set_Up_By_Host()
        {
            _Context.ReturnText("Hello", Encoding.UTF8, "text/plain");

            Assert.AreEqual(0, _Environment.Count);
        }

        [TestMethod]
        public void ReturnText_Assumes_UTF8_If_Encoding_Is_Null()
        {
            UseEnvironmentWithRequiredFields();

            _Context.ReturnText("1", null, "text/plain");

            Assert.AreEqual("UTF-8", _Context.ResponseHeadersDictionary.ContentTypeValue.Charset, ignoreCase: true);
        }

        [TestMethod]
        public void ReturnText_Assumes_PlainText_If_MimeType_Is_Null()
        {
            UseEnvironmentWithRequiredFields();

            _Context.ReturnText("1", Encoding.UTF7, null);

            Assert.AreEqual("text/plain", _Context.ResponseHeadersDictionary.ContentTypeValue.MediaType);
        }

        [TestMethod]
        public void ReturnText_Assumes_PlainText_If_MimeType_Is_Empty()
        {
            UseEnvironmentWithRequiredFields();

            _Context.ReturnText("1", Encoding.UTF7, "");

            Assert.AreEqual("text/plain", _Context.ResponseHeadersDictionary.ContentTypeValue.MediaType);
        }

        [TestMethod]
        [DataRow(null,                   -1, new byte[0],            -1)]
        [DataRow(new byte[] { 1, 2, 3 }, 0,  new byte[] { 1, 2, 3 }, 3)]
        [DataRow(new byte[] { 1, 2, 3 }, 1,  new byte[] { 2, 3 },    3)]
        public void RequestBodyBytes_Returns_Content_Of_Stream(byte[] streamContent, int startPosition, byte[] expectedContent, int expectedPosition)
        {
            var env = UseEnvironmentWithRequiredFields();
            using(var stream = new MemoryStream()) {
                if(streamContent != null) {
                    stream.Write(streamContent, 0, streamContent.Length);
                    stream.Position = startPosition;
                    env.Environment[EnvironmentKey.RequestBody] = stream;
                }

                var actualContent = _Context.RequestBodyBytes();
                var actualPosition = stream.Position;

                Assertions.AreEqual(expectedContent, actualContent);
                if(expectedPosition != -1) {
                    Assert.AreEqual(expectedPosition, actualPosition);
                }
            }
        }

        [TestMethod]
        public void RequestBodyBytes_Caches_Result_And_Reuses_Cached_Result()
        {
            var env = UseEnvironmentWithRequiredFields();
            env.AddRequestBody(new byte[] { 1, 2, 3 }, new ContentTypeValue("text/plain"));

            var result1 = _Context.RequestBodyBytes();
            var result2 = _Context.RequestBodyBytes();

            Assert.AreSame(result1, result2);
        }

        [TestMethod]
        public void RequestBodyBytes_Refreshes_Cache_If_Stream_Changes()
        {
            var env = UseEnvironmentWithRequiredFields();

            env.AddRequestBody(new byte[] { 1, 2, 3 }, new ContentTypeValue("text/plain"));
            var result1 = _Context.RequestBodyBytes();

            env.AddRequestBody(new byte[] { 1, 2, 3 }, new ContentTypeValue("text/plain"));
            var result2 = _Context.RequestBodyBytes();

            Assertions.AreEqual(result1, result2);
            Assert.AreNotSame(result1, result2);
        }

        [TestMethod]
        [DataRow(null,                                  null,       "")]
        [DataRow(new byte[] { 0xC2, 0xA3, 0x31 },       null,       "£1")]
        [DataRow(new byte[] { 0xA3, 0x00, 0x31, 0x00 }, "Unicode",  "£1")]
        public void RequestBodyText_Uses_Correct_Encoding(byte[] contentBytes, string charset, string expected)
        {
            if(contentBytes != null) {
                var env = UseEnvironmentWithRequiredFields();
                env.AddRequestBody(contentBytes, new ContentTypeValue("does-not-matter", charset));
            }

            var actual = _Context.RequestBodyText();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(UnknownCharsetException))]
        public void RequestBodyText_Throws_Exception_If_Encoding_Is_Unusable()
        {
            var env = UseEnvironmentWithRequiredFields();
            env.AddRequestBody(
                new byte[] { 0xA3, 0x00, 0x31, 0x00 },
                new ContentTypeValue("does-not-matter", charset: "who-knows")
            );

            _Context.RequestBodyText();
        }

        [TestMethod]
        public void RequestBodyForm_Returns_Form_With_Case_Sensitive_Keys()
        {
            var env = UseEnvironmentWithRequiredFields();
            env.SetRequestBody("value=1&Value=2");

            var form = _Context.RequestBodyForm(caseSensitiveKeys: true);

            Assert.AreEqual(2, form.Count);
            Assert.AreEqual("1", form.GetValue("value"));
            Assert.AreEqual("2", form.GetValue("Value"));
        }

        [TestMethod]
        public void RequestBodyForm_Returns_Form_With_Case_Insensitive_Keys()
        {
            var env = UseEnvironmentWithRequiredFields();
            env.SetRequestBody("value=1&Value=2");

            var form = _Context.RequestBodyForm(caseSensitiveKeys: false);

            Assert.AreEqual(1, form.Count);
            Assert.AreEqual("1,2", form.GetValue("VALUE"));
        }

        [TestMethod]
        [DataRow("192.168.0.1", null,                   null,             "192.168.0.1",  false)]     // Without an X-Forwarded-For header all requests from the LAN are considered to be local or LAN
        [DataRow("192.168.0.1", "1.2.3.4",              "192.168.0.1",  "1.2.3.4",      true)]      // X-Forwarded-For headers from LAN requests are used (remote client)
        [DataRow("192.168.0.1", "192.168.0.2",          "192.168.0.1",  "192.168.0.2",  false)]     // X-Forwarded-For headers from LAN requests are used (LAN client)
        [DataRow("1.2.3.4",     "192.168.0.1",          null,           "1.2.3.4",      true)]      // X-Forwarded-For headers from non-LAN requests are ignored
        [DataRow("192.168.0.1", "192.168.0.2, 1.2.3.4", "192.168.0.1",  "1.2.3.4",      true)]      // Only the last entry in the XFF header is considered
        [DataRow("192.168.0.1", "garbage",              null,           "192.168.0.1",  false)]     // If the entire XFF header is unparseable then it is ignored…
        [DataRow("192.168.0.1", "garbage, 1.2.3.4",     "192.168.0.1",  "1.2.3.4",      true)]      // If the last entry in the header is parseable then it is used
        public void Request_IpAddress_Is_Resolved_Correctly(string serverRemoteIpAddress, string xForwardedFor, string expectedProxyIpAddress, string expectedClientIpAddress, bool expectedIsInternet)
        {
            var expectedClientIpAddressParsed = IPAddress.Parse(expectedClientIpAddress);

            var env = UseEnvironmentWithRequiredFields();
            env.Environment[EnvironmentKey.ServerRemoteIpAddress] = serverRemoteIpAddress;
            env.RequestHeaders["X-Forwarded-For"] = xForwardedFor;

            var actualProxyIpAddress =          _Context.ProxyIpAddress;
            var actualClientIpAddress =         _Context.ClientIpAddress;
            var actualClientIpAddressParsed =   _Context.ClientIpAddressParsed;
            var actualIsInternet =              _Context.IsInternet;
            var actualIsLocalOrLan =            _Context.IsLocalOrLan;

            Assert.AreEqual(expectedProxyIpAddress,         actualProxyIpAddress);
            Assert.AreEqual(expectedClientIpAddress,        actualClientIpAddress);
            Assert.AreEqual(expectedClientIpAddressParsed,  actualClientIpAddressParsed);
            Assert.AreEqual(expectedIsInternet,             actualIsInternet);
            Assert.AreEqual(!expectedIsInternet,            actualIsLocalOrLan);
        }
    }
}
