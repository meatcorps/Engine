namespace Meatcorps.Engine.Core.Interfaces.Components;

/// <summary>
/// Implemented by components that need to be notified when added or removed from a game object.
/// </summary>
public interface IGameComponentAddedRemoved
{
    /// <summary>
    /// Called when the component is added to a game object.
    /// </summary>
    /// <param name="component"></param>
    void OnAdded(IGameComponent component);
    
    /// <summary>
    /// Called when the component is removed from a game object.
    /// </summary>
    /// <param name="component"></param>
    void OnRemoved(IGameComponent component);
}
