using Meatcorps.Engine.Core.Enums;

namespace Meatcorps.Engine.Core.Interfaces.Config;

/// <summary>
/// Typed get/set access to a group-and-key configuration store.
/// Entries are organized by <c>group</c> (e.g. "Graphics") and <c>key</c> (e.g. "FullScreen").
/// </summary>
public interface IUniversalConfig
{
    /// <summary>Returns the string value for the given group/key, or <paramref name="defaultValue"/> if not set.</summary>
    /// <param name="expose">When <c>true</c>, registers the key so it appears in config editors and debug tooling.</param>
    public string GetOrDefault(string group, string key, string defaultValue, bool expose = true);

    /// <summary>Sets a string value for the given group/key.</summary>
    public void Set(string group, string key, string value);

    /// <summary>Returns the int value for the given group/key, or <paramref name="defaultValue"/> if not set.</summary>
    /// <param name="expose">When <c>true</c>, registers the key so it appears in config editors and debug tooling.</param>
    public int GetOrDefault(string group, string key, int defaultValue, bool expose = true);

    /// <summary>Sets an int value for the given group/key.</summary>
    public void Set(string group, string key, int value);

    /// <summary>Returns the float value for the given group/key, or <paramref name="defaultValue"/> if not set.</summary>
    /// <param name="expose">When <c>true</c>, registers the key so it appears in config editors and debug tooling.</param>
    public float GetOrDefault(string group, string key, float defaultValue, bool expose = true);

    /// <summary>Sets a float value for the given group/key.</summary>
    public void Set(string group, string key, float value);

    /// <summary>Returns the bool value for the given group/key, or <paramref name="defaultValue"/> if not set.</summary>
    /// <param name="expose">When <c>true</c>, registers the key so it appears in config editors and debug tooling.</param>
    public bool GetOrDefault(string group, string key, bool defaultValue, bool expose = true);

    /// <summary>Sets a bool value for the given group/key.</summary>
    public void Set(string group, string key, bool value);

    /// <summary>Returns all registered group names.</summary>
    public IEnumerable<string> GetGroups();

    /// <summary>Returns all key-value pairs in the given group with their resolved <see cref="ConfigValueType"/>.</summary>
    public IEnumerable<(string key, string value, ConfigValueType type)> GetKeys(string group);

    /// <summary>Persists the current config state to its backing store.</summary>
    public void Save();
}