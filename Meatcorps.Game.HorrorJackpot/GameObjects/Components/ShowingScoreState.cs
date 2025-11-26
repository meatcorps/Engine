using Meatcorps.Game.HorrorJackpot.Data;
using Meatcorps.Game.HorrorJackpot.GameEnums;

namespace Meatcorps.Game.HorrorJackpot.GameObjects.Components;

public class ShowingScoreState: BaseStateMachine
{
    public ShowingScoreState(MainGameObject mainGameObject) : base(mainGameObject)
    {
    }

    protected override GameInternalState TargetState => GameInternalState.ShowingScore;
    protected override void UpdateState(float deltaTime)
    {
        DrumRenderer.BlinkSpeed = 0.25f;
        DrumRenderer.GlowColor = Target.GetColor(Target.CurrentDrumType);

        if (Target.ShowScoreTimer.Output || Target.ButtonPressed)
        {
            Target.HideOverlay();
            Target.InternalState = Target.CurrentDrumType == DrumTypes.Reroll
                ? GameInternalState.PlayerIsApplyingForce
                : GameInternalState.WaitingForPlayers;
                    
            if (Target.InternalState == GameInternalState.WaitingForPlayers)
                Target.Sounds.Play(GameSounds.Haha);
        }
    }
}