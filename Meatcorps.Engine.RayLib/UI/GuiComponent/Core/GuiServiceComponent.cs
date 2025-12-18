using Meatcorps.Engine.Core.Interfaces.Components;
using Meatcorps.Engine.Core.ObjectManager;

namespace Meatcorps.Engine.RayLib.UI.GuiComponent.Core;

public class GuiServiceComponent : IGameComponent
{
    private readonly List<Action> _drawActions = new();
    private readonly ObjectManager _objectManager;
    private GuiService _guiService = null!;

    public GuiServiceComponent(ObjectManager? objectManager)
    {
        _objectManager = objectManager ?? GlobalObjectManager.ObjectManager;
    }

    public void Initialize()
    {
        _guiService = _objectManager.Get<GuiService>()!;
    }


    public void PreUpdate(float deltaTime)
    {
        _drawActions.Clear();
    }

    public void Update(float deltaTime)
    {
    }

    public void LateUpdate(float deltaTime)
    {
    }

    public void Draw()
    {
        foreach (var draw in _drawActions)
            draw();
    }

    public void AddItem(BaseGuiItem item)
    {
        _guiService.AddItem(item);
    }

    public void CloseItem()
    {
        _guiService.Stop();
    }

    public void Start()
    {
        _guiService.CurrentComponent = this;
    }

    public void RegisterDraw(Action draw)
    {
        _drawActions.Add(draw);
    }
}