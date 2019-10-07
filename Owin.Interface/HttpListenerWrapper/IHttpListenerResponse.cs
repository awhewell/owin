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
using System.Text;

namespace Owin.Interface.HttpListenerWrapper
{
    /// <summary>
    /// The interface for wrappers around an HttpListenerResponse.
    /// </summary>
    public interface IHttpListenerResponse
    {
        //
        // Summary:
        //     Gets or sets the number of bytes in the body data included in the response.
        //
        // Returns:
        //     The value of the response's Content-Length header.
        //
        // Exceptions:
        //   T:System.ArgumentOutOfRangeException:
        //     The value specified for a set operation is less than zero.
        //
        //   T:System.InvalidOperationException:
        //     The response is already being sent.
        //
        //   T:System.ObjectDisposedException:
        //     This object is closed.
        long ContentLength64 { get; set; }

        //
        // Summary:
        //     Gets or sets whether the response uses chunked transfer encoding.
        //
        // Returns:
        //     true if the response is set to use chunked transfer encoding; otherwise, false.
        //     The default is false.
        bool SendChunked { get; set; }

        //
        // Summary:
        //     Gets or sets the value of the HTTP Location header in this response.
        //
        // Returns:
        //     A System.String that contains the absolute URL to be sent to the client in the
        //     Location header.
        //
        // Exceptions:
        //   T:System.ArgumentException:
        //     The value specified for a set operation is an empty string ("").
        //
        //   T:System.ObjectDisposedException:
        //     This object is closed.
        string RedirectLocation { get; set; }

        //
        // Summary:
        //     Gets or sets the HTTP version used for the response.
        //
        // Returns:
        //     A System.Version object indicating the version of HTTP used when responding to
        //     the client. Note that this property is now obsolete.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     The value specified for a set operation is null.
        //
        //   T:System.ArgumentException:
        //     The value specified for a set operation does not have its System.Version.Major
        //     property set to 1 or does not have its System.Version.Minor property set to either
        //     0 or 1.
        //
        //   T:System.ObjectDisposedException:
        //     This object is closed.
        Version ProtocolVersion { get; set; }

        //
        // Summary:
        //     Gets a System.IO.Stream object to which a response can be written.
        //
        // Returns:
        //     A System.IO.Stream object to which a response can be written.
        //
        // Exceptions:
        //   T:System.ObjectDisposedException:
        //     This object is closed.
        Stream OutputStream { get; }

        //
        // Summary:
        //     Gets or sets a value indicating whether the server requests a persistent connection.
        //
        // Returns:
        //     true if the server requests a persistent connection; otherwise, false. The default
        //     is true.
        //
        // Exceptions:
        //   T:System.ObjectDisposedException:
        //     This object is closed.
        bool KeepAlive { get; set; }

        //
        // Summary:
        //     Gets or sets the collection of header name/value pairs returned by the server.
        //
        // Returns:
        //     A System.Net.WebHeaderCollection instance that contains all the explicitly set
        //     HTTP headers to be included in the response.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     The System.Net.WebHeaderCollection instance specified for a set operation is
        //     not valid for a response.
        WebHeaderCollection Headers { get; set; }

        //
        // Summary:
        //     Gets or sets the collection of cookies returned with the response.
        //
        // Returns:
        //     A System.Net.CookieCollection that contains cookies to accompany the response.
        //     The collection is empty if no cookies have been added to the response.
        CookieCollection Cookies { get; set; }

        //
        // Summary:
        //     Gets or sets the MIME type of the content returned.
        //
        // Returns:
        //     A System.String instance that contains the text of the response's Content-Type
        //     header.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     The value specified for a set operation is null.
        //
        //   T:System.ArgumentException:
        //     The value specified for a set operation is an empty string ("").
        //
        //   T:System.ObjectDisposedException:
        //     This object is closed.
        string ContentType { get; set; }

        //
        // Summary:
        //     Gets or sets the HTTP status code to be returned to the client.
        //
        // Returns:
        //     An System.Int32 value that specifies the HTTP status code for the requested resource.
        //     The default is System.Net.HttpStatusCode.OK, indicating that the server successfully
        //     processed the client's request and included the requested resource in the response
        //     body.
        //
        // Exceptions:
        //   T:System.ObjectDisposedException:
        //     This object is closed.
        //
        //   T:System.Net.ProtocolViolationException:
        //     The value specified for a set operation is not valid. Valid values are between
        //     100 and 999 inclusive.
        int StatusCode { get; set; }

