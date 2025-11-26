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
using Meatcorps.Game.HorrorJackpot.GameObjects.Components;
using Meatcorps.Game.HorrorJackpot.Services;
using Meatcorps.Game.HorrorJackpot.Shaders;
using Raylib_cs;

namespace Meatcorps.Game.HorrorJackpot.GameObjects;

public class MainGameObject: ResourceGameObject
{
    public DrumRenderer DrumRenderer = null!;
    public GameInternalState InternalState { get; set; } = GameInternalState.WaitingForPlayers;
    private PlayerInputRouter<GameInput> _controller;
    public FixedTimer ApplyForceTimer = new(2000);
    public TimerOn ShowScoreTimer = new(7500);
    public TimerOn WaitForPlayerTimer = new(30000);
    public TimerOn IgnoreInputTimer = new(500);
    public DrumTypes CurrentDrumType = DrumTypes.Reroll;
    private EdgeDetector _buttonPressed = new();
    private GameInternalState _previousInternalState = GameInternalState.WaitingForPlayers;
    public float MaxForce = 2;
    public IArcadePointsMutator ArcadePointMutator { get; private set; }
    public ArcadeGame GameInfo { get; private set; }
    public IPlayerCheckin PlayerCheckin { get; private set; }
    private PersistentDatabase _storage;
    private SmoothValue _jackpotSmoothValue;
    public TimerOn ButtonWaitingTimer = new(10000);
    public EdgeDetector ButtonWaitingTrigger = new();
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
    public string Text = "Hello World!";
    
    private GameSprites _skullTop = GameSprites.SkullTop1;
    private GameSprites _skullBottom = GameSprites.SkullBottom1;

    public bool ButtonPressed { get; private set; }

    private RandomEnum<GameSounds> _idleRandom = new RandomEnum<GameSounds>()
        .Add(GameSounds.Idle1, 25)
        .Add(GameSounds.Idle2, 25)
        .Add(GameSounds.Idle3, 25)
        .Add(GameSounds.Idle4, 25)
        .Add(GameSounds.Idle5, 25)
        .Add(GameSounds.Idle6, 25);

    public RandomEnum<GameSounds> JackpotRandom = new RandomEnum<GameSounds>()
        .Add(GameSounds.Jackpot1, 25)
        .Add(GameSounds.Jackpot2, 25)
        .Add(GameSounds.Jackpot3, 25)
        .Add(GameSounds.Jackpot4, 25);
    
    public RandomEnum<GameSounds> NothingRandom = new RandomEnum<GameSounds>()
        .Add(GameSounds.Nothing1, 25)
        .Add(GameSounds.Nothing2, 25)
        .Add(GameSounds.Nothing3, 25)
        .Add(GameSounds.Nothing4, 25)
        .Add(GameSounds.Nothing5, 25)
        .Add(GameSounds.Nothing6, 25);
    
    public RandomEnum<GameSounds> RollRandom = new RandomEnum<GameSounds>()
        .Add(GameSounds.Roll1, 25)
        .Add(GameSounds.Roll2, 25)
        .Add(GameSounds.Roll3, 25)
        .Add(GameSounds.Roll4, 25);

    public RandomEnum<GameSounds> ScanRandom = new RandomEnum<GameSounds>()
        .Add(GameSounds.Scan1, 25)
        .Add(GameSounds.Scan2, 25);
    
    public RandomEnum<GameSounds> ButtonRandom = new RandomEnum<GameSounds>()
        .Add(GameSounds.PresstheButton, 25)
        .Add(GameSounds.PresstheButton2, 25)
        .Add(GameSounds.ChopChop, 25);
    
    public RandomEnum<GameSounds> Score10Random = new RandomEnum<GameSounds>()
        .Add(GameSounds.Score10won1, 25)
        .Add(GameSounds.Score10won2, 25)
        .Add(GameSounds.Score10won3, 25)
        .Add(GameSounds.Score10won4, 25);
    
    public RandomEnum<GameSounds> Score100Random = new RandomEnum<GameSounds>()
        .Add(GameSounds.Score100won1, 25)
        .Add(GameSounds.Score100won2, 25)
        .Add(GameSounds.Score100won3, 25)
        .Add(GameSounds.Score100won4, 25);
    
    public int Jackpot
    {
        get
        {
            return _internalJackpot;
        }
        set
        {
            if (_internalJackpot == value)
                return;
            
            if (value < GameInfo.PricePoints)
                value = GameInfo.PricePoints;
            
            _storage["JackpotAmount"] = value.ToString();
            _storage.Dirty = true;
            _internalJackpot = value;
        }
    }
    
