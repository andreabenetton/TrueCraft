using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TrueCraft.API.Server
{
    public interface IEventScheduler
    {
        HashSet<string> DisabledEvents { get; }

        /// <summary>
        ///     Schedules a synchronous event to occur some time in the future. Wrapped internally as a Task-returning
        ///     action so the dispatcher only sees one shape.
        /// </summary>
        /// <param name="subject">The subject of the event. If the subject is disposed, the event is cancelled.</param>
        /// <param name="when">When to trigger the event.</param>
        /// <param name="action">The event to trigger.</param>
        void ScheduleEvent(string name, IEventSubject subject, TimeSpan when, Action<IMultiplayerServer> action);

        /// <summary>
        ///     Schedules an asynchronous event to occur some time in the future. Awaited by the dispatcher.
        /// </summary>
        void ScheduleEvent(string name, IEventSubject subject, TimeSpan when, Func<IMultiplayerServer, Task> action);

        /// <summary>
        ///     Awaits all pending scheduled events whose scheduled time has transpired.
        /// </summary>
        Task UpdateAsync(CancellationToken cancellationToken = default);
    }
}