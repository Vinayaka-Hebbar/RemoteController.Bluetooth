using System.Collections.Generic;

namespace RemoteController.Collection
{
    public interface IObservableCollection<T> : IEnumerable<T>
    {
        int Count { get; }
        void Move(int oldIndex, int newIndex);
        int IndexOf(T item);
        void Insert(int index, T item);
    }
}
