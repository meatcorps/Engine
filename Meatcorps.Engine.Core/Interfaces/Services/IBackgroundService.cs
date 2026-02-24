namespace Meatcorps.Engine.Core.Interfaces.Services;

/// <summary>
/// Represents a service that participates in the engine's fixed-timestep update loop
/// without being tied to a specific scene or game object.
/// Register implementations via <c>ObjectManager.Add&lt;IBackgroundService&gt;</c> to be driven each frame.
/// </summary>
public interface IBackgroundService
{
    /// <summary>Called before <see cref="Update"/> each tick. Use for input polling or pre-frame state preparation.</summary>
    void PreUpdate(float deltaTime);

    /// <summary>Main per-tick update. Use for game logic that runs independent of scene objects.</summary>
    void Update(float deltaTime);

    /// <summary>Called after all <see cref="Update"/> calls. Use to react to state changes made during Update.</summary>
    void LateUpdate(float deltaTime);
}