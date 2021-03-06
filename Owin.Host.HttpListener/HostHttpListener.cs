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
using System.Net;
using System.Threading;
using AWhewell.Owin.Interface;
using AWhewell.Owin.Interface.Host.HttpListener;
using AWhewell.Owin.Interface.Host.HttpListener.HttpListenerWrapper;
using AWhewell.Owin.Utility;
using InterfaceFactory;

namespace AWhewell.Owin.Host.HttpListener
{
    /// <summary>
    /// The default implementation of <see cref="IHostHttpListener"/>.
    /// </summary>
    class HostHttpListener : IHostHttpListener
    {
        /// <summary>
        /// The listener that the host uses.
        /// </summary>
        private IHttpListener _HttpListener;

        /// <summary>
        /// The pipeline that the host sends messages down.
        /// </summary>
        private IPipeline _Pipeline;

        private string _Root = "/";
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Root
        {
            get => _Root;
            set {
                var root = value ?? "/";
                if(!root.StartsWith("/")) {
                    root = $"/{root}";
                }
                if(root.EndsWith("/") && root.Length > 1) {
                    root = root.Substring(0, root.Length - 1);
                }
                _Root = root;

                SetPrefix();
            }
        }

        private int _Port = 80;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public int Port
        {
            get => _Port;
            set {
                _Port = value;
                SetPrefix();
            }
        }

        private bool _UseStrongWildcard;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool UseStrongWildcard
        {
            get => _UseStrongWildcard;
            set {
                _UseStrongWildcard = value;
                SetPrefix();
            }
        }

