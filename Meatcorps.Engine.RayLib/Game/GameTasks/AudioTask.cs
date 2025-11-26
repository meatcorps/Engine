using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Game.GameTasks;

public class AudioTask: IGameLoopTask
{
    public int Priority => 0;
    public bool Enabled { get; set; } = true;
    public bool IsInitialized { get; private set; }

    private bool _autoInitialized;
    
    public void Initialize(GameHost host)
    {
        IsInitialized = true;
        //
    }

    public void Task(GameLoopType type, float deltaTime)
    {
        if (type == GameLoopType.PostRaylibInit)
            if (GlobalObjectManager.ObjectManager.GetList<IResourceLoadOnInit>()!.Any(x => x is IAudioInitNeeded))
            {
                if (!Raylib.IsAudioDeviceReady())
                {
                    Raylib.InitAudioDevice();
                    _autoInitialized = true;
                }
            }

        if (type == GameLoopType.AfterClosingWindow && _autoInitialized)
        {
            Raylib.CloseAudioDevice();
        }
    }
}