using System.Numerics;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Camera;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Game.Snake.GameObjects.Abstractions;
using Meatcorps.Game.Snake.Resources;
using Raylib_cs;

namespace Meatcorps.Game.Snake.GameObjects.UI;

public class GuidePage : SnakeGameObject, IIntroSlide
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
        Raylib.DrawRectangle(0,0, _renderer.RenderWidth, _renderer.RenderHeight, Raylib.ColorAlpha(Color.DarkGray, 0.5f));
        
        Raylib.DrawTextEx(Fonts.GetFont(), "HOW TO PLAY THIS GAME", new Vector2(16, 16), 24, 0, Color.Magenta);
        
        Sprites.DrawAnimationWithNormal(SnakeSprites.ArcadeStickAnimationRotate, _pokeTimer.NormalizedElapsed, new Vector2(16, 56), Color.Red);
        Sprites.DrawAnimationWithNormal(SnakeSprites.ArcadeStickAnimationRotate, _pokeTimer.NormalizedElapsed, new Vector2(48, 56), Color.Blue);
        Raylib.DrawTextEx(Fonts.GetFont(), "USE THE POKE TO STEER THE SNAKE", new Vector2(88, 66), 8, 0, Color.White);
        
        var positionY = 86;
        Sprites.Draw(SnakeSprites.Wall, new Vector2(16, positionY), Color.White);
        Raylib.DrawTextEx(Fonts.GetFont(), "WATCH OUT FOR WALLS. YOU GOT 3 SECONDS TO MOVE", new Vector2(48, positionY + 4), 8, 0, Color.White);
        
        positionY += 16;
        Sprites.Draw(SnakeSprites.Meat1, new Vector2(16, positionY), Color.White);
        Raylib.DrawTextEx(Fonts.GetFont(), "50 POINTS CAN ROT!", new Vector2(48, positionY + 4), 8, 0, Color.White);
        positionY += 16;
        Sprites.Draw(SnakeSprites.Meat2, new Vector2(16, positionY), Color.White);
        Raylib.DrawTextEx(Fonts.GetFont(), "100 POINTS CAN ROT!", new Vector2(48, positionY + 4), 8, 0, Color.White);
        positionY += 16;
        Sprites.DrawAnimationWithNormal(SnakeSprites.FlyAnimation, _flyAnimation.NormalizedElapsed, new Vector2(16, positionY), Color.White);
        Raylib.DrawTextEx(Fonts.GetFont(), "NASTY FLIES WILL EAT MEAT. ONCE ROT NEGATIVE THE POINTS!", new Vector2(48, positionY + 4), 8, 0, Color.White);
        
        positionY += 24;
        Raylib.DrawTextEx(Fonts.GetFont(), "POWER UPS 30 SECONDS ACTIVATED", new Vector2(16, positionY + 2), 12, 0, Color.Gray);
        
        positionY += 16;
        Sprites.Draw(SnakeSprites.RotProof, new Vector2(16, positionY), Color.White);
        Raylib.DrawTextEx(Fonts.GetFont(), "HANDLE ROT MEAT. NO NEGATIVE POINTS", new Vector2(48, positionY + 4), 8, 0, Color.White);
        
        positionY += 16;
        Sprites.Draw(SnakeSprites.ThroughWalls, new Vector2(16, positionY), Color.White);
        Raylib.DrawTextEx(Fonts.GetFont(), "GO THROUGH WALLS!", new Vector2(48, positionY + 4), 8, 0, Color.White);
        
        positionY += 16;
        Sprites.Draw(SnakeSprites.WorldSlower, new Vector2(16, positionY), Color.White);
        Sprites.Draw(SnakeSprites.WorldFaster, new Vector2(32, positionY), Color.White);
        Raylib.DrawTextEx(Fonts.GetFont(), "MANIPULATE THE WORLD SPEED!", new Vector2(64, positionY + 4), 8, 0, Color.White);
        
        positionY += 16;
        Sprites.Draw(SnakeSprites.SnakeSlower, new Vector2(16, positionY), Color.White);
        Sprites.Draw(SnakeSprites.SnakeFaster, new Vector2(32, positionY), Color.White);
        Raylib.DrawTextEx(Fonts.GetFont(), "SNAKE WILL MOVE SLOWER OR FASTER", new Vector2(64, positionY + 4), 8, 0, Color.White);
        
        positionY += 16;
        Sprites.Draw(SnakeSprites.Score2X, new Vector2(16, positionY), Color.White);
        Sprites.Draw(SnakeSprites.Score3X, new Vector2(32, positionY), Color.White);
        Sprites.Draw(SnakeSprites.Score4X, new Vector2(48, positionY), Color.White);
        Raylib.DrawTextEx(Fonts.GetFont(), "MULTIPLY THE POINTS!", new Vector2(80, positionY + 4), 8, 0, Color.White);
        
        positionY += 24;
        Raylib.DrawTextEx(Fonts.GetFont(), "LITTLE WARNING. WHEN REACHING LEVEL 10 THE GAME WILL END!", new Vector2(16, positionY + 4), 8, 0, Color.Red);
        positionY += 16;
        Raylib.DrawTextEx(Fonts.GetFont(), "THINK SMART WITH THE POWER UPS! ", new Vector2(16, positionY + 4), 8, 0, Color.Red);
        positionY += 16;
        Raylib.DrawTextEx(Fonts.GetFont(), "YOU ALMOST DIED? INSERT POINTS TO RECOVER! MAX 3 TIMES...", new Vector2(16, positionY + 4), 8, 0, Color.Red);


        base.OnDraw();
    }

    protected override void OnDispose()
    {
    }

    public bool Active { get; set; }
}