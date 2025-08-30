using Meatcorps.Game.Snake.Resources;

namespace Meatcorps.Game.Snake.Data;

public interface IConsumableItem
{
    SnakeSprites Sprite { get; }
    int Points { get; }
    bool CanDecay { get; }
    float LifetimeInWorld { get; }
    string Group { get; }
    ConsumableRule Rule { get; }
    SnakeSounds DropSound { get; }
    SnakeSounds PickupSound { get; }
    float PickupPitchSound { get; }
    SnakeSounds TimeOverSound { get; }

    void Refresh(Player player, IConsumableItem item);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <returns>This item need to keep alive after the call</returns>
    bool Once(Player player);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="deltaTime"></param>
    /// <param name="player"></param>
    /// <returns>This item need to keep alive after the call</returns>
    bool Update(float deltaTime, Player player);
    void End(Player player);
}

