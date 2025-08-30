using System.Numerics;
using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.AsciiScript.Commands;
using Meatcorps.Engine.AsciiScript.Enums;
using Meatcorps.Engine.AsciiScript.Services;
using Meatcorps.Engine.Collision.Extensions;
using Meatcorps.Engine.Collision.Modules;
using Meatcorps.Engine.Collision.Services;
using Meatcorps.Engine.Collision.Utilities;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.GridSystem;
using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Tween;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Audio;
using Meatcorps.Engine.RayLib.Camera;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.GameObjects;
using Meatcorps.Engine.RayLib.GameObjects.UI;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.PostProcessing;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Engine.RayLib.Text;
using Meatcorps.Engine.RayLib.UI.Data;
using Meatcorps.Engine.Session;
using Meatcorps.Game.Pacman.Data;
using Meatcorps.Game.Pacman.GameEnums;
using Meatcorps.Game.Pacman.GameObjects;
using Meatcorps.Game.Pacman.GameObjects.Abstractions;
using Meatcorps.Game.Pacman.GameObjects.GhostManagers;
using Meatcorps.Game.Pacman.Resources;
using Raylib_cs;

namespace Meatcorps.Game.Pacman.Scenes;

public class LevelScene : BaseScene
{
    private readonly string _levelPath;
    public int TotalPlayers => DemoMode ? 1 : _sessionService.CurrentSession.TotalPlayers;
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
    private WorldService _worldService;
    private TimerOn _cameraMovementTimer = new(4000);
    private TimerOn _demoModeResetTimer = new(40000);
    private CameraControllerGameObject _cameraManager;
    private string _nextLevel = "";

    public LevelScene(string levelPath = "Assets/Level1.txt", bool demoMode = false)
    {
        _levelPath = levelPath;
        UpdateTimeMultiplier = 1f;
        DemoMode = demoMode;
    }

    protected override void OnInitialize()
    {
        _worldService = CollisionModule.Setup(SceneObjectManager)
            .SetGridSpatialGridSize(16)
            .Load();
        
        if (GlobalObjectManager.ObjectManager.Get<IUniversalConfig>()!.GetOrDefault("Debug", "QuickStart", false))
        {
            _cameraMovementTimer = new TimerOn(100);
        }
        _font = GlobalObjectManager.ObjectManager.Get<TextManager<DefaultFont>>()!.GetFont();
        _uiMessage = new UIMessageEmitter(TextKitStyles.HudDefault(_font));
        _musicManager = GlobalObjectManager.ObjectManager.Get<MusicManager<GameMusic>>()!;
        _soundFxManager = GlobalObjectManager.ObjectManager.Get<SoundFxManager<GameSounds>>()!;
        _sessionService = GlobalObjectManager.ObjectManager.Get<SessionService<GameSessionData, GamePlayerData>>()!;
        _playerCheckin = GlobalObjectManager.ObjectManager.Get<IPlayerCheckin>()!;
        if (!DemoMode)
        {
            _musicManager.Play(GameMusic.LevelAmbient);
            _musicManager.Pause();
        }
        else
            _uiMessage.Enabled = false;

        AddGameObject(_uiMessage);
        _firstLevelData = true;
        _parser
            .Register(() => new DelayCommand())
            .Register(() => new SimpleCommand("PAUSE", () => { _parser.State = ScriptParserState.Paused; }))
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
                .Register(() => new StringVariableCommand("PAUSESONG", sound => { _musicManager.Pause(); }))
                .Register(() => new StringVariableCommand("RESUMESONG", sound => { _musicManager.Resume(); }))
                .Register(() => new SimpleCommand("ENDLEVEL", () => { GameHost.SwitchScene(new LevelScene()); }))
                .Register(() => new StringVariableCommand("NEXTLEVEL",
                    level => { _nextLevel = "Assets/" + level; }))
                .Register(() => new IntVariableCommand("SPEED",
                    speed => { _level.Speed = speed; }))
                .Register(() => new IntVariableCommand("SCAREDTIME",
                    time =>
                    {
                        _level.ScaredTime = time;
                        SetTimersGhost();
                    }))
                .Register(() => new IntVariableCommand("STAYATHOME",
                    time =>
                    {
                        _level.StayAtHomeTime = time;
                        SetTimersGhost();
                    }))
                .Register(() => new IntVariableCommand("CHASETIME",
                    time =>
                    {
                        _level.ChaseTime = time;
                        SetTimersGhost();
                    }))
                .Register(() => new IntVariableCommand("SCATTERTIME",
                    time =>
                    {
                        _level.ScatterTime = time;
                        SetTimersGhost();
                    }))
                .Register(() => new DelayCommand("DELAYCOUNTDOWN", (on, firstTick) =>
                {
                    if (firstTick)
                    {
                        _uiMessage.ClearAll();
                        _uiMessage.Countdown((int)on.TimeRemaining, UIMessagePresets.Countdown(_font));
                    }
                }))
                .Register(() => new StringVariableCommand("MESSAGE",
                    message => { _uiMessage.Show(message, UIMessagePresets.Default(_font)); }))
                .Register(() => new StringVariableCommand("LEVELNAME",
                    message => { _uiMessage.Show(message, UIMessagePresets.Default(_font)); }));
        }
        else
        {
            _parser.Register(() => new SimpleCommand("ENDLEVEL", () => { Died(null); }));
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

        cameraManager.SetPosition(center);
        _cameraManager = cameraManager;
        AddGameObject(cameraManager);
        AddGameObject(new PersistentCanvas(640, 380));
        AddGameObject(new GameObjects.Background());
        AddGameObject(new Map());
        AddGameObject(new TargetSeekerGameObject());
        _parser.Parse(_levelPath);
    }

