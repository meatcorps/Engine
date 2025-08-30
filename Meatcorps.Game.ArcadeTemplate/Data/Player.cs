using Meatcorps.Engine.Session.Data;
using Meatcorps.Game.ArcadeTemplate.GameEnums;
using Meatcorps.Game.ArcadeTemplate.Resources;
using Raylib_cs;

namespace Meatcorps.Game.ArcadeTemplate.Data;

public class Player
{
    public required SessionDataBag<GamePlayerData> SessionDataBag { get; init; }
    public bool IsDead { get; set; }

    public int Score
    {
        get => SessionDataBag.Get<int>(GamePlayerData.Score); 
        set => SessionDataBag.Set(GamePlayerData.Score, value);
    }

    public int StartScore { get; private set; }
    public int PlayerId { get; init; }
    public float WorldSpeed { get; set; }
    
    public Color Color { get; init; }

    public Player()
    {
    }
    
    public void Initialize()
    {
        StartScore = Score;
        
    }
    
    public void PreUpdate()
    {
        WorldSpeed = 1;
    }
    
    public void PostUpdate()
    {
    }
    
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