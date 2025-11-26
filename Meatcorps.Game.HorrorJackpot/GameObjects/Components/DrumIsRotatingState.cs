using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Game.HorrorJackpot.Data;

namespace Meatcorps.Game.HorrorJackpot.GameObjects.Components;

public class DrumIsRotatingState: BaseStateMachine
{
    public DrumIsRotatingState(MainGameObject mainGameObject) : base(mainGameObject)
    {
    }

    protected override GameInternalState TargetState => GameInternalState.DrumIsRotating;
    protected override void UpdateState(float deltaTime)
    {
        if (Target.CurrentDrumType == DrumTypes.Jackpot)
            DrumRenderer.BlinkSpeed = 0.25f;
        else
        {
            DrumRenderer.BlinkSpeed = 0;
            DrumRenderer.Glow = 0.25f;
        }
        if (!Target.PlayerCheckin.IsPlayerCheckedIn(1, out var _))
            Target.InternalState = GameInternalState.WaitingForPlayers;
                
        DrumRenderer.GlowColor = Target.GetColor(Target.CurrentDrumType);
                
        if (DrumRenderer.Speed.EqualsSafe(0)) 
            Target.InternalState = GameInternalState.ApplyScore;
    }
}