    private void SetTimersGhost()
    {
        foreach (var ghost in GetGameObjects<Ghost>())
            ghost.SetTimers();
    }

    private void LoadLevel(List<List<char>> levelData)
    {
        var analyzer = new GridAnalyzerYX<char>(levelData);
        var allowedDirections = new List<PointInt>();
        var allDirections = new PointInt[]
            { new PointInt(-1, 0), new PointInt(1, 0), new PointInt(0, -1), new PointInt(0, 1) };
        
        if (_firstLevelData)
            _level.Map.Clear();
        
        foreach (var gridItem in analyzer.IterateAll())
        {
            var position = gridItem.Item1;
            var character = gridItem.Item2;
            
            switch (character)
            {
                case '$':
                    AddGameObject(new PowerPellet(position));
                    break;
                case '!':
                    AddGameObject(new ExtraCollectable(position));
                    break;
            }
        }

        if (_firstLevelData)
        {
            foreach (var gridItem in analyzer.IterateAll())
            {
                allowedDirections.Clear();

                var position = gridItem.Item1;
                var character = gridItem.Item2;
                var walkable = false;
                var isGhostHome = false;
                var isOneWay = false;

                switch (character)
                {
                    case '.' or '5' or '6' or '7' or '8':
                        if (_firstLevelData)
                            AddGameObject(new Collectable(position));
                        break;
                }

                walkable = character != '#';
                // TODO: Maby in the future upgrade this to mare advance room scanning. 
                // We need to see if it's really needed
                isGhostHome = character is '2' or '3' or '4' or 'x' or 'X';
                isOneWay = character is 'X';

                foreach (var direction in allDirections)
                {
                    var exist = analyzer.Neighbor(position, direction, out var neighbor);
                    if (((!exist) || neighbor != '#') && walkable)
                        allowedDirections.Add(direction);

                    // This will make it easier to decide what kind of block we need to render. Better to reuse it :)
                    if (exist && neighbor != '#' && !walkable)
                        allowedDirections.Add(direction);
                }

                _level.Map.Register(position, new MapItem
                {
                    Position = position,
                    Directions = allowedDirections.ToArray(),
                    Walkable = walkable,
                    GhostHome = isGhostHome,
                    OneWay = isOneWay,
                });

                if (!walkable)
                    _worldService.RegisterStaticRectFBody(this, _level.ToWorldRectangle(position).ToRectF())
                        .SetLayer(LayerBits.Bit(CollisionLayer.Wall))
                        .SetMask(LayerBits.MaskOf(CollisionLayer.PacMan, CollisionLayer.Ghost));

                if (isOneWay)
                {
                    _worldService.RegisterStaticRectFBody(this, _level.ToWorldRectangle(position).ToRectF())
                        .SetLayer(LayerBits.Bit(CollisionLayer.Wall))
                        .SetMask(LayerBits.MaskOf(CollisionLayer.PacMan));
                    var rect = _level.ToWorldRectangle(position);
                    rect.Inflate(16, 16);
                    _worldService.RegisterRectFBodySensor(GetGameObject<Map>()!, rect.ToRectF())
                        .SetLayer(LayerBits.Bit(CollisionLayer.OneWay))
                        .SetMask(LayerBits.MaskOf(CollisionLayer.Ghost));
                }
            }

            _players.Clear();
            var player1StartChar = '@';
            if (_sessionService.CurrentSession.TotalPlayers == 2)
            {
                player1StartChar = 'T';
            }

            if (analyzer.Search(player1StartChar, out var player1Position))
                SpawnPlayer(1, player1Position);

            if (TotalPlayers != 1 && analyzer.Search('t', out var player2Position))
            {
                SpawnPlayer(2, player2Position);
            }

            foreach (var player in _players)
            {
                AddGameObject(player.PacMan);
                AddGameObject(player.Ui);
            }

            foreach (var gridItem in analyzer.IterateAll())
            {
                var position = gridItem.Item1;
                var character = gridItem.Item2;
                switch (character)
                {
                    case '1':
                        analyzer.Search('5', out var corner1);
                        AddGameObject(GhostFactory.Create(_level.ToWorldPosition(position), corner1, GhostType.Blinky,
                            _level,
                            _worldService));
                        break;
                    case '2':
                        analyzer.Search('6', out var corner2);
                        AddGameObject(GhostFactory.Create(_level.ToWorldPosition(position), corner2, GhostType.Pinky,
                            _level,
                            _worldService));
                        break;
                    case '3':
                        analyzer.Search('7', out var corner3);
                        AddGameObject(GhostFactory.Create(_level.ToWorldPosition(position), corner3, GhostType.Inky,
                            _level,
                            _worldService));
                        break;
                    case '4':
                        analyzer.Search('8', out var corner4);
                        AddGameObject(GhostFactory.Create(_level.ToWorldPosition(position), corner4, GhostType.Clyde,
                            _level,
                            _worldService));
                        break;
                }
            }

            GetGameObject<Map>()!.LevelLoaded();
        }

        _firstLevelData = false;
    }

