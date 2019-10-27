# Owin
An OWIN server for Virtual Radar Server version 3.

## Why another OWIN server?

One of the aims of VRS v3 is to be available in three flavours - .NET Framework, which
is the fully featured version, Mono and DotNet Core. The reasons behind this decision
are outside of the scope of the project. The bare facts are that VRS must support all
three platforms and that the web server is one of the cornerstones of VRS.

Originally VRS v3 used Microsoft's Katana server and Microsoft's Web API 2. However,
Microsoft only produce builds of these for the .NET Framework, they do not produce
DotNet Core builds. When the platform scope widened to include DotNet Core it became
apparent that Katana and WebAPI2 were not going to work.

The next plan was to use NancyFX. It is a well established server and supports all
three platforms. It kind-of supports OWIN. However, it does not support weak URL
wildcards on HTTP.sys, which is a pre-requisite for use by VRS, and despite claims
to the contrary it is quite opinionated. I would have to rewrite all of my tests.

So eventually I decided to write an OWIN web server and Web API for VRS that would be
built to .NET Standard 2.0, which should mean that it can be used without alteration
by all three platforms.

Moreover it can follow the interface-driven approach that VRS follows and it can make
use of the same class factory that VRS uses, which makes it easier for VRS plugins
to swap out some or all of the library at run-time and replace it with their own parts.

The downside is that the server and web API are written with VRS in mind and are missing
features that VRS does not need.