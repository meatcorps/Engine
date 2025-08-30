using System.Numerics;
using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.AsciiScript.Commands;
using Meatcorps.Engine.AsciiScript.Services;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.GridSystem;
using Meatcorps.Engine.Core.ObjectManager;
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
using Meatcorps.Game.ArcadeTemplate.Data;
using Meatcorps.Game.ArcadeTemplate.GameEnums;
using Meatcorps.Game.ArcadeTemplate.GameObjects.Abstractions;
using Meatcorps.Game.ArcadeTemplate.Resources;
using Raylib_cs;

namespace Meatcorps.Game.ArcadeTemplate.Scenes;

public class LevelScene : BaseScene
{
    private readonly string _levelPath;
    public int TotalPlayers => DemoMode ? 2 : _sessionService.CurrentSession.TotalPlayers;
    public bool DemoMode { get; }
    private LevelData _level { get; set; } = new();
    private List<Player> _players = new();
    private AsciiScriptParser _parser = new();
    private bool _firstLevelData = true;
    private UIMessageEmitter _uiMessage;
    private Font _font;
    private MusicManager<GameMusic> _musicManager;
    private SoundFxManager<GameSounds> _soundFxManager; 
    private SessionService<GameSessionData, GamePlayerData> _sessionService;
    private int _cachedScore;
    private IPlayerCheckin _playerCheckin;

    public LevelScene(string levelPath = "Assets/Level1.txt", bool demoMode = false)
    {
        _levelPath = levelPath;
        UpdateTimeMultiplier = 1f;
        DemoMode = demoMode;
    }

    protected override void OnInitialize()
    {
        _font = GlobalObjectManager.ObjectManager.Get<TextManager<DefaultFont>>()!.GetFont();
        _uiMessage = new UIMessageEmitter(TextKitStyles.HudDefault(_font));
        _musicManager = GlobalObjectManager.ObjectManager.Get<MusicManager<GameMusic>>()!;
        _soundFxManager = GlobalObjectManager.ObjectManager.Get<SoundFxManager<GameSounds>>()!;
        _sessionService = GlobalObjectManager.ObjectManager.Get<SessionService<GameSessionData, GamePlayerData>>()!;
        _playerCheckin = GlobalObjectManager.ObjectManager.Get<IPlayerCheckin>()!;
        if (!DemoMode)
            _musicManager.Play(GameMusic.LevelAmbient);
        AddGameObject(_uiMessage);
        _firstLevelData = true;
        _parser
            .Register(() => new DelayCommand())
            .Register(() => new ExternalIntVariableCondition("WAITFORPOINTS",
                (minimalAmount, firstRun) =>
                {
                    if (firstRun)
                        _cachedScore = CurrentScorePlayers();
                    return CurrentScorePlayers() >= minimalAmount + _cachedScore;
                }
                ))
            .Register(() => new BlockGridCommand("LEVELDATA", LoadLevel));

        if (!DemoMode)
        {
            _parser.Register(() => new StringVariableCommand("PLAYSOUND", sound =>
                {
                    if (Enum.TryParse<GameSounds>(sound, out var result))
                        _soundFxManager.Play(result);
                    else
                        Console.WriteLine("Invalid sound " + sound);
                }))
                .Register(() => new StringVariableCommand("PLAYSONG", sound =>
                {

                    if (Enum.TryParse<GameMusic>(sound, out var result))
                        _musicManager.Play(result);
                    else
                        Console.WriteLine("Invalid music " + sound);
                }))
                .Register(() => new StringVariableCommand("PAUSESONG", sound =>
                {
                    _musicManager.Pause();
                }))
                .Register(() => new StringVariableCommand("RESUMESONG", sound =>
                {
                    _musicManager.Resume();
                }))
                .Register(() => new SimpleCommand("ENDLEVEL", () =>
                {
                    GameHost.SwitchScene(new LevelScene());
                }))
                .Register(() => new StringVariableCommand("NEXTLEVEL", level =>
                {
                    GameHost.SwitchScene(new LevelScene("Assets/" + level));
                }))
                .Register(() => new DelayCommand("DELAYCOUNTDOWN", (on, firstTick) =>
                {
                    if (firstTick)
                    {
                        _uiMessage.ClearAll();
                        _uiMessage.Countdown((int)on.TimeRemaining, UIMessagePresets.Countdown(_font));
                    }
                }))
                .Register(() => new StringVariableCommand("MESSAGE", message =>
                {
                    _uiMessage.Show(message, UIMessagePresets.Default(_font));
                }))
                .Register(() => new StringVariableCommand("LEVELNAME", message =>
                {
                    _uiMessage.Show(message, UIMessagePresets.Default(_font));
                }));
        }
        else
        {
            _parser.Register(() => new SimpleCommand("ENDLEVEL", () =>
                {
                    Died(null);
                }))
                .Register(() => new StringVariableCommand("NEXTLEVEL", level =>
                {
                    Died(null);
                }));
        }
            
        _parser.Load();

        SceneObjectManager.Register(_level);
        var cameraManager = new CameraControllerGameObject(GlobalObjectManager.ObjectManager.Get<ICamera>()!);
        var center = new Vector2(320 - ((640 - _level.LevelWidth * _level.GridSize) / 2),
            180 - ((360 - _level.LevelHeight * _level.GridSize) / 2));

        if (!DemoMode)
        {
            center.Y -= 16;
        }
        else
        {
            cameraManager.SetZoom(1.1f);
        }
        cameraManager.SetPosition(center);
        AddGameObject(cameraManager);
        AddGameObject(new PersistentCanvas(640, 380));
        AddGameObject(new GameObjects.Background());
        
        _parser.Parse(_levelPath);
    }

