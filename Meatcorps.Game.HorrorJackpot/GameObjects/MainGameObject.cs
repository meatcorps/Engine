using System.Numerics;
using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Arcade.Enums;
using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.Arcade.RayLib.Utilities;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Storage.Services;
using Meatcorps.Engine.Core.Tween;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.Hardware.ArduinoController.ArduinoController;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Game.HorrorJackpot.Data;
using Meatcorps.Game.HorrorJackpot.GameEnums;
using Meatcorps.Game.HorrorJackpot.GameObjects.Abstractions;
using Meatcorps.Game.HorrorJackpot.Services;
using Meatcorps.Game.HorrorJackpot.Shaders;
using Raylib_cs;

namespace Meatcorps.Game.HorrorJackpot.GameObjects;

public class MainGameObject: ResourceGameObject
{
    private DrumRenderer _drumRenderer = null!;
    private GameInternalState _internalState = GameInternalState.WaitingForPlayers;
    private PlayerInputRouter<GameInput> _controller;
    private FixedTimer _applyForceTimer = new(2000);
    private TimerOn _showScoreTimer = new(7500);
    private TimerOn _waitForPlayerTimer = new(30000);
    private TimerOn _ignoreInputTimer = new(500);
    private DrumTypes _currentDrumType = DrumTypes.Reroll;
    private EdgeDetector _buttonPressed = new();
    private GameInternalState _previousInternalState = GameInternalState.WaitingForPlayers;
    private float _maxForce = 2;
    private IArcadePointsMutator _arcadePointMutator;
    private ArcadeGame _gameInfo;
    private IPlayerCheckin _playerCheckin;
    private PersistentDatabase _storage;
    private SmoothValue _jackpotSmoothValue;
    private TimerOn _buttonWaitingTimer = new(10000);
    private EdgeDetector _buttonWaitingTrigger = new();
    private OverlayGameObject? _overlay;
    private OverlayGameObject? _nextOverlay;
    private DrumTypes _previousDrumType = DrumTypes.Nothing;
    private int _internalJackpot;
    private ArcadeServer _arcadeServer;
    private Texture2D _qrCodeTexture;
    private TimerOn _idleTimer = new(1000);
    private FixedTimer _animationTimer = new(100);
    private FixedTimer _buttonAnimationTimer = new(250);
    private FixedTimer _blinkTimer = new(32);
    private string _text = "Hello World!";
    
    private GameSprites _skullTop = GameSprites.SkullTop1;
    private GameSprites _skullBottom = GameSprites.SkullBottom1;

    private RandomEnum<GameSounds> _idleRandom = new RandomEnum<GameSounds>()
        .Add(GameSounds.Idle1, 25)
        .Add(GameSounds.Idle2, 25)
        .Add(GameSounds.Idle3, 25)
        .Add(GameSounds.Idle4, 25)
        .Add(GameSounds.Idle5, 25)
        .Add(GameSounds.Idle6, 25);

    private RandomEnum<GameSounds> _jackpotRandom = new RandomEnum<GameSounds>()
        .Add(GameSounds.Jackpot1, 25)
        .Add(GameSounds.Jackpot2, 25)
        .Add(GameSounds.Jackpot3, 25)
        .Add(GameSounds.Jackpot4, 25);
    
    private RandomEnum<GameSounds> _nothingRandom = new RandomEnum<GameSounds>()
        .Add(GameSounds.Nothing1, 25)
        .Add(GameSounds.Nothing2, 25)
        .Add(GameSounds.Nothing3, 25)
        .Add(GameSounds.Nothing4, 25)
        .Add(GameSounds.Nothing5, 25)
        .Add(GameSounds.Nothing6, 25);
    
    private RandomEnum<GameSounds> _rollRandom = new RandomEnum<GameSounds>()
        .Add(GameSounds.Roll1, 25)
        .Add(GameSounds.Roll2, 25)
        .Add(GameSounds.Roll3, 25)
        .Add(GameSounds.Roll4, 25);

    private RandomEnum<GameSounds> _scanRandom = new RandomEnum<GameSounds>()
        .Add(GameSounds.Scan1, 25)
        .Add(GameSounds.Scan2, 25);
    
