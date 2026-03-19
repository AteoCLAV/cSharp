using System;
using System.Collections.Generic;
using System.Linq;

namespace queue.Models
{
    public class Queue<T>
    {
        private readonly LinkedList<T> _items;

        public Queue()
        {
            _items = new LinkedList<T>();
        }

        public T CurrentItem
        {
            get
            {
                if (IsEmpty)
                    throw new InvalidOperationException("Очередь пуста");

                var firstNode = _items.First;
                if (firstNode == null)
                    throw new InvalidOperationException("Очередь повреждена: первый элемент не найден");

                return firstNode.Value;
            }
        }

        public int Count => _items.Count;

        public bool IsEmpty => _items.Count == 0;

        public void Enqueue(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Элемент не может быть null");

            _items.AddLast(item);
        }

        public T Dequeue()
        {
            if (IsEmpty)
                throw new InvalidOperationException("Очередь пуста");

            var firstNode = _items.First;
            if (firstNode == null)
                throw new InvalidOperationException("Очередь повреждена: первый элемент не найден");

            T value = firstNode.Value;
            _items.RemoveFirst();
            return value;
        }

        public void Clear()
        {
            _items.Clear();
        }

        public IEnumerable<T> GetAllItems()
        {
            return _items.ToList();
        }

        public bool TryGetCurrentItem(out T item)
        {
            if (IsEmpty)
            {
                item = default(T)!;
                return false;
            }

            var firstNode = _items.First;
            if (firstNode == null)
            {
                item = default(T)!;
                return false;
            }

            item = firstNode.Value;
            return true;
        }

        public bool TryDequeue(out T item)
        {
            if (IsEmpty)
            {
                item = default(T)!;
                return false;
            }

            var firstNode = _items.First;
            if (firstNode == null)
            {
                item = default(T)!;
                return false;
            }

            item = firstNode.Value;
            _items.RemoveFirst();
            return true;
        }
    }
}