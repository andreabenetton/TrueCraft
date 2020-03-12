using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrueCraft.Core.Profiling
{
    public static class Profiler
    {
        private static readonly object Lock = new object();

        static Profiler()
        {
            Stopwatch = new Stopwatch();
            EnabledBuckets = new List<string>();
            ActiveTimers = new Stack<ActiveTimer>();
            LogLag = false;
            Stopwatch.Start();
        }

        private static Stopwatch Stopwatch { get; }
        private static List<string> EnabledBuckets { get; }
        private static Stack<ActiveTimer> ActiveTimers { get; }

        public static bool LogLag { get; set; }

        [Conditional("DEBUG")]
        public static void EnableBucket(string bucket)
        {
            EnabledBuckets.Add(bucket);
        }

        [Conditional("DEBUG")]
        public static void DisableBucket(string bucket)
        {
            EnabledBuckets.Remove(bucket);
        }

        [Conditional("DEBUG")]
        public static void Start(string bucket)
        {
            lock (Lock)
            {
                ActiveTimers.Push(new ActiveTimer
                {
                    Started = Stopwatch.ElapsedTicks,
                    Finished = -1,
                    Bucket = bucket
                });
            }
        }

        [Conditional("DEBUG")]
        public static void Done(long lag = -1)
        {
            lock (Lock)
            {
                if (ActiveTimers.Count > 0)
                {
                    var timer = ActiveTimers.Pop();
                    timer.Finished = Stopwatch.ElapsedTicks;
                    var elapsed = (timer.Finished - timer.Started) / 10000.0;
                    foreach (var bucket in EnabledBuckets)
                        if (Match(bucket, timer.Bucket))
                        {
                            Console.WriteLine("[@{0:0.00}s] {1} took {2}ms",
                                Stopwatch.ElapsedMilliseconds / 1000.0, timer.Bucket, elapsed);
                            break;
                        }

                    if (LogLag && lag != -1 && elapsed > lag)
                        Console.WriteLine("{0} is lagging by {1}ms", timer.Bucket, elapsed);
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