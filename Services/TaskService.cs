using System;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteController.Services
{
    public class TaskService : IDisposable
    {
        public Guid Id { get; }

        private readonly CancellationTokenSource cts;

        public event Action<TaskService> Completed;

        public TaskService(Guid id)
        {
            Id = id;
            cts = new CancellationTokenSource();
        }

        public void Start()
        {
            Task task = new Task(DoWork, cts.Token, TaskCreationOptions.LongRunning);
            task.ConfigureAwait(false);
            task.Start();
        }

        void StopTask(Task obj)
        {
            Running = false;
            OnStop();
            Completed?.Invoke(this);
            // release any resource
        }

        protected virtual void OnStop()
        {
        }

        protected virtual void DoWork()
        {
            Running = true;
        }

        #region IDisposable Support
        protected bool Running = false;

        protected virtual void Dispose(bool disposing)
        {
            if (cts.IsCancellationRequested)
                return;
            cts.Cancel();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
