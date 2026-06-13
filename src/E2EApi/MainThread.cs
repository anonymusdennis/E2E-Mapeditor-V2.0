using System;
using System.Collections.Generic;
using System.Threading;

namespace E2EApi
{
    /// <summary>
    /// Marshals work onto Unity's main thread. Background threads (e.g. the
    /// mod's web UI server) must use this for every game/Unity call.
    /// </summary>
    public static class MainThread
    {
        private static readonly Queue<Action> Queue = new Queue<Action>();

        /// <summary>Queue an action for the next frame (fire and forget).</summary>
        public static void Post(Action action)
        {
            ApiRunner.Ensure();
            lock (Queue)
            {
                Queue.Enqueue(action);
            }
        }

        /// <summary>
        /// Run a function on the main thread and block until it finishes
        /// (up to <paramref name="timeoutMs"/>). Throws on timeout.
        /// </summary>
        public static T Run<T>(Func<T> func, int timeoutMs = 5000)
        {
            T result = default(T);
            Exception error = null;
            var done = new ManualResetEvent(false);
            Post(() =>
            {
                try
                {
                    result = func();
                }
                catch (Exception e)
                {
                    error = e;
                }
                finally
                {
                    done.Set();
                }
            });
            if (!done.WaitOne(timeoutMs, false))
            {
                throw new TimeoutException("MainThread.Run timed out");
            }
            if (error != null)
            {
                throw error;
            }
            return result;
        }

        public static void Run(Action action, int timeoutMs = 5000)
        {
            Run<object>(() => { action(); return null; }, timeoutMs);
        }

        /// <summary>Drains the queue; called once per frame by the ApiRunner.</summary>
        internal static void Drain()
        {
            while (true)
            {
                Action action;
                lock (Queue)
                {
                    if (Queue.Count == 0)
                    {
                        return;
                    }
                    action = Queue.Dequeue();
                }
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    Log.Error("MainThread action threw: " + e);
                }
            }
        }
    }
}
