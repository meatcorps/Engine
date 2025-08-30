using System.Numerics;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Game.Pacman.GameEnums;
using Meatcorps.Game.Pacman.GameObjects.Abstractions;
using Meatcorps.Game.Pacman.Resources;
using Raylib_cs;

namespace Meatcorps.Game.Pacman.GameObjects.UI;

public class GuidePage : ResourceGameObject, IIntroSlide
{
    private IRenderTargetStrategy _renderer;
    private FixedTimer _pokeTimer = new(500);
    private FixedTimer _flyAnimation = new(50);

    protected override void OnInitialize()
    {
        base.OnInitialize();
        Layer = 1;
        Camera = CameraLayer.UI;

        _renderer = GlobalObjectManager.ObjectManager.Get<IRenderTargetStrategy>()!;
    }

    protected override void OnUpdate(float deltaTime)
    {
        _pokeTimer.Update(deltaTime);
        _flyAnimation.Update(deltaTime);
    }

    protected override void OnDraw()
    {
        Raylib.DrawRectangle(0, 0, _renderer.RenderWidth, _renderer.RenderHeight,
            Raylib.ColorAlpha(Color.DarkGray, 0.5f));

        Raylib.DrawTextEx(Fonts.GetFont(), "HOW TO PLAY THIS GAME", new Vector2(16, 16), 24, 0, Color.Magenta);

        Sprites.DrawAnimationWithNormal(GameSprites.ArcadeStickAnimationRotate, _pokeTimer.NormalizedElapsed,
            new Vector2(16, 56), Color.Red);
        Sprites.DrawAnimationWithNormal(GameSprites.ArcadeStickAnimationRotate, _pokeTimer.NormalizedElapsed,
            new Vector2(48, 56), Color.Blue);
        Raylib.DrawTextEx(Fonts.GetFont(), "USE THE POKE TO STEER PACMAN", new Vector2(88, 66), 8, 0, Color.White);
        Sprites.DrawAnimationWithNormal(GameSprites.GhostBlinkyAnimation, _pokeTimer.NormalizedElapsed,
            new Vector2(16, 90), Color.White);
        Sprites.Draw(GameSprites.GhostAngry,
            new Vector2(16, 90), Color.White);
        Sprites.DrawAnimationWithNormal(GameSprites.GhostPinkyAnimation, _pokeTimer.NormalizedElapsed,
            new Vector2(32, 90), Color.White);
        Sprites.Draw(GameSprites.GhostAngry,
            new Vector2(32, 90), Color.White);
        Sprites.DrawAnimationWithNormal(GameSprites.GhostInkyAnimation, _pokeTimer.NormalizedElapsed,
            new Vector2(48, 90), Color.White);
        Sprites.Draw(GameSprites.GhostAngry,
            new Vector2(48, 90), Color.White);
        Sprites.DrawAnimationWithNormal(GameSprites.GhostClydeAnimation, _pokeTimer.NormalizedElapsed,
            new Vector2(64, 90), Color.White);
        Sprites.Draw(GameSprites.GhostAngry,
            new Vector2(64, 90), Color.White);
        Raylib.DrawTextEx(Fonts.GetFont(), "WATCH OUT FOR BLINKY, PINKY, INKY & CLYDE!", new Vector2(88, 94), 8, 0, Color.White);
        Sprites.DrawAnimationWithNormal(GameSprites.GhostScaredAnimation, _pokeTimer.NormalizedElapsed,
            new Vector2(16, 108), Color.White);
        Sprites.Draw(GameSprites.GhostScared,
            new Vector2(16, 108), Color.White);
        Raylib.DrawTextEx(Fonts.GetFont(), "NOW YOU CAN ATTACK THEM!", new Vector2(88, 113), 8, 0, Color.Red);
        
        Raylib.DrawTextEx(Fonts.GetFont(), "YOU CAN BEAT THE LEVEL TO COLLECT ALL THE MEAT!", new Vector2(16, 180), 8, 0, Color.White);

        base.OnDraw();
    }

    protected override void OnDispose()
    {
    }

    public bool Active { get; set; }
}