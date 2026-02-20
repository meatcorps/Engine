namespace Meatcorps.Engine.Core.Utilities;

public sealed class ResourcePool<T> : IDisposable
{
    private readonly bool _autoSize;
    private readonly Func<T> _createItem;
    private readonly Action<T> _resetItem;
    private readonly Queue<T> _itemsReady = new();
    private readonly List<T> _itemsInUse = new();
    
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

    public void Return(T item)
    {
        _resetItem(item);   
        _itemsInUse.Remove(item);
        _itemsReady.Enqueue(item);
    }

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