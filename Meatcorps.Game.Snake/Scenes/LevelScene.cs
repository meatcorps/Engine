using System.Numerics;
using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.AsciiScript.Commands;
using Meatcorps.Engine.AsciiScript.Interfaces;
using Meatcorps.Engine.AsciiScript.Services;
using Meatcorps.Engine.Boids.Data;
using Meatcorps.Engine.Boids.Enums;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.GridSystem;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Utilities;
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
using Meatcorps.Game.Snake.Data;
using Meatcorps.Game.Snake.GameObjects.Consumables;
using Meatcorps.Game.Snake.GameObjects.Flies;
using Meatcorps.Game.Snake.GameObjects.UI;
using Meatcorps.Game.Snake.Resources;
using Raylib_cs;

namespace Meatcorps.Game.Snake.Scenes;

public class LevelScene : BaseScene
{
    private readonly string _levelPath;
    private int TotalPlayers => DemoMode ? 2 : _sessionService.CurrentSession.TotalPlayers;
    public bool DemoMode { get; }
    private LevelData _level { get; set; } = new();
    private readonly List<Player> _players = new();
    private readonly AsciiScriptParser _parser = new();
    private bool _firstLevelData = true;
    private UIMessageEmitter _uiMessage = null!;
    private FlyFlockGameObject _flies = null!;
    private Font _font;
    private MusicManager<SnakeMusic> _musicManager = null!;
    private SoundFxManager<SnakeSounds> _soundFxManager = null!; 
    private SessionService<SnakeSessionData, SnakePlayerData> _sessionService = null!;
    private int _cachedScore;
    private IPlayerCheckin _playerCheckin = null!;
    private bool _levelReplaceMode;

    public LevelScene(string levelPath = "Assets/Level1_Easy.txt", bool demoMode = false)
    {
        _levelPath = levelPath;
        UpdateTimeMultiplier = 1f;
        DemoMode = demoMode;
    }

