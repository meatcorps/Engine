namespace Meatcorps.Engine.Core.Interfaces.Components;

/// <summary>
/// Defines the core lifecycle for all game components and objects.
/// Implementations are driven by the engine's fixed-timestep update loop.
/// </summary>
public interface IGameComponent
{
    /// <summary>
    /// Called once when the component is first registered with its owner.
    /// Use this for one-time setup and dependency resolution.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Called before <see cref="Update"/> each tick.
    /// Used internally to process deferred operations such as queued additions and removals.
    /// </summary>
    /// <param name="deltaTime">Fixed time elapsed since the last tick, in seconds.</param>
    void PreUpdate(float deltaTime);

    /// <summary>
    /// Main update step called every fixed timestep tick.
    /// Skipped when the owner is disabled or the scene is paused.
    /// </summary>
    /// <param name="deltaTime">Fixed time elapsed since the last tick, in seconds.</param>
    void Update(float deltaTime);

    /// <summary>
    /// Called after all <see cref="Update"/> calls have completed for the current tick.
    /// Use this to react to state changes made during Update.
    /// Skipped when the owner is disabled or the scene is paused.
    /// </summary>
    /// <param name="deltaTime">Fixed time elapsed since the last tick, in seconds.</param>
    void LateUpdate(float deltaTime);

    /// <summary>
    /// Called during the render phase. Use this to issue draw calls.
    /// </summary>
    void Draw();
}