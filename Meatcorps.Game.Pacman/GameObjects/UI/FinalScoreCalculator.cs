using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.Arcade.Services;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Audio;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Extensions;
using Meatcorps.Engine.RayLib.UI;
using Meatcorps.Engine.Session;
using Meatcorps.Engine.Session.Data;
using Meatcorps.Game.Pacman.Data;
using Meatcorps.Game.Pacman.GameEnums;
using Meatcorps.Game.Pacman.GameObjects.Abstractions;
using Meatcorps.Game.Pacman.Resources;
using Raylib_cs;

namespace Meatcorps.Game.Pacman.GameObjects.UI;

public class FinalScoreCalculator : ResourceGameObject
{
    private readonly Rect _bounds;
    private readonly int _playerId;
    private readonly IArcadePointsMutator _arcadePointMutator;
    private readonly IPlayerCheckin _playerCheckin;
    private readonly SessionDataBag<GamePlayerData> _playerSessionData;
    private readonly InlineRender _inlineRender = new();
    private readonly string _playerName;
    private int _itemsInList = 1;
    private TimerOn _iterateTimer;
    private int _previousLine = -1;
    private readonly ArcadeGame _gameInfo;
    private int _relativePoints;

    public FinalScoreCalculator(Rect bounds, int playerId)
    {
        Layer = 2;
        _bounds = bounds;
        _playerId = playerId;
        _arcadePointMutator = GlobalObjectManager.ObjectManager.Get<IArcadePointsMutator>()!;
        _gameInfo = GlobalObjectManager.ObjectManager.Get<ArcadeGame>()!;
        _playerCheckin = GlobalObjectManager.ObjectManager.Get<IPlayerCheckin>()!;
        GlobalObjectManager.ObjectManager.Get<SessionService<GameSessionData, GamePlayerData>>()!
            .CurrentSession.TryGetPlayerData(_playerId, out _playerName, out _playerSessionData);
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        Camera = CameraLayer.UI;
        _inlineRender.Bounds = _bounds;
        _inlineRender.VAlign = VAlign.Top;
        _inlineRender.HAlign = HAlign.Center;
        _inlineRender.ItemSpacing = 2;
        _inlineRender.LineSpacing = 2;
        _inlineRender.AddLabel(Fonts.GetFont(), null, _playerCheckin.GetPlayerName(_playerId), 16,
                _playerId == 1 ? Color.Red : Color.Blue).AddNewLine()
            .AddLabel(Fonts.GetFont(), null, "LET'S SUM IT UP!", 10, _playerId == 1 ? Color.Red : Color.Blue)
            .AddSpacer(8, 16).AddNewLine();

        var highestScoreObject =
            GlobalObjectManager.ObjectManager.Get<HighScoreService>()!.GetScores().FirstOrDefault();
        var highestScore = highestScoreObject?.Score ?? int.MaxValue;

        var currentPoints = _playerSessionData.Get<int>(GamePlayerData.Score);
        var finalScore = currentPoints;
        finalScore -= _playerSessionData.Get<int>(GamePlayerData.Died) * 1000;
        var pointsWon = (int)MathF.Ceiling(finalScore / 100f);
        if (highestScore < finalScore)
            pointsWon += _gameInfo.PricePoints * 10;
        _relativePoints = pointsWon;

        ListValues("Score:", _playerSessionData.Get<int>(GamePlayerData.Score));
        ListValues("Died:", _playerSessionData.Get<int>(GamePlayerData.Died), 1000, " x100");
        ListValues("Lost:", -_playerSessionData.Get<int>(GamePlayerData.Died) * 1000);
        ListValues("Eat:", _playerSessionData.Get<int>(GamePlayerData.Eaten));
        ListValues("Super Pacman:", _playerSessionData.Get<int>(GamePlayerData.SuperPacMan));
        ListValues("Hitting Ghost:", _playerSessionData.Get<int>(GamePlayerData.GhostEaten));
        ListValues("End score:", finalScore, currentPoints);
        if (highestScore < finalScore)
            ListValues("Beat the HighScore!:", _gameInfo.PricePoints * 10, 0, "+ Points!");
        ListValues("Leaderboard rank:",
            GlobalObjectManager.ObjectManager.Get<HighScoreService>()!.GetLeaderboardPosition(finalScore), 0);

        if (_relativePoints != 0)
        {
            if (_relativePoints > 0)
                ListValues("Points won:", _relativePoints, _gameInfo.PricePoints, "!", 16);
            else
                ListValues("Points lost:", _relativePoints, _gameInfo.PricePoints, "don't cry", 16);
        }
        else
        {
            
            ListValues("No points for you!", _relativePoints, _gameInfo.PricePoints, "...", 16);
        }

        _iterateTimer = new TimerOn(_itemsInList * 1000);
        for (var i = 0; i < _itemsInList; i++)
            SetLineOnOrOff(i, false);

        _arcadePointMutator.SubmitPoints(_playerId, pointsWon + _gameInfo.PricePoints);

        GlobalObjectManager.ObjectManager.Get<HighScoreService>()!.AddScore(new ArcadeScoreItem
        {
            Name = _playerName,
            Date = DateTime.Now,
            Score = finalScore
        });
    }

    private void SetLineOnOrOff(int line, bool on)
    {
        if (_inlineRender.TryGetItem($"{line}_1", out var item1))
            item1.Enabled = on;
        if (_inlineRender.TryGetItem($"{line}_2", out var item2))
            item2.Enabled = on;
        if (_inlineRender.TryGetItem($"{line}_3", out var item3))
            item3.Enabled = on;
    }

    private void ListValues(string label, float value, float from = 0, string afterFix = "", int size = 8)
    {
        var smoothValue = new SmoothValue(from, 1);
        smoothValue.RealValue = value;

        _inlineRender
            .AddLabel(Fonts.GetFont(), $"{_itemsInList}_1", label, size, Color.White).AddNewLine()
            .AddSmoothValue(Fonts.GetFont(), $"{_itemsInList}_2", smoothValue, size + 2,
                from < value ? Color.Green : Color.Red);

        if (afterFix != "")
            _inlineRender.AddLabel(Fonts.GetFont(), $"{_itemsInList}_3", afterFix, size - 2, Color.White);

        _inlineRender.AddNewLine();
        _itemsInList++;
    }

    protected override void OnUpdate(float deltaTime)
    {
        _iterateTimer.Update(true, deltaTime);
        var currentLine = (int)MathF.Floor(_iterateTimer.NormalizedElapsed * _itemsInList);
        if (currentLine != _previousLine)
        {
            if (currentLine == _itemsInList - 1)
            {
                if (_relativePoints > 0)
                    Sounds.Play(GameSounds.PowerUpScore);
                else
                    Sounds.Play(GameSounds.Alarm, 1, 0.5f);

                GlobalObjectManager.ObjectManager.Get<MusicManager<GameMusic>>()!.Stop();
            }
            else
                Sounds.Play(GameSounds.Scorechange);


            SetLineOnOrOff(currentLine, true);
            _previousLine = currentLine;
        }

        _inlineRender.Update(deltaTime);
    }

    protected override void OnDraw()
    {
        _inlineRender.Draw();
    }

    protected override void OnDispose()
    {
    }
}