    protected override void OnInitialize()
    {
        _font = GlobalObjectManager.ObjectManager.Get<TextManager<DefaultFont>>()!.GetFont();
        _uiMessage = new UIMessageEmitter(TextKitStyles.HudDefault(_font));
        _musicManager = GlobalObjectManager.ObjectManager.Get<MusicManager<SnakeMusic>>()!;
        _soundFxManager = GlobalObjectManager.ObjectManager.Get<SoundFxManager<SnakeSounds>>()!;
        _sessionService = GlobalObjectManager.ObjectManager.Get<SessionService<SnakeSessionData, SnakePlayerData>>()!;
        _playerCheckin = GlobalObjectManager.ObjectManager.Get<IPlayerCheckin>()!;
        if (!DemoMode)
            _musicManager.Play(SnakeMusic.LevelAmbient);
        AddGameObject(_uiMessage);
        _firstLevelData = true;
        _parser
            .Register(IAsciiScriptCommand () => new DelayCommand())
            .Register(IAsciiScriptCommand () => new ExternalIntVariableCondition("WAITFORPOINTS", bool (minimalAmount, firstRun) =>
                {
                    if (firstRun)
                        _cachedScore = CurrentScorePlayers();
                    return CurrentScorePlayers() >= minimalAmount + _cachedScore;
                }
                ))
            .Register(IAsciiScriptCommand () => new IntVariableCommand("RANDOMMEAT1", void (total) =>
            {
                for (var i = 0; i < total; i++)
                    AddGameObject(new GameObjects.Consumable(new Meat1()));
            }))
            .Register(IAsciiScriptCommand () => new IntVariableCommand("RANDOMMEAT2", void (total) =>
            {
                for (var i = 0; i < total; i++)
                    AddGameObject(new GameObjects.Consumable(new Meat2()));
            }))
            .Register(IAsciiScriptCommand () => new IntVariableCommand("SPAWNFLIES", void (total) =>
            {
                _flies.SpawnRandom(total,
                    new RectF(0, 0, _level.LevelWidth * _level.GridSize, _level.LevelHeight * _level.GridSize));
            }))
            .Register(IAsciiScriptCommand () => new SimpleCommand("REMOVEFLIES", void () =>
            {
                _flies.KillAll();
            }))
            .Register(IAsciiScriptCommand () => new SimpleCommand("REPLACEWALLS", void () =>
            {
                _levelReplaceMode = true;
            }))
            .Register(IAsciiScriptCommand () => new SimpleCommand("ADDWALLS", void () =>
            {
                _levelReplaceMode = false;
            }))
            .Register(IAsciiScriptCommand () => new BlockGridCommand("LEVELDATA", LoadLevel));

        if (!DemoMode)
        {
            _parser.Register(IAsciiScriptCommand () => new StringVariableCommand("PLAYSOUND", void (sound) =>
                {
                    if (Enum.TryParse<SnakeSounds>(sound, out var result))
                        _soundFxManager.Play(result);
                    else
                        Console.WriteLine("Invalid sound " + sound);
                }))
                .Register(IAsciiScriptCommand () => new StringVariableCommand("PLAYSONG", void (sound) =>
                {

                    if (Enum.TryParse<SnakeMusic>(sound, out var result))
                        _musicManager.Play(result);
                    else
                        Console.WriteLine("Invalid music " + sound);
                }))
                .Register(IAsciiScriptCommand () => new StringVariableCommand("PAUSESONG", void (_) =>
                {
                    _musicManager.Pause();
                }))
                .Register(IAsciiScriptCommand () => new StringVariableCommand("RESUMESONG", void (_) =>
                {
                    _musicManager.Resume();
                }))
                .Register(IAsciiScriptCommand () => new SimpleCommand("ENDLEVEL", void () =>
                {
                    GameHost.SwitchScene(new LevelScene());
                }))
                .Register(IAsciiScriptCommand () => new StringVariableCommand("NEXTLEVEL", void (level) =>
                {
                    foreach (var player in _players)
                    {
                        if (player.IsDead)
                            return;
                    }
                    GameHost.SwitchScene(new LevelScene("Assets/" + level));
                }))
                .Register(IAsciiScriptCommand () => new DelayCommand("DELAYCOUNTDOWN", void (on, firstTick) =>
                {
                    if (firstTick)
                    {
                        _uiMessage.ClearAll();
                        _uiMessage.Countdown((int)on.TimeRemaining, UIMessagePresets.Countdown(_font));
                    }
                }))
                .Register(IAsciiScriptCommand () => new StringVariableCommand("MESSAGE", void (message) =>
                {
                    _uiMessage.Show(message, UIMessagePresets.Default(_font));
                }))
                .Register(IAsciiScriptCommand () => new StringVariableCommand("LEVELNAME", void (message) =>
                {
                    _uiMessage.Show(message, UIMessagePresets.Default(_font));
                }));
        }
        else
        {
            _parser.Register(IAsciiScriptCommand () => new SimpleCommand("ENDLEVEL", void () =>
                {
                    Died(null);
                }))
                .Register(IAsciiScriptCommand () => new SimpleCommand("ENDGAME", EndGame))
                .Register(IAsciiScriptCommand () => new StringVariableCommand("NEXTLEVEL", void (_) =>
                {
                    Died(null);
                }));
        }
            
        _parser.Load();

        SceneObjectManager.Register(_level);
        var cameraManager = new CameraControllerGameObject(GlobalObjectManager.ObjectManager.Get<ICamera>()!);
        var center = new Vector2(320 - ((640f - _level.LevelWidth * _level.GridSize) / 2),
            180 - ((360f - _level.LevelHeight * _level.GridSize) / 2));

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
        SetupFlies();
        
        _parser.Parse(_levelPath);
    }

    private void SetupFlies()
    {
        _flies = new FlyFlockGameObject(
            new BoidConfig
            {
                MaxSpeed = 180f,           // more darting speed
                MaxForce = 350f,           // very high turn rate

                NeighborRadius = 30f,      // they only care about *very* close neighbors
                DesiredSeparation = 14f,   // stay close but avoid collision

                WeightSeek = 2.5f,         // heavily prefer target seeking
                WeightSeparation = 1.2f,   // keep some personal space
                WeightAlignment = 0.2f,    // almost no “formation” flying
                WeightCohesion = 0.15f,    // no real group gathering
                WeightWander = 1f,       // small randomness for buzzing motion

                WanderJitter = 6.0f,       // faster jitter changes for erratic flight
                RetargetIntervalSeconds = 0.2f, // retarget more often (react to new meat quickly)

                BoundsPolicy = BoidBoundsPolicy.Clamp
            },
            new SpatialEntityGrid(32),
            new RectF(0, 0, _level.LevelWidth * _level.GridSize, _level.LevelHeight * _level.GridSize),
            new OldestMeatSelector(_level));
        
        AddGameObject(_flies);
    }