    private RandomEnum<GameSounds> _buttonRandom = new RandomEnum<GameSounds>()
        .Add(GameSounds.PresstheButton, 25)
        .Add(GameSounds.PresstheButton2, 25)
        .Add(GameSounds.ChopChop, 25);
    
    private RandomEnum<GameSounds> _score10Random = new RandomEnum<GameSounds>()
        .Add(GameSounds.Score10won1, 25)
        .Add(GameSounds.Score10won2, 25)
        .Add(GameSounds.Score10won3, 25)
        .Add(GameSounds.Score10won4, 25);
    
    private RandomEnum<GameSounds> _score100Random = new RandomEnum<GameSounds>()
        .Add(GameSounds.Score100won1, 25)
        .Add(GameSounds.Score100won2, 25)
        .Add(GameSounds.Score100won3, 25)
        .Add(GameSounds.Score100won4, 25);
    
    private int _jackpot
    {
        get
        {
            return _internalJackpot;
        }
        set
        {
            if (_internalJackpot == value)
                return;
            
            if (value < _gameInfo.PricePoints)
                value = _gameInfo.PricePoints;
            
            _storage["JackpotAmount"] = value.ToString();
            _storage.Dirty = true;
            _internalJackpot = value;
        }
    }
    
    protected override void OnInitialize()
    {
        Camera = CameraLayer.UI;
        base.OnInitialize();
        _drumRenderer = new DrumRenderer();
        _controller = GlobalObjectManager.ObjectManager.Get<PlayerInputRouter<GameInput>>()!;
        
        _arcadePointMutator = GlobalObjectManager.ObjectManager.Get<IArcadePointsMutator>()!;
        _gameInfo = GlobalObjectManager.ObjectManager.Get<ArcadeGame>()!;
        _playerCheckin = GlobalObjectManager.ObjectManager.Get<IPlayerCheckin>()!;
        _storage = GlobalObjectManager.ObjectManager.Get<PersistentDatabase>()!;
        _arcadeServer = GlobalObjectManager.ObjectManager.Get<ArcadeServer>()!;
        _qrCodeTexture = QrcodeHelper.CreateTexture(_arcadeServer.AutoSignIn(_gameInfo), 4, 2);
        SetupOverlays();
        
        if (_storage.TryGetValue("JackpotAmount", out var jackpotFromStorage))
        {
            if (int.TryParse((string)jackpotFromStorage, out var jackpot))
                _internalJackpot = jackpot;
            else
                _jackpot = 1000;
        }
        
        _jackpotSmoothValue  = new(0, 200f, false);
    }

