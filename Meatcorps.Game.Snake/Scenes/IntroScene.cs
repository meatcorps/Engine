using Meatcorps.Engine.Arcade.RayLib.GameObjects;
using Meatcorps.Engine.Arcade.Services;
using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.Hardware.ArduinoController.ArduinoController;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Audio;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.GameObjects.UI;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Engine.RayLib.Text;
using Meatcorps.Engine.RayLib.UI.Data;
using Meatcorps.Engine.Session;
using Meatcorps.Engine.Session.Data;
using Meatcorps.Game.Snake.Data;
using Meatcorps.Game.Snake.GameObjects.UI;
using Meatcorps.Game.Snake.Resources;
using Raylib_cs;

namespace Meatcorps.Game.Snake.Scenes;

public class IntroScene: BaseScene
{
    private Font _font;
    private UIMessageEmitter _uiMessage;
    private PlayerInputRouter<SnakeInput> _controller;
    private TimerOn _startTimer = new(2000);
    private FixedTimer _sliderTimer = new(10000);
    private int currentSlide = 0;
    private MusicManager<SnakeMusic> _musicManager;
    private SoundFxManager<SnakeSounds> _soundManager;
    private TextManager<DefaultFont> _fontManager;
    private SessionService<SnakeSessionData, SnakePlayerData> _sessionService;
    public int TotalPlayersReady { get; set; } = 0;
    private bool _waitingForPlayers = false;

    protected override void OnInitialize()
    {
        _font = GlobalObjectManager.ObjectManager.Get<TextManager<DefaultFont>>()!.GetFont();
        _uiMessage = new UIMessageEmitter(TextKitStyles.HudDefault(_font));
        
        _controller = GlobalObjectManager.ObjectManager.Get<PlayerInputRouter<SnakeInput>>()!;
        _sessionService = GlobalObjectManager.ObjectManager.Get<SessionService<SnakeSessionData, SnakePlayerData>>()!;
        AddScene(new LevelScene("Assets/Level1_Easy.txt", true));
        AddGameObject(_uiMessage);
        AddGameObject(new IntroUI());
        var guidePage = new GuidePage();
        guidePage.Enabled = false;
        AddGameObject(guidePage);

        var leaderboard = new Leaderboard();
        leaderboard.Enabled = false;
        AddGameObject(leaderboard);

        _controller.GetState(1, SnakeInput.Action).Animation = new BlinkAnimation(250);
        _controller.GetState(2, SnakeInput.Action).Animation = new BlinkAnimation(250);
        _musicManager = GlobalObjectManager.ObjectManager.Get<MusicManager<SnakeMusic>>()!;
        _soundManager = GlobalObjectManager.ObjectManager.Get<SoundFxManager<SnakeSounds>>()!;
        _fontManager = GlobalObjectManager.ObjectManager.Get<TextManager<DefaultFont>>()!;
        _musicManager.Play(SnakeMusic.IntroOutro);
        _sessionService.StartSession();
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (TotalPlayersReady == 0)
            _sliderTimer.Update(deltaTime);
        
        if (TotalPlayersReady > 0)
        {
            _startTimer.Update(true, deltaTime);
            if (_startTimer.Output && !_waitingForPlayers)
            {
                _waitingForPlayers = true;
                _controller.GetState(1, SnakeInput.Action).Animation = null;
                _controller.GetState(2, SnakeInput.Action).Animation = null;
                AddGameObject(new WaitForPlayersGameObject(TotalPlayersReady, new LevelScene(), new IntroScene()));
                _soundManager.Play(SnakeSounds.Warning);
            }

            return;
        }

        if (_controller.GetState(1, SnakeInput.Action).IsPressed)
        {
            TotalPlayersReady = 1;
        }

        if (_controller.GetState(2, SnakeInput.Action).IsPressed)
        {
            TotalPlayersReady = 2;
        }

        if (TotalPlayersReady > 0)
        {
            if (TotalPlayersReady == 2)
                _controller.GetState(2, SnakeInput.Action).Animation = new BlinkAnimation(50);
            else
                _controller.GetState(2, SnakeInput.Action).Animation = null;
            
            _controller.GetState(1, SnakeInput.Action).Animation = new BlinkAnimation(50);

            
            _musicManager.Stop();
            _soundManager.Play(SnakeSounds.PowerUpScore);
            _uiMessage.ClearAll();
            GetScene<LevelScene>()!.Enabled = false;
            ShowSlide(0);
        }

        if (_sliderTimer.Output)
        {
            ShowSlide(currentSlide);
            currentSlide++;
            if (currentSlide > 2)
                currentSlide = 0;
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
        _controller.GetState(1, SnakeInput.Action).Animation = null;
        _controller.GetState(2, SnakeInput.Action).Animation = null;
    }
}