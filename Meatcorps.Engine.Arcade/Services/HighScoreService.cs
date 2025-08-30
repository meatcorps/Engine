using System.Text.Json;
using System.Text.Json.Serialization;
using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Storage.Services;

namespace Meatcorps.Engine.Arcade.Services;

public class HighScoreService
{
    private readonly int _maxScores;
    private ArcadeScores _scores;
    private PersistentDatabase _database;
    
    public HighScoreService()
    {
        _database = GlobalObjectManager.ObjectManager.Get<PersistentDatabase>()!;

        if (_database.ContainsKey("highscores"))
            _scores = JsonSerializer.Deserialize<ArcadeScores>((string)_database["highscores"]) ?? new ArcadeScores();
        else 
            _scores = new ArcadeScores
            {
                Scores = new List<ArcadeScoreItem>()
            };

#if DEBUG
        Console.WriteLine("Leaderboard rank:");
        foreach (var score in GetLeaderboard(10))
        {
            Console.WriteLine($"#{score.rank} {score.item.Name} ({score.item.Score})");
        }
        
        Console.WriteLine();
#endif
        
    }
    
    public IReadOnlyList<ArcadeScoreItem> GetScores() => _scores.Scores;

    public HighScoreService(int maxScores): this()
    {
        _maxScores = maxScores;
        
    }

    public int GetLeaderboardPosition(int score)
    {
        var rank = 0;
        var previous = int.MinValue;
        foreach (var scoreItem in _scores.Scores)
        {
            if (previous != scoreItem.Score)
                rank++;

            previous = scoreItem.Score;
            
            if (score >= scoreItem.Score)
                break;
        }
        return rank;
    }
    
    public IEnumerable<(int rank, ArcadeScoreItem item)> GetLeaderboard(int maxItems = int.MaxValue)
    {
        var rank = 0;
        var previous = int.MinValue;
        var count = 0;
        foreach (var scoreItem in _scores.Scores)
        {
            if (previous != scoreItem.Score)
                rank++;

            previous = scoreItem.Score;

            if (count == maxItems)
                yield break;
            
            yield return (rank, scoreItem); 
            count++;
        }
    }
    
    public void AddScore(ArcadeScoreItem score)
    {
        if (score.Score == 0)
            return;
        
        if (_scores.Scores == null)
            _scores.Scores = new List<ArcadeScoreItem>();
        
        _scores.Scores.Add(score);
        _scores.Scores.Sort((a, b) => b.Score.CompareTo(a.Score));
        
        if (_scores.Scores.Count > _maxScores)
            _scores.Scores.RemoveRange(_maxScores, _scores.Scores.Count - _maxScores);
        
        Save();
    }

    private void Save()
    {
        _database["highscores"] = JsonSerializer.Serialize(_scores);
        _database.Dirty = true;
    }
}