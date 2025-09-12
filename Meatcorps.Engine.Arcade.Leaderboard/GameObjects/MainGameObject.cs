using System.Numerics;
using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.RayLib.GameObjects;
using Meatcorps.Engine.Arcade.Leaderboard.GameObjects.Abstractions;
using Meatcorps.Engine.Arcade.Services;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Tween;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.GameObjects.UI;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.Text;
using Meatcorps.Engine.RayLib.UI.Data;
using Raylib_cs;

namespace Meatcorps.Engine.Arcade.Leaderboard.GameObjects;

public class MainGameObject : ResourceGameObject
{
    private PersistentCanvas _canvas = null!;
    private ArcadeDataService _arcadeDataService = null!;
    private IDisposable _subscription = null!;
    private bool _isDisposed = false;
    private IReadOnlyList<ArcadePlayer> _players = new List<ArcadePlayer>();
    private Dictionary<string, SmoothValue> _scorePositions = new();
    private Dictionary<string, SmoothValue> _scores = new();
    private FixedTimer _titleTimer;
    private FixedTimer _backgroundTimer;
    private FixedTimer _sendMessageTimer = new(5000);
    private FixedTimer _leaderBlink = new(650);
    private int _messageIndex = 0;
    private Color _titleColor;
    private IRenderTargetStrategy _renderer;
    private UIMessageEmitter _uiMessage;

    protected override void OnInitialize()
    {
        base.OnInitialize();
        Camera = CameraLayer.UI;
        _titleTimer = new FixedTimer(3000);
        _backgroundTimer = new FixedTimer(5000);
        _uiMessage = Scene.GetGameObject<UIMessageEmitter>()!;
        _renderer = GlobalObjectManager.ObjectManager.Get<IRenderTargetStrategy>()!;
        _arcadeDataService = GlobalObjectManager.ObjectManager.Get<ArcadeDataService>()!;
        
        _subscription = _arcadeDataService.DataChanged.Subscribe(_ =>
        {
            _players = _arcadeDataService.Players().OrderByDescending(x => x.Points).ToList();
            var position = 0;
            foreach (var player in _players)
            {
                position++;
                if (!_scorePositions.ContainsKey(player.Id))
                    _scorePositions.Add(player.Id, new SmoothValue(10, 6f, false));
                if (!_scores.ContainsKey(player.Id))
                    _scores.Add(player.Id, new SmoothValue(0, 2f, true));

                _scorePositions[player.Id].RealValue = position;
                _scores[player.Id].RealValue = player.Points;
            }
        });
    }

    protected override void OnUpdate(float deltaTime)
    {
        _titleTimer.Update(deltaTime);
        _backgroundTimer.Update(deltaTime);
        _sendMessageTimer.Update(deltaTime);
        _leaderBlink.Update(deltaTime);
        
        foreach (var position in _scorePositions.Values) 
            position.Update(deltaTime);
        
        foreach (var score in _scores.Values) 
            score.Update(deltaTime);
        
        _titleColor = Raylib.ColorLerp(Color.Red, Color.Blue, Tween.NormalToUpDown(Tween.ApplyEasing(_titleTimer.NormalizedElapsed, EaseType.EaseInOut)));

        if (_sendMessageTimer.Output && _players.Count > 0)
        {
            _messageIndex++;
            var targetText = "";
            var color = _messageIndex == 2 ? Color.Red : new Color(Raylib.GetRandomValue(0, 255), Raylib.GetRandomValue(0, 255), 255);
            switch (_messageIndex)
            {
                case 1:
                    targetText = "GO!!! BEAT: " + _players[0].Name;
                    break;
                case 2:
                    targetText = "BIGGEST LOSER: " + _players[^1].Name + " (" + _players[^1].Points + ")";
                    break;
                case 3:
                    targetText = "TOTAL JUNKIES: " + _players.Count;
                    break;
            }
            
            _uiMessage.Show(targetText, new UIMessageStyle
            {
                AnchorFrom = Anchor.BottomLeft,
                AnchorTo = Anchor.Bottom,
                AnchorAfter = Anchor.BottomRight,
                HoldDurationInMilliseconds = 4500,
                AppearDurationInMilliseconds = 250,
                DisappearDurationInMilliseconds = 250,
                ColorFrom = Color.Black,
                ColorTo = color,
                ColorAfter = Color.Black,
                Style = TextKitStyles.HudDefault(Fonts.GetFont()) with { PixelOutline = true, OutlineColor = Color.Black, UseOutline = true}
            });
            
            if (_messageIndex >= 3)
                _messageIndex = 0;
        }
    }

    protected override void OnDraw()
    {
        Raylib.DrawRectangleGradientH(0, 0, _renderer.RenderWidth, _renderer.RenderHeight, Raylib.ColorFromHSV(_backgroundTimer.NormalizedElapsed * 360, 0.2f, 0.2f), Raylib.ColorFromHSV(_backgroundTimer.NormalizedElapsed * 360, 0.1f, 0.1f));

        Raylib.DrawTextEx(Fonts.GetFont(), "LEADERBOARD", new Vector2(16, 16), 24f, 1, _titleColor);
        
        var rank = 0;
        var counter = 0;
        var previousPoints = int.MaxValue;
        
        foreach (var player in _players)
        {
            if (!_scores.ContainsKey(player.Id) || !_scorePositions.ContainsKey(player.Id))
                return;
            
            if (player.Points != previousPoints)
                rank++;

            counter++;
            
            var color = Color.White;

            if (counter == 1)
            {
                color = Raylib.ColorLerp(Color.Yellow, Color.Black, Tween.NormalToUpDown(_leaderBlink.NormalizedElapsed));
            }
            
            var points = (int)_scores[player.Id].DisplayValue;
            var stringLength = (points == 0 ? 1 : (int)Math.Floor(Math.Log10(Math.Abs(points))) + 1) * 17;
            var startPos = new Vector2(16, 48 + 22 * _scorePositions[player.Id].DisplayValue);
            var endPos = new Vector2(_renderer.RenderWidth - 16 - stringLength, startPos.Y);
            if (counter > 1)
                Raylib.DrawRectangle(0, 44 + counter * 22, _renderer.RenderWidth, 2, new Color(0,0,0,0.2f));
            
            Raylib.DrawTextEx(Fonts.GetFont(), rank.ToString() + "#", startPos, 12f, 1, counter == 1 ? color : Color.Blue);
            Raylib.DrawTextEx(Fonts.GetFont(), player.Name, startPos + new Vector2(48, 0), 16f, 1, color);
            Raylib.DrawTextEx(Fonts.GetFont(), points.ToString(), endPos, 16f, 1, color);
            previousPoints = player.Points;
            
            if (counter > 9)
                break;
        }
    }

    protected override void OnDispose()
    {
        if (_isDisposed)
            return;

        _subscription.Dispose();

        _isDisposed = true;
    }
}