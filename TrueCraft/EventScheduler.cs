using System;
using TrueCraft.API.Server;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TrueCraft.API;
using System.Diagnostics;
using TrueCraft.Core.Profiling;
using System.Collections.Concurrent;

namespace TrueCraft;

public class EventScheduler : IEventScheduler
{
    private readonly Profiler Profiler;

    private IList<ScheduledEvent> Events { get; set; } // Sorted
    private readonly object EventLock = new object();
    private IMultiplayerServer Server { get; set; }
    private HashSet<IEventSubject> Subjects { get; set; }
    private Stopwatch Stopwatch { get; set; }
    private ConcurrentQueue<ScheduledEvent> ImmediateEventQueue { get; set; }
    private ConcurrentQueue<ScheduledEvent> LaterEventQueue { get; set; }
    private ConcurrentQueue<IEventSubject> DisposedSubjects { get; set; }
    public HashSet<string> DisabledEvents { get; private set; }

    public EventScheduler(IMultiplayerServer server, Profiler profiler)
    {
        Profiler = profiler;
        Events = new List<ScheduledEvent>();
        ImmediateEventQueue = new ConcurrentQueue<ScheduledEvent>();
        LaterEventQueue = new ConcurrentQueue<ScheduledEvent>();
        DisposedSubjects = new ConcurrentQueue<IEventSubject>();
        Server = server;
        Subjects = new HashSet<IEventSubject>();
        Stopwatch = new Stopwatch();
        DisabledEvents = new HashSet<string>();
        Stopwatch.Start();
    }
    
    private void ScheduleEvent(ScheduledEvent e)
    {
        int i;
        for (i = 0; i < Events.Count; i++)
        {
            if (Events[i].When > e.When)
                break;
        }
        Events.Insert(i, e);
    }

    public void ScheduleEvent(string name, IEventSubject subject, TimeSpan when, Action<IMultiplayerServer> action)
    {
        // Wrap the sync action as a Task-returning func so the dispatcher only sees one shape.
        ScheduleEvent(name, subject, when, s =>
        {
            action(s);
            return Task.CompletedTask;
        });
    }


    public void ScheduleEvent(string name, IEventSubject subject, TimeSpan when, Func<IMultiplayerServer, Task> action)
    {
        if (DisabledEvents.Contains(name))
            return;
        long _when = Stopwatch.ElapsedTicks + when.Ticks;
        if (subject is not null && !Subjects.Contains(subject))
        {
            Subjects.Add(subject);
            subject.Disposed += Subject_Disposed;
        }
        var queue = when.TotalSeconds > 3 ? LaterEventQueue : ImmediateEventQueue;
        queue.Enqueue(new ScheduledEvent
        {
            Name = name,
            Subject = subject,
            When = _when,
            Action = action
        });
    }

    void Subject_Disposed(object sender, EventArgs e)
    {
        DisposedSubjects.Enqueue((IEventSubject)sender);
    }

    public async Task UpdateAsync(CancellationToken cancellationToken = default)
    {
        Profiler.Start("scheduler");
        Profiler.Start("scheduler.receive-events");
        long start = Stopwatch.ElapsedTicks;
        long limit = Stopwatch.ElapsedMilliseconds + 10;
        while (ImmediateEventQueue.Count > 0 && Stopwatch.ElapsedMilliseconds < limit)
        {
            ScheduledEvent e;
            bool dequeued = false;
            while (!(dequeued = ImmediateEventQueue.TryDequeue(out e))
                && Stopwatch.ElapsedMilliseconds < limit) ;
            if (dequeued)
                ScheduleEvent(e);
        }
        while (LaterEventQueue.Count > 0 && Stopwatch.ElapsedMilliseconds < limit)
        {
            ScheduledEvent e;
            bool dequeued = false;
            while (!(dequeued = LaterEventQueue.TryDequeue(out e))
                && Stopwatch.ElapsedMilliseconds < limit) ;
            if (dequeued)
                ScheduleEvent(e);
        }
        Profiler.Done();
        Profiler.Start("scheduler.dispose-subjects");
        while (DisposedSubjects.Count > 0 && Stopwatch.ElapsedMilliseconds < limit)
        {
            IEventSubject subject;
            bool dequeued = false;
            while (!(dequeued = DisposedSubjects.TryDequeue(out subject))
                && Stopwatch.ElapsedMilliseconds < limit) ;
            if (dequeued)
            {
                // Cancel all events with this subject
                for (int i = 0; i < Events.Count; i++)
                {
                    if (Events[i].Subject == subject)
                    {
                        Events.RemoveAt(i);
                        i--;
                    }
                }
                Subjects.Remove(subject);
            }
        }
        limit = Stopwatch.ElapsedMilliseconds + 10;
        Profiler.Done();
        for (int i = 0; i < Events.Count && Stopwatch.ElapsedMilliseconds < limit; i++)
        {
            var e = Events[i];
            if (e.When <= start)
            {
                Profiler.Start("scheduler." + e.Name);
                await e.Action(Server).ConfigureAwait(false);
                Events.RemoveAt(i);
                i--;
                Profiler.Done();
            }
            if (e.When > start)
                break; // List is sorted, we can exit early
        }
        Profiler.Done(20);
    }

    private struct ScheduledEvent
    {
        public long When;
        public Func<IMultiplayerServer, Task> Action;
        public IEventSubject Subject;
        public string Name;
    }
}
