namespace Meatcorps.Engine.Core.Interfaces.Config;

/// <summary>
/// Implemented by objects that need to react when a config value changes at runtime.
/// Register implementations via <c>ObjectManager.Add&lt;IConfigChangeTracker&gt;</c> to receive notifications.
/// </summary>
public interface IConfigChangeTracker
{
    /// <summary>
    /// Called when a config value changes.
    /// </summary>
    /// <param name="group">The config group that changed (e.g. "Graphics").</param>
    /// <param name="key">The key that changed (e.g. "FullScreen").</param>
    /// <param name="value">The new value.</param>
    public void ConfigChanged(string group, string key, object value);
}