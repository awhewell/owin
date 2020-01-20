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
using System.Threading.Tasks;
using AWhewell.Owin.Interface;
using AWhewell.Owin.Utility;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Test.AWhewell.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    [TestClass]
    public class Pipeline_Tests
    {
        private IClassFactory                       _Snapshot;
        private IPipeline                           _Pipeline;
        private Mock<IPipelineBuilderEnvironment>   _BuilderEnvironment;
        private List<IExceptionLogger>              _ExceptionLoggers;
        private List<Func<AppFunc, AppFunc>>        _MiddlewareBuilders;
        private List<Func<AppFunc, AppFunc>>        _StreamManipulatorBuilders;
        private Dictionary<string, object>          _Environment;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _MiddlewareBuilders = new List<Func<AppFunc, AppFunc>>();
            _StreamManipulatorBuilders = new List<Func<AppFunc, AppFunc>>();
            _ExceptionLoggers = new List<IExceptionLogger>();
            _BuilderEnvironment = MockHelper.FactoryImplementation<IPipelineBuilderEnvironment>();
            _BuilderEnvironment.Setup(r => r.MiddlewareBuilders).Returns(_MiddlewareBuilders);
            _BuilderEnvironment.Setup(r => r.StreamManipulatorBuilders).Returns(_StreamManipulatorBuilders);
            _BuilderEnvironment.Setup(r => r.ExceptionLoggers).Returns(_ExceptionLoggers);

            _Environment = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            _Pipeline = Factory.Resolve<IPipeline>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        private MockExceptionLogger SetupExceptionLogger()
        {
            var result = new MockExceptionLogger();
            _ExceptionLoggers.Add(result);

            return result;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Construct_Throws_If_Passed_Null()
        {
            _Pipeline.Construct(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Construct_Throws_If_Called_Twice()
        {
            _Pipeline.Construct(_BuilderEnvironment.Object);
            _Pipeline.Construct(_BuilderEnvironment.Object);
        }

        [TestMethod]
        public void Construct_Builds_Middleware_AppFunc_And_ProcessRequest_Uses_It()
        {
            var middleware = new MockMiddleware();
            _MiddlewareBuilders.Add(middleware.AppFuncBuilder);
            _Pipeline.Construct(_BuilderEnvironment.Object);

            _Pipeline.ProcessRequest(_Environment).Wait();
            _Pipeline.ProcessRequest(_Environment).Wait();

            Assert.AreEqual(1, middleware.AppFuncBuilderCallCount);
        }

        [TestMethod]
        public void Construct_Builds_ExceptionLoggers_Property()
        {
            var exceptionLogger = SetupExceptionLogger();

            _Pipeline.Construct(_BuilderEnvironment.Object);

            Assert.AreEqual(1, _Pipeline.ExceptionLoggers.Count);
            Assert.AreSame(exceptionLogger, _Pipeline.ExceptionLoggers[0]);
        }

        [TestMethod]
        public void HasStreamManipulators_Is_Clear_Before_Construct_Is_Called()
        {
            Assert.AreEqual(false, _Pipeline.HasStreamManipulators);
        }

        [TestMethod]
        public void HasStreamManipulators_Is_Set_When_Stream_Manipulator_Builders_Are_Added()
        {
            _StreamManipulatorBuilders.Add(new MockMiddleware().AppFuncBuilder);

            _Pipeline.Construct(_BuilderEnvironment.Object);

            Assert.AreEqual(true, _Pipeline.HasStreamManipulators);
        }

        [TestMethod]
        public void HasStreamManipulators_Is_Clear_When_Stream_Manipulator_Builders_Are_Not_Added()
        {
            _Pipeline.Construct(_BuilderEnvironment.Object);

            Assert.AreEqual(false, _Pipeline.HasStreamManipulators);
        }

        [TestMethod]
        public void LogException_Passes_Exception_To_Logger()
        {
            var logger = SetupExceptionLogger();
            _Pipeline.Construct(_BuilderEnvironment.Object);

            var ex = new InvalidOperationException();
            _Pipeline.LogException(ex);

            Assert.AreEqual(1, logger.CallCount);
            Assert.AreSame(ex, logger.LastExceptionLogged);
        }

        [TestMethod]
        public void LogException_Passes_Exception_To_Many_Loggers()
        {
            var logger1 = SetupExceptionLogger();
            var logger2 = SetupExceptionLogger();
            _Pipeline.Construct(_BuilderEnvironment.Object);

            var ex = new InvalidOperationException();
            _Pipeline.LogException(ex);

            Assert.AreEqual(1, logger1.CallCount);
            Assert.AreEqual(1, logger2.CallCount);
        }

        [TestMethod]
        public void LogException_Swallows_Exceptions_In_Logger()
        {
            var logger1 = SetupExceptionLogger();
            var logger2 = SetupExceptionLogger();
            _Pipeline.Construct(_BuilderEnvironment.Object);

            foreach(bool throwInLogger1 in new bool[] { true, false }) {
                logger1.Reset();
                logger2.Reset();

                if(throwInLogger1) {
                    logger1.LogExceptionCallback = ex => throw new InvalidOperationException();
                    logger2.LogExceptionCallback = null;
                } else {
                    logger1.LogExceptionCallback = null;
                    logger2.LogExceptionCallback = ex => throw new InvalidOperationException();
                }

                var ex = new NotImplementedException();
                _Pipeline.LogException(ex);

                if(throwInLogger1) {
                    Assert.AreEqual(1, logger2.CallCount);
                } else {
                    Assert.AreEqual(1, logger1.CallCount);
                }
            }
        }

        [TestMethod]
        public void LogException_No_Url_Does_Nothing_If_No_Loggers_Registered()
        {
            _Pipeline.Construct(_BuilderEnvironment.Object);
            _Pipeline.LogException(new InvalidOperationException());
        }

        [TestMethod]
        public void LogException_No_Url_Does_Nothing_If_Construct_Never_Called()
        {
            var logger = SetupExceptionLogger();
            _Pipeline.LogException(new InvalidOperationException());

            Assert.AreEqual(0, logger.CallCount);
        }

        [TestMethod]
        public void LogException_No_Url_Does_Nothing_If_Passed_Null()
        {
            var logger = SetupExceptionLogger();
            _Pipeline.Construct(_BuilderEnvironment.Object);

            _Pipeline.LogException(null);

            Assert.AreEqual(0, logger.CallCount);
        }

        [TestMethod]
        public void LogException_With_Url_Does_Nothing_If_No_Loggers_Registered()
        {
            _Pipeline.Construct(_BuilderEnvironment.Object);
            _Pipeline.LogException("url", new InvalidOperationException());
        }

        [TestMethod]
        public void LogException_With_Url_Does_Nothing_If_Construct_Never_Called()
        {
            var logger = SetupExceptionLogger();
            _Pipeline.LogException("url", new InvalidOperationException());

            Assert.AreEqual(0, logger.CallCount);
        }

        [TestMethod]
        public void LogException_With_Url_Does_Nothing_If_Passed_Null_Exception()
        {
            var logger = SetupExceptionLogger();
            _Pipeline.Construct(_BuilderEnvironment.Object);

            _Pipeline.LogException("not null", null);

            Assert.AreEqual(0, logger.CallCount);
        }

        [TestMethod]
        public void LogException_With_Url_Logs_Exception_Even_If_Url_Is_Null()
        {
            var logger = SetupExceptionLogger();
            _Pipeline.Construct(_BuilderEnvironment.Object);

            _Pipeline.LogException(null, new InvalidOperationException());

            Assert.AreEqual(1, logger.CallCount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProcessRequest_Throws_If_Passed_Null()
        {
            _Pipeline.Construct(_BuilderEnvironment.Object);

            try {
                _Pipeline.ProcessRequest(null).Wait();
            } catch(AggregateException ex) {
                throw ex.Flatten().InnerException;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ProcessRequest_Throws_If_Not_Yet_Constructed()
        {
            try {
                _Pipeline.ProcessRequest(_Environment).Wait();
            } catch(AggregateException ex) {
                throw ex.Flatten().InnerException;
            }
        }

        [TestMethod]
        public void ProcessRequest_Sets_404_Status_Code_When_Pipeline_Is_Empty()
        {
            _Pipeline.Construct(_BuilderEnvironment.Object);

            _Pipeline.ProcessRequest(_Environment).Wait();

            Assert.AreEqual(404, (int)_Environment[EnvironmentKey.ResponseStatusCode]);
        }

        [TestMethod]
        public void ProcessRequest_Does_Not_Change_Status_Code_If_Already_Set()
        {
            _Pipeline.Construct(_BuilderEnvironment.Object);
            _Environment[EnvironmentKey.ResponseStatusCode] = 201;

            _Pipeline.ProcessRequest(_Environment).Wait();

            Assert.AreEqual(201, (int)_Environment[EnvironmentKey.ResponseStatusCode]);
        }

        [TestMethod]
        public void ProcessRequest_Sets_Status_200_If_Pipeline_Finishes_Early_Without_Setting_Status_And_Without_Stream_Manipulators()
        {
            var middleware = new MockMiddleware {
                ChainToNextAppFunc = false
            };
            _MiddlewareBuilders.Add(middleware.AppFuncBuilder);
            _Pipeline.Construct(_BuilderEnvironment.Object);

            _Pipeline.ProcessRequest(_Environment).Wait();

            Assert.AreEqual(200, (int)_Environment[EnvironmentKey.ResponseStatusCode]);
        }

        [TestMethod]
        public void ProcessRequest_Sets_Status_200_If_Pipeline_Finishes_Early_Without_Setting_Status_And_Before_Calling_Stream_Manipulators()
        {
            var middleware = new MockMiddleware {
                ChainToNextAppFunc = false
            };
            _MiddlewareBuilders.Add(middleware.AppFuncBuilder);
            var streamManipulator = new MockMiddleware {
                Action = () => {
                    Assert.IsTrue(_Environment.ContainsKey(EnvironmentKey.ResponseStatusCode));
                },
            };
            _StreamManipulatorBuilders.Add(streamManipulator.AppFuncBuilder);
            _Pipeline.Construct(_BuilderEnvironment.Object);

            _Pipeline.ProcessRequest(_Environment).Wait();

            Assert.AreEqual(200, (int)_Environment[EnvironmentKey.ResponseStatusCode]);
        }

        [TestMethod]
        public void ProcessRequest_Calls_Middleware()
        {
            var middleware = new MockMiddleware();
            _MiddlewareBuilders.Add(middleware.AppFuncBuilder);
            _Pipeline.Construct(_BuilderEnvironment.Object);

            _Pipeline.ProcessRequest(_Environment).Wait();

            Assert.AreEqual(1, middleware.Environments.Count);
            Assert.AreSame(_Environment, middleware.LastEnvironment);

            Assert.AreEqual(1, middleware.Nexts.Count);
            Assert.IsNotNull(middleware.LastNext);
        }

        [TestMethod]
        public void ProcessRequest_Calls_Two_Middleware()
        {
            var middleware_1 = new MockMiddleware();
            var middleware_2 = new MockMiddleware();
            middleware_1.Action = () => Assert.AreEqual(0, middleware_2.AppFuncCallCount);
            middleware_2.Action = () => Assert.AreEqual(1, middleware_1.AppFuncCallCount);

            _MiddlewareBuilders.Add(middleware_1.AppFuncBuilder);
            _MiddlewareBuilders.Add(middleware_2.AppFuncBuilder);
            _Pipeline.Construct(_BuilderEnvironment.Object);

            _Pipeline.ProcessRequest(_Environment).Wait();

            Assert.AreEqual(1, middleware_1.AppFuncCallCount);
            Assert.AreEqual(1, middleware_2.AppFuncCallCount);
        }

        [TestMethod]
        public void ProcessRequest_Middleware_Can_End_Processing_Early()
        {
            var middleware_1 = new MockMiddleware() { ChainToNextAppFunc = false, };
            var middleware_2 = new MockMiddleware();

            _MiddlewareBuilders.Add(middleware_1.AppFuncBuilder);
            _MiddlewareBuilders.Add(middleware_2.AppFuncBuilder);
            _Pipeline.Construct(_BuilderEnvironment.Object);

            _Pipeline.ProcessRequest(_Environment).Wait();

            Assert.AreEqual(1, middleware_1.AppFuncCallCount);
            Assert.AreEqual(0, middleware_2.AppFuncCallCount);
        }

        [TestMethod]
        public void ProcessRequest_Middleware_Can_End_Chain_In_One_Request_And_Not_End_Chain_In_Another()
        {
            var middleware_1 = new MockMiddleware() { ChainToNextAppFunc = false, };
            var middleware_2 = new MockMiddleware();

            _MiddlewareBuilders.Add(middleware_1.AppFuncBuilder);
            _MiddlewareBuilders.Add(middleware_2.AppFuncBuilder);
            _Pipeline.Construct(_BuilderEnvironment.Object);
            _Pipeline.ProcessRequest(_Environment).Wait();
            middleware_1.ChainToNextAppFunc = true;
            _Pipeline.ProcessRequest(_Environment).Wait();

            Assert.AreEqual(2, middleware_1.AppFuncCallCount);
            Assert.AreEqual(1, middleware_2.AppFuncCallCount);
        }

        [TestMethod]
        public void ProcessRequest_Calls_Stream_Manipulators()
        {
            var middleware = new MockMiddleware();
            _StreamManipulatorBuilders.Add(middleware.AppFuncBuilder);
            _Pipeline.Construct(_BuilderEnvironment.Object);

            _Pipeline.ProcessRequest(_Environment).Wait();

            Assert.AreEqual(1, middleware.Environments.Count);
            Assert.AreSame(_Environment, middleware.LastEnvironment);

            Assert.AreEqual(1, middleware.Nexts.Count);
            Assert.IsNotNull(middleware.LastNext);
        }

        [TestMethod]
        public void ProcessRequest_Calls_Stream_Manipulators_After_Middleware()
        {
            var middleware = new MockMiddleware();
            var streamManipulator = new MockMiddleware();
            middleware.Action = () => Assert.AreEqual(0, streamManipulator.AppFuncCallCount);
            streamManipulator.Action = () => Assert.AreEqual(1, middleware.AppFuncCallCount);

            _MiddlewareBuilders.Add(middleware.AppFuncBuilder);
            _StreamManipulatorBuilders.Add(streamManipulator.AppFuncBuilder);
            _Pipeline.Construct(_BuilderEnvironment.Object);

            _Pipeline.ProcessRequest(_Environment).Wait();

            Assert.AreEqual(1, middleware.AppFuncCallCount);
            Assert.AreEqual(1, streamManipulator.AppFuncCallCount);
        }

        [TestMethod]
        public void ProcessRequest_Does_Not_Let_Stream_Manipulators_Break_The_Chain()
        {
            var middleware_1 = new MockMiddleware() { ChainToNextAppFunc = false, };
            var middleware_2 = new MockMiddleware();

            _StreamManipulatorBuilders.Add(middleware_1.AppFuncBuilder);
            _StreamManipulatorBuilders.Add(middleware_2.AppFuncBuilder);
            _Pipeline.Construct(_BuilderEnvironment.Object);

            _Pipeline.ProcessRequest(_Environment).Wait();

            Assert.AreEqual(1, middleware_1.AppFuncCallCount);
            Assert.AreEqual(1, middleware_2.AppFuncCallCount);
        }

        [TestMethod]
        public void ProcessRequest_Calls_Stream_Manipulators_Even_If_Middleware_Breaks_Chain()
        {
            var middleware_1 = new MockMiddleware() { ChainToNextAppFunc = false, };
            var middleware_2 = new MockMiddleware();
            var streamManipulator = new MockMiddleware();

            _MiddlewareBuilders.Add(middleware_1.AppFuncBuilder);
            _MiddlewareBuilders.Add(middleware_2.AppFuncBuilder);
            _StreamManipulatorBuilders.Add(streamManipulator.AppFuncBuilder);
            _Pipeline.Construct(_BuilderEnvironment.Object);

            _Pipeline.ProcessRequest(_Environment).Wait();

            Assert.AreEqual(1, middleware_1.AppFuncCallCount);
            Assert.AreEqual(0, middleware_2.AppFuncCallCount);
            Assert.AreEqual(1, streamManipulator.AppFuncCallCount);
        }

        [TestMethod]
        public void ProcessRequest_Replaces_Response_Stream_If_Stream_Manipulators_Are_Used()
        {
            using(var hostStream = new MemoryStream()) {
                _Environment[EnvironmentKey.ResponseBody] = hostStream;

                var middleware = new MockMiddleware();
                middleware.Action = () => {
                    var pipelineStream = middleware.LastEnvironment[EnvironmentKey.ResponseBody] as Stream;
                    Assert.IsNotNull(pipelineStream);
                    Assert.AreNotSame(hostStream, pipelineStream);

                    pipelineStream.WriteByte(99);
                };

                _StreamManipulatorBuilders.Add(middleware.AppFuncBuilder);
                _Pipeline.Construct(_BuilderEnvironment.Object);

                _Pipeline.ProcessRequest(_Environment).Wait();

                Assert.AreSame(hostStream, _Environment[EnvironmentKey.ResponseBody]);

                var hostStreamContent = hostStream.ToArray();
                Assert.AreEqual(1, hostStreamContent.Length);
                Assert.AreEqual(99, hostStreamContent[0]);
            }
        }

        [TestMethod]
        public void ProcessRequest_Does_Not_Replace_Stream_If_Host_Stream_Is_Null()
        {
            _Environment[EnvironmentKey.ResponseBody] = Stream.Null;

            var middleware = new MockMiddleware();
            middleware.Action = () => {
                var pipelineStream = middleware.LastEnvironment[EnvironmentKey.ResponseBody] as Stream;
                Assert.IsTrue(pipelineStream == Stream.Null);
            };

            _StreamManipulatorBuilders.Add(middleware.AppFuncBuilder);
            _Pipeline.Construct(_BuilderEnvironment.Object);

            _Pipeline.ProcessRequest(_Environment).Wait();
        }

        [TestMethod]
        public void ProcessRequest_Does_Not_Replace_Stream_If_Stream_Manipulators_Are_Not_Used()
        {
            using(var hostStream = new MemoryStream()) {
                _Environment[EnvironmentKey.ResponseBody] = hostStream;

                var middleware = new MockMiddleware();
                middleware.Action = () => {
                    var pipelineStream = middleware.LastEnvironment[EnvironmentKey.ResponseBody] as Stream;
                    Assert.AreSame(hostStream, pipelineStream);
                };

                _MiddlewareBuilders.Add(middleware.AppFuncBuilder);
                _Pipeline.Construct(_BuilderEnvironment.Object);

                _Pipeline.ProcessRequest(_Environment).Wait();
            }
        }

        [TestMethod]
        public void ProcessRequest_Ignores_Position_Of_Replacement_Stream()
        {
            using(var hostStream = new MemoryStream()) {
                _Environment[EnvironmentKey.ResponseBody] = hostStream;

                var middleware = new MockMiddleware();
                middleware.Action = () => {
                    var pipelineStream = middleware.LastEnvironment[EnvironmentKey.ResponseBody] as Stream;
                    Assert.IsNotNull(pipelineStream);
                    Assert.AreNotSame(hostStream, pipelineStream);

                    pipelineStream.Write(new byte[] { 1, 2, 3 });
                    --pipelineStream.Position;
                };

                _StreamManipulatorBuilders.Add(middleware.AppFuncBuilder);
                _Pipeline.Construct(_BuilderEnvironment.Object);

                _Pipeline.ProcessRequest(_Environment).Wait();

                var hostStreamContent = hostStream.ToArray();
                Assert.AreEqual(3, hostStreamContent.Length);
                Assert.IsTrue(new byte[] { 1, 2, 3 }.SequenceEqual(hostStreamContent));
            }
        }

        [TestMethod]
        public void ProcessRequest_Overrides_Content_Length_Header_If_Using_Replacement_Stream()
        {
            using(var hostStream = new MemoryStream()) {
                _Environment[EnvironmentKey.ResponseBody] = hostStream;
                var responseHeaders = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
                _Environment[EnvironmentKey.ResponseHeaders] = responseHeaders;

                var middleware = new MockMiddleware();
                middleware.Action = () => {
                    var pipelineStream = middleware.LastEnvironment[EnvironmentKey.ResponseBody] as Stream;
                    pipelineStream.Write(new byte[] { 1, 2, 3 });
                    responseHeaders["Content-Length"] = new string[] { "20" };
                    --pipelineStream.Position;
                };

                _StreamManipulatorBuilders.Add(middleware.AppFuncBuilder);
                _Pipeline.Construct(_BuilderEnvironment.Object);

                _Pipeline.ProcessRequest(_Environment).Wait();

                Assert.AreEqual(1,   responseHeaders["Content-Length"].Length);
                Assert.AreEqual("3", responseHeaders["Content-Length"][0]);
            }
        }

        [TestMethod]
        [DataRow(false, false,  0,  true,   null)]
        [DataRow(false, true,   0,  true,   null)]
        [DataRow(true,  false,  1,  true,   null)]
        [DataRow(true,  true,   1,  false,  500)]
        public void ProcessRequest_Deals_With_Exceptions_In_Accordance_With_Builder_Flags(bool exceptionLoggingRequested, bool exceptionSwallowingRequested, int expectedLoggerCallCount, bool expectedSeenException, int? expectedStatusCode)
        {
            _BuilderEnvironment.Object.PipelineLogsExceptions =     exceptionLoggingRequested;
            _BuilderEnvironment.Object.PipelineSwallowsExceptions = exceptionSwallowingRequested;
            var logger = SetupExceptionLogger();
            var middleware = new MockMiddleware();
            var exception = new InvalidOperationException();
            middleware.Action = () => throw exception;
            _MiddlewareBuilders.Add(middleware.AppFuncBuilder);
            _Pipeline.Construct(_BuilderEnvironment.Object);

            var seenException = false;
            int? statusCode = null;
            try {
                _Pipeline.ProcessRequest(_Environment).Wait();
            } catch(AggregateException ex) {
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                seenException = Object.ReferenceEquals(ex.InnerException, exception);
            }
            if(_Environment.ContainsKey(EnvironmentKey.ResponseStatusCode)) {
                statusCode = (int)_Environment[EnvironmentKey.ResponseStatusCode];
            }

            Assert.AreEqual(expectedLoggerCallCount, logger.CallCount);
            Assert.AreEqual(expectedSeenException, seenException);
            Assert.AreEqual(expectedStatusCode, statusCode);
        }

        [TestMethod]
        public void ProcessRequest_Log_Exceptions_Has_No_Effect_If_No_Logger_Registered()
        {
            _BuilderEnvironment.Object.PipelineLogsExceptions = true;
            _BuilderEnvironment.Object.PipelineSwallowsExceptions = true;
            var middleware = new MockMiddleware();
            var exception = new InvalidOperationException();
            middleware.Action = () => throw exception;
            _MiddlewareBuilders.Add(middleware.AppFuncBuilder);
            _Pipeline.Construct(_BuilderEnvironment.Object);

            var seenException = false;
            int? statusCode = null;
            try {
                _Pipeline.ProcessRequest(_Environment).Wait();
            } catch(AggregateException ex) {
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                seenException = Object.ReferenceEquals(ex.InnerException, exception);
            }
            if(_Environment.ContainsKey(EnvironmentKey.ResponseStatusCode)) {
                statusCode = (int)_Environment[EnvironmentKey.ResponseStatusCode];
            }

            Assert.AreEqual(true, seenException);
            Assert.AreEqual(null, statusCode);
        }

        [TestMethod]
        public void ProcessRequest_Exception_Logging_Tries_To_Pass_Request_Url_To_Logger()
        {
            var logger = SetupExceptionLogger();
            _BuilderEnvironment.Object.PipelineLogsExceptions = true;
            _BuilderEnvironment.Object.PipelineSwallowsExceptions = true;
            var middleware = new MockMiddleware();
            var exception = new InvalidOperationException();
            middleware.Action = () => throw exception;
            _MiddlewareBuilders.Add(middleware.AppFuncBuilder);
            _Pipeline.Construct(_BuilderEnvironment.Object);

            _Environment[EnvironmentKey.RequestPathBase]    = "/root";
            _Environment[EnvironmentKey.RequestPath]        = "/path/to/file";
            _Environment[EnvironmentKey.RequestQueryString] = "a=1&b=c%20d";

            _Pipeline.ProcessRequest(_Environment).Wait();

            Assert.AreEqual(1, logger.RequestUrls.Count);
            Assert.AreEqual("/root/path/to/file?a=1&b=c%20d", logger.RequestUrls[0]);
        }

        [TestMethod]
        public void ProcessRequest_Adds_Custom_Key_With_Unique_ID_For_Request()
        {
            var middleware = new MockMiddleware();
            _MiddlewareBuilders.Add(middleware.AppFuncBuilder);
            _Pipeline.Construct(_BuilderEnvironment.Object);
            _Pipeline.ProcessRequest(_Environment).Wait();

            var id1 = (long)_Environment[CustomEnvironmentKey.RequestID];

            TestCleanup();
            TestInitialise();

            middleware = new MockMiddleware();
            _MiddlewareBuilders.Add(middleware.AppFuncBuilder);
            _Pipeline.Construct(_BuilderEnvironment.Object);
            _Pipeline.ProcessRequest(_Environment).Wait();

            var id2 = (long)_Environment[CustomEnvironmentKey.RequestID];

            Assert.AreNotEqual(0L, id1);
            Assert.AreNotEqual(0L, id2);
            Assert.AreNotEqual(id1, id2);
        }
    }
}