    protected override void OnUpdate(float deltaTime)
    {
        var done = true;
        if (_overlay is not null)
            done = _overlay.Done;
        
        if (done)
            _overlay = null;
        
    
        if (_nextOverlay is not null && done)
        {
            _overlay = _nextOverlay;
            Scene.AddGameObject(_overlay);
            _nextOverlay = null;
        }
        
        _idleTimer.Update(_internalState == GameInternalState.WaitingForPlayers, deltaTime);

        if (_idleTimer.Output)
        {
            _idleTimer = new TimerOn(Raylib.GetRandomValue(10000, 60000));
            ShowSkullOverlay();
        }

        _buttonAnimationTimer.Update(deltaTime);
        _drumRenderer.Update(deltaTime);
        _applyForceTimer.Update(deltaTime);
        _showScoreTimer.Update(_internalState == GameInternalState.ShowingScore, deltaTime);
        _waitForPlayerTimer.Update(_internalState == GameInternalState.PlayerJoin, deltaTime);
        _currentDrumType = DrumHelper.GetDrumTypeFromNormal(_drumRenderer.Rotation);

        if (_currentDrumType != _previousDrumType)
        {
            Sounds.Play(GameSounds.Shortblip, 0.2f, 0.3f + _drumRenderer.Speed / 2.5f);
        }
        
        _previousDrumType = _currentDrumType;
        var buttonPressed = _controller.GetState(1, GameInput.Action).IsPressed;
        _jackpotSmoothValue.RealValue = _jackpot;
        _jackpotSmoothValue.Update(deltaTime);
        
        _buttonWaitingTimer.Update(_internalState == GameInternalState.PlayerIsApplyingForce, deltaTime);
        _buttonWaitingTrigger.Update(_buttonWaitingTimer.Output);
        
        _blinkTimer.Update(deltaTime);
        
        _jackpotSmoothValue.SetSpeed(Math.Max(1, Math.Abs(_jackpotSmoothValue.DisplayValue - _jackpotSmoothValue.RealValue)));
        
        
        switch (_internalState)
        {
            case GameInternalState.WaitingForPlayers:
                _text = "Next victim! " + _gameInfo.PricePoints + " Life points!\n";;
                SkullAnimation(deltaTime);
                _gameInfo.State = GameState.Idle;
                _drumRenderer.BlinkSpeed = 2;
                _drumRenderer.GlowColor = Color.Red;
                
                if (buttonPressed)
                {
                    _internalState = GameInternalState.PlayerJoin;
                    ShowOverlay(_playerJoinOverlay);
                    Sounds.Play(_scanRandom.Get());
                }

                break;
            case GameInternalState.PlayerJoin:
                _gameInfo.State = GameState.Waiting;
                if (_playerCheckin.IsPlayerCheckedIn(1, out var player))
                {
                    if (_arcadePointMutator.RequestPoints(1, _gameInfo.PricePoints))
                    {
                        _internalState = GameInternalState.PlayerIsApplyingForce;
                        _jackpot += _gameInfo.PricePoints;
                    }
                    else
                    {
                        _internalState = GameInternalState.WaitingForPlayers;
                        ShowScoreOverlay(GameSprites.ResultNothing, "NOT ENOUGH\nPOINTS!");
                        Sounds.Play(GameSounds.NotEnough);
                    }

                    HideOverlay();
                }

                if (_waitForPlayerTimer.Output)
                {
                    _internalState = GameInternalState.WaitingForPlayers;
                    Sounds.Play(GameSounds.Haha);
                    HideOverlay();
                }

                break;
            case GameInternalState.PlayerIsApplyingForce:
                if (_buttonWaitingTrigger.IsRisingEdge)
                {
                    Sounds.Play(_buttonRandom.Get());
                    _buttonWaitingTimer.Reset();
                }
                if (!_playerCheckin.IsPlayerCheckedIn(1, out var _))
                    _internalState = GameInternalState.WaitingForPlayers;

                _gameInfo.State = GameState.Active;
                if (buttonPressed && _ignoreInputTimer.Output)
                {
                    _drumRenderer.Speed = CalculateForce();
                    _internalState = GameInternalState.DrumIsRotating;
                }
                
                if (_applyForceTimer.NormalizedElapsed < 0.01f)
                    Sounds.Play(GameSounds.Shortblip, 0.2f, 1);
                if (_applyForceTimer.NormalizedElapsed > 0.5f && _applyForceTimer.NormalizedElapsed < 0.51f)
                    Sounds.Play(GameSounds.Shortblip, 0.2f, 1);
                
                _drumRenderer.BlinkSpeed = 0;
                _drumRenderer.Glow = 1;
                _drumRenderer.GlowColor = Raylib.ColorLerp(Color.Black, Color.Red, CalculateForce() / _maxForce);
                break;
            case GameInternalState.DrumIsRotating:
                if (_currentDrumType == DrumTypes.Jackpot)
                    _drumRenderer.BlinkSpeed = 0.25f;
                else
                {
                    _drumRenderer.BlinkSpeed = 0;
                    _drumRenderer.Glow = 0.25f;
                }
                if (!_playerCheckin.IsPlayerCheckedIn(1, out var _))
                    _internalState = GameInternalState.WaitingForPlayers;
                
                _drumRenderer.GlowColor = GetColor(_currentDrumType);
                
                if (_drumRenderer.Speed.EqualsSafe(0)) 
                    _internalState = GameInternalState.ApplyScore;
                break;
            case GameInternalState.ApplyScore:
                if (!_playerCheckin.IsPlayerCheckedIn(1, out var _))
                    _internalState = GameInternalState.WaitingForPlayers;
                switch (_currentDrumType)
                {
                    case DrumTypes.Score10:
                        ShowScoreOverlay(GameSprites.ResultPoint10, "YOU GAINED\n10 POINTS!");
                        _arcadePointMutator.SubmitPoints(1,_gameInfo.PricePoints + 10);
                        _jackpot -= 10;
                        Sounds.Play(GameSounds.Powerupcollect, 0.1f, 0.8f);
                        Sounds.Play(_score10Random.Get());
                        break;
                    case DrumTypes.Score100:
                        ShowScoreOverlay(GameSprites.ResultPoint100, "YOU GAINED\n100 POINTS!");
                        _arcadePointMutator.SubmitPoints(1,_gameInfo.PricePoints + 100);
                        Sounds.Play(GameSounds.Powerupcollect, 0.1f, 1f);
                        _jackpot -= 100;
                        Sounds.Play(_score100Random.Get());
                        break;
                    case DrumTypes.Jackpot:
                        ShowScoreOverlay(GameSprites.ResultJackpot, "YOU GAINED\n" + (_jackpot - _gameInfo.PricePoints) + " POINTS!");
                        _arcadePointMutator.SubmitPoints(1,_jackpot);
                        _jackpot = _gameInfo.PricePoints * 2;
                        Sounds.Play(GameSounds.PowerUpScore, 0.1f, 1f);
                        Sounds.Play(_jackpotRandom.Get());
                        break;
                    case DrumTypes.Reroll:
                        ShowScoreOverlay(GameSprites.ResultReroll, "");
                        Sounds.Play(GameSounds.Placed, 0.1f, 1f);
                        Sounds.Play(_rollRandom.Get());
                        break;
                    case DrumTypes.Nothing:
                        ShowScoreOverlay(GameSprites.ResultNothing, "YOU LOST\n" + _gameInfo.PricePoints + " POINTS!");
                        Sounds.Play(GameSounds.Backgroundplaced, 0.1f, 0.2f);
                        Sounds.Play(_nothingRandom.Get());
                        break;
                }
                _internalState = GameInternalState.ShowingScore;
                break;
            case GameInternalState.ShowingScore:
                _drumRenderer.BlinkSpeed = 0.25f;
                _drumRenderer.GlowColor = GetColor(_currentDrumType);

                if (_showScoreTimer.Output || buttonPressed)
                {
                    HideOverlay();
                    _internalState = _currentDrumType == DrumTypes.Reroll
                        ? GameInternalState.PlayerIsApplyingForce
                        : GameInternalState.WaitingForPlayers;
                    
                    if (_internalState == GameInternalState.WaitingForPlayers)
                        Sounds.Play(GameSounds.Haha);
                }

                break;
        }
        _ignoreInputTimer.Update(_previousInternalState == _internalState, deltaTime);
        

        if (_previousInternalState != _internalState)
        {
            if (_internalState is GameInternalState.WaitingForPlayers or GameInternalState.ShowingScore)
                _controller.GetState(1, GameInput.Action).Animation = new BlinkAnimation(1000);
            else if (_internalState == GameInternalState.PlayerIsApplyingForce)
            {
                _controller.GetState(1, GameInput.Action).Animation = new BlinkAnimation(100);
                Sounds.Play(GameSounds.Bang);
            }
            else 
                _controller.GetState(1, GameInput.Action).Animation = null;
        }
        
        _previousInternalState = _internalState;
        
        if (_internalState is not GameInternalState.WaitingForPlayers and not GameInternalState.PlayerJoin)
        {
            _text = _playerCheckin.GetPlayerName(1) + " (" + _arcadePointMutator.GetPoints(1) + ")\n";
        }
    }

