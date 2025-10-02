using System.Numerics;
using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.Arcade.RayLib.GameObjects;
using Meatcorps.Engine.AsciiScript.Commands;
using Meatcorps.Engine.AsciiScript.Services;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.GridSystem;
using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.Hardware.ArduinoController.ArduinoController;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Audio;
using Meatcorps.Engine.RayLib.Camera;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.GameObjects;
using Meatcorps.Engine.RayLib.GameObjects.UI;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Engine.RayLib.Text;
using Meatcorps.Engine.RayLib.UI.Data;
using Meatcorps.Engine.Session;
using Meatcorps.Game.HorrorJackpot.Data;
using Meatcorps.Game.HorrorJackpot.GameEnums;
using Meatcorps.Game.HorrorJackpot.GameObjects;
using Meatcorps.Game.HorrorJackpot.GameObjects.Abstractions;
using Meatcorps.Game.HorrorJackpot.Resources;
using Raylib_cs;

namespace Meatcorps.Game.HorrorJackpot.Scenes;

public class LevelScene : BaseScene
{
    private Font _font;
    private UIMessageEmitter _uiMessage;
    private PlayerInputRouter<GameInput> _controller;
    private TimerOn _startTimer = new(2000);
    private FixedTimer _sliderTimer = new(10000);
    private int currentSlide = 0;
    private MusicManager<GameMusic> _musicManager;
    private SoundFxManager<GameSounds> _soundManager;
    private TextManager<DefaultFont> _fontManager;
    private SessionService<GameSessionData, GamePlayerData> _sessionService;
    public int TotalPlayersReady { get; set; } = 0;
    private bool _waitingForPlayers = false;

    protected override void OnInitialize()
    {
        _font = GlobalObjectManager.ObjectManager.Get<TextManager<DefaultFont>>()!.GetFont();
        _uiMessage = new UIMessageEmitter(TextKitStyles.HudDefault(_font));

        _controller = GlobalObjectManager.ObjectManager.Get<PlayerInputRouter<GameInput>>()!;
        _sessionService = GlobalObjectManager.ObjectManager.Get<SessionService<GameSessionData, GamePlayerData>>()!;
        AddGameObject(_uiMessage);
        AddGameObject(new MainGameObject());

        _controller.GetState(1, GameInput.Action).Animation = new BlinkAnimation(250);
        
        _musicManager = GlobalObjectManager.ObjectManager.Get<MusicManager<GameMusic>>()!;
        _soundManager = GlobalObjectManager.ObjectManager.Get<SoundFxManager<GameSounds>>()!;
        _fontManager = GlobalObjectManager.ObjectManager.Get<TextManager<DefaultFont>>()!;
        _musicManager.Play(GameMusic.IntroOutro);
        _sessionService.StartSession();
    }

    protected override void OnUpdate(float deltaTime)
    {
        
    }

    protected override void OnDispose()
    {
        _controller.GetState(1, GameInput.Action).Animation = null;
        _controller.GetState(2, GameInput.Action).Animation = null;
    }
}