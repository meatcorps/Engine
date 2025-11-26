using Meatcorps.Engine.Arcade.Enums;
using Meatcorps.Game.HorrorJackpot.GameEnums;

namespace Meatcorps.Game.HorrorJackpot.GameObjects.Components;

public class PlayerJoinState: BaseStateMachine
{
    public PlayerJoinState(MainGameObject mainGameObject) : base(mainGameObject)
    {
    }

    protected override GameInternalState TargetState => GameInternalState.PlayerJoin;
    
    protected override void UpdateState(float deltaTime)
    {
        GameInfo.State = GameState.Waiting;
        if (Target.PlayerCheckin.IsPlayerCheckedIn(1, out var player))
        {
            if (Target.ArcadePointMutator.RequestPoints(1, GameInfo.PricePoints))
            {
                Target.InternalState = GameInternalState.PlayerIsApplyingForce;
                Target.Jackpot += GameInfo.PricePoints;
            }
            else
            {
                Target.InternalState = GameInternalState.WaitingForPlayers;
                Target.ShowScoreOverlay(GameSprites.ResultNothing, "NOT ENOUGH\nPOINTS!");
                Target.Sounds.Play(GameSounds.NotEnough);
            }

            Target.HideOverlay();
        }

        if (Target.WaitForPlayerTimer.Output)
        {
            Target.InternalState = GameInternalState.WaitingForPlayers;
            Target.Sounds.Play(GameSounds.Haha);
            Target.HideOverlay();
        }
    }
}