    private void SkullAnimation(float deltaTime)
    {
        
        _animationTimer.Update(deltaTime);

        if (_animationTimer.Output)
        {
            _skullTop = GameSprites.SkullTop1;
            
            var topRandom = Raylib.GetRandomValue(0, 100);
            
            if (topRandom > 80 && topRandom < 90)
                _skullTop = GameSprites.SkullTop2;
            if (topRandom >= 90)
                _skullTop = GameSprites.SkullTop3;

            if (_skullBottom == GameSprites.SkullBottom1)
            {
                _skullBottom = GameSprites.SkullBottom2;
                return;
            }

            if (_skullBottom == GameSprites.SkullBottom2)
            {
                _skullBottom = GameSprites.SkullBottom3;
                return;
            }
            
            if (_skullBottom == GameSprites.SkullBottom3)
            {
                _skullBottom = GameSprites.SkullBottom4;
                return;
            }
            
            if (_skullBottom == GameSprites.SkullBottom4)
            {
                _skullBottom = GameSprites.SkullBottom1;
                return;
            }
        }
    }

    private Color GetColor(DrumTypes drumType)
    {
        switch (drumType)
        {
            case DrumTypes.Reroll:
                return Color.SkyBlue;   
            case DrumTypes.Nothing:
                return Color.Red;
            case DrumTypes.Score10:
                return Color.Pink;
            case DrumTypes.Score100:
                return Color.Lime;
            case DrumTypes.Jackpot:
                return Color.Yellow;
        }
        
        return Color.White;
    }
    
