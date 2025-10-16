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
using Meatcorps.Game.KillTheSkulls.Data;
using Meatcorps.Game.KillTheSkulls.GameEnums;
using Meatcorps.Game.KillTheSkulls.GameObjects;
using Meatcorps.Game.KillTheSkulls.GameObjects.Abstractions;
using Meatcorps.Game.KillTheSkulls.GameObjects.UI;
using Raylib_cs;

namespace Meatcorps.Game.KillTheSkulls.Scenes;

public class LevelScene : BaseScene
{
    private readonly string _levelPath;
    public int TotalPlayers => DemoMode ? 2 : _sessionService.CurrentSession.TotalPlayers;
    public bool DemoMode { get; }
    private LevelData _level { get; set; }
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
    public int RandomNumber = -1;
    public int MaxIdle = 4;
    private List<int> _notSpawnedEnemies = new();
    public int GetLiveCount = 0;
    public bool StartGame = false;
    private ScoreEmitter _scoreEmitter; 

    public LevelScene(string levelPath = "Assets/Level1.txt", bool demoMode = false)
    {
        _levelPath = levelPath;
        UpdateTimeMultiplier = 1f;
        DemoMode = demoMode;
    }

    protected override void OnInitialize()
    {
        UpdateTimeMultiplier = 1;
        _font = GlobalObjectManager.ObjectManager.Get<TextManager<DefaultFont>>()!.GetFont();
        _uiMessage = new UIMessageEmitter(TextKitStyles.HudDefault(_font));
        _musicManager = GlobalObjectManager.ObjectManager.Get<MusicManager<GameMusic>>()!;
        _soundFxManager = GlobalObjectManager.ObjectManager.Get<SoundFxManager<GameSounds>>()!;
        _sessionService = GlobalObjectManager.ObjectManager.Get<SessionService<GameSessionData, GamePlayerData>>()!;
        _playerCheckin = GlobalObjectManager.ObjectManager.Get<IPlayerCheckin>()!;
        _scoreEmitter = new ScoreEmitter();
        AddGameObject(_scoreEmitter);
        
        if (!DemoMode)
            _musicManager.Play(GameMusic.LevelAmbient);

        SpawnPlayer(1);
        
        AddGameObject(_uiMessage);
        
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
            .Register(() => new SimpleCommand("STARTGAME",
                () =>
                {
                    StartGame = true;
                }
            ));

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
                
                .Register(() => new IntVariableCommand("SETMAXIDLE",
                    (value) =>
                    {
                        MaxIdle = value;
                    }
                ))
                .Register(() => new SimpleCommand("INCREASESPEED",
                    () =>
                    {
                        UpdateTimeMultiplier += 0.1f;
                        if (UpdateTimeMultiplier > 2)
                            UpdateTimeMultiplier = 2;
                    }
                ))
                .Register(() => new StringVariableCommand("PAUSESONG", sound => { _musicManager.Pause(); }))
                .Register(() => new StringVariableCommand("RESUMESONG", sound => { _musicManager.Resume(); }))
                .Register(() => new SimpleCommand("ENDLEVEL", () => { GameHost.SwitchScene(new LevelScene()); }))
                .Register(() => new StringVariableCommand("NEXTLEVEL",
                    level => { GameHost.SwitchScene(new LevelScene("Assets/" + level)); }))
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
            _parser.Register(() => new SimpleCommand("ENDLEVEL", () => { Died(null); }))
                .Register(() => new StringVariableCommand("NEXTLEVEL", level => { Died(null); }));
        }

        _parser.Load();

