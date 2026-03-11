using Meatcorps.Engine.Core.Interfaces.Components;

namespace Meatcorps.Engine.RayLib.Interfaces;

/// <summary>
/// Represents an action that can be executed on a game component of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">
/// The type of game component this action operates on. Must implement <see cref="IGameComponent"/>.
/// </typeparam>
public interface IComponentAction<in T> where T : IGameComponent
{
    /// <summary>
    /// Executes the specified action on the given game component.
    /// </summary>
    /// <param name="component">
    /// The instance of the game component of type <typeparamref name="T"/> on which the action will be performed.
    /// The component must implement <see cref="IGameComponent"/>.
    /// </param>
    void Execute(T component);
}