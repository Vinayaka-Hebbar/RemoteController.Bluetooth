#if QUEUE_CLIENT
using RemoteController.Messages;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks; 
#endif

namespace RemoteController.Core
{
    public class ServerEventDispatcher
#if QUEUE_CLIENT
        : IDisposable
#endif
    {
        private readonly ServerConnectionManager _manager;

#if QUEUE_CLIENT
        private readonly EventWaitHandle eventHandle;
        private readonly ConcurrentQueue<IMessage> messages = new ConcurrentQueue<IMessage>();
        bool isRunning;

        public ServerEventDispatcher(ServerConnectionManager manager)
        {
            _manager = manager;
            eventHandle = new AutoResetEvent(false);
            messages = new ConcurrentQueue<IMessage>();
        } 
#else
        public ServerEventDispatcher(ServerConnectionManager manager)
        {
            _manager = manager;
        }
#endif

#if QUEUE_CLIENT
        public void StartDispatcher()
        {
            isRunning = true;
            var task = new Task(DispatchMessages, creationOptions: TaskCreationOptions.LongRunning);
            task.ConfigureAwait(false);
            task.Start();
        } 

        async void DispatchMessages()
        {
            try
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
            catch (SocketException)
            {
                System.Diagnostics.Debug.WriteLine("Client Closed");
                // socket closed
            }
            catch (System.IO.IOException)
            {
                System.Diagnostics.Debug.WriteLine("Unable to send data");
                // invalid write
            }
        }
#endif

#if QUEUE_CLIENT
        public void Process(IMessage message)
        {
            messages.Enqueue(message);
            eventHandle.Set();
        } 
#endif

        public async void Send(byte[] message)
        {
            await _manager.Send(message);
        }

#if QUEUE_CLIENT
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
#endif
    }
}