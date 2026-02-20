using Meatcorps.Engine.Arcade.RayLib.GameObjects;
using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.Hardware.ArduinoController.ArduinoController;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Audio;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.GameObjects.UI;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Engine.RayLib.Text;
using Meatcorps.Engine.RayLib.UI.GuiComponent.Core;
using Meatcorps.Engine.RayLib.UI.GuiComponent.GuiSettings;
using Meatcorps.Engine.Session;
using Meatcorps.Game.CyberMaze.Data;
using Meatcorps.Game.CyberMaze.GameEnums;
using Meatcorps.Game.CyberMaze.GameObjects.UI;
using Raylib_cs;

namespace Meatcorps.Game.CyberMaze.Scenes;

public class IntroScene : BaseScene
{
    private Font _font;
    private UIMessageEmitter _uiMessage = null!;
    private PlayerInputRouter<GameInput> _controller = null!;
    private TimerOn _startTimer = new(2000);
    private readonly FixedTimer _sliderTimer = new(10000);
    private int currentSlide;
    private MusicManager<GameMusic> _musicManager = null!;
    private SoundFxManager<GameSounds> _soundManager = null!;
    private TextManager<DefaultFont> _fontManager = null!;
    private SessionService<GameSessionData, GamePlayerData> _sessionService = null!;
    public int TotalPlayersReady { get; set; }
    private bool _waitingForPlayers;
    private GameConfig<GameSettings> _config = null!;
    public bool StandAloneMode { get; private set; }
    private DefaultGuiSettings<GameInput, GameSounds>? _guiSettings;

    protected override void OnInitialize()
    {
        
        SceneObjectManager.Register(new GuiService());
        StandAloneMode = GlobalObjectManager.ObjectManager.Get<IGuiSettings>() != null;
        AddGameObject(new VersionUI());
        _config = GlobalObjectManager.ObjectManager.Get<GameConfig<GameSettings>>()!;
        _font = GlobalObjectManager.ObjectManager.Get<TextManager<DefaultFont>>()!.GetFont();
        _uiMessage = new UIMessageEmitter(TextKitStyles.HudDefault(_font));

        _controller = GlobalObjectManager.ObjectManager.Get<PlayerInputRouter<GameInput>>()!;
        _sessionService = GlobalObjectManager.ObjectManager.Get<SessionService<GameSessionData, GamePlayerData>>()!;
        AddScene(new LevelScene("Assets/Level1.txt", true));
        AddGameObject(_uiMessage);
        AddGameObject(new IntroUI());
        var guidePage = new GuidePage();
        guidePage.Enabled = false;
        AddGameObject(guidePage);

        var leaderboard = new Leaderboard();
        leaderboard.Enabled = false;
        AddGameObject(leaderboard);

        _controller.GetState(1, GameInput.Action).Animation = new BlinkAnimation(250);
        _controller.GetState(2, GameInput.Action).Animation = new BlinkAnimation(250);
        _musicManager = GlobalObjectManager.ObjectManager.Get<MusicManager<GameMusic>>()!;
        _soundManager = GlobalObjectManager.ObjectManager.Get<SoundFxManager<GameSounds>>()!;
        _fontManager = GlobalObjectManager.ObjectManager.Get<TextManager<DefaultFont>>()!;
        _musicManager.Play(GameMusic.IntroOutro);
        _sessionService.StartSession();

        if (StandAloneMode)
        {
            GlobalObjectManager.ObjectManager.Get<IGuiSettings>()!.Font = _fontManager.GetFont();
            
            var guiService = new GuiService();
            SceneObjectManager.Register(guiService);
            SceneObjectManager.Add<IBackgroundService>(guiService);
            
            if (GlobalObjectManager.ObjectManager.Get<IGuiSettings>()! is DefaultGuiSettings<GameInput, GameSounds>
                guiSettings)
            {
                _guiSettings = guiSettings;
                _guiSettings.Load();
                var menu = AddGameObject(new MainMenuGameObject<GameInput>(_guiSettings));
                menu.Layer = 10;
                menu.AddMenuAction("Start Game", () => TotalPlayersReady = 1);
            }
        }
    }

    protected override void OnUpdate(float deltaTime)
    {

        _guiSettings?.Update(deltaTime);

        if (GetGameObject<MainMenuGameObject<GameInput>>() != null)
            GetGameObject<MainMenuGameObject<GameInput>>()!.Enabled = TotalPlayersReady == 0;
        
        if (_sliderTimer.Output)
        {
            ShowSlide(currentSlide);
            currentSlide++;
            if (currentSlide > 2)
                currentSlide = 0;
        }
        
#if DEBUG
        if (_config.GetOrDefault("Debug", "AutoStartPlayer1", false))
        {
            TotalPlayersReady = 1;
            _startTimer = new TimerOn(2);
        }
        if (_config.GetOrDefault("Debug", "AutoStartPlayer2", false))
        {
            TotalPlayersReady = 2;
            _startTimer = new TimerOn(2);
        }
#endif
        
        if (TotalPlayersReady == 0)
            _sliderTimer.Update(deltaTime);

        if (TotalPlayersReady > 0)
        {
            _startTimer.Update(true, deltaTime);
            if (_startTimer.Output && !_waitingForPlayers)
            {
                _waitingForPlayers = true;
                _controller.GetState(1, GameInput.Action).Animation = null;
                _controller.GetState(2, GameInput.Action).Animation = null;
                AddGameObject(new WaitForPlayersGameObject(TotalPlayersReady, new LevelScene(), new IntroScene()));
                _soundManager.Play(GameSounds.Warning);
            }

            return;
        }
        
        if (_controller.GetState(1, GameInput.Action).IsPressed && !StandAloneMode)
        {
            TotalPlayersReady = 1;
        }

        if (_controller.GetState(2, GameInput.Action).IsPressed && !StandAloneMode)
        {
            TotalPlayersReady = 2;
        }

        if (TotalPlayersReady > 0)
        {
            if (TotalPlayersReady == 2)
                _controller.GetState(2, GameInput.Action).Animation = new BlinkAnimation(50);
            else
                _controller.GetState(2, GameInput.Action).Animation = null;

            _controller.GetState(1, GameInput.Action).Animation = new BlinkAnimation(50);


            _musicManager.Stop();
            _soundManager.Play(GameSounds.PowerUpScore);
            _uiMessage.ClearAll();
            GetScene<LevelScene>()!.Enabled = false;
            ShowSlide(0);
        }
    }

    private void ShowSlide(int slide)
    {
        foreach (var slideObject in GetGameObjects<BaseGameObject>())
        {
            if (slideObject is IIntroSlide)
                slideObject.Enabled = false;
        }

        switch (slide)
        {
            case 0:
                GetGameObject<IntroUI>()!.Enabled = true;
                break;
            case 1:
                GetGameObject<Leaderboard>()!.Enabled = true;
                break;
            case 2:
                GetGameObject<GuidePage>()!.Enabled = true;
                break;
        }
    }

    protected override void OnDispose()
    {
        _controller.GetState(1, GameInput.Action).Animation = null;
        _controller.GetState(2, GameInput.Action).Animation = null;
    }
}