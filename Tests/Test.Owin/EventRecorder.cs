// Copyright © 2010 onwards, Andrew Whewell
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

namespace Test.AWhewell.Owin
{
    /// <summary>
    /// An object that can be hooked to an event to determine that it has been raised and record the parameters passed to the event.
    /// </summary>
    /// <remarks>
    /// This only works with standard events that pass two parameters, a sender object and an args based on <see cref="EventArgs"/>.
    /// </remarks>
    public class EventRecorder<T>
        where T: EventArgs
    {
        /// <summary>
        /// Gets the number of times the event has been raised.
        /// </summary>
        public int CallCount { get; private set; }

        /// <summary>
        /// Gets the sender parameter from the last time the event was raised.
        /// </summary>
        public object Sender { get; private set; }

        /// <summary>
        /// Gets the args parameter from the last time the event was raised.
        /// </summary>
        public T Args { get; private set; }

        /// <summary>
        /// Gets a list of every sender parameter passed to the event. There will be <see cref="CallCount"/>
        /// entries in the list.
        /// </summary>
        public List<object> AllSenders { get; private set; }

        /// <summary>
        /// Gets a list of every args parameter passed to the event. There will be <see cref="CallCount"/>
        /// entries in the list.
        /// </summary>
        public List<T> AllArgs { get; private set; }

        /// <summary>
        /// Raised by <see cref="Handler"/> whenever the event is raised. Can be used to test the state of
        /// objects when the event was raised.
        /// </summary>
        /// <remarks>
        /// The sender passed to the event is the EventRecorder, <em>not</em> the sender of the original event.
        /// By the time the event is raised the EventRecorder's <see cref="Sender"/> property will be set to the
        /// sender of the original event.
        /// </remarks>
        public event EventHandler<T> EventRaised;

        /// <summary>
        /// Raises <see cref="EventRaised"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnEventRaised(T args)
        {
            EventRaised?.Invoke(this, args);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public EventRecorder()
        {
            AllSenders = new List<object>();
            AllArgs = new List<T>();
        }

        /// <summary>
        /// An event handler matching the EventHandler and/or EventHandler&lt;&gt; delegate that can be attached
        /// to an event and record the parameters passed by the code that raises the event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public virtual void Handler(object sender, T args)
        {
            ++CallCount;
            Sender = sender;
            Args = args;

            AllSenders.Add(sender);
            AllArgs.Add(args);

            OnEventRaised(args);
        }
    }
}
