namespace Meatcorps.Game.Web.TruthOrDare.Data;

public class TruthDareItem
{
    public TruthDareType Type { get; set; }
    public int ItemId { get; set; }
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public int RandomValue { get; set; }
}

public enum TruthDareType
{
    Truth,
    Dare
}