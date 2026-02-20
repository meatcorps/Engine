namespace Meatcorps.Engine.Arcade.Data;

[Serializable]
public class ArcadeScoreItem
{
    public string Name { get; init; } = string.Empty;
    public int Score { get; init; }
    public DateTime Date { get; init; }
}