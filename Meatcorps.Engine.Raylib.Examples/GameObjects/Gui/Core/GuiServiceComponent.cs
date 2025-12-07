using Meatcorps.Engine.Core.Interfaces.Components;
using Meatcorps.Engine.Core.ObjectManager;

namespace Meatcorps.Engine.Raylib.Examples.GameObjects.Gui.Core;

public class GuiServiceComponent : IGameComponent
{
    private readonly ObjectManager _objectManager;
    private GuiService _guiService = null!;
    private List<Action> _drawActions = new List<Action>();

    public GuiServiceComponent(ObjectManager objectManager)
    {
        _objectManager = objectManager;
    }

    public void AddItem(BaseGuiItem item)
    {
        _guiService.AddItem(item);
    }
    public void CloseItem()
    {
        _guiService.Stop();
    }

    public void Initialize()
    {
        _guiService = _objectManager.Get<GuiService>()!;
    }

    public void Start()
    {
        _guiService.CurrentComponent = this;
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

    public void RegisterDraw(Action draw)
    {
        _drawActions.Add(draw);
    }
    
    public void Draw()
    {
        foreach (var draw in _drawActions)
            draw();
    }
}