using System.Collections;
using Meatcorps.Engine.Core.Settings;
// ReSharper disable MemberHidesStaticFromOuterClass

namespace Meatcorps.Engine.Core.ObjectManager;

/// <summary>
/// A type-keyed, tag-keyed service registry used as a lightweight DI container.
/// Supports single instances, <see cref="List{T}"/> collections, and <see cref="HashSet{T}"/> collections.
/// All registered <see cref="IDisposable"/> instances are disposed when <see cref="Dispose"/> is called.
/// </summary>
public class ObjectManager : IDisposable
{
    private readonly Dictionary<(Type, string), object> _registry = new();
    private bool _disposed;

    /// <summary>
    /// Registers a single instance under type <typeparamref name="T"/> and the given tag.
    /// Overwrites any existing registration for the same type and tag.
    /// </summary>
    /// <param name="instance">The instance to register.</param>
    /// <param name="tag">Optional tag to differentiate multiple registrations of the same type.</param>
    /// <returns>The registered instance.</returns>
    public T Register<T>(T instance, string tag = "default") where T : class
    {
        var key = (typeof(T), tag);
        _registry[key] = instance;
        
        return instance;
    }
    
    /// <summary>
    /// Registers the instance only if no registration exists for type <typeparamref name="T"/> and the given tag.
    /// If already registered, returns the existing instance without overwriting it.
    /// </summary>
    /// <param name="instance">The instance to register if not already present.</param>
    /// <param name="tag">Optional tag to differentiate multiple registrations of the same type.</param>
    /// <returns>The existing or newly registered instance.</returns>
    public T RegisterOnce<T>(T instance, string tag = "default") where T : class
    {
        if (_registry.ContainsKey((typeof(T), tag)))
            return Get<T>()!;
        Register(instance, tag);
        return instance;
    }

    /// <summary>
    /// Pre-registers an empty <see cref="List{T}"/> under the given tag.
    /// Must be called before using <see cref="Add{T}"/> or <see cref="Remove{T}(T, string)"/> for list-based storage.
    /// Does nothing if a list is already registered for this type and tag.
    /// </summary>
    /// <param name="tag">Optional tag to differentiate multiple list registrations of the same type.</param>
    public void RegisterList<T>(string tag = "default") where T : class
    {
        if (_registry.ContainsKey((typeof(List<T>), tag)))
            return;
        
        var key = (typeof(List<T>), tag);
        _registry[key] = new List<T>();
    }

    /// <summary>
    /// Pre-registers an empty <see cref="HashSet{T}"/> under the given tag.
    /// Must be called before using <see cref="Add{T}"/> or <see cref="Remove{T}(T, string)"/> for set-based storage.
    /// Does nothing if a set is already registered for this type and tag.
    /// </summary>
    /// <param name="tag">Optional tag to differentiate multiple set registrations of the same type.</param>
    public void RegisterSet<T>(string tag = "default") where T : class
    {
        if (_registry.ContainsKey((typeof(HashSet<T>), tag)))
            return;
        
        var key = (typeof(HashSet<T>), tag);
        _registry[key] = new HashSet<T>();
    }

    /// <summary>
    /// Retrieves a single registered instance of type <typeparamref name="T"/> by tag.
    /// </summary>
    /// <param name="tag">The tag used during registration.</param>
    /// <returns>The registered instance, or <c>null</c> if not found.</returns>
    public T? Get<T>(string tag = "default") where T : class
    {
        var key = (typeof(T), tag);
        return _registry.TryGetValue(key, out var value) ? value as T : null;
    }

    /// <summary>
    /// Retrieves a registered <see cref="List{T}"/> by tag.
    /// </summary>
    /// <param name="tag">The tag used during <see cref="RegisterList{T}"/>.</param>
    /// <returns>The registered list, or <c>null</c> if not found.</returns>
    public List<T>? GetList<T>(string tag = "default") where T : class
    {
        var key = (typeof(List<T>), tag);
        return _registry.TryGetValue(key, out var value) ? value as List<T> : null;
    }

    /// <summary>
    /// Retrieves a registered <see cref="HashSet{T}"/> by tag.
    /// </summary>
    /// <param name="tag">The tag used during <see cref="RegisterSet{T}"/>.</param>
    /// <returns>The registered set, or <c>null</c> if not found.</returns>
    public HashSet<T>? GetSet<T>(string tag = "default") where T : class
    {
        var key = (typeof(HashSet<T>), tag);
        return _registry.TryGetValue(key, out var value) ? value as HashSet<T> : null;
    }

    /// <summary>
    /// Adds an instance to a registered <see cref="List{T}"/> or <see cref="HashSet{T}"/> with the given tag.
    /// </summary>
    /// <param name="instance">The instance to add.</param>
    /// <param name="tag">The tag of the target collection.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no <see cref="List{T}"/> or <see cref="HashSet{T}"/> is registered for the given type and tag.
    /// Call <see cref="RegisterList{T}"/> or <see cref="RegisterSet{T}"/> first.
    /// </exception>
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

    /// <summary>
    /// Removes an instance from a registered <see cref="List{T}"/> or <see cref="HashSet{T}"/> with the given tag.
    /// </summary>
    /// <param name="instance">The instance to remove.</param>
    /// <param name="tag">The tag of the target collection.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no <see cref="List{T}"/> or <see cref="HashSet{T}"/> is registered for the given type and tag.
    /// </exception>
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
    
    /// <summary>
    /// Removes the entire registration for type <typeparamref name="T"/> and the given tag.
    /// This removes single instances, lists, or sets registered under that key.
    /// </summary>
    /// <param name="tag">The tag of the registration to remove.</param>
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
            catch (Exception)
            {
                if (MeatcorpsEngineLibSettings.IsDebug)
                    Console.WriteLine(instance.GetType().FullName);
                throw;
                /* optional: log; never throw during shutdown */
            }
        }
    }

    // Reference equality comparer to dedupe instances by reference
    private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceEqualityComparer Instance = new();
        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);
        public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }
}