    protected override void OnDraw()
    {
        _drumRenderer.Draw();
        
        var flickerRandom = Raylib.GetRandomValue(0, 100) / 500f; 
        var flickerRandom2 = Raylib.GetRandomValue(0, 100) / 500f; 
        
        Sprites.Draw(GameSprites.UIPanel, new Vector2(0, 0), Color.White, 0f);
        var textSize = Raylib.MeasureTextEx(Fonts.GetFont(), _text, 10, 1);
        Raylib.DrawTextEx(Fonts.GetFont(), _text, new Vector2((360 - textSize.X) / 2, 620), 10, 1, Raylib.ColorAlpha(Color.Red, 0.8f + flickerRandom));
        
        Raylib.DrawTextEx(Fonts.GetFont(GameFonts.Digit), "88888888", new Vector2(31, 3), 64, 1, Color.Black);
        Raylib.DrawTextEx(Fonts.GetFont(GameFonts.Digit), "88888888", new Vector2(33, 1), 64, 1, Color.Black);

        Raylib.DrawTextEx(Fonts.GetFont(GameFonts.Digit), "88888888", new Vector2(32, 2), 64, 1, new Color(32, 0, 0));
        Raylib.DrawTextEx(Fonts.GetFont(GameFonts.Digit), _jackpotSmoothValue.DisplayValue.ToString("F0"), new Vector2(32, 2), 64, 1, Raylib.ColorAlpha(Color.Red, 0.8f + flickerRandom2));

        if (_internalState == GameInternalState.WaitingForPlayers)
        {
            Sprites.DrawAnimationWithNormal(GameSprites.ArcadeButtonAnimation, _buttonAnimationTimer.NormalizedElapsed, new Vector2(116, 400), Color.Red, 0, Vector2.Zero, 4);
            Raylib.DrawTextEx(Fonts.GetFont(), "PRESS TO\nACTIVATE", new Vector2(48, 525), 32, 1, Raylib.ColorAlpha(Color.Red, 0.8f + flickerRandom2));

        }
        
        if (_internalState == GameInternalState.PlayerIsApplyingForce)
        {
            var normal = Tween.ApplyEasing(Tween.PingPong(_applyForceTimer.NormalizedElapsed * 2, 1f),
                EaseType.EaseInOut);
            var posX = (int)(normal * 302) - 20;
            Sprites.Draw(GameSprites.SpeedSliderBg, new Vector2(0, 288), Color.White);
            
            if (normal.Between(0.4f, 0.6f))
                Sprites.Draw(GameSprites.SpeedSliderPointer, new Vector2(posX, 222 + Raylib.GetRandomValue(-5, 5)), Raylib.ColorLerp(Color.White, Color.Red, normal));
            else
                Sprites.Draw(GameSprites.SpeedSliderPointer, new Vector2(posX, 222), Raylib.ColorLerp(Color.White, Color.Red, normal));
        }
        
        //Raylib.DrawTextEx(Fonts.GetFont(), DrumHelper.GetDrumTypeFromNormal(_drumRenderer.Rotation).ToString(), new Vector2(64, 300), 16, 1, Color.White);
        //Raylib.DrawLine(0, 320, 360, 320, Color.White);

        if (_internalState == GameInternalState.PlayerJoin)
        {
            
        }

        base.OnDraw();
    }

