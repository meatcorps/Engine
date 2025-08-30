using Meatcorps.Engine.Arcade.Services;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.UI;
using Meatcorps.Game.Pacman.GameObjects.Abstractions;
using Raylib_cs;

namespace Meatcorps.Game.Pacman.GameObjects.UI;

public class Leaderboard : ResourceGameObject, IIntroSlide
{
    private IRenderTargetStrategy _renderer;
    private InlineRender _scoreRenderer;
    private TimerOn _iterateTimer;
    private FixedTimer _timer;
    private int _totalItems;
    private int _previousLine = -1;
    private InlineRenderExtensions.LabelData _labelData;

    protected override void OnInitialize()
    {
        base.OnInitialize();
        _timer = new FixedTimer(1000);
        Layer = 1;
        Camera = CameraLayer.UI;
        _iterateTimer = new TimerOn(2000);
        _renderer = GlobalObjectManager.ObjectManager.Get<IRenderTargetStrategy>()!;
        var scores = GlobalObjectManager.ObjectManager.Get<HighScoreService>()!.GetLeaderboard(9);
        var bounds = new Rect(48, 32, _renderer.RenderWidth - 96, _renderer.RenderHeight - 128);
        _scoreRenderer = new InlineRender
            {
                Bounds = bounds,
                ItemSpacing = 0,
                LineSpacing = 2
            }
            .AddLabel(Fonts.GetFont(), "title", "Leaderboard", 24, Color.Magenta).AddNewLine()
            .AddSpacer(4, 4).AddNewLine();
        _scoreRenderer.TryGetItem("title", out var title);
        _labelData = (InlineRenderExtensions.LabelData)title!.Data!;

        var counter = 0;
        foreach (var score in scores)
        {
            _scoreRenderer
                .AddLabel(Fonts.GetFont(), $"{counter}_1", score.rank.ToString() + " ", 16, Color.Blue,
                    fixedWidth: false)
                .AddLabel(Fonts.GetFont(), $"{counter}_2", score.item.Name, 12, Color.White, fixedWidth: true,
                    fixedHeight: true,
                    vAlign: VAlign.Middle)
                .AddLabel(Fonts.GetFont(), $"{counter}_3", score.item.Score.ToString("00000000"), 12, Color.White,
                    fixedWidth: false, fixedHeight: true, vAlign: VAlign.Middle).AddNewLine()
                .AddNewLine($"{counter}_4")
                .AddRectIcon($"{counter}_5", 2, 2, Raylib.ColorAlpha(Color.Gray, 0.5f), fixedWidth: true)
                .AddNewLine($"{counter}_6");
            SetLineOnOrOff(counter, false);
            counter++;
        }

        _totalItems = counter;
    }

    protected override void OnEnabled()
    {
        _iterateTimer.Reset();
        for (var i = 0; i < _totalItems; i++)
            SetLineOnOrOff(i, false);
        base.OnEnabled();
    }


    private void SetLineOnOrOff(int line, bool on)
    {
        for (var i = 1; i <= 6; i++)
        {
            if (_scoreRenderer.TryGetItem($"{line}_{i}", out var item1))
                item1!.Enabled = on;
        }
    }

    protected override void OnUpdate(float deltaTime)
    {
        _timer.Update(deltaTime);
        _labelData.Color = Raylib.ColorFromHSV(_timer.NormalizedElapsed * 360, 1, 1);
        _iterateTimer.Update(true, deltaTime);
        var currentLine = (int)MathF.Floor(_iterateTimer.NormalizedElapsed * _totalItems);
        if (currentLine != _previousLine)
        {
            SetLineOnOrOff(currentLine, true);
            _previousLine = currentLine;
        }

        _scoreRenderer.Update();
    }

    protected override void OnDraw()
    {
        Raylib.DrawRectangle(32, 32, _renderer.RenderWidth - 64, _renderer.RenderHeight - 80,
            Raylib.ColorAlpha(Color.Black, 0.5f));
        _scoreRenderer.Draw();
    }

    protected override void OnDispose()
    {
    }
}