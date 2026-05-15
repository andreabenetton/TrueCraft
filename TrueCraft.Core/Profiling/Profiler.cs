using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TrueCraft.Core.Profiling
{
    public class Profiler
    {
        private readonly ILogger<Profiler> _log;
        private readonly object _lock = new object();
        private readonly Stopwatch _stopwatch;
        private readonly List<string> _enabledBuckets = new();
        private readonly Stack<ActiveTimer> _activeTimers = new();

        public Profiler(ILogger<Profiler> log)
        {
            _log = log;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        public bool LogLag { get; set; }

        [Conditional("DEBUG")]
        public void EnableBucket(string bucket) => _enabledBuckets.Add(bucket);

        [Conditional("DEBUG")]
        public void DisableBucket(string bucket) => _enabledBuckets.Remove(bucket);

        [Conditional("DEBUG")]
        public void Start(string bucket)
        {
            lock (_lock)
            {
                _activeTimers.Push(new ActiveTimer
                {
                    Started = _stopwatch.ElapsedTicks,
                    Finished = -1,
                    Bucket = bucket
                });
            }
        }

        [Conditional("DEBUG")]
        public void Done(long lag = -1)
        {
            lock (_lock)
            {
                if (_activeTimers.Count > 0)
                {
                    var timer = _activeTimers.Pop();
                    timer.Finished = _stopwatch.ElapsedTicks;
                    var elapsed = (timer.Finished - timer.Started) / 10000.0;
                    foreach (var bucket in _enabledBuckets)
                        if (Match(bucket, timer.Bucket))
                        {
                            _log.LogInformation("[@{Elapsed:0.00}s] {Bucket} took {Took}ms",
                                _stopwatch.ElapsedMilliseconds / 1000.0, timer.Bucket, elapsed);
                            break;
                        }

                    if (LogLag && lag != -1 && elapsed > lag)
                        _log.LogWarning("{Bucket} is lagging by {Elapsed}ms", timer.Bucket, elapsed);
                }
            }
        }

        private static bool Match(string mask, string value)
        {
            if (value == null)
                value = string.Empty;
            var i = 0;
            var j = 0;
            for (; j < value.Length && i < mask.Length; j++)
                if (mask[i] == '?')
                {
                    i++;
                }
                else if (mask[i] == '*')
                {
                    i++;
                    if (i >= mask.Length)
                        return true;
                    while (++j < value.Length && value[j] != mask[i]) ;
                    if (j-- == value.Length)
                        return false;
                }
                else
                {
                    if (char.ToUpper(mask[i]) != char.ToUpper(value[j]))
                        return false;
                    i++;
                }

            return i == mask.Length && j == value.Length;
        }

        private struct ActiveTimer
        {
            public long Started, Finished;
            public string Bucket;
        }
    }
}