    private void SpawnPlayer(int playerId, PointInt position)
    {
        if (!_playerCheckin.IsPlayerCheckedIn(playerId, out var _) && !DemoMode)
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
            PlayerId = playerId,
            Color = TotalPlayers == 1
                ? Color.Yellow
                : (
                    playerId == 1
                        ? Color.Magenta
                        : Color.Blue),
        };
        PacMan.Setup(_worldService, player, _level.ToWorldPosition(position));
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
        _level.TotalGhostEaten = 0;
        _level.GhostScaredResetTimer = false;

        _cameraMovementTimer.Update(true, deltaTime);
        var center1 = new Vector2(320 - ((640 - _level.LevelWidth * _level.GridSize) / 2),
            500 - ((360 - _level.LevelHeight * _level.GridSize) / 2));
        var center2 = new Vector2(320 - ((640 - _level.LevelWidth * _level.GridSize) / 2),
            180 - ((360 - _level.LevelHeight * _level.GridSize) / 2));

        if (!_cameraMovementTimer.Output)
            _cameraManager.SetPosition(Tween.Lerp(center1, center2,
                Tween.ApplyEasing(_cameraMovementTimer.NormalizedElapsed, EaseType.EaseInOutQuart)));
    }

    protected override void OnUpdate(float deltaTime)
    {
        _parser.Update(deltaTime);
        _demoModeResetTimer.Update(DemoMode, deltaTime);
        if (_demoModeResetTimer.Output)
            Died(null);
    }

    protected override void OnLateUpdate(float deltaTime)
    {
        if (!DemoMode)
        {
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

        _worldService.Step(deltaTime);
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
        {
            _nextLevel = "";
            EndGame();
        }
        else
        {
            foreach (var player in _players)
                player.Score = player.StartScore;

            GameHost.SwitchScene(new LevelScene(_levelPath));
        }
    }

    public void EndGame()
    {
        if (DemoMode)
            return;

        if (_nextLevel == "")
            GameHost.SwitchScene(new EndScene());
        else
            GameHost.SwitchScene(new LevelScene(_nextLevel));
    }

    protected override void OnDispose()
    {
    }
}