using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Game;

namespace Meatcorps.Engine.RayLib.Interfaces;

public interface IGameLoopTask
{
    /// <summary>
    /// Task order
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Indicates whether the game loop task is enabled and should be executed.
    /// </summary>
    bool Enabled { get; set; }

    /// <summary>
    /// Indicates whether the game loop task has been initialized.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// When the GameHost get initialized this will be called. This is before Raylib is called! Please use the enum PostRaylibInit for raylib specific tasks.
    /// </summary>
    /// <param name="host"></param>
    void Initialize(GameHost host);
    
    /// <summary>
    /// Task to run for the game loop.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="deltaTime">NOTICE: This is only set inside the update loop!</param>
    void Task(GameLoopType type, float deltaTime);
}