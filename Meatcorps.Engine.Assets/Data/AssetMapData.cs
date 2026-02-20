namespace Meatcorps.Engine.Assets.Data;

public class AssetMapData
{
    public readonly List<AssetMapItemData> Items = new();
}

public class AssetMapItemData
{
    public string Path { get; set; } = string.Empty;
    public long Length { get; set; }
    public long PlainLength { get; set; }
    public long Position { get; set; }
}