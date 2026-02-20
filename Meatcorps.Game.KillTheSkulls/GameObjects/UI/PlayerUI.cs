using System.Numerics;
using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Text;
using Meatcorps.Game.KillTheSkulls.Data;
using Meatcorps.Game.KillTheSkulls.GameEnums;
using Meatcorps.Game.KillTheSkulls.GameObjects.Abstractions;
using Raylib_cs;

namespace Meatcorps.Game.KillTheSkulls.GameObjects.UI;

public class PlayerUI : ResourceGameObject
{
    private readonly Player _player;
    private readonly SmoothValue _smoothValue;
    private readonly FixedTimer _audioValueChangeTimer = new FixedTimer(50);
    private IArcadePointsMutator _pointMutator = null!;
    private IPlayerCheckin _playerCheckin = null!;
    private TextStyle _textStyle;
    private float _lastValue;

    public PlayerUI(Player player)
    {
        _player = player;
        Camera = CameraLayer.UI;
        _smoothValue = new(_player.Score);
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        _pointMutator = GlobalObjectManager.ObjectManager.Get<IArcadePointsMutator>()!;
        _playerCheckin = GlobalObjectManager.ObjectManager.Get<IPlayerCheckin>()!;
        _textStyle = TextStyle.Create(Fonts.GetFont(), 32, 0, 32, Color.Magenta);
        _textStyle.OutlineColor = Color.Black;
        _textStyle.UseOutline = true;
        _textStyle.PixelOutline = true;
    }

    protected override void OnUpdate(float deltaTime)
    {
        _smoothValue.RealValue = _player.Score;
        _audioValueChangeTimer.Update(deltaTime);
        if ((int)_lastValue != (int)_smoothValue.DisplayValue)
        {
            _lastValue = _smoothValue.DisplayValue;
            Sounds.Play(GameSounds.Shortblip, 0.1f);
        }

        _smoothValue.Update(deltaTime);
    }

    protected override void OnDraw()
    {
        TextKit.Draw(ref _textStyle, ((int)_smoothValue.DisplayValue).ToString("000000"), new Vector2(32, 64));

        var text = _playerCheckin.GetPlayerName(_player.PlayerId) + " (" + _pointMutator.GetPoints(_player.PlayerId) +
                   ")";
        Raylib.DrawTextEx(Fonts.GetFont(),"SCORE", new Vector2(32, 52), 8, 0,
            Color.White);
        Raylib.DrawTextEx(Fonts.GetFont(), text, new Vector2(32, 64 + 32), 8, 0,
            Color.White);
        
        Raylib.DrawTextEx(Fonts.GetFont(),"LIVES", new Vector2(RenderTarget!.RenderWidth - 72, 52), 8, 0,
            Color.White);
        TextKit.Draw(ref _textStyle, (_player.Lives).ToString("00"), new Vector2(RenderTarget!.RenderWidth - 96, 64));
        
        Raylib.DrawTextEx(Fonts.GetFont(),"STREAK", new Vector2(RenderTarget!.RenderWidth - 180, 52), 8, 0,
            Color.White);
        TextKit.Draw(ref _textStyle, (_player.Streak).ToString("00"), new Vector2(RenderTarget!.RenderWidth - 196, 64));
    }

    protected override void OnDispose()
    {
    }
}