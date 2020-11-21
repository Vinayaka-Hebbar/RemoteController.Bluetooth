using RemoteController.Messages;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteController.Core
{
    public class ServerEventDispatcher : IDisposable
    {
        private readonly ServerConnectionManager _manager;
        private readonly EventWaitHandle eventHandle;
        private readonly ConcurrentQueue<IMessage> messages = new ConcurrentQueue<IMessage>();
        bool isRunning;

        public ServerEventDispatcher(ServerConnectionManager manager)
        {
            _manager = manager;
            eventHandle = new AutoResetEvent(false);
            messages = new ConcurrentQueue<IMessage>();
        }

        public void StartDispatcher()
        {
            isRunning = true;
            var task = new Task(DispatchMessages, creationOptions: TaskCreationOptions.LongRunning);
            task.ConfigureAwait(false);
            task.Start();
        }

        async void DispatchMessages()
        {
            while (isRunning)
            {
                if (eventHandle.WaitOne() && isRunning)
                {
                    var count = messages.Count;
                    while (count > 0)
                    {
                        count--;
                        if (messages.TryDequeue(out IMessage message))
                        {
                            await _manager.Send(message);
                        }
                    }
                }
            }
        }

        public void Process(IMessage message)
        {
            messages.Enqueue(message);
            eventHandle.Set();
        }

        private bool disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    isRunning = false;
                    eventHandle.Set();
                    eventHandle.Dispose();
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}