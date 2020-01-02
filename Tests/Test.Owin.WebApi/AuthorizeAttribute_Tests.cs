// Copyright © 2020 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Linq;
using System.Security.Principal;
using AWhewell.Owin.Interface.WebApi;
using AWhewell.Owin.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.AWhewell.Owin.WebApi
{
    [TestClass]
    public class AuthorizeAttribute_Tests
    {
        private MockOwinEnvironment _Environment;

        [TestInitialize]
        public void TestInitialise()
        {
            _Environment = new MockOwinEnvironment();
        }

        private void AddUser(string userName, params string[] roles)
        {
            var principal = new GenericPrincipal(
                new GenericIdentity(userName),
                roles
            );

            var context = new OwinContext(_Environment.Environment);
            context.RequestPrincipal = principal;
        }

        [TestMethod]
        public void AttributeUsage_Flags_Are_Correct()
        {
            var usageAttribute = typeof(AuthorizeAttribute)
                .GetCustomAttributes(inherit: false)
                .OfType<AttributeUsageAttribute>()
                .FirstOrDefault();

            Assert.IsNotNull(usageAttribute);
            Assert.AreEqual(AttributeTargets.Class | AttributeTargets.Method, usageAttribute.ValidOn);
            Assert.AreEqual(true, usageAttribute.Inherited);
            Assert.AreEqual(true, usageAttribute.AllowMultiple);
        }

        [TestMethod]
        //       -- Attribute --                -- Request --               Expected
        [DataRow(null,          null,           null,       null,           false)]     // No roles or users specified and request is anonymous
        [DataRow(null,          null,           "alice",    null,           true)]      // No roles or users specified but authenticated user present
        [DataRow(null,          null,           "",         null,           false)]     // No roles or users specified but unauthenticated user present
        [DataRow("alice",       null,           null,       null,           false)]     // User specified, request is anonymous
        [DataRow("alice",       null,           "bob",      null,           false)]     // User specified, request is for different user
        [DataRow("alice",       null,           "alice",    null,           true)]      // User specified, request matches
        [DataRow(" alice ",     null,           "alice",    null,           true)]      // Whitespace on attribute user - will be trimmed
        [DataRow("alice",       null,           " alice ",  null,           false)]     // Whitespace on request user - will not be trimmed
        [DataRow("ALICE",       null,           "alice",    null,           true)]      // User is case insensitive
        [DataRow("alice,bob",   null,           "alice",    null,           true)]      // Multiple users can be specified on attribute
        [DataRow("alice,bob",   null,           "bob",      null,           true)]      //                "             "
        [DataRow("alice,bob",   null,           "charlie",  null,           false)]     //                "             "
        [DataRow(null,          "admin",        null,       null,           false)]     // Only role specified, request is anonymous
        [DataRow(null,          "admin",        "alice",    "user",         false)]     // Only role specified, request for different role
        [DataRow(null,          "admin",        "alice",    "admin",        true)]      // Only role specified, request for correct role
        [DataRow(null,          "admin",        "alice",    "ADMIN",        true)]      // Role is not case sensitive
        [DataRow(null,          "admin,editor", "alice",    "admin",        true)]      // Multiple roles specified without a user
        [DataRow(null,          "admin,editor", "alice",    "editor",       true)]      //                "             "
        [DataRow(null,          "admin,editor", "alice",    "user",         false)]     //                "             "
        [DataRow(null,          "admin",        "alice",    "admin,editor", true)]      // Only one role match required
        [DataRow(null,          "editor",       "alice",    "admin,editor", true)]      //                "             "
        [DataRow(null,          "user",         "alice",    "admin,editor", false)]     //                "             "
        [DataRow(null,          "admin,editor", "alice",    "user,viewer",  false)]     //                "             "
        [DataRow(null,          "admin,editor", "alice",    "user,editor",  true)]      //                "             "
        [DataRow("alice",       "admin",        "alice",    "admin",        true)]      // User and role both match
        [DataRow("alice",       "user",         "alice",    "admin",        false)]     // Only user matches
        [DataRow("alice",       "admin",        "bob",      "admin",        false)]     // Only role matches
        [DataRow("alice",       "admin,editor", "alice",    "admin",        true)]      // Multiple roles can be specified on attribute
        [DataRow("alice",       "admin,editor", "alice",    "editor",       true)]      //                "             "
        [DataRow("alice",       "admin,editor", "alice",    "user",         false)]     //                "             "
        [DataRow("alice",       "admin",        null,       null,           false)]     // Both user and role specified, request is anonymous
        public void AllowRequest_Honours_User_And_Roles(string attrUsers, string attrRoles, string requestUser, string requestRoles, bool expected)
        {
            if(requestUser != null) {
                if(requestRoles == null) {
                    AddUser(requestUser);
                } else {
                    AddUser(requestUser, requestRoles.Split(','));
                }
            }

            var attr = new AuthorizeAttribute();
            attr.Users = attrUsers ?? attr.Users;
            attr.Roles = attrRoles ?? attr.Roles;

            var actual = attr.AllowRequest(_Environment.Environment);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AllowRequest_Sets_Status_401_If_Request_Is_Rejected()
        {
            var attr = new AuthorizeAttribute();

            attr.AllowRequest(_Environment.Environment);

            Assert.AreEqual(401, _Environment.ResponseStatusCode);
        }

        [TestMethod]
        public void AllowRequest_Does_Not_Set_Status_If_Request_Is_Accepted()
        {
            var attr = new AuthorizeAttribute();
            AddUser("any");

            attr.AllowRequest(_Environment.Environment);

            Assert.AreEqual(null, _Environment.ResponseStatusCode);
        }
    }
}
