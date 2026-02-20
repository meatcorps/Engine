using Meatcorps.Engine.Session.Data;
using Meatcorps.Game.KillTheSkulls.GameEnums;

namespace Meatcorps.Game.KillTheSkulls.Data;

public class Player
{
    public required SessionDataBag<GamePlayerData> SessionDataBag { get; init; }

    public int Score
    {
        get => SessionDataBag.Get<int>(GamePlayerData.Score);
        set => SessionDataBag.Set(GamePlayerData.Score, value);
    }
    
    public int MaxStreak
    {
        get => SessionDataBag.Get<int>(GamePlayerData.MaxStreak);
        set => SessionDataBag.Set(GamePlayerData.MaxStreak, value);
    }
    
    public int Lives
    {
        get => SessionDataBag.Get<int>(GamePlayerData.Lives);
        set => SessionDataBag.Set(GamePlayerData.Lives, value);
    }
    
    public int Streak
    {
        get => SessionDataBag.Get<int>(GamePlayerData.Streak);
        set => SessionDataBag.Set(GamePlayerData.Streak, value);
    }

    public int StartScore { get; set; }
    public int PlayerId { get; init; }

    public void AddValue(GamePlayerData item, int amount = 1)
    {
        var data = SessionDataBag.Get<int>(item);
        SessionDataBag.Set(item, data + amount);
    }

    public void MaxValue(GamePlayerData item, int amount)
    {
        var data = SessionDataBag.Get<int>(item);
        SessionDataBag.Set(item, Math.Max(data, amount));
    }
}