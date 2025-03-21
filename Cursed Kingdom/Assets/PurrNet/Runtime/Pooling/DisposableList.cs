using System;
using System.Collections;
using System.Collections.Generic;

namespace PurrNet.Pooling
{
    public struct DisposableHashSet<T> : ISet<T>, IDisposable
    {
        private readonly HashSet<T> _set;

        public DisposableHashSet(int capacity)
        {
            var set = HashSetPool<T>.Instantiate();

            if (set.Count < capacity)
                set = new HashSet<T>(set);

            _set = set;
            isDisposed = false;
        }

        public void Dispose()
        {
            if (isDisposed) return;

            HashSetPool<T>.Destroy(_set);
            isDisposed = true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableHashSet<T>));
            return _set.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableHashSet<T>));
            return GetEnumerator();
        }

        public void Add(T item)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableHashSet<T>));
            if (item == null) throw new ArgumentNullException(nameof(item));

            _set.Add(item);
        }

        public void UnionWith(IEnumerable<T> other)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableHashSet<T>));
            _set.UnionWith(other);
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableHashSet<T>));
            _set.IntersectWith(other);
        }

        bool ISet<T>.Add(T item)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableHashSet<T>));
            return _set.Add(item);
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableHashSet<T>));
            _set.ExceptWith(other);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableHashSet<T>));
            _set.SymmetricExceptWith(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableHashSet<T>));
            return _set.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableHashSet<T>));
            return _set.IsSupersetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableHashSet<T>));
            return _set.IsProperSupersetOf(other);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableHashSet<T>));
            return _set.IsProperSubsetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableHashSet<T>));
            return _set.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableHashSet<T>));
            return _set.SetEquals(other);
        }

        public void Clear()
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableHashSet<T>));
            _set.Clear();
        }

        public bool Contains(T item)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableHashSet<T>));
            return _set.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableHashSet<T>));
            _set.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableHashSet<T>));
            return _set.Remove(item);
        }

        public int Count
        {
            get
            {
                if (isDisposed) throw new ObjectDisposedException(nameof(DisposableHashSet<T>));
                return _set.Count;
            }
        }

        public bool IsReadOnly => false;
        public bool isDisposed { get; private set; }
    }

    public struct DisposableList<T> : IList<T>, IDisposable
    {
        private readonly bool _shouldDispose;

        public List<T> list { get; }

        public DisposableList(List<T> list)
        {
            this.list = list;
            isDisposed = false;
            _shouldDispose = true;
        }

        public DisposableList(int capacity)
        {
            var newList = ListPool<T>.Instantiate();

            if (newList.Capacity < capacity)
                newList.Capacity = capacity;

            list = newList;
            isDisposed = false;
            _shouldDispose = true;
        }

        public void AddRange(IEnumerable<T> collection)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
            foreach (var item in collection)
                list.Add(item);
        }

        public void Dispose()
        {
            if (isDisposed) return;

            if (_shouldDispose && list != null)
                ListPool<T>.Destroy(list);
            isDisposed = true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
            return GetEnumerator();
        }

        public void Add(T item)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
            list.Add(item);
        }

        public void Clear()
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
            list.Clear();
        }

        public bool Contains(T item)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
            return list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
            list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
            return list.Remove(item);
        }

        public int Count
        {
            get
            {
                if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
                return list.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
                return false;
            }
        }

        public bool isDisposed { get; private set; }

        public int IndexOf(T item)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
            return list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
            list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
            list.RemoveAt(index);
        }

        public T this[int index]
        {
            get
            {
                if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
                return list[index];
            }
            set
            {
                if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
                list[index] = value;
            }
        }
    }
}