    protected override void OnInitialize()
    {
        Camera = CameraLayer.UI;
        base.OnInitialize();
        DrumRenderer = new DrumRenderer();
        _controller = GlobalObjectManager.ObjectManager.Get<PlayerInputRouter<GameInput>>()!;
        
        ArcadePointMutator = GlobalObjectManager.ObjectManager.Get<IArcadePointsMutator>()!;
        GameInfo = GlobalObjectManager.ObjectManager.Get<ArcadeGame>()!;
        PlayerCheckin = GlobalObjectManager.ObjectManager.Get<IPlayerCheckin>()!;
        _storage = GlobalObjectManager.ObjectManager.Get<PersistentDatabase>()!;
        _arcadeServer = GlobalObjectManager.ObjectManager.Get<ArcadeServer>()!;
        _qrCodeTexture = QrcodeHelper.CreateTexture(_arcadeServer.AutoSignIn(GameInfo), 4, 2);
        SetupOverlays();
        
        if (_storage.TryGetValue("JackpotAmount", out var jackpotFromStorage))
        {
            if (int.TryParse((string)jackpotFromStorage, out var jackpot))
                _internalJackpot = jackpot;
            else
                Jackpot = 1000;
        }
        
        _jackpotSmoothValue  = new(0, 200f, false);
        
        AddComponent(new WaitingForPlayersState(this));
        AddComponent(new PlayerJoinState(this));
        AddComponent(new PlayerIsApplyingForceState(this));
        AddComponent(new DrumIsRotatingState(this));
        AddComponent(new ApplyScoreState(this));
        AddComponent(new ShowingScoreState(this));
    }

    protected override void OnPreUpdate(float deltaTime)
    {
        base.OnPreUpdate(deltaTime);
        
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
        
        _idleTimer.Update(InternalState == GameInternalState.WaitingForPlayers, deltaTime);

        if (_idleTimer.Output)
        {
            _idleTimer = new TimerOn(Raylib.GetRandomValue(10000, 60000));
            ShowSkullOverlay();
        }

        _buttonAnimationTimer.Update(deltaTime);
        DrumRenderer.Update(deltaTime);
        ApplyForceTimer.Update(deltaTime);
        ShowScoreTimer.Update(InternalState == GameInternalState.ShowingScore, deltaTime);
        WaitForPlayerTimer.Update(InternalState == GameInternalState.PlayerJoin, deltaTime);
        CurrentDrumType = DrumHelper.GetDrumTypeFromNormal(DrumRenderer.Rotation);

        if (CurrentDrumType != _previousDrumType)
        {
            Sounds.Play(GameSounds.Shortblip, 0.2f, 0.3f + DrumRenderer.Speed / 2.5f);
        }
        
        _previousDrumType = CurrentDrumType;
        ButtonPressed = _controller.GetState(1, GameInput.Action).IsPressed;
        _jackpotSmoothValue.RealValue = Jackpot;
        _jackpotSmoothValue.Update(deltaTime);
        
        ButtonWaitingTimer.Update(InternalState == GameInternalState.PlayerIsApplyingForce, deltaTime);
        ButtonWaitingTrigger.Update(ButtonWaitingTimer.Output);
        
        _blinkTimer.Update(deltaTime);
        
        _jackpotSmoothValue.SetSpeed(Math.Max(1, Math.Abs(_jackpotSmoothValue.DisplayValue - _jackpotSmoothValue.RealValue)));
    }

    protected override void OnUpdate(float deltaTime)
    {
    }

    protected override void OnLateUpdate(float deltaTime)
    {
        IgnoreInputTimer.Update(_previousInternalState == InternalState, deltaTime);

        if (_previousInternalState != InternalState)
        {
            if (InternalState is GameInternalState.WaitingForPlayers or GameInternalState.ShowingScore)
                _controller.GetState(1, GameInput.Action).Animation = new BlinkAnimation(1000);
            else if (InternalState == GameInternalState.PlayerIsApplyingForce)
            {
                _controller.GetState(1, GameInput.Action).Animation = new BlinkAnimation(100);
                Sounds.Play(GameSounds.Bang);
            }
            else 
                _controller.GetState(1, GameInput.Action).Animation = null;
        }
        
        _previousInternalState = InternalState;
        
        if (InternalState is not GameInternalState.WaitingForPlayers and not GameInternalState.PlayerJoin)
        {
            Text = PlayerCheckin.GetPlayerName(1) + " (" + ArcadePointMutator.GetPoints(1) + ")\n";
        }
        
        base.OnLateUpdate(deltaTime);
    }

