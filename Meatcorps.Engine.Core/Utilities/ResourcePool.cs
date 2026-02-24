namespace Meatcorps.Engine.Core.Utilities;

/// <summary>
/// A pre-allocated object pool that recycles instances to avoid repeated allocations. Rent an item with Rent(), return it with Return() which resets it via the supplied delegate before re-queuing.
/// </summary>
public sealed class ResourcePool<T> : IDisposable
{
    private readonly bool _autoSize;
    private readonly Func<T> _createItem;
    private readonly Action<T> _resetItem;
    private readonly Queue<T> _itemsReady = new();
    private readonly List<T> _itemsInUse = new();

    /// <param name="initialPoolSize">Number of items to pre-allocate at construction.</param>
    /// <param name="autoSize">When true, creates new items if the pool is empty instead of throwing.</param>
    /// <param name="createItem">Factory delegate used to instantiate new items.</param>
    /// <param name="resetItem">Delegate called on each item before it is returned to the pool.</param>
    public ResourcePool(int initialPoolSize, bool autoSize, Func<T> createItem, Action<T> resetItem)
    {
        _autoSize = autoSize;
        _createItem = createItem;
        _resetItem = resetItem;

        for (int i = 0; i < initialPoolSize; i++)
        {
            _itemsReady.Enqueue(_createItem());
        }
    }

    /// <summary>Takes an item from the pool. Creates a new one if autoSize is true and pool is empty. Throws if pool is empty and autoSize is false.</summary>
    public T Rent()
    {
        T returnItem;

        if (_itemsReady.TryDequeue(out var result))
            returnItem = result!;
        else
        {
            if (_autoSize)
                returnItem = _createItem();
            else
                throw new Exception("No resources available.");
        }
        _itemsInUse.Add(returnItem);

        return returnItem;
    }

    /// <summary>Returns an item to the pool. The reset delegate is called before re-queuing.</summary>
    public void Return(T item)
    {
        _resetItem(item);
        _itemsInUse.Remove(item);
        _itemsReady.Enqueue(item);
    }

    /// <summary>Returns all currently rented items to the pool, resetting each one.</summary>
    public void ReleaseAll()
    {
        foreach (var item in _itemsInUse)
        {
            _resetItem(item);
            _itemsReady.Enqueue(item);
        }
        _itemsInUse.Clear();
    }

    public void Dispose()
    {
        ReleaseAll();

        while (_itemsReady.TryDequeue(out var result))
        {
            if (result is IDisposable disposable)
                disposable.Dispose();
        }
        _itemsReady.Clear();
    }
}