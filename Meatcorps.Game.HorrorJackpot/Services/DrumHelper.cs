using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Game.HorrorJackpot.Data;

namespace Meatcorps.Game.HorrorJackpot.Services;

public static class DrumHelper
{
    private static DrumTypes[] _drumTypes =
    [
        DrumTypes.Nothing,
        DrumTypes.Score10,
        DrumTypes.Nothing,
        DrumTypes.Reroll,
        DrumTypes.Nothing,
        DrumTypes.Score100,
        DrumTypes.Nothing,
        DrumTypes.Score10,
        DrumTypes.Nothing,
        DrumTypes.Score10,
        DrumTypes.Nothing,
        DrumTypes.Reroll,
        DrumTypes.Nothing,
        DrumTypes.Score10,
        DrumTypes.Nothing,
        DrumTypes.Score10,
        DrumTypes.Nothing,
        DrumTypes.Score100,
        DrumTypes.Nothing,
        DrumTypes.Score10,
        DrumTypes.Nothing,
        DrumTypes.Score100,
        DrumTypes.Nothing,
        DrumTypes.Score10,
        DrumTypes.Nothing,
        DrumTypes.Score10,
        DrumTypes.Nothing,
        DrumTypes.Reroll,
        DrumTypes.Nothing,
        DrumTypes.Score10,
        DrumTypes.Jackpot,
        DrumTypes.Reroll
    ];

    public static DrumTypes GetDrumTypeFromNormal(float normal)
    {
        normal = (normal + 0.25f).Wrap(1);
        return _drumTypes[(int) (normal * _drumTypes.Length)];
    }
}