using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Game.CyberMaze.GameObjects.Abstractions;
using Raylib_cs;

namespace Meatcorps.Game.CyberMaze.GameObjects.UI;

public class VersionUI: ResourceGameObject
{
    protected override void OnInitialize()
    {
        base.OnInitialize();
        Layer = 8;
        Camera = CameraLayer.UI;
    }

    protected override void OnUpdate(float deltaTime)
    {
    }

    protected override void OnDraw()
    {
        var text = "ALPHA VERSION 0.0.2 Bugs, requests?\nReddit: r/meatcorps, info@meatcorps\nYT: @MeatcorpsOfficial Thnx!";
        var size = Fonts.MeasureText(DefaultFont.Default, text, 8, 1);
        var rectCanvas = new RectF(16, 16, RenderTarget!.RenderWidth - 32, RenderTarget!.RenderHeight - 32);
        var rectText = new RectF(0, 0, size.X, size.Y);
        rectText = rectCanvas.Align(rectText, UVHelper.RightTop);
        var bg = rectText;
        bg.Inflate(4, 4);
        bg.DrawFilled(Color.Black);
        Raylib.DrawTextEx(Fonts.GetFont(), text, rectCanvas.Align(rectText, UVHelper.RightTop).Position, 8, 1, Color.Red);
        base.OnDraw();
    }

    protected override void OnDispose()
    {
    }
}