    private void LoadLevel(List<List<char>> levelData)
    {
        var analyzer = new GridAnalyzerYX<char>(levelData);
        var playerPositions = new List<PointInt>();

        foreach (var gridItem in analyzer.IterateAll())
        {
            var position = gridItem.Item1;
            var character = gridItem.Item2;
            
            if (_levelReplaceMode && character != '#' && _level.WallGrid.IsOccupied(position))
                _level.WallGrid.Get(position)!.Remove();
            
            switch (character)
            {
                case '#':
                    if (!_level.WallGrid.IsOccupied(position))
                        AddGameObject(new GameObjects.Wall(5000, position, _firstLevelData));
                    break;
                case 'E':
                    AddGameObject(new GameObjects.Consumable(GetRandomMeat.Get(), position));
                    break;
                case 'e':
                    AddGameObject(new GameObjects.Consumable(GetRandomMeat.Get(), position, false));
                    break;
                case 'S':
                    AddGameObject(new GameObjects.Consumable(new Score2X(), position, false));
                    break;
                case '3':
                    AddGameObject(new GameObjects.Consumable(new Score3X(), position, false));
                    break;
                case '4':
                    AddGameObject(new GameObjects.Consumable(new Score4X(), position, false));
                    break;
                case 'X':
                    AddGameObject(new GameObjects.Consumable(new ThroughWalls(), position, false));
                    break;
                case 'R':
                    AddGameObject(new GameObjects.Consumable(new RotProof(), position, false));
                    break;
                case '<':
                    AddGameObject(new GameObjects.Consumable(new SnakeSlower(), position, false));
                    break;
                case '>':
                    AddGameObject(new GameObjects.Consumable(new SnakeFaster(), position, false));
                    break;
                case '[':
                    AddGameObject(new GameObjects.Consumable(new WorldSlower(), position, false));
                    break;
                case ']':
                    AddGameObject(new GameObjects.Consumable(new WorldFaster(), position, false));
                    break;
            }
            
        }

        if (_firstLevelData)
        {
            _players.Clear();

            if (analyzer.Search('!', out var player1Position))
            {
                playerPositions.Clear();
                playerPositions.Add(player1Position);
                foreach (var position in analyzer.NeighborEqualSearch('1', player1Position))
                    playerPositions.Add(position);

                SpawnPlayer(1, playerPositions.ToArray());
            }

            if (TotalPlayers != 1 && analyzer.Search('@', out var player2Position))
            {
                playerPositions.Clear();
                playerPositions.Add(player2Position);
                foreach (var position in analyzer.NeighborEqualSearch('2', player2Position))
                    playerPositions.Add(position);

                SpawnPlayer(2, playerPositions.ToArray());
            }

            foreach (var player in _players)
            {
                AddGameObject(player.Snake);
                AddGameObject(player.PlayerUI);
            }
        }

        _firstLevelData = false;
    }

    private void SpawnPlayer(int playerId, PointInt[] positions)
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
        
        var player = new Player
        {
            SessionDataBag = playerData,
            MoveTimer = new FixedTimer(250),
            Color = playerId == 1 ? Color.Magenta : Color.Blue,
            PlayerId = playerId
        };
        var snake = new GameObjects.Snake(player, positions, positions[0] - positions[1]);
        var ui = new PlayerUI(player);
        ui.Enabled = !DemoMode;
        player.PlayerUI = ui;
        player.Snake = snake;
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
        _level.SnakeGrid.Clear();
    }

    protected override void OnUpdate(float deltaTime)
    {
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

    public void Died(GameObjects.Snake? snake)
    {
        if (snake == null)
            foreach (var player in _players)
                if (player.IsDead)
                    return;
        
        if (DemoMode)
        {
            GlobalObjectManager.ObjectManager.Get<BaseScene>()!.RemoveScene(this);
            GlobalObjectManager.ObjectManager.Get<BaseScene>()!.AddScene(new LevelScene(_levelPath, true));
            return;
        }
        
        if (snake is null)
            return;
        
        snake.Player.AddValue(SnakePlayerData.Lives, -1);
        snake.Player.AddValue(SnakePlayerData.Died);

        if (snake.Player.SessionDataBag.Get<int>(SnakePlayerData.Lives) <= 0)
            EndGame();
        else
        {
            foreach (var player in _players)
                player.Score = player.StartScore;
            
            GameHost.SwitchScene(new LevelScene(_levelPath));
        }
    }

    private void EndGame()
    {
        GameHost.SwitchScene(new EndScene());
    }

    protected override void OnDispose()
    {
    }
}