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
        _gui.Start();
        _gui.AddItem(new PanelElement(new RectF(32, 32, 200, 200)));
            //_gui.AddItem(new RectangleLinesElement(Color.White, 2));
            _gui.AddItem(new StackElement(new RectF(0, 0, 200, 200), uv: UVHelper.LeftTop));
                _gui.AddItem(new PanelElement(new RectF(0, 0, 50, 50)));
                    _gui.AddItem(new RectangleLinesElement(Color.White, 2));
                _gui.CloseItem();
                _gui.AddItem(new PanelElement(new RectF(0, 0, 100, 20)));
                    _gui.AddItem(new RectangleLinesElement(Color.Red, 2));
                    _gui.AddItem(new TextElement<DefaultFont>(Color.White, _textManager.GetFont(), "Hello"));
                _gui.CloseItem();
                _gui.AddItem(new PanelElement(new RectF(0, 0, 40, 50)));
                    _gui.AddItem(new RectangleLinesElement(Color.Green, 2));
                _gui.CloseItem();
                _gui.AddItem(new PanelElement(new RectF(0, 0, 30, 50)));
                    _gui.AddItem(new RectangleLinesElement(Color.Blue, 2));
                _gui.CloseItem();
                _gui.AddItem(new StackElement(new RectF(0, 0, 0, 0), 4, new Vector2(1, 0), UVHelper.Left));
                    _gui.AddItem(new PanelElement(new RectF(0, 0, 50, 50)));
                    _gui.AddItem(new RectangleLinesElement(Color.White, 2));
                    _gui.CloseItem();
                    _gui.AddItem(new PanelElement(new RectF(0, 0, 50, 50)));
                    _gui.AddItem(new RectangleLinesElement(Color.White, 2));
                    _gui.CloseItem();
                    _gui.AddItem(new PanelElement(new RectF(0, 0, 50, 50)));
                    _gui.AddItem(new RectangleLinesElement(Color.White, 2));
                    _gui.CloseItem();
                _gui.CloseItem();
            _gui.CloseItem();
        _gui.CloseItem();
    }


    protected override void OnDraw()
    {
        base.OnDraw();
    }
    
    protected override void OnDispose()
    {
    }
}