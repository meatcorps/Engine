using System.Numerics;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Game.ArcadeTemplate.GameObjects.Abstractions;
using Meatcorps.Game.ArcadeTemplate.Scenes;
using Raylib_cs;

namespace Meatcorps.Game.ArcadeTemplate.GameObjects.UI;

public class EndGameGameObject : ResourceGameObject
{
    private IRenderTargetStrategy _renderer;
    private EndScene _endScene;
    private FixedTimer _timer = new(5000);

    protected override void OnInitialize()
    {
        Layer = 1;
        Camera = CameraLayer.UI;
        base.OnInitialize();
        _endScene = (EndScene)Scene;
        _renderer = GlobalObjectManager.ObjectManager.Get<IRenderTargetStrategy>()!;
    }

    protected override void OnUpdate(float deltaTime)
    {
        _timer.Update(deltaTime);
    }

    protected override void OnDraw()
    {
        var str = $"BACK TO START IN {_endScene.TimeLeft} SECONDS"; 
        var size = Raylib.MeasureTextEx(Fonts.GetFont(), str, 10, 1);
        Raylib.DrawRectangleGradientH(0, 0, _renderer.RenderWidth, _renderer.RenderHeight, Raylib.ColorFromHSV(_timer.NormalizedElapsed * 360, 0.2f, 0.4f), Raylib.ColorFromHSV(_timer.NormalizedElapsed * 360, 0.1f, 0.1f));
        Raylib.DrawTextEx(Fonts.GetFont(), str, new Vector2((float)_renderer.RenderWidth / 2 - size.X / 2, _renderer.RenderHeight - 16 - size.Y), 10, 1, Color.White);
        base.OnDraw();
    }

    protected override void OnDispose()
    {
    }
}