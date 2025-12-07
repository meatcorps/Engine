using System.Buffers;
using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.Raylib.Examples.GameObjects.Gui;
using Meatcorps.Engine.Raylib.Examples.GameObjects.Gui.Core;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.Resources;
using Raylib_cs;

namespace Meatcorps.Engine.Raylib.Examples.GameObjects;

public class GuiTest : BaseGameObject
{
    private GuiService _guiService;
    private GuiServiceComponent _gui;
    private TextManager<DefaultFont> _textManager;

    protected override void OnInitialize()
    {
        _guiService = new GuiService();
        Scene.SceneObjectManager.Register(_guiService);
        Scene.SceneObjectManager.Add<IBackgroundService>(_guiService);
        _gui = new GuiServiceComponent(Scene.SceneObjectManager);
        _textManager = GlobalObjectManager.ObjectManager.Get<TextManager<DefaultFont>>()!;
        AddComponent(_gui);
        Camera = CameraLayer.UI;
    }

    protected override void OnUpdate(float deltaTime)
    {
        Console.WriteLine("---------- UPDATE");
        _gui.Start();
        _gui.AddItem(new PanelElement(new RectF(32, 32, 200, 200)).SetMargin(MarginF.All(10)));
            _gui.AddItem(new RectangleLinesElement(Color.White, 2));
            _gui.AddItem(new PanelElement(new RectF(0, 0, 100, 100)).SetPadding(PaddingF.All(10)));
                _gui.AddItem(new RectangleLinesElement(Color.White, 2));
            _gui.CloseItem();
        _gui.CloseItem();
    }


    protected override void OnDraw()
    {
        var reference = new RectF(32, 32, 200, 200);
        reference.DrawFilled(Color.DarkGray);
        Console.WriteLine("---------- DRAW");
        base.OnDraw();
    }
    
    protected override void OnDispose()
    {
    }
}