        var cameraManager = new CameraControllerGameObject(GlobalObjectManager.ObjectManager.Get<ICamera>()!);
        var center = new Vector2(320, 180);

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
        AddGameObject(new GameObjects.PipeOverlay());
        var rows = new List<LevelRow>();
        for (var i = 0; i < 5; i++)
        {
            var row = new LevelRow
            {
                LedBar = new GameObjects.LedBar(new Vector2(i * 128f, 240f), GetColumnColor(i)),
                Thunder = new GameObjects.Thunder(new Vector2(i * 128f, 128f)),
                Enemy = new Enemy(new Vector2(i * 128f, 112f), i)
            };
            rows.Add(row);
            AddGameObject(row.LedBar);
            AddGameObject(row.Thunder);
            AddGameObject(row.Enemy);
            AddGameObject(new RowManager(row, i));
        }
        _level = new LevelData(rows.ToArray());
        SceneObjectManager.Register(_level);
        _parser.Parse(_levelPath);
    }

    private Color GetColumnColor(int column)
    {
        switch (column)
        {
            case 0:
                return Color.White;
            case 1:
                return Color.Blue;
            case 2:
                return Color.Yellow;
            case 3:
                return Color.Green;
            case 4:
                return Color.Magenta;
        }

        return Color.Black;
    }

    private void SpawnPlayer(int playerId)
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
            PlayerId = playerId,
            Color = playerId == 1 ? Color.Magenta : Color.Blue,
        };
        _players.Add(player);
        
        if (!DemoMode)
            AddGameObject(new PlayerUI(player));
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
        foreach (var row in _level.LevelRows)
        {
            row.Hit = false;
            row.Miss = false;
        }
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (StartGame)
        {
            _notSpawnedEnemies.Clear();
            var counter = 0;
            foreach (var row in _level.LevelRows)
            {
                if (row.Enemy.State == EnemyState.Idle)
                    _notSpawnedEnemies.Add(counter);
                counter++;
            }

            if (_notSpawnedEnemies.Count > MaxIdle)
            {
                var totalNotSpawned = _notSpawnedEnemies.Count;
                RandomNumber =
                    _notSpawnedEnemies[Math.Min(_notSpawnedEnemies.Count - 1, new Random().Next(0, totalNotSpawned))];
            }
            else
                RandomNumber = -1;
        }

        _parser.Update(deltaTime);
    }

    public void EnemyAttack()
    {
        foreach (var row in _level.LevelRows)
        {
            row.Enemy.Attack();
        }
    }

    protected override void OnLateUpdate(float deltaTime)
    {
        if (DemoMode || !StartGame)
            return;
        
        if (!_playerCheckin.IsPlayerCheckedIn(1, out var _))
            EndGame();
        
        var player = _players[0];
        var missed = false;
        var hit = false;
        var counter = 0;
        foreach (var row in _level.LevelRows)
        {
            if (row.Miss)
            {
                missed = true;
            }

            if (row.Hit)
            {
                hit = true;
                player.Streak++;
                GetLiveCount++;
                var totalScore = 100 * Math.Min(player.Streak, 10);
                player.Score += totalScore;
                _scoreEmitter.ShowScore(new Vector2(counter * 128 + 32, 240), $"+{totalScore}");
            }

            counter++;
        }

        player.MaxStreak = Math.Max(player.MaxStreak, player.Streak);
        
        if (missed)
        {
            player.Lives--;
            player.Streak = 0;
            GetLiveCount = 0;
            _scoreEmitter.Negative(new Vector2(640 - 64, 64), $"-1");
            _scoreEmitter.Negative(new Vector2(640 - 192, 64), $"ZERO!");
        }

        
        if (player.Lives == 10)
            GetLiveCount = 0;

        if (GetLiveCount == 10)
        {
            GetLiveCount = 0;
            player.Score += 1000;
            if (player.Lives < 10)
                player.Lives++;
            _soundFxManager.Play(GameSounds.PowerUpScore);
            _uiMessage.Show("You got a life!", DefaultTextStyle(_font));
        }
        
        
        if (hit && GetLiveCount >= 2)
        {
            _scoreEmitter.ShowScore(new Vector2(640 - 192, 64), $"{10 - GetLiveCount} FOR\nEXTRA LIFE");
        }
        
        if (player.Lives == 0)
            EndGame();
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
    
    public static UIMessageStyle DefaultTextStyle(Font font) => new()
    {
        Style = TextKitStyles.HudDefault(font) with
        {
            Color = new Color(0, 255, 255),
            UseOutline = true,
            OutlineColor = Color.Black,
            OutlineSize = 1
        },
        AppearDurationInMilliseconds = 180,
        DisappearDurationInMilliseconds = 180,
        ColorFrom = Color.Magenta,
        ColorTo = new Color(0, 255, 255),
        ColorAfter = Color.Magenta,
    };
}