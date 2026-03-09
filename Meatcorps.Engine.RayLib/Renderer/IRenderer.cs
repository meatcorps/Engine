using Meatcorps.Engine.RayLib.Interfaces;

namespace Meatcorps.Engine.RayLib.Renderer;

/// <summary>
/// Defines the interface for rendering game objects.
/// </summary>
public interface IRenderer
{
    /// <summary>
    /// Represents the layer of the game object within its rendering scene.
    /// This property determines the rendering order and grouping of objects.
    /// Objects with lower layer values are typically rendered before objects with higher values.
    /// </summary>
    public int Layer { get; }

    /// <summary>
    /// Represents the layer index of the scene within which the game object is rendered.
    /// This property is used to organize and manage rendering priorities across different scenes.
    /// Objects are grouped and rendered based on their scene layer to maintain rendering order and scene segregation.
    /// </summary>
    public int SceneLayer { get; }

    /// <summary>
    /// Represents the rendering target strategy used for rendering game objects.
    /// This property determines where and how a game object is rendered, typically by specifying
    /// a strategy that dictates rendering bounds, camera configurations, and post-processing steps.
    /// If null, the default rendering strategy is used.
    /// </summary>
    public IRenderTargetStrategy? RenderTarget { get; set; }

    /// <summary>
    /// Renders the current game object or scene layer to the specified rendering target.
    /// This method is used to implement the drawing logic for objects that are part of the rendering pipeline.
    /// </summary>
    /// <remarks>
    /// Implementing classes should provide the specific drawing logic inside this method.
    /// Depending on the rendering strategy, this method may interact with render targets, scenes, or other game systems.
    /// If overridden as part of a component system, ensure that all necessary rendering contexts (e.g., layers, visibility) are respected.
    /// </remarks>
    void Draw();
}