    public void SkullAnimation(float deltaTime)
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

    public Color GetColor(DrumTypes drumType)
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
        DrumRenderer.Draw();
        
        var flickerRandom = Raylib.GetRandomValue(0, 100) / 500f; 
        var flickerRandom2 = Raylib.GetRandomValue(0, 100) / 500f; 
        
        Sprites.Draw(GameSprites.UIPanel, new Vector2(0, 0), Color.White, 0f);
        var textSize = Raylib.MeasureTextEx(Fonts.GetFont(), Text, 10, 1);
        Raylib.DrawTextEx(Fonts.GetFont(), Text, new Vector2((360 - textSize.X) / 2, 620), 10, 1, Raylib.ColorAlpha(Color.Red, 0.8f + flickerRandom));
        
        Raylib.DrawTextEx(Fonts.GetFont(GameFonts.Digit), "88888888", new Vector2(31, 3), 64, 1, Color.Black);
        Raylib.DrawTextEx(Fonts.GetFont(GameFonts.Digit), "88888888", new Vector2(33, 1), 64, 1, Color.Black);

        Raylib.DrawTextEx(Fonts.GetFont(GameFonts.Digit), "88888888", new Vector2(32, 2), 64, 1, new Color(32, 0, 0));
        Raylib.DrawTextEx(Fonts.GetFont(GameFonts.Digit), _jackpotSmoothValue.DisplayValue.ToString("F0"), new Vector2(32, 2), 64, 1, Raylib.ColorAlpha(Color.Red, 0.8f + flickerRandom2));

        if (InternalState == GameInternalState.WaitingForPlayers)
        {
            Sprites.DrawAnimationWithNormal(GameSprites.ArcadeButtonAnimation, _buttonAnimationTimer.NormalizedElapsed, new Vector2(116, 400), Color.Red, 0, Vector2.Zero, 4);
            Raylib.DrawTextEx(Fonts.GetFont(), "PRESS TO\nACTIVATE", new Vector2(48, 525), 32, 1, Raylib.ColorAlpha(Color.Red, 0.8f + flickerRandom2));

        }
        
        if (InternalState == GameInternalState.PlayerIsApplyingForce)
        {
            var normal = Tween.ApplyEasing(Tween.PingPong(ApplyForceTimer.NormalizedElapsed * 2, 1f),
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

        if (InternalState == GameInternalState.PlayerJoin)
        {
            
        }

        base.OnDraw();
    }

    public float CalculateForce()
    {
        var force = Tween.ApplyEasing(Tween.PingPong(ApplyForceTimer.NormalizedElapsed * 4), EaseType.EaseInOut);
        return force * MaxForce;
    }

    protected override void OnDispose()
    {
        Raylib.UnloadTexture(_qrCodeTexture);
    }

    public OverlaySettings PlayerJoinOverlay = null!;

    public void ShowOverlay(OverlaySettings settings)
    {
        _overlay?.Hide();

        _nextOverlay = new OverlayGameObject(settings);
    }

    public void ShowScoreOverlay(GameSprites sprite, string text)
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
                Raylib.DrawTextEx(Fonts.GetFont(), "WAIT FOR " + (ShowScoreTimer.TimeRemaining / 1000).ToString("F1") + "S", new Vector2(48, 480), 16, 1, Raylib.ColorAlpha(Color.Red, f));
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

    public void HideOverlay()
    {
        if (_overlay != null)
            _overlay.Hide();
    }
    
    private void SetupOverlays()
    {
        PlayerJoinOverlay = new OverlaySettings
        {
            OnDrawLayer = (f, color) =>
            {
                Raylib.DrawTextEx(Fonts.GetFont(), "SCAN QR CODE", new Vector2(78, 200), 16, 1, Raylib.ColorAlpha(Color.Red, f));
                Raylib.DrawTexture(_qrCodeTexture, (360 - _qrCodeTexture.Width) / 2, (640 - _qrCodeTexture.Height) / 2,
                    Raylib.ColorAlpha(Color.White, f));
                Raylib.DrawTextEx(Fonts.GetFont(), "TIMEOUT IN " + (WaitForPlayerTimer.TimeRemaining / 1000).ToString("F1") + "S", new Vector2(48, 430), 16, 1, Raylib.ColorAlpha(Color.Red, f));
            },
            AppearTimeMs = 1000,
            DisappearTimeMs = 1000,
            DurationMs = 28000
        };
    }
}

public enum GameInternalState
{
    WaitingForPlayers,
    PlayerJoin,
    PlayerIsApplyingForce,
    DrumIsRotating,
    ApplyScore,
    ShowingScore,
}