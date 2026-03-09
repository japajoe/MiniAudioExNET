using System.Collections.Generic;

namespace MiniAudioEx.Utilities
{
    public sealed class ConcurrentList<T>
    {
        private readonly List<T> items;
        private readonly object syncRoot = new object();

        public int Count
        {
            get
            {
                lock (syncRoot)
                {
                    return items.Count;
                }
            }
        }

        public T this[int index]
        {
            get
            {
                lock (syncRoot)
                {
                    return items[index];
                }
            }
            set
            {
                lock (syncRoot)
                {
                    items[index] = value;
                }
            }
        }

        public ConcurrentList()
        {
            this.items = new List<T>();
        }

        public void Clear()
        {
            lock (syncRoot)
            {
                items.Clear();
            }
        }

        public void Add(T item)
        {
            lock (syncRoot)
            {
                items.Add(item);
            }
        }

        public void Remove(T item)
        {
            lock (syncRoot)
            {
                items.Remove(item);
            }
        }

        public void Remove(List<T> items)
        {
            lock (syncRoot)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    this.items.Remove(items[i]);
                }
            }
        }

        public void RemoveAt(int index)
        {
            lock (syncRoot)
            {
                items.RemoveAt(index);
            }
        }
    }
}