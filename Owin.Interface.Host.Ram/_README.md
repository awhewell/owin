# Owin.Interface.Host.Ram

Interfaces exclusive to the Owin.Host.Ram library.

The RAM host is contained entirely within memory, it does not talk on
the network.

Its intended use is to send requests through a pipeline without the
overhead of using the local loopback and to support unit testing of
constructed pipelines.
