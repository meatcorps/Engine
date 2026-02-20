using System.Numerics;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Game.KillTheSkulls.GameEnums;
using Meatcorps.Game.KillTheSkulls.GameObjects.Abstractions;
using Raylib_cs;

namespace Meatcorps.Game.KillTheSkulls.GameObjects.UI;

public class GuidePage : ResourceGameObject, IIntroSlide
{
    private readonly FixedTimer _pokeTimer = new(500);
    private readonly FixedTimer _flyAnimation = new(50);

    protected override void OnInitialize()
    {
        base.OnInitialize();
        Layer = 1;
        Camera = CameraLayer.UI;
    }

    protected override void OnUpdate(float deltaTime)
    {
        _pokeTimer.Update(deltaTime);
        _flyAnimation.Update(deltaTime);
    }

    protected override void OnDraw()
    {
        Raylib.DrawRectangle(0, 0, RenderTarget!.RenderWidth, RenderTarget!.RenderHeight,
            Raylib.ColorAlpha(Color.DarkGray, 0.5f));

        Raylib.DrawTextEx(Fonts.GetFont(), "HOW TO PLAY THIS GAME", new Vector2(16, 16), 24, 0, Color.Magenta);

        Sprites.DrawAnimationWithNormal(GameSprites.ArcadeButtonAnimation, _pokeTimer.NormalizedElapsed,
            new Vector2(16, 56), Color.White);
        Sprites.DrawAnimationWithNormal(GameSprites.ArcadeButtonAnimation, _pokeTimer.NormalizedElapsed,
            new Vector2(48, 56), Color.Blue);
        Sprites.DrawAnimationWithNormal(GameSprites.ArcadeButtonAnimation, _pokeTimer.NormalizedElapsed,
            new Vector2(80, 56), Color.Green);
        Sprites.DrawAnimationWithNormal(GameSprites.ArcadeButtonAnimation, _pokeTimer.NormalizedElapsed,
            new Vector2(112, 56), Color.Yellow);
        Sprites.DrawAnimationWithNormal(GameSprites.ArcadeButtonAnimation, _pokeTimer.NormalizedElapsed,
            new Vector2(144, 56), Color.Red);
        Raylib.DrawTextEx(Fonts.GetFont(), "USE THE CORRECT COLORED BUTTONS AT THE CORRECT TIME", new Vector2(192, 72), 8, 0, Color.White);
        Raylib.DrawTextEx(Fonts.GetFont(), "WITHOUT MISSES YOUR POINTS WILL BE MULTIPLIED", new Vector2(16, 98), 8, 0, Color.White);
        Raylib.DrawTextEx(Fonts.GetFont(), "EVERY STREAK OF 10 WILL GET +1 LIFE WITH A MAX OF 10", new Vector2(16, 130), 8, 0, Color.White);
        Raylib.DrawTextEx(Fonts.GetFont(), "EVERY 5000 SCORE THE DIFFICULTY WILL BE INCREASED!!", new Vector2(16, 162), 8, 0, Color.Red);

        base.OnDraw();
    }

    protected override void OnDispose()
    {
    }

    public bool Active { get; set; }
}