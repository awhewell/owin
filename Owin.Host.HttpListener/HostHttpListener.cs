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
using System.IO;
using System.Net;
using System.Threading;
using InterfaceFactory;
using Owin.Interface;
using Owin.Interface.HttpListenerWrapper;

namespace Owin.Host.HttpListener
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
        /// Creates a new object.
        /// </summary>
        public HostHttpListener()
        {
            _HttpListener = Factory.Resolve<IHttpListener>();
            _HttpListener.IgnoreWriteExceptions = true;
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
        public void Initialise()
        {
            if(_Pipeline != null) {
                throw new InvalidOperationException("You cannot initialise a host twice");
            }

            var pipelineBuilder = Factory.ResolveSingleton<IPipelineBuilder>();
            var builderEnvironment = Factory.Resolve<IPipelineBuilderEnvironment>();

            builderEnvironment.Properties[ApplicationStartupKey.HostType] = "Owin.Host.HttpListener";
            builderEnvironment.Properties[ApplicationStartupKey.Version] =  Constants.Version;

            _Pipeline = pipelineBuilder.CreatePipeline(builderEnvironment);
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
                _HttpListener.Start();
                _HttpListener.BeginGetContext(ContextReceived, null);
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
            var listener = _HttpListener;
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
                        _HttpListener?.BeginGetContext(ContextReceived, null);
                    } catch {
                        context = null;
                    }
                }

                var root = Root;
                var pathQuery = context == null ? null : SplitRawUrlIntoPathAndQuery(context.Request.RawUrl);
                if(IsRequestValid(context, root, pathQuery)) {
                    var cancellationToken = new CancellationToken();
                    var environment = OwinEnvironmentFromContext(context, cancellationToken, root, pathQuery);
                    _Pipeline.ProcessRequest(environment);
                }
            }
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

                path = Uri.UnescapeDataString(path);
            }

            return new Tuple<string, string>(path, query);
        }

        private bool IsRequestValid(IHttpListenerContext context, string root, Tuple<string,string> pathQuery)
        {
            var result = context != null;
            var path = pathQuery?.Item1;

            result = result &&
            (      String.Equals(path, root, StringComparison.OrdinalIgnoreCase)
                || path.StartsWith(root == "/" ? root : root + '/', StringComparison.OrdinalIgnoreCase)
            );

            return result;
        }

        private OwinDictionary<object> OwinEnvironmentFromContext(IHttpListenerContext context, CancellationToken cancellationToken, string root, Tuple<string,string> pathQuery)
        {
            var request = context.Request;

            var result = new OwinDictionary<object> {
                { EnvironmentKey.Version,       Constants.Version },
                { EnvironmentKey.CallCancelled, cancellationToken },
                { EnvironmentKey.RequestMethod, request.HttpMethod },
            };

            result[EnvironmentKey.RequestBody] = !request.HasEntityBody ? Stream.Null : request.InputStream;

            var headerDictionary = new HeadersDictionary();
            foreach(var headerKey in request.Headers.AllKeys) {
                headerDictionary[headerKey] = request.Headers[headerKey];
            }
            result[EnvironmentKey.RequestHeaders] = headerDictionary;

            var path = pathQuery.Item1;
            var query = pathQuery.Item2;

            result[EnvironmentKey.RequestPathBase] = root == "/" ? "" : root;
            result[EnvironmentKey.RequestPath] = root == "/" ? path : path.Substring(root.Length);
            result[EnvironmentKey.RequestQueryString] = query;
            result[EnvironmentKey.RequestProtocol] = $"HTTP/{request.ProtocolVersion}";
            result[EnvironmentKey.RequestScheme] = request.Url.Scheme;

            return result;
        }
    }
}
