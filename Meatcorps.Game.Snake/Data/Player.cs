using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.Session.Data;
using Meatcorps.Game.Snake.GameObjects.UI;
using Meatcorps.Game.Snake.Resources;
using Raylib_cs;

namespace Meatcorps.Game.Snake.Data;

public class Player
{
    public required SessionDataBag<SnakePlayerData> SessionDataBag { get; init; }
    private float _originalTimeMoveTimer;
    public GameObjects.Snake Snake { get; set; }
    public PlayerUI PlayerUI { get; set; }
    public FixedTimer MoveTimer { get; init; }
    public SnakeModifiers Modifiers { get; init; } = new ();
    public bool IsDead { get; set; }

    public int Score
    {
        get => SessionDataBag.Get<int>(SnakePlayerData.Score); 
        set => SessionDataBag.Set(SnakePlayerData.Score, value);
    }

    public int StartScore { get; private set; }

    public int PlayerId { get; init; }
    public Color Color { get; init; }
    public float WorldSpeed { get; set; }

    public Player()
    {
    }
    
    public void Initialize()
    {
        _originalTimeMoveTimer = MoveTimer.DurationInMs;
        StartScore = Score;
        
    }
    
    public void PreUpdate()
    {
        WorldSpeed = 1;
        Modifiers.Reset();
    }
    
    public void PostUpdate()
    {
        MoveTimer.ChangeSpeed(_originalTimeMoveTimer / Math.Max(0.1f, Modifiers.SpeedModifier));
    }
    
    public void AddValue(SnakePlayerData item, int amount = 1)
    {
        var data = SessionDataBag.Get<int>(item);
        SessionDataBag.Set(item, data + amount);
    }
    
    public void MaxValue(SnakePlayerData item, int amount)
    {
        var data = SessionDataBag.Get<int>(item);
        SessionDataBag.Set(item, Math.Max(data, amount));
    }
}