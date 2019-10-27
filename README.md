# Owin
An OWIN server for Virtual Radar Server version 3.

## Why another OWIN server?

One of the aims of VRS v3 is to be available in three flavours - .NET Framework, which
is the fully featured version, Mono and DotNet Core. The reasons behind this decision
are outside of the scope of the project but the bare facts are that VRS must support
all three platforms and that the web server is one of the cornerstones of VRS.

Originally VRS v3 used Microsoft's Katana server and Microsoft's Web API 2. However,
Microsoft only produce builds of these for the .NET Framework, they do not produce
DotNet Core builds. When the platform scope widened to include DotNet Core it became
apparent that Katana and WebAPI2 were not going to work.

The first idea was to write wrappers around Katana and Web API 2 in Katana and their
equivalents in DotNet Core. However, one major weakness in both flavours of Web API
is that the web API controllers have a mandatory base class, and wrappers around base
classes are impractical. The wrapper should hide the implementation that it wraps but
the base class infects everything that it touches, you end up having to reference the
assembly containing the base class in everything that uses the wrapper.

The next plan was to use NancyFX. It is a well established server and supports all
three platforms. It kind-of supports OWIN. However, it does not support weak URL
wildcards on HTTP.sys, which is a pre-requisite for use by VRS, and despite claims
to the contrary it is quite opinionated. I'm not against opinionated libraries and
I didn't mind having to rewrite the code to fit in with their view of the world, but
it would have involved rewriting all of my tests and that was going to be a pain.
There were also question marks around NancyFX's performance, particularly in routing
web API calls.

So eventually I decided to write an OWIN web server and Web API for VRS that would be
built to .NET Standard 2.0, which should mean that it can be used without alteration
by all three platforms.

Moreover it can follow the interface-driven approach that VRS follows and it can make
use of the same class factory that VRS uses, which makes it easier for VRS plugins
to swap out some or all of the library at run-time and replace it with their own parts.

When VRS was switched over to Katana and Web API 2 it turned out that the configuration
was tricky when plugins were involved. Writing my own server meant that I could have
the server support the programmatic configuration of the middleware pipleline when it
is split into into chunks that are spread around different parts of the program (e.g.
the main program and the plugins), which would simplify some aspects of VRS.

The downside is that the server and web API are written with VRS in mind and are missing
features that VRS does not need.
