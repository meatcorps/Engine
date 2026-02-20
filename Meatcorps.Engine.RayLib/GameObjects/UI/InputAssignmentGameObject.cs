using System.Numerics;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Tween;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.GameObjects.UI;

public class InputAssignmentGameObject<T> : BaseGameObject where T : Enum
{
    private readonly FixedTimer _animationTimer = new(1000);
    private readonly FixedTimer _animationTimerQuick = new(500);
    private readonly TimerOn _countDownToStartTimer;
    private readonly Action _onReady;
    private readonly int _totalPlayers;
    private readonly bool _useSceneObjectManager;
    private IDefaultFont _defaultFont = null!;
    private InputManager<T> _inputManager = null!;
    private IRenderTargetStrategy _renderTarget = null!;


    public InputAssignmentGameObject(int totalPlayers, Action onReady, int countDownToStartInSeconds = 3,
        bool useSceneObjectManager = false)
    {
        Layer = 10;
        Camera = CameraLayer.UI;
        _totalPlayers = totalPlayers;
        _onReady = onReady;
        _useSceneObjectManager = useSceneObjectManager;
        _countDownToStartTimer = new TimerOn(countDownToStartInSeconds * 1000);
    }

    public string Title { get; set; } = "Waiting for players";

    protected override void OnInitialize()
    {
        _inputManager = _useSceneObjectManager 
            ? Scene.SceneObjectManager.Get<InputManager<T>>()! 
            : GlobalObjectManager.ObjectManager.Get<InputManager<T>>()!;

        _inputManager.AssignPlayers(_totalPlayers);
        _renderTarget = GlobalObjectManager.ObjectManager.Get<IRenderTargetStrategy>()!;
        _defaultFont = GlobalObjectManager.ObjectManager.Get<IDefaultFont>()!;
    }

    protected override void OnUpdate(float deltaTime)
    {
        _animationTimerQuick.Update(deltaTime);
        _animationTimer.Update(deltaTime);
        _countDownToStartTimer.Update(_inputManager.Ready, deltaTime);
        if (_countDownToStartTimer.Output)
        {
            _onReady();
            Enabled = false;
            Scene.RemoveGameObject(this);
        }
    }

    protected override void OnDraw()
    {
        if (_inputManager.Ready)
            Title = "Getting ready in...";

        var normal = Tween.ApplyEasing(Tween.NormalToUpDown(_animationTimer.NormalizedElapsed), EaseType.EaseInOut);
        var status = _inputManager.CurrentInputStatus.ToArray();
        var titleSize = Raylib.MeasureTextEx(_defaultFont.GetFont(), Title, 16, 1);
        if (_animationTimerQuick.NormalizedElapsed > 0.5)
            Raylib.DrawTextEx(_defaultFont.GetFont(), Title,
                new Vector2(_renderTarget.RenderWidth / 2f - titleSize.X / 2, 24), 16, 1, Color.White);
        var errors = false;
        var previousControllerType = PlayerInputType.KeyboardMouse;

        for (var i = 0; i < _totalPlayers; i++)
        {
            var wait = previousControllerType == PlayerInputType.Unknown;
            var statusText = errors ? "Wait..." : "Press any\nbutton / key";

            var colorStatus = new Color(0f, 1f, 1f, normal);

            if (wait)
            {
                colorStatus = Color.Gray;
                statusText = "Wait...";
            }
            else if (status[i].Type != PlayerInputType.Unknown && status[i].Online)
            {
                colorStatus = Color.Green;
                statusText = "Ready!\n" + status[i].Type;
            }
            else if (status[i].Type != PlayerInputType.Unknown && !status[i].Online)
            {
                colorStatus = _animationTimerQuick.NormalizedElapsed > 0.5 ? Color.Red : Color.Black;
                statusText = "Please reconnect\n" + status[i].Type;
                errors = true;
            }

            if (_inputManager.Ready)
            {
                colorStatus = Color.Green;
                statusText = (_countDownToStartTimer.TimeRemaining / 1000).ToString("F1") + " seconds";
            }

            var height = (_renderTarget.RenderHeight - 64) / _totalPlayers;
            var rect = new Rect(16, 48 + height * i, _renderTarget.RenderWidth - 32,
                height - 5);

            Raylib.DrawRectangleLinesEx(rect.ToRectangle(), 2, colorStatus);
            Raylib.DrawTextEx(_defaultFont.GetFont(), "Player " + (i + 1), new Vector2(rect.X + 16, rect.Y + 16), 16, 1,
                colorStatus);

            var size = Math.Clamp(rect.Height / 2 - 24, 8, 32);

            var statusSize = Raylib.MeasureTextEx(_defaultFont.GetFont(), statusText, size, 1);
            Raylib.DrawTextEx(_defaultFont.GetFont(), statusText,
                rect.Center.ToVector2() - statusSize / 2 + new Vector2(0, 9), size, 1, colorStatus);

            previousControllerType = status[i].Type;
        }

        base.OnDraw();
    }

    protected override void OnDispose()
    {
    }
}