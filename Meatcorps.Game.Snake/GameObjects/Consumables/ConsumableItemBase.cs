using Meatcorps.Game.Snake.Data;
using Meatcorps.Game.Snake.Resources;

namespace Meatcorps.Game.Snake.GameObjects.Consumables;

public abstract class ConsumableItemBase: IConsumableItem
{
    public SnakeSprites Sprite { get; }
    public int Points { get; }
    public bool CanDecay { get; }
    public virtual float LifetimeInWorld { get; } = 0;
    public virtual string Group => GetType().Name;
    public ConsumableRule Rule { get; }
    public abstract SnakeSounds DropSound { get; }
    public abstract SnakeSounds PickupSound { get; }
    public virtual float PickupPitchSound => 1;
    public virtual SnakeSounds TimeOverSound { get; } = SnakeSounds.PowerUpAway;

    public ConsumableItemBase(SnakeSprites sprite, int points, bool canDecay = false, ConsumableRule rule = ConsumableRule.Stack)
    {
        Sprite = sprite;
        Points = points;
        CanDecay = canDecay;
        Rule = rule;
    }

    public virtual void Refresh(Player player, IConsumableItem item)
    {
        throw new NotImplementedException();
    }

    public virtual bool Once(Player player)
    {
        return false;
    }

    public virtual bool Update(float deltaTime, Player player)
    {
        return false;
    }

    public virtual void End(Player player)
    {
    }
}