using RemoteController.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteController.Core
{
    public class MessageDispatcher : IDisposable
    {
        public event Action<IMessage> Received = delegate { };

        private readonly System.Collections.Concurrent.ConcurrentQueue<IMessage> messages;

        private bool isRunning;

        public MessageDispatcher(EventWaitHandle waitHandle)
        {
            WaitHandle = waitHandle;
            messages = new System.Collections.Concurrent.ConcurrentQueue<IMessage>();
        }

        public MessageDispatcher() : this(new AutoResetEvent(false))
        {
        }

        public void Start()
        {
            isRunning = true;
            var task = new Task(StartProcess, creationOptions: TaskCreationOptions.LongRunning);
            task.ConfigureAwait(false);
            task.Start();
        }

        private void StartProcess()
        {
            while (isRunning)
            {
                if (WaitHandle.WaitOne() && isRunning)
                {
                    var count = messages.Count;
                    while (count > 0)
                    {
                        count--;
                        if(messages.TryDequeue(out IMessage message))
                        {
                            Received(message);
                        }

                    }
                }
            }
        }

        public void Process(IMessage message)
        {
            messages.Enqueue(message);
            WaitHandle.Set();
        }

        public EventWaitHandle WaitHandle { get; }

        public void Dispose()
        {
            isRunning = false;
            WaitHandle.Reset();
        }
    }
}
