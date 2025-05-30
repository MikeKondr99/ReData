using System.Collections;

namespace Pattern.Core;

public static class Once
{
    public static Once<T> From<T>(T item)
    {
        return new Once<T>(item);
    }
}

public struct Once<T> : IEnumerable<T>
{
    private readonly T _item;
    

    public Once(T item)
    {
        _item = item;
    }
    

    public IEnumerator<T> GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private struct Enumerator : IEnumerator<T>
    {
        private readonly Once<T> _collection;
        private bool _hasYielded;

        public Enumerator(Once<T> collection)
        {
            _collection = collection;
            _hasYielded = false;
        }

        public T Current => _collection._item;

        object? IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (_hasYielded)
            {
                return false; // Already yielded the item
            }
            _hasYielded = true;
            return true; // Yield the item
        }

        public void Reset()
        {
            _hasYielded = false;
        }

        public void Dispose()
        {
            // No resources to dispose
        }
    }
}