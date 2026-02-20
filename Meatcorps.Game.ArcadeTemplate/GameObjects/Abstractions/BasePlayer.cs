using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Game.ArcadeTemplate.Data;
using Meatcorps.Game.ArcadeTemplate.Scenes;

namespace Meatcorps.Game.ArcadeTemplate.GameObjects.Abstractions;

public abstract class BasePlayer: ResourceGameObject
{
    protected new readonly IArcadePointsMutator PointMutator;
    protected readonly IPlayerCheckin PlayerCheckin;
    public Player Player { get; }

    protected BasePlayer(Player player)
    {
        Player = player;
        PointMutator = GlobalObjectManager.ObjectManager.Get<IArcadePointsMutator>()!;
        PlayerCheckin = GlobalObjectManager.ObjectManager.Get<IPlayerCheckin>()!;
    }

    protected void EndPlayer()
    {
        ((LevelScene)Scene).Died(this);
    }
    
    
    protected void EndGame()
    {
        ((LevelScene)Scene).EndGame();
    }
    
    protected abstract void PlayerLost();

    protected override void OnPreUpdate(float deltaTime)
    {
        if (!PlayerCheckin.IsPlayerCheckedIn(Player.PlayerId, out var _))
            PlayerLost();
        
        base.OnPreUpdate(deltaTime);
    }
}