        private bool _BindToLocalhost;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool BindToLocalhost
        {
            get => _BindToLocalhost;
            set {
                _BindToLocalhost = value;
                SetPrefix();
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsListening => _HttpListener.IsListening;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<RequestProcessedEventArgs> RequestProcessed;

        /// <summary>
        /// Raises <see cref="RequestProcessed"/>.
        /// </summary>
        /// <param name="requestID"></param>
        protected virtual void OnRequestProcessed(long requestID)
        {
            if(RequestProcessed != null) {
                var args = new RequestProcessedEventArgs(requestID);
                RequestProcessed?.Invoke(this, args);
            }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public HostHttpListener()
        {
            _HttpListener = Factory.Resolve<IHttpListener>();
            SetPrefix();
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~HostHttpListener()
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
        /// Finalises or disposes of the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                _HttpListener.Dispose();
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="environment"></param>
        public void Initialise(IPipelineBuilder builder, IPipelineBuilderEnvironment environment)
        {
            if(builder == null) {
                throw new ArgumentNullException(nameof(builder));
            }
            if(environment == null) {
                throw new ArgumentNullException(nameof(environment));
            }
            if(_Pipeline != null) {
                throw new InvalidOperationException("You cannot initialise a host twice");
            }

            Action hostFinalCallback = () => {
                environment.PipelineLogsExceptions = false;
                environment.PipelineSwallowsExceptions = false;
            };

            environment.Properties[ApplicationStartupKey.HostType] =            "AWhewell.Owin.Host.HttpListener";
            environment.Properties[ApplicationStartupKey.Version] =             Constants.Version;
            environment.Properties[ApplicationStartupKey.HostFinalCallback] =   hostFinalCallback;

            _Pipeline = builder.CreatePipeline(environment);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Start()
        {
            if(_Pipeline == null) {
                throw new InvalidOperationException("You must initialise the host before starting it");
            }

            if(!IsListening) {
                _HttpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous | AuthenticationSchemes.Basic;
                _HttpListener.IgnoreWriteExceptions = true;
                _HttpListener.Start();
                _HttpListener.BeginGetContext(ContextReceived, _HttpListener);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Stop()
        {
            if(IsListening) {
                _HttpListener.Stop();
            }
        }

        /// <summary>
        /// Sets the prefix for the HttpListener.
        /// </summary>
        private void SetPrefix()
        {
            if(_HttpListener.Prefixes.Count > 0) {
                _HttpListener.Prefixes.Clear();
            }
            _HttpListener.Prefixes.Add(
                String.Format("http://{0}:{1}{2}/",
                    BindToLocalhost ? "localhost"
                                    : UseStrongWildcard ? "+" : "*",
                    Port,
                    Root == "/" ? "" : Root
                )
            );
        }

        /// <summary>
        /// Called on a background thread when a request is received.
        /// </summary>
        /// <param name="asyncResult"></param>
        private void ContextReceived(IAsyncResult asyncResult)
        {
            var listener = (IHttpListener)asyncResult.AsyncState;
            if(listener?.IsListening ?? false) {
                var fetchNextContext = true;

                IHttpListenerContext context;
                try {
                    context = listener.EndGetContext(asyncResult);
                } catch(HttpListenerException) {
                    context = null;
                } catch {
                    fetchNextContext = false;
                    context = null;
                }

                if(fetchNextContext) {
                    try {
                        _HttpListener?.BeginGetContext(ContextReceived, _HttpListener);
                    } catch {
                        context = null;
                    }
                }

                if(context != null) {
                    long? requestID = null;
                    try {
                        requestID = ProcessRequest(context);
                    } catch(Exception ex) {
                        _Pipeline.LogException(context?.Request?.RawUrl, ex);
                    }

                    try {
                        // Do not call Close before Dispose. It will close random connections on Mono and it is redundant on .NET framework.
                        // Calling Close instead of Dispose will leave connections hanging open on Mono.
                        context.Response.Dispose();
                    } catch {
                        ; // This will get hit a lot, the close call will fail if clients close connections etc.
                    }

                    try {
                        if(requestID != null) {
                            OnRequestProcessed(requestID.Value);
                        }
                    } catch {
                    }
                }
            }
        }

        private long? ProcessRequest(IHttpListenerContext context)
        {
            long? requestID = null;

            var root = Root;
            var pathQuery = SplitRawUrlIntoPathAndQuery(context.Request.RawUrl);
            if(IsRequestValid(root, pathQuery)) {
                var cancellationToken = new CancellationToken();
                var environment = OwinEnvironmentFromContext(context, cancellationToken, root, pathQuery);

                try {
                    _Pipeline.ProcessRequest(environment).Wait();
                    requestID = environment[CustomEnvironmentKey.RequestID] as long?;
                } catch(AggregateException agEx) {
                    if(agEx.InnerExceptions.Count != 1) {
                        throw;
                    }

                    // These can happen when the connection is reset by the client while the stream is being written
                    if(agEx.InnerException is HttpListenerException) {
                        ;
                    } else if(agEx.InnerException is ObjectDisposedException objDispEx && objDispEx.ObjectName == nameof(HttpListenerResponse)) {
                        ;
                    } else {
                        throw;
                    }
                }
            }

            return requestID;
        }

        private Tuple<string, string> SplitRawUrlIntoPathAndQuery(string rawUrl)
        {
            var path = String.Empty;
            var query = String.Empty;

            if(!String.IsNullOrEmpty(rawUrl)) {
                var queryIdx = rawUrl.IndexOf('?');
                if(queryIdx == -1) {
                    path = rawUrl;
                } else {
                    path = rawUrl.Substring(0, queryIdx);
                    query = rawUrl.Substring(queryIdx + 1);
                }

                path = OwinConvert.UrlDecode(path);
            }

            return new Tuple<string, string>(path, query);
        }

        private bool IsRequestValid(string root, Tuple<string,string> pathQuery)
        {
            var path = pathQuery?.Item1;

            var result = String.Equals(path, root, StringComparison.OrdinalIgnoreCase)
                      || path.StartsWith(root == "/" ? root : root + '/', StringComparison.OrdinalIgnoreCase);

            return result;
        }

        private IDictionary<string, object> OwinEnvironmentFromContext(IHttpListenerContext context, CancellationToken cancellationToken, string root, Tuple<string,string> pathQuery)
        {
            var request = context.Request;
            var response = context.Response;

            var path = pathQuery.Item1;
            var query = pathQuery.Item2;

            var result = OwinContext.Create(new HttpListenerEnvironment(context));
            result.Version =                Constants.Version;
            result.CallCancelled =          cancellationToken;
            result.RequestMethod =          request.HttpMethod;
            result.RequestBody =            !request.HasEntityBody ? Stream.Null : request.InputStream;
            result.RequestHeaders =         new HeadersWrapper_Request(request.Headers);
            result.RequestPathBase =        root == "/" ? "" : root;
            result.RequestPath =            root == "/" ? path : path.Substring(root.Length);
            result.RequestQueryString =     query;
            result.RequestProtocol =        $"HTTP/{request.ProtocolVersion}";
            result.RequestScheme =          request.Url.Scheme;

            result.ResponseBody =           response.OutputStream;
            result.ResponseHeaders =        new HeadersWrapper_Response(response);

            result.ServerIsLocal =          request.IsLocal;
            result.ServerLocalIpAddress =   request.LocalEndPoint?.Address?.ToString();
            result.ServerLocalPortNumber =  request.LocalEndPoint?.Port;
            result.ServerRemoteIpAddress =  request.RemoteEndPoint?.Address?.ToString();
            result.ServerRemotePortNumber = request.RemoteEndPoint?.Port;

            if(String.Equals(request.Url.Scheme, "https", StringComparison.OrdinalIgnoreCase)) {
                result.SslClientCertificate = request.GetClientCertificate();
            }

            return result.Environment;
        }
    }
}
