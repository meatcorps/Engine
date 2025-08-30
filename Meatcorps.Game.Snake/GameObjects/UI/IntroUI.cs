using System.Numerics;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Tween;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Audio;
using Meatcorps.Engine.RayLib.Camera;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.GameObjects.UI;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.Text;
using Meatcorps.Engine.RayLib.UI.Data;
using Meatcorps.Game.Snake.Data;
using Meatcorps.Game.Snake.GameObjects.Abstractions;
using Meatcorps.Game.Snake.Resources;
using Meatcorps.Game.Snake.Scenes;
using Raylib_cs;

namespace Meatcorps.Game.Snake.GameObjects.UI;

public class IntroUI: SnakeGameObject, IIntroSlide
{
    private IRenderTargetStrategy _renderer;
    private FixedTimer _timer = new(1000);
    private FixedTimer _buttonTimer = new(250);
    private FixedTimer _fastTimer = new(50);
    private FixedTimer _messageTimer = new(3000);
    private UIMessageEmitter _uiMessage;
    private int _counter = 0;
    private PlayerInputRouter<SnakeInput> _controller;
    private IntroScene _introScene;

    protected override void OnInitialize()
    {
        base.OnInitialize();
        Layer = 1;
        Camera = CameraLayer.UI;
        _renderer = GlobalObjectManager.ObjectManager.Get<IRenderTargetStrategy>()!;
        _uiMessage = Scene.GetGameObject<UIMessageEmitter>()!;
        _introScene = (IntroScene)Scene;
    }

    protected override void OnAlwaysUpdate(float deltaTime)
    {
        _messageTimer.Update(deltaTime);
        
        if (_messageTimer.Output && _introScene.TotalPlayersReady == 0)
        {
            _counter++;
            if (_counter == 4)
                _counter = 1;

            var targetText = "HELLO WORLD!";
            
            switch (_counter)
            {
                case 1:
                    targetText = "MEATCORPS DID IT AGAIN!";
                    break;
                case 2:
                    targetText = "A GORE VERSION OF SNAKE!";
                    break;
                case 3:
                    targetText = "EXPECT THE WORST!";
                    break;
            }
            
            var randomColor = new Color(Raylib.GetRandomValue(0, 255), Raylib.GetRandomValue(0, 255), 255);
            
            _uiMessage.Show(targetText, new UIMessageStyle
            {
                AnchorFrom = Anchor.BottomLeft,
                AnchorTo = Anchor.Bottom,
                AnchorAfter = Anchor.BottomRight,
                HoldDurationInMilliseconds = 2500,
                AppearDurationInMilliseconds = 250,
                DisappearDurationInMilliseconds = 250,
                ColorFrom = Color.Black,
                ColorTo = randomColor,
                ColorAfter = Color.Black,
                Style = TextKitStyles.HudDefault(Fonts.GetFont()) with { PixelOutline = true, OutlineColor = Color.Black, UseOutline = true}
            });

        }
        base.OnAlwaysUpdate(deltaTime);
    }

    protected override void OnUpdate(float deltaTime)
    {
        _timer.Update(deltaTime);
        _buttonTimer.Update(deltaTime);
        
        if (_introScene.TotalPlayersReady > 0)
            _fastTimer.Update(deltaTime);
    }

    protected override void OnDraw()
    {
        var targetWidth = 440;
        var targetHeight = 240;
        var startPosX = (_renderer.RenderWidth - targetWidth) / 2;
        var startPosY = (_renderer.RenderHeight - targetHeight) / 2;

        var targetOffsetStart = -5;
        var targetOffsetEnd = 5;

        var targetOffset = (int)Tween.Lerp(targetOffsetStart, targetOffsetEnd,
            Tween.ApplyEasing(Tween.NormalToUpDown(_timer.NormalizedElapsed), EaseType.EaseInOutQuint));
        
        startPosY += targetOffset;
        
        var colorTitle = Raylib.ColorFromHSV(_messageTimer.NormalizedElapsed * 360, 1, 1);
        
        Raylib.DrawRectangle(startPosX, startPosY, targetWidth, targetHeight, Raylib.ColorAlpha(Color.Black, 0.5f));
        if (_introScene.TotalPlayersReady == 0)
        {
            Raylib.DrawTextEx(Fonts.GetFont(), "MEATCORPS PROUDLY PRESENTS:",
                new Vector2(startPosX + 8, startPosY + 16), 16, 0, Color.White);
        }

        Raylib.DrawTextEx(Fonts.GetFont(), _introScene.TotalPlayersReady > 0 ? "READY?" : "SNAKE!", new Vector2(startPosX + 40, startPosY + 80), 64, 0, colorTitle);
         
        if (_introScene.TotalPlayersReady == 0 || _fastTimer.NormalizedElapsed > 0.5f)
        {
            Sprites.DrawAnimationWithNormal(SnakeSprites.ArcadeButtonPlayerOneAnimation, _buttonTimer.NormalizedElapsed, new Vector2(startPosX + 16, startPosY + 172), Color.LightGray);
            Raylib.DrawTextEx(Fonts.GetFont(), "START 1 PLAYER", new Vector2(startPosX + 64, startPosY + 180), 16, 0,
                Color.Red);
        }

        if ((_introScene.TotalPlayersReady == 0 ||
             _fastTimer.NormalizedElapsed > 0.5f && _introScene.TotalPlayersReady == 2) &&
            _introScene.TotalPlayersReady != 1)
        {
            Raylib.DrawTextEx(Fonts.GetFont(), "START 2 PLAYERS",
                new Vector2(startPosX + targetWidth - 298, startPosY + 204), 16, 0, Color.Blue);
            Sprites.DrawAnimationWithNormal(SnakeSprites.ArcadeButtonPlayerTwoAnimation, _buttonTimer.NormalizedElapsed, new Vector2(startPosX + targetWidth - 48, startPosY + 196), Color.LightGray);
        }
        
        
        Raylib.DrawRectangleGradientV(0, _renderer.RenderHeight - 64, _renderer.RenderWidth, 80, Raylib.ColorAlpha(Color.Black, 0f), Color.Black);
    }

    protected override void OnDispose()
    {
    }
}