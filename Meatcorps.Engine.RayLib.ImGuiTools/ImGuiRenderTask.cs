using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Game;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.Renderer;

namespace Meatcorps.Engine.RayLib.ImGuiTools;

public class ImGuiRenderTask: IGameLoopTask
{
    private ImGuiManager _manager = null!;
    private RenderService _renderService = null!;
    public int Priority => 51;
    public bool Enabled { get; set; } = true;
    public bool IsInitialized { get; private set; }
    
    public void Initialize(GameHost host)
    {
        _manager = GlobalObjectManager.ObjectManager.Get<ImGuiManager>()!;
        _renderService = GlobalObjectManager.ObjectManager.Get<RenderService>()!;
        IsInitialized = true;
    }

    public void Task(GameLoopType type, float deltaTime)
    {
        if (type == GameLoopType.AfterUpdate && _manager.Enabled)
            _renderService.RegisterRender(_manager);
    }
}