        //
        // Summary:
        //     Gets or sets a text description of the HTTP status code returned to the client.
        //
        // Returns:
        //     The text description of the HTTP status code returned to the client. The default
        //     is the RFC 2616 description for the System.Net.HttpListenerResponse.StatusCode
        //     property value, or an empty string ("") if an RFC 2616 description does not exist.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     The value specified for a set operation is null.
        //
        //   T:System.ArgumentException:
        //     The value specified for a set operation contains non-printable characters.
        string StatusDescription { get; set; }

        //
        // Summary:
        //     Gets or sets the System.Text.Encoding for this response's System.Net.HttpListenerResponse.OutputStream.
        //
        // Returns:
        //     An System.Text.Encoding object suitable for use with the data in the System.Net.HttpListenerResponse.OutputStream
        //     property, or null if no encoding is specified.
        Encoding ContentEncoding { get; set; }


        //
        // Summary:
        //     Closes the connection to the client without sending a response.
        void Abort();

        //
        // Summary:
        //     Adds the specified header and value to the HTTP headers for this response.
        //
        // Parameters:
        //   name:
        //     The name of the HTTP header to set.
        //
        //   value:
        //     The value for the name header.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     name is null or an empty string ("").
        //
        //   T:System.ArgumentException:
        //     You are not allowed to specify a value for the specified header. -or- name or
        //     value contains invalid characters.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     The length of value is greater than 65,535 characters.
        void AddHeader(string name, string value);

        //
        // Summary:
        //     Adds the specified System.Net.Cookie to the collection of cookies for this response.
        //
        // Parameters:
        //   cookie:
        //     The System.Net.Cookie to add to the collection to be sent with this response.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     cookie is null.
        void AppendCookie(Cookie cookie);

        //
        // Summary:
        //     Appends a value to the specified HTTP header to be sent with this response.
        //
        // Parameters:
        //   name:
        //     The name of the HTTP header to append value to.
        //
        //   value:
        //     The value to append to the name header.
        //
        // Exceptions:
        //   T:System.ArgumentException:
        //     name is null or an empty string (""). -or- You are not allowed to specify a value
        //     for the specified header. -or- name or value contains invalid characters.
        //
        //   T:System.ArgumentOutOfRangeException:
        //     The length of value is greater than 65,535 characters.
        void AppendHeader(string name, string value);

        //
        // Summary:
        //     Returns the specified byte array to the client and releases the resources held
        //     by this System.Net.HttpListenerResponse instance.
        //
        // Parameters:
        //   responseEntity:
        //     A System.Byte array that contains the response to send to the client.
        //
        //   willBlock:
        //     true to block execution while flushing the stream to the client; otherwise, false.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     responseEntity is null.
        //
        //   T:System.ObjectDisposedException:
        //     This object is closed.
        void Close(byte[] responseEntity, bool willBlock);

        //
        // Summary:
        //     Sends the response to the client and releases the resources held by this System.Net.HttpListenerResponse
        //     instance.
        void Close();

        //
        // Summary:
        //     Copies properties from the specified System.Net.HttpListenerResponse to this
        //     response.
        //
        // Parameters:
        //   templateResponse:
        //     The System.Net.HttpListenerResponse instance to copy.
        void CopyFrom(IHttpListenerResponse templateResponse);

        //
        // Summary:
        //     Configures the response to redirect the client to the specified URL.
        //
        // Parameters:
        //   url:
        //     The URL that the client should use to locate the requested resource.
        void Redirect(string url);

        //
        // Summary:
        //     Adds or updates a System.Net.Cookie in the collection of cookies sent with this
        //     response.
        //
        // Parameters:
        //   cookie:
        //     A System.Net.Cookie for this response.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     cookie is null.
        //
        //   T:System.ArgumentException:
        //     The cookie already exists in the collection and could not be replaced.
        void SetCookie(Cookie cookie);
    }
}
