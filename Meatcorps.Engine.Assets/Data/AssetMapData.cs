namespace Meatcorps.Engine.Assets.Data;

public class AssetMapData
{
    public List<AssetMapItemData> Items = new();
}

public class AssetMapItemData
{
    public string Path { get; set; }
    public long Length { get; set; }
    public long PlainLength { get; set; }
    public long Position { get; set; }
}