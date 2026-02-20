using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Game.KillTheSkulls.Data;
using Meatcorps.Game.KillTheSkulls.Scenes;

namespace Meatcorps.Game.KillTheSkulls.GameObjects.Abstractions;

public abstract class BasePlayer : ResourceGameObject
{
    protected new readonly IArcadePointsMutator PointMutator;
    protected readonly IPlayerCheckin PlayerCheckin;
    public Player Player { get; }

    public BasePlayer(Player _player)
    {
        Player = _player;
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

    abstract protected void PlayerLost();

    protected override void OnPreUpdate(float deltaTime)
    {
        if (!PlayerCheckin.IsPlayerCheckedIn(Player.PlayerId, out var _))
            PlayerLost();

        base.OnPreUpdate(deltaTime);
    }
}