    private float CalculateForce()
    {
        var force = Tween.ApplyEasing(Tween.PingPong(_applyForceTimer.NormalizedElapsed * 4), EaseType.EaseInOut);
        return force * _maxForce;
    }

    protected override void OnDispose()
    {
        Raylib.UnloadTexture(_qrCodeTexture);
    }

    private enum GameInternalState
    {
        WaitingForPlayers,
        PlayerJoin,
        PlayerIsApplyingForce,
        DrumIsRotating,
        ApplyScore,
        ShowingScore,
    }

    private OverlaySettings _playerJoinOverlay = null!;

    private void ShowOverlay(OverlaySettings settings)
    {
        _overlay?.Hide();

        _nextOverlay = new OverlayGameObject(settings);
    }

    private void ShowScoreOverlay(GameSprites sprite, string text)
    {
        var textLength = Raylib.MeasureTextEx(Fonts.GetFont(), text, 16, 1);
        var setting = new OverlaySettings
        {
            OnDrawLayer = (f, color) =>
            {
                var fp = Tween.ApplyEasing(f, EaseType.EaseInBounce);
                var rect = Sprites.GetSprite(sprite);
                Raylib.DrawTextEx(Fonts.GetFont(), text, new Vector2((360 - textLength.X) / 2, 150), 16, 1, Raylib.ColorAlpha(Color.Red, f));
                Sprites.Draw(sprite, (rect.Size / 2) * (1 - fp) + new Vector2(0, 160f),
                    Raylib.ColorAlpha(Color.White, f), 0, Vector2.Zero, fp);
                Raylib.DrawTextEx(Fonts.GetFont(), "WAIT FOR " + (_showScoreTimer.TimeRemaining / 1000).ToString("F1") + "S", new Vector2(48, 480), 16, 1, Raylib.ColorAlpha(Color.Red, f));
            },
            AppearTimeMs = 1000,
            DisappearTimeMs = 1000,
            DurationMs = 7500 - 1000
        };
        ShowOverlay(setting);
    }
    
    private void ShowSkullOverlay()
    {
        var trigger = new EdgeDetector();
        var sound = _idleRandom.Get();
        var setting = new OverlaySettings
        {
            OnDrawLayer = (f, color) =>
            {
                trigger.Update(f.EqualsSafe(1));
                var skullTop = GameSprites.SkullTop1;
                var skullBottom = GameSprites.SkullBottom1;


                if (trigger.IsRisingEdge)
                    Sounds.Play(sound);
                        
                if (f.EqualsSafe(1))
                {
                    skullTop = _skullTop;
                    skullBottom = _skullBottom;
                    
                    if (!Sounds.IsPlaying(sound))
                        HideOverlay();
                }

                Sprites.Draw(skullTop, new Vector2(0, 160),
                    Raylib.ColorAlpha(Color.White, f));
                Sprites.Draw(skullBottom, new Vector2(0, 410),
                    Raylib.ColorAlpha(Color.White, f));
            },
            AppearTimeMs = 1000,
            DisappearTimeMs = 1000,
            DurationMs = 7500 - 1000
        };
        ShowOverlay(setting);
    }

    private void HideOverlay()
    {
        if (_overlay != null)
            _overlay.Hide();
    }
    
    private void SetupOverlays()
    {
        _playerJoinOverlay = new OverlaySettings
        {
            OnDrawLayer = (f, color) =>
            {
                Raylib.DrawTextEx(Fonts.GetFont(), "SCAN QR CODE", new Vector2(78, 200), 16, 1, Raylib.ColorAlpha(Color.Red, f));
                Raylib.DrawTexture(_qrCodeTexture, (360 - _qrCodeTexture.Width) / 2, (640 - _qrCodeTexture.Height) / 2,
                    Raylib.ColorAlpha(Color.White, f));
                Raylib.DrawTextEx(Fonts.GetFont(), "TIMEOUT IN " + (_waitForPlayerTimer.TimeRemaining / 1000).ToString("F1") + "S", new Vector2(48, 430), 16, 1, Raylib.ColorAlpha(Color.Red, f));
            },
            AppearTimeMs = 1000,
            DisappearTimeMs = 1000,
            DurationMs = 28000
        };
    }
}