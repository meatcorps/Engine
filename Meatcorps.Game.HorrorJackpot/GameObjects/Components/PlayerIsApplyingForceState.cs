using Meatcorps.Engine.Arcade.Enums;
using Meatcorps.Game.HorrorJackpot.GameEnums;
using Raylib_cs;

namespace Meatcorps.Game.HorrorJackpot.GameObjects.Components;

public class PlayerIsApplyingForceState: BaseStateMachine
{
    public PlayerIsApplyingForceState(MainGameObject mainGameObject) : base(mainGameObject)
    {
    }

    protected override GameInternalState TargetState => GameInternalState.PlayerIsApplyingForce;
    protected override void UpdateState(float deltaTime)
    {
        if (Target.ButtonWaitingTrigger.IsRisingEdge)
        {
            Target.Sounds.Play(Target.ButtonRandom.Get());
            Target.ButtonWaitingTimer.Reset();
        }
        if (!Target.PlayerCheckin.IsPlayerCheckedIn(1, out var _))
            Target.InternalState = GameInternalState.WaitingForPlayers;

        GameInfo.State = GameState.Active;
        if (Target.ButtonPressed && Target.IgnoreInputTimer.Output)
        {
            DrumRenderer.Speed = Target.CalculateForce();
            Target.InternalState = GameInternalState.DrumIsRotating;
        }
                
        if (Target.ApplyForceTimer.NormalizedElapsed < 0.01f)
            Target.Sounds.Play(GameSounds.Shortblip, 0.2f, 1);
        if (Target.ApplyForceTimer.NormalizedElapsed > 0.5f && Target.ApplyForceTimer.NormalizedElapsed < 0.51f)
            Target.Sounds.Play(GameSounds.Shortblip, 0.2f, 1);
                
        DrumRenderer.BlinkSpeed = 0;
        DrumRenderer.Glow = 1;
        DrumRenderer.GlowColor = Raylib.ColorLerp(Color.Black, Color.Red, Target.CalculateForce() / Target.MaxForce);
    }
}