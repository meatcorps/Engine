using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Game.Snake.Data;
using Meatcorps.Game.Snake.Resources;

namespace Meatcorps.Game.Snake.GameObjects.Consumables;

public abstract class PowerUp : ConsumableItemBase
{
    public override float LifetimeInWorld => 10000f;
    public float NormalizedElapsed => _timer.NormalizedElapsed;
    public float TimeRemaining => _timer.TimeRemaining;
    
    private readonly TimerOn _timer;
    public override SnakeSounds DropSound => SnakeSounds.Placed;
    public override SnakeSounds PickupSound => SnakeSounds.Powerupcollect;
    
    public PowerUp(SnakeSprites sprite, int points, ConsumableRule rule, float powerUpLifeTime = 30000) : base(sprite, points, false, rule)
    {
        _timer = new TimerOn(powerUpLifeTime);
    }
    
    protected abstract void OnPowerUp(float deltaTime, Player player);
    
    public override bool Once(Player player) => true;

    public override bool Update(float deltaTime, Player player)
    {
        _timer.Update(true, deltaTime);
        OnPowerUp(deltaTime, player);
        return !_timer.Output;
    }

    public override void Refresh(Player player, IConsumableItem item)
    {
        _timer.Reset();
    }
}

public class Score2X: PowerUp
{
    public override string Group { get; } = "Score";
    public override float PickupPitchSound => 1f;
    public override SnakeSounds PickupSound => SnakeSounds.PowerUpScore;

    public Score2X() : base(SnakeSprites.Score2X, 10, ConsumableRule.Replace)
    {
    }

    protected override void OnPowerUp(float deltaTime, Player player)
    {
        player.Modifiers.ScoreModifier = 2;
    }
}

public class Score3X: PowerUp
{
    public override string Group { get; } = "Score";
    public override float PickupPitchSound => 1.5f;
    public override SnakeSounds PickupSound => SnakeSounds.PowerUpScore;

    public Score3X() : base(SnakeSprites.Score3X, 10, ConsumableRule.Replace)
    {
    }

    protected override void OnPowerUp(float deltaTime, Player player)
    {
        player.Modifiers.ScoreModifier = 3;
    }
}

public class Score4X: PowerUp
{
    public override string Group { get; } = "Score";
    public override float PickupPitchSound => 2f;
    public override SnakeSounds PickupSound => SnakeSounds.PowerUpScore;

    public Score4X() : base(SnakeSprites.Score4X, 10, ConsumableRule.Replace)
    {
    }

    protected override void OnPowerUp(float deltaTime, Player player)
    {
        player.Modifiers.ScoreModifier = 4;
    }
}

public class SnakeFaster: PowerUp
{
    public override SnakeSounds PickupSound => SnakeSounds.Snakefaster;

    public SnakeFaster() : base(SnakeSprites.SnakeFaster, 10, ConsumableRule.Stack)
    {
    }

    public override bool Once(Player player)
    {
        if (player.Modifiers.SpeedModifier >= 4)
            return false;
        player.Modifiers.SpeedModifier += 0.5f;
        return false;
    }

    protected override void OnPowerUp(float deltaTime, Player player)
    {
    }
}

public class SnakeSlower: PowerUp
{
    public override SnakeSounds PickupSound => SnakeSounds.Snakeslower;
    
    public SnakeSlower() : base(SnakeSprites.SnakeSlower, 10, ConsumableRule.Stack)
    {
    }

    public override bool Once(Player player)
    {
        if (player.Modifiers.SpeedModifier < 0.5f)
            return false;
        player.Modifiers.SpeedModifier -= 0.5f;
        return false;
    }

    protected override void OnPowerUp(float deltaTime, Player player)
    {
    }
}

public class RotProof: PowerUp
{
    public RotProof() : base(SnakeSprites.RotProof, 10, ConsumableRule.Replace)
    {
    }

    protected override void OnPowerUp(float deltaTime, Player player)
    {
        player.Modifiers.RotProof = true;
    }
}

public class ThroughWalls: PowerUp
{
    public ThroughWalls() : base(SnakeSprites.ThroughWalls, 10, ConsumableRule.Replace)
    {
    }

    protected override void OnPowerUp(float deltaTime, Player player)
    {
        player.Modifiers.PassThroughWalls = true;
    }
}

public class WorldSlower: PowerUp
{
    public override SnakeSounds PickupSound => SnakeSounds.Worldslower;
    public override string Group { get; } = "WorldSpeed";

    public WorldSlower() : base(SnakeSprites.WorldSlower, 10, ConsumableRule.Replace)
    {
    }

    protected override void OnPowerUp(float deltaTime, Player player)
    {
        player.WorldSpeed = 0.5f;
    }
}

public class WorldFaster: PowerUp
{
    public override SnakeSounds PickupSound => SnakeSounds.Worldfaster;
    public override string Group { get; } = "WorldSpeed";

    public WorldFaster() : base(SnakeSprites.WorldFaster, 10, ConsumableRule.Replace)
    {
    }

    protected override void OnPowerUp(float deltaTime, Player player)
    {
        player.WorldSpeed = 3f;
    }
}