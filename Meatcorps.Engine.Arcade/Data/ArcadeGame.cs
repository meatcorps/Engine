using Meatcorps.Engine.Arcade.Enums;

namespace Meatcorps.Engine.Arcade.Data;

public class ArcadeGame
{
    public required int Code { get; init; }
    public required string Name { get; init; }
    public string Description { get; init; } = "";
    public required int PricePoints { get; init; }

    public GameState State { get; set; } = GameState.Idle;
}