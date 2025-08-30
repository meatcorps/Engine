using System.Numerics;
using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Camera;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.Text;
using Meatcorps.Game.Snake.Data;
using Meatcorps.Game.Snake.GameObjects.Abstractions;
using Meatcorps.Game.Snake.GameObjects.Consumables;
using Meatcorps.Game.Snake.Resources;
using Raylib_cs;

namespace Meatcorps.Game.Snake.GameObjects.UI;

public class PlayerUI : SnakeGameObject
{
    private readonly Player _player;
    private readonly SmoothValue _smoothValue;
    private FixedTimer _audioValueChangeTimer = new FixedTimer(50);
    private PointInt _targetScreenSize;
    private Vector2 _iconPosition = Vector2.Zero;
    private Vector2 _scorePosition = Vector2.Zero;
    private Vector2 _perkPosition = Vector2.Zero;
    private Vector2 _perkDirection = Vector2.Zero;
    private TextStyle _textStyle;
    private IArcadePointsMutator _pointMutator;
    private IPlayerCheckin _playerCheckin;

    public PlayerUI(Player player)
    {
        _player = player;
        Camera = CameraLayer.UI;
        _smoothValue = new(_player.Score, 1f);
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        var camera = GlobalObjectManager.ObjectManager.Get<IRenderTargetStrategy>()!;
        _pointMutator = GlobalObjectManager.ObjectManager.Get<IArcadePointsMutator>()!;
        _playerCheckin = GlobalObjectManager.ObjectManager.Get<IPlayerCheckin>()!;
        _targetScreenSize = new PointInt(camera.RenderWidth, camera.RenderHeight);
        _textStyle = TextStyle.Create(Fonts.GetFont(), 16, 0, 16, _player.Color);
        _textStyle.OutlineColor = Color.Black;
        _textStyle.UseOutline = true;
        _textStyle.PixelOutline = true;

        if (_player.PlayerId == 1)
        {
            _iconPosition = new Vector2(16, 15);
            _scorePosition = new Vector2(36, 16);
            _perkPosition = new Vector2(16, 32);
            _perkDirection = new Vector2(14, 0);
        }
        else
        {
            _iconPosition = new Vector2(_targetScreenSize.X - 32, 15);
            _scorePosition = new Vector2(_targetScreenSize.X - 32 - 2 - 96, 16);
            _perkPosition = new Vector2(_targetScreenSize.X - 32, 32);
            _perkDirection = new Vector2(-14, 0);
        }
    }

    protected override void OnUpdate(float deltaTime)
    {
        _smoothValue.RealValue = _player.Score;
        _audioValueChangeTimer.Update(deltaTime);
        if (_audioValueChangeTimer.Output && !_smoothValue.IsAtRealValue)
            Sounds.Play(SnakeSounds.Scorechange);
        
        _smoothValue.Update(deltaTime);
    }

    protected override void OnDraw()
    {
        Sprites.Draw(SnakeSprites.SnakeHead1, _iconPosition, _player.Color, 0f);
        
        TextKit.Draw(ref _textStyle, ((int)_smoothValue.DisplayValue).ToString("000000"), _scorePosition);

        var counter = 0;
        foreach (var perk in _player.Snake.Perks)
        {
            var lifeTimeNormal = 1f;
            var timeRemaining = 10000f;
            if (perk is PowerUp powerUp)
            {
                lifeTimeNormal = powerUp.NormalizedElapsed;
                timeRemaining = powerUp.TimeRemaining;
            }

            var color = Raylib.ColorAlpha(Color.White, 0.5f);
            if (timeRemaining > 9000)
            {
                color = Raylib.ColorLerp(Color.White, color, lifeTimeNormal);
            }
            
            Sprites.Draw(perk.Sprite, _perkPosition + _perkDirection * counter, color, 0f, Vector2.Zero, 1f);
            if (timeRemaining < 9000)
            {
                Raylib.DrawTextEx(Fonts.GetFont(), (timeRemaining / 1000).ToString("F0"), _perkPosition + _perkDirection * counter + new Vector2(4, 8), 8, 0, Color.White);
            }
            counter++;
        }

        
        var text = _playerCheckin.GetPlayerName(_player.PlayerId) + " (" + _pointMutator.GetPoints(_player.PlayerId) + ")";
        var size = Raylib.MeasureTextEx(Fonts.GetFont(), text, 8, 0);
        if (_player.PlayerId == 1)
        {
            var additionalOffset = counter > 0 ? new Vector2(4, 4) : new Vector2(0, 4);
            Raylib.DrawTextEx(Fonts.GetFont(), text, _perkPosition + _perkDirection * counter + additionalOffset, 8, 0,
                Color.White);
        }
        else
        {
            var additionalOffset = counter > 0 ? new Vector2(0, 4) : new Vector2(4, 4);
            Raylib.DrawTextEx(Fonts.GetFont(), text,
                (_perkPosition + _perkDirection * (counter - 1)) - new Vector2(size.X, 0) + additionalOffset, 8, 0,
                Color.White);
        }

    }

    protected override void OnDispose()
    {
    }
}