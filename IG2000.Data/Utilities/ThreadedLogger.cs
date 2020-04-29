using System;
using System.Collections.Generic;
using System.Threading;

namespace IG2000.Data.Utilities
{
    public abstract class ThreadedLogger : IDisposable
    {
        Queue<Action> queue = new Queue<Action>();
        ManualResetEvent hasNewItems = new ManualResetEvent(false);
        ManualResetEvent terminate = new ManualResetEvent(false);
        ManualResetEvent waiting = new ManualResetEvent(false);

        Thread loggingThread;

        public ThreadedLogger()
        {
            loggingThread = new Thread(new ThreadStart(ProcessQueue));
            loggingThread.IsBackground = true;
            // this is performed from a bg thread, to ensure the queue is serviced from a single thread
            loggingThread.Start();
        }

        void ProcessQueue()
        {
            while (true)
            {
                waiting.Set();
                int i = ManualResetEvent.WaitAny(new WaitHandle[] { hasNewItems, terminate });
                // terminate was signaled 
                if (i == 1) return;
                hasNewItems.Reset();
                waiting.Reset();

                Queue<Action> queueCopy;
                lock (queue)
                {
                    queueCopy = new Queue<Action>(queue);
                    queue.Clear();
                }

                foreach (var log in queueCopy)
                {
                    log();
                }
            }
        }

        /// <summary>
        /// Add the log to the queue 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="context"></param>
        public void LogMessage(object row, Sheev.Common.BaseModels.IBaseContextModel context)
        {
            lock (queue)
            {
                queue.Enqueue(() => AsyncLogMessage(row, context));
            }
            hasNewItems.Set();
        }

        /// <summary>
        /// Log the message
        /// </summary>
        /// <param name="row"></param>
        /// <param name="context"></param>
        protected abstract void AsyncLogMessage(object row, Sheev.Common.BaseModels.IBaseContextModel context);


        public void Flush()
        {
            waiting.WaitOne();
        }


        public void Dispose()
        {
            terminate.Set();
            loggingThread.Join();
        }
    }
}
