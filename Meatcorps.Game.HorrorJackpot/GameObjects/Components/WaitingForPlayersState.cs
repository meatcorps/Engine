using Meatcorps.Engine.Arcade.Enums;
using Raylib_cs;

namespace Meatcorps.Game.HorrorJackpot.GameObjects.Components;

public class WaitingForPlayersState: BaseStateMachine
{
    public WaitingForPlayersState(MainGameObject mainGameObject) : base(mainGameObject)
    {
    }

    protected override GameInternalState TargetState => GameInternalState.WaitingForPlayers;
    protected override void UpdateState(float deltaTime)
    {
        Text = "Next victim! " + GameInfo.PricePoints + " Life points!\n";
        Target.SkullAnimation(deltaTime);
        GameInfo.State = GameState.Idle;
        DrumRenderer.BlinkSpeed = 2;
        DrumRenderer.GlowColor = Color.Red;
                
        if (Target.ButtonPressed)
        {
            Target.InternalState = GameInternalState.PlayerJoin;
            Target.ShowOverlay(Target.PlayerJoinOverlay);
            Target.Sounds.Play(Target.ScanRandom.Get());
        }
    }
}