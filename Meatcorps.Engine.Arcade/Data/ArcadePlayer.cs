namespace Meatcorps.Engine.Arcade.Data;


[Serializable]
public class ArcadePlayer
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public int Points { get; set; }
    public int CurrentGame { get; set; }
}