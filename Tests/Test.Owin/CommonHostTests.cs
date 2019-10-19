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
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Owin.Interface;

namespace Test.Owin
{
    public class CommonHostTests
    {
        protected void All_Hosts_Root_Defaults_To_Slash(IHost host)
        {
            Assert.AreEqual("/", host.Root);
        }

        protected void All_Hosts_Root_Coalesces_Null_To_Slash(IHost host)
        {
            host.Root = null;
            Assert.AreEqual("/", host.Root);
        }

        protected void All_Hosts_Root_Replaces_Empty_String_With_Slash(IHost host)
        {
            host.Root = "";
            Assert.AreEqual("/", host.Root);
        }

        protected void All_Hosts_Root_Always_Prefixed_With_Slash(IHost host)
        {
            host.Root = "A";
            Assert.AreEqual("/A", host.Root);
        }

        protected void All_Hosts_Root_Does_Not_Prefix_With_Slash_When_Already_Starts_With_Slash(IHost host)
        {
            host.Root = "/A";
            Assert.AreEqual("/A", host.Root);
        }

        protected void All_Hosts_Root_Strips_Trailing_Slash(IHost host)
        {
            host.Root = "/A/";
            Assert.AreEqual("/A", host.Root);
        }

        protected void All_Hosts_Port_Defaults_To_80(IHost host)
        {
            Assert.AreEqual(80, host.Port);
        }

        protected void All_Hosts_Initialise_Throws_If_Called_Twice(IHost host)
        {
            var builder =     MockHelper.FactoryImplementation<IPipelineBuilder>();
            var environment = MockHelper.FactoryImplementation<IPipelineBuilderEnvironment>();

            host.Initialise(builder.Object, environment.Object);
            host.Initialise(builder.Object, environment.Object);
        }

        protected void All_Hosts_Initialise_Throws_If_Builder_Is_Null(IHost host)
        {
            var environment = MockHelper.FactoryImplementation<IPipelineBuilderEnvironment>();
            host.Initialise(null, environment.Object);
        }

        protected void All_Hosts_Initialise_Throws_If_Environment_Is_Null(IHost host)
        {
            var builder = MockHelper.FactoryImplementation<IPipelineBuilder>();
            host.Initialise(builder.Object, null);
        }

        protected void All_Hosts_Start_Throws_If_Not_Initialised(IHost host)
        {
            host.Start();
        }
    }
}
