using System.Buffers;
using System.Collections;

namespace Meatcorps.Engine.Core.Utilities;

public sealed class ThreadSafeList<T>
{
    private readonly List<T> _list = new();
    private readonly object _lock = new();

    public int Count { get { lock (_lock) return _list.Count; } }

    public void Add(T item) { lock (_lock) _list.Add(item); }
    public bool Remove(T item) { lock (_lock) return _list.Remove(item); }

    // Struct enumerator that acquires the lock and releases it on Dispose
    public struct LockedEnumerator : IEnumerator<T>
    {
        private readonly object _lockObj;
        private List<T>.Enumerator _inner;

        internal LockedEnumerator(object lockObj, List<T> list)
        {
            _lockObj = lockObj;
            Monitor.Enter(_lockObj);
            _inner = list.GetEnumerator();
        }

        public T Current => _inner.Current;

        public bool MoveNext() => _inner.MoveNext();
        public void Reset() => ((IEnumerator)_inner).Reset();
        object? IEnumerator.Current => _inner.Current;

        public void Dispose()
        {
            _inner.Dispose();
            Monitor.Exit(_lockObj);
        }
    }

    // Enable `foreach (var x in list)` without allocating
    public LockedEnumerator GetEnumerator() => new(_lock, _list);
}