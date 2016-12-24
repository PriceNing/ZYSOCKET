﻿#if!Net2
using System.Collections.Generic;
using System.Threading.Tasks;


namespace ZYSocket.MicroThreading
{
    public class AsyncSignal
    {
        private TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

        public Task WaitAsync()
        {
            lock (this)
            {
                tcs = new TaskCompletionSource<bool>();
                var result = tcs.Task;
                return result;
            }
        }

        public void Set()
        {
            lock (this)
            {
                tcs.TrySetResult(true);
            }
        }
    }

    public class AsyncAutoResetEvent
    {
        // Credit: http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266923.aspx
        private readonly static Task completed = Task.FromResult(true);
        private readonly Queue<TaskCompletionSource<bool>> waits = new Queue<TaskCompletionSource<bool>>();
        private bool signaled;

        public Task WaitAsync()
        {
            lock (waits)
            {
                if (signaled)
                {
                    signaled = false;
                    return completed;
                }
                else
                {
                    var tcs = new TaskCompletionSource<bool>();
                    waits.Enqueue(tcs);
                    return tcs.Task;
                }
            }
        }

        public void Set()
        {
            TaskCompletionSource<bool> toRelease = null;
            lock (waits)
            {
                if (waits.Count > 0)
                    toRelease = waits.Dequeue();
                else if (!signaled)
                    signaled = true;
            }
            if (toRelease != null)
                toRelease.SetResult(true);
        }
    }

}
#endif