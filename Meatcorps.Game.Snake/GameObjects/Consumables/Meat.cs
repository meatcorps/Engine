using Meatcorps.Game.Snake.Data;
using Meatcorps.Game.Snake.Resources;
using Raylib_cs;

namespace Meatcorps.Game.Snake.GameObjects.Consumables;

public class Meat1: ConsumableItemBase
{
    
    public Meat1() : base(SnakeSprites.Meat1, 50, true)
    {
    }

    public override SnakeSounds DropSound => SnakeSounds.Meatonground;
    public override SnakeSounds PickupSound => SnakeSounds.Snakeeating;
}

public class Meat2: ConsumableItemBase
{
    public Meat2() : base(SnakeSprites.Meat2, 100, true)
    {
    }
    
    public override SnakeSounds DropSound => SnakeSounds.Meatonground;
    public override SnakeSounds PickupSound => SnakeSounds.Snakeeating;
}

public static class GetRandomMeat
{
    public static ConsumableItemBase Get()
    {
        return Raylib.GetRandomValue(0, 100) > 50 ? new Meat1() : new Meat2();
    }
}