    private void LoadLevel(List<List<char>> levelData)
    {
        var analyzer = new GridAnalyzerYX<char>(levelData);

        foreach (var gridItem in analyzer.IterateAll())
        {
            var position = gridItem.Item1;
            var character = gridItem.Item2;
            switch (character)
            {
                case '#':
                    // Todo Add GameObject here
                    // AddGameObject(new GameObjects.YourGameObject(position));
                    break;
            }
            
        }

        if (_firstLevelData)
        {
            _players.Clear();

            if (analyzer.Search('!', out var player1Position))
            {
                SpawnPlayer(1, player1Position);
            }

            if (TotalPlayers != 1 && analyzer.Search('@', out var player2Position))
            {
                SpawnPlayer(2, player2Position);
            }

            foreach (var player in _players)
            {
                //TODO: Your game object 
                //AddGameObject(player.YourPlayerObject);
            }
        }

        _firstLevelData = false;
    }

    private void SpawnPlayer(int playerId, PointInt positions)
    {
        if (!_playerCheckin.IsPlayerCheckedIn(playerId, out var _))
            return;
        
        if (!_sessionService.CurrentSession.TryGetPlayerData(playerId, out var _, out var playerData))
        {
            if (DemoMode)
                playerData = _sessionService.CreateTemporaryPlayer(playerId, "Player " + playerId);
            else
                throw new Exception("Player data not found");
        }
        
        //TODO: Place player spawn logic here
        
        var player = new Player
        {
            SessionDataBag = playerData,
            PlayerId = playerId,
            Color = playerId == 1 ? Color.Magenta : Color.Blue,
        };
        _players.Add(player);
    }

    private int CurrentScorePlayers()
    {
        var score = 0;
        foreach (var player in _players)
            score = Math.Max(player.Score, score);
        return score;
    }

    protected override void OnPreUpdate(float deltaTime)
    {
    }

    protected override void OnUpdate(float deltaTime)
    {
        // TODO: REMOVE THIS
        if (Raylib.IsKeyPressed(KeyboardKey.Enter) && !DemoMode)
            EndGame();
        // TODO: UNTIL HERE
        _parser.Update(deltaTime);
    }

    protected override void OnLateUpdate(float deltaTime)
    {
        if (DemoMode)
            return;
        
        var min = 1f;

        foreach (var player in _players)
        {
            if (player.WorldSpeed < min)
            {
                min = player.WorldSpeed;
            }
        }

        UpdateTimeMultiplier = min;
    }

    public void Died(BasePlayer? playerObject)
    {
        if (playerObject == null)
            foreach (var player in _players)
                if (player.IsDead)
                    return;
        
        if (DemoMode)
        {
            GlobalObjectManager.ObjectManager.Get<BaseScene>()!.RemoveScene(this);
            GlobalObjectManager.ObjectManager.Get<BaseScene>()!.AddScene(new LevelScene(_levelPath, true));
            return;
        }
        
        if (playerObject is null)
            return;
        
        playerObject.Player.AddValue(GamePlayerData.Lives, -1);
        playerObject.Player.AddValue(GamePlayerData.Died);

        if (playerObject.Player.SessionDataBag.Get<int>(GamePlayerData.Lives) <= 0)
            EndGame();
        else
        {
            foreach (var player in _players)
                player.Score = player.StartScore;
            
            GameHost.SwitchScene(new LevelScene(_levelPath));
        }
    }

    public void EndGame()
    {
        GameHost.SwitchScene(new EndScene());
    }

    protected override void OnDispose()
    {
    }
}