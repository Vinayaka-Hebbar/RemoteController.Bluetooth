using System;
using System.Collections.Generic;

namespace RemoteController.Services
{
    public class ServiceContainer : IDisposable
    {
        static readonly object _lock = new object();
        private readonly TaskCollection tasks;

        static ServiceContainer current;

        private ServiceContainer()
        {
            tasks = new TaskCollection();
        }

        public static ServiceContainer Current
        {
            get
            {
                lock (_lock)
                {
                    if (current == null)
                        current = new ServiceContainer();
                    return current;
                }
            }
        }

        public void AddSevice(TaskService worker)
        {
            lock (_lock)
            {
                tasks.Add(worker.Id, worker);
            }
        }



        public T GetService<T>(Guid id) where T : TaskService
        {
            lock (_lock)
            {
                if (tasks.TryGetValue(id, out TaskService task))
                    return (T)task;

                return null;
            }
        }

        public void RemoveService(Guid id)
        {
            lock (_lock)
            {
                tasks.Remove(id);
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                foreach (TaskService item in tasks.Values)
                {
                    item.Dispose();
                }
                tasks.Clear();
            }
        }

        sealed class TaskCollection : Dictionary<Guid, TaskService>
        {
            public TaskCollection() : base(new Comparer())
            {

            }
        }

        readonly struct Comparer : IEqualityComparer<Guid>
        {
            public bool Equals(Guid x, Guid y)
            {
                return x.Equals(y);
            }

            public int GetHashCode(Guid obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
