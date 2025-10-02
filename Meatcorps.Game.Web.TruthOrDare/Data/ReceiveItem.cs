namespace Meatcorps.Game.Web.TruthOrDare.Data;

public class ReceiveItem
{
    public string Type { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Text { get; set; } = "";
    public int LeftPosition { get; set; } = Random.Shared.Next(0, 1000);
}