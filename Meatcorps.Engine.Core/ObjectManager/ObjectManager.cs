using System.Collections;

namespace Meatcorps.Engine.Core.ObjectManager;

public class ObjectManager : IDisposable
{
    private readonly Dictionary<(Type, string), object> _registry = new();
    private bool _disposed;

    public void Register<T>(T instance, string tag = "default") where T : class
    {
        var key = (typeof(T), tag);
        _registry[key] = instance;
    }
    
    public void RegisterOnce<T>(T instance, string tag = "default") where T : class
    {
        if (_registry.ContainsKey((typeof(T), tag)))
            return;
        Register(instance, tag);
    }

    public void RegisterList<T>(string tag = "default") where T : class
    {
        if (_registry.ContainsKey((typeof(List<T>), tag)))
            return;
        
        var key = (typeof(List<T>), tag);
        _registry[key] = new List<T>();
    }

    public void RegisterSet<T>(string tag = "default") where T : class
    {
        if (_registry.ContainsKey((typeof(HashSet<T>), tag)))
            return;
        
        var key = (typeof(HashSet<T>), tag);
        _registry[key] = new HashSet<T>();
    }

    public T? Get<T>(string tag = "default") where T : class
    {
        var key = (typeof(T), tag);
        return _registry.TryGetValue(key, out var value) ? value as T : null;
    }

    public List<T>? GetList<T>(string tag = "default") where T : class
    {
        var key = (typeof(List<T>), tag);
        return _registry.TryGetValue(key, out var value) ? value as List<T> : null;
    }

    public HashSet<T>? GetSet<T>(string tag = "default") where T : class
    {
        var key = (typeof(HashSet<T>), tag);
        return _registry.TryGetValue(key, out var value) ? value as HashSet<T> : null;
    }

    public void Add<T>(T instance, string tag = "default") where T : class
    {
        var listKey = (typeof(List<T>), tag);
        var setKey = (typeof(HashSet<T>), tag);

        if (_registry.TryGetValue(listKey, out var listObj) && listObj is List<T> list)
        {
            list.Add(instance);
            return;
        }

        if (_registry.TryGetValue(setKey, out var setObj) && setObj is HashSet<T> set)
        {
            set.Add(instance);
            return;
        }

        throw new InvalidOperationException(
            $"No List<{typeof(T).Name}> or HashSet<{typeof(T).Name}> found with tag '{tag}'.");
    }

    public void Remove<T>(T instance, string tag = "default") where T : class
    {
        var listKey = (typeof(List<T>), tag);
        var setKey = (typeof(HashSet<T>), tag);

        if (_registry.TryGetValue(listKey, out var listObj) && listObj is List<T> list)
        {
            list.Remove(instance);
            return;
        }

        if (_registry.TryGetValue(setKey, out var setObj) && setObj is HashSet<T> set)
        {
            set.Remove(instance);
            return;
        }

        throw new InvalidOperationException(
            $"No List<{typeof(T).Name}> or HashSet<{typeof(T).Name}> found with tag '{tag}' to remove from.");
    }
    
    public void Remove<T>(string tag = "default") where T : class
        => _registry.Remove((typeof(T), tag));

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        // Snapshot to avoid "collection modified" during disposal
        var values = _registry.Values.ToArray();

        // Prevent double-disposal if an object appears multiple times
        var disposed = new HashSet<object>(ReferenceEqualityComparer.Instance);

        foreach (var value in values)
        {
            // If the value itself is disposable, dispose it once
            TryDisposeOnce(value, disposed);

            // If it is a collection, dispose disposable items inside
            if (value is IEnumerable enumerable && value is not string)
            {
                foreach (var item in enumerable)
                    TryDisposeOnce(item, disposed);
            }
        }

        _registry.Clear();
    }

    private static void TryDisposeOnce(object? instance, HashSet<object> disposed)
    {
        if (instance is null) return;
        if (ReferenceEquals(instance, /* this */ null)) { /* keep your existing 'value != this' if needed */ }
        
        if (instance is IDisposable disposable && disposed.Add(instance))
        {
            try
            {
                disposable.Dispose();
            }
            catch (Exception e)
            {
#if DEBUG
                throw e;
#endif
                /* optional: log; never throw during shutdown */
            }
        }
        Console.WriteLine();
    }

    // Reference equality comparer to dedupe instances by reference
    private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceEqualityComparer Instance = new();
        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);
        public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }
}