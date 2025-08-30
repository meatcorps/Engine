using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Game.Pacman.Data;
using Meatcorps.Game.Pacman.GameEnums;
using Meatcorps.Game.Pacman.Scenes;

namespace Meatcorps.Game.Pacman.GameObjects.Abstractions;

public abstract class BasePlayer : ResourceGameObject
{
    protected readonly IArcadePointsMutator PointMutator;
    protected readonly IPlayerCheckin PlayerCheckin;
    protected readonly PlayerInputRouter<GameInput> Controller;
    public Player Player { get; }
    

    public BasePlayer(Player _player)
    {
        Player = _player;
        PointMutator = GlobalObjectManager.ObjectManager.Get<IArcadePointsMutator>()!;
        PlayerCheckin = GlobalObjectManager.ObjectManager.Get<IPlayerCheckin>()!;
        Controller = GlobalObjectManager.ObjectManager.Get<PlayerInputRouter<GameInput>>()!;
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
        if (!PlayerCheckin.IsPlayerCheckedIn(Player.PlayerId, out var _) && !DemoMode)
            PlayerLost();

        base.OnPreUpdate(deltaTime);
    }
}