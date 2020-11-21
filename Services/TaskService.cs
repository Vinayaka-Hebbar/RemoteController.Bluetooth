using System;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteController.Services
{
    public abstract class TaskService : IDisposable
    {
        public Guid Id { get; }

        private CancellationTokenSource cts;

        public bool WasStarted => IsRunning;

        public event Action<TaskService> Completed;

        public TaskService(Guid id)
        {
            Id = id;
        }

        public TaskService()
        {
            Id = Guid.NewGuid();
        }

        public virtual void Start()
        {
            cts = new CancellationTokenSource();
            Task task = new Task(OnStart, cts.Token, TaskCreationOptions.LongRunning);
            task.ConfigureAwait(false);
            task.Start();
        }

        void OnStart()
        {
            IsRunning = true;
            DoWork(cts.Token);
        }

        public void Stop()
        {
            IsRunning = false;
            if (cts == null || cts.IsCancellationRequested)
                return;
            cts.Cancel();
            cts.Dispose();
            cts = null;
            Completed?.Invoke(this);
        }

        protected virtual void OnStop()
        {
        }

        protected abstract void DoWork(CancellationToken cancellationToken);

        #region IDisposable Support
        protected bool IsRunning = false;

        public bool IsDisposed { get; private set; }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
                IsDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
