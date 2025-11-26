using Meatcorps.Game.HorrorJackpot.Data;
using Meatcorps.Game.HorrorJackpot.GameEnums;

namespace Meatcorps.Game.HorrorJackpot.GameObjects.Components;

public class ApplyScoreState: BaseStateMachine
{
    public ApplyScoreState(MainGameObject mainGameObject) : base(mainGameObject)
    {
    }

    protected override GameInternalState TargetState => GameInternalState.ApplyScore;
    protected override void UpdateState(float deltaTime)
    {
        if (!Target.PlayerCheckin.IsPlayerCheckedIn(1, out var _))
            Target.InternalState = GameInternalState.WaitingForPlayers;
        
        switch (Target.CurrentDrumType)
        {
            case DrumTypes.Score10:
                Target.ShowScoreOverlay(GameSprites.ResultPoint10, "YOU GAINED\n10 POINTS!");
                Target.ArcadePointMutator.SubmitPoints(1,GameInfo.PricePoints + 10);
                Target.Jackpot -= 10;
                Target.Sounds.Play(GameSounds.Powerupcollect, 0.1f, 0.8f);
                Target.Sounds.Play(Target.Score10Random.Get());
                break;
            case DrumTypes.Score100:
                Target.ShowScoreOverlay(GameSprites.ResultPoint100, "YOU GAINED\n100 POINTS!");
                Target.ArcadePointMutator.SubmitPoints(1,GameInfo.PricePoints + 100);
                Target.Sounds.Play(GameSounds.Powerupcollect, 0.1f, 1f);
                Target.Jackpot -= 100;
                Target.Sounds.Play(Target.Score100Random.Get());
                break;
            case DrumTypes.Jackpot:
                Target.ShowScoreOverlay(GameSprites.ResultJackpot, "YOU GAINED\n" + (Target.Jackpot - GameInfo.PricePoints) + " POINTS!");
                Target.ArcadePointMutator.SubmitPoints(1, Target.Jackpot);
                Target.Jackpot = GameInfo.PricePoints * 2;
                Target.Sounds.Play(GameSounds.PowerUpScore, 0.1f, 1f);
                Target.Sounds.Play(Target.JackpotRandom.Get());
                break;
            case DrumTypes.Reroll:
                Target.ShowScoreOverlay(GameSprites.ResultReroll, "");
                Target.Sounds.Play(GameSounds.Placed, 0.1f, 1f);
                Target.Sounds.Play(Target.RollRandom.Get());
                break;
            case DrumTypes.Nothing:
                Target.ShowScoreOverlay(GameSprites.ResultNothing, "YOU LOST\n" + GameInfo.PricePoints + " POINTS!");
                Target.Sounds.Play(GameSounds.Backgroundplaced, 0.1f, 0.2f);
                Target.Sounds.Play(Target.NothingRandom.Get());
                break;
        }
        Target.InternalState = GameInternalState.ShowingScore;
    }
}