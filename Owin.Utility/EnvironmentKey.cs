// Copyright © 2019 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace AWhewell.Owin.Utility
{
    /// <summary>
    /// A collection of standard OWIN environment dictionary keys.
    /// </summary>
    public static class EnvironmentKey
    {
        // Request keys
        public const string RequestBody =           "owin.RequestBody";         // [Required] A Stream with the request body, if any. Stream.Null MAY be used as a placeholder if there is no request body.
        public const string RequestHeaders =        "owin.RequestHeaders";      // [Required] An IDictionary<string, string[]> of request headers.
        public const string RequestMethod =         "owin.RequestMethod";       // [Required] A string containing the HTTP request method of the request (e.g., "GET", "POST").
        public const string RequestPath =           "owin.RequestPath";         // [Required] A string containing the request path. The path MUST be relative to the "root" of the application delegate.
        public const string RequestPathBase =       "owin.RequestPathBase";     // [Required] A string containing the portion of the request path corresponding to the "root" of the application delegate.
        public const string RequestProtocol =       "owin.RequestProtocol";     // [Required] A string containing the protocol name and version (e.g. "HTTP/1.0" or "HTTP/1.1").
        public const string RequestQueryString =    "owin.RequestQueryString";  // [Required] A string containing the query string component of the HTTP request URI, without the leading "?" (e.g., "foo=bar&amp;baz=quux"). The value may be an empty string.
        public const string RequestScheme =         "owin.RequestScheme";       // [Required] A string containing the URI scheme used for the request (e.g., "http", "https").

        // Response keys
        public const string ResponseBody =          "owin.ResponseBody";            // [Required] A Stream used to write out the response body, if any.
        public const string ResponseHeaders =       "owin.ResponseHeaders";         // [Required] An IDictionary<string, string[]> of response headers.
        public const string ResponseStatusCode =    "owin.ResponseStatusCode";      // [Optional] An optional int containing the HTTP response status code as defined in RFC 2616 section 6.1.1. The default is 200.
        public const string ResponseReasonPhrase =  "owin.ResponseReasonPhrase";    // [Optional] An optional string containing the reason phrase associated the given status code. If none is provided then the server SHOULD provide a default as described in RFC 2616 section 6.1.1
        public const string ResponseProtocol =      "owin.ResponseProtocol";        // [Optional] An optional string containing the protocol name and version (e.g. "HTTP/1.0" or "HTTP/1.1"). If none is provided then the "owin.RequestProtocol" key's value is the default.

        // Other keys
        public const string CallCancelled =         "owin.CallCancelled";   // [Required] A CancellationToken indicating if the request has been canceled/aborted.
        public const string Version =               "owin.Version";         // [Required] A string indicating the OWIN version.

        // Common request keys
        public const string ServerIsLocal =         "server.IsLocal";           // [Optional] True if the request was from a browser on the same machine as the server, false otherwise.
        public const string ServerLocalIpAddress =  "server.LocalIpAddress";    // [Optional] The local address that the request was received on, E.G. 127.0.0.1
        public const string ServerLocalPort =       "server.LocalPort";         // [Optional] The local port that the request was received on, E.G. 80
        public const string ServerRemoteIpAddress = "server.RemoteIpAddress";   // [Optional] The IP Address of the remote client, E.G. 192.168.1.1 or ::1
        public const string ServerRemotePort =      "server.RemotePort";        // [Optional] The port that the remote client is taking replies on, E.G. 1234
        public const string SslClientCertificate =  "ssl.ClientCertificate";    // [Optional] Client certificate presented to server in HTTPS connections.
    }
}
