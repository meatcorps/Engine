using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Utilities;

public class ScreenResolutionIterator
{
    private List<ScreenResolutionMode> _modeList = new List<ScreenResolutionMode>();
    private string[] _ratios = [];
    
    public ScreenResolutionIterator(string[]? ratios = null)
    {
        _ratios = ratios ?? [];
    }

    public void Load(int maxWidth, int maxHeight)
    {
        _modeList.Clear();
        Console.WriteLine($"Loading modes for MAX {maxWidth}x{maxHeight}");
        var maxMode = new ScreenResolutionMode(maxWidth, maxHeight, string.Empty);
        
        foreach (var possibleMode in ParseModes())
        {
            var ratioMatch = RatiosMatch(possibleMode);
            if (ratioMatch && possibleMode <= maxMode)
                _modeList.Add(possibleMode);    
        }
    }

    public void Load(int monitor = -1)
    {
        if (monitor == -1) 
            monitor = Raylib.GetCurrentMonitor();
        
        Load(Raylib.GetMonitorWidth(monitor), Raylib.GetMonitorHeight(monitor));
    }
    
    public IEnumerable<ScreenResolutionMode> GetModes()
    {
        return _modeList;
    }

    public bool TryGetMode(string widthXHeight, out ScreenResolutionMode mode)
    {
        foreach (var possibleMode in _modeList)
        {
            if (possibleMode.ToString() == widthXHeight)
            {
                mode = possibleMode;
                return true;
            }
        }
        mode = default;
        return false;
    }

    public int GetModeIndex(int width, int height)
    {
        for (var i = 0; i < _modeList.Count; i++)
        {
            var mode = _modeList[i];
            if (mode.Width == width && mode.Height == height)
                return i;
        }
        return -1;
    }

    private bool RatiosMatch(ScreenResolutionMode mode)
    {
        if (_ratios.Length == 0)
            return true;
        
        return _ratios.Any(ratio => ratio == mode.Ratio);
    }
    
    private IEnumerable<ScreenResolutionMode> ParseModes()
    {
        var lines = ScreenResolutionRawData.AllModes.Split("\n");
        foreach (var line in lines.Skip(1))
        {
            var data = line.Split(",");
            if (data.Length != 4) 
                continue;
            yield return new ScreenResolutionMode(int.Parse(data[0]), int.Parse(data[1]), data[2]);
        }
    }
}

public struct ScreenResolutionMode: IEqualityComparer<ScreenResolutionMode>
{
    public int Width { get; init; }
    public int Height { get; init; }
    public string Ratio { get; init; }
    public int TotalPixels { get; init; }

    public ScreenResolutionMode()
    {
    }

    public ScreenResolutionMode(int width, int height, string ratio)
    {
        Width = width;
        Height = height;
        Ratio = ratio;
        TotalPixels = width * height;
    }

    public static bool operator <= (ScreenResolutionMode mode1, ScreenResolutionMode mode2)
    {
        return mode1.TotalPixels <= mode2.TotalPixels;
    }
    
    public static bool operator >= (ScreenResolutionMode mode1, ScreenResolutionMode mode2)
    {
        return mode1.TotalPixels >= mode2.TotalPixels;
    }

    public bool Equals(ScreenResolutionMode x, ScreenResolutionMode y)
    {
        return x.Width == y.Width && x.Height == y.Height;
    }

    public int GetHashCode(ScreenResolutionMode obj)
    {
        return HashCode.Combine(obj.Width, obj.Height);
    }

    public override bool Equals(object obj)
    {
        if (obj is ScreenResolutionMode resolution)
            return resolution.Equals(this);
            
        return false;
    }

    public override string ToString()
    {
        return $"{Width}x{Height}";
    }
}

#region MODEDATA
public static class ScreenResolutionRawData
{
    public static string AllModes = "width,height,aspect_ratio,total_pixels\n" +
                                    "640,480,4:3,307200\n" +
                                    "800,600,4:3,480000\n" +
                                    "854,480,16:9,409920\n" +
                                    "1024,768,4:3,786432\n" +
                                    "1280,720,16:9,921600\n" +
                                    "1280,800,16:10,1024000\n" +
                                    "1366,768,16:9,1049088\n" +
                                    "1440,900,16:10,1296000\n" +
                                    "1600,900,16:9,1440000\n" +
                                    "1400,1050,4:3,1470000\n" +
                                    "1680,1050,16:10,1764000\n" +
                                    "1600,1200,4:3,1920000\n" +
                                    "1920,1080,16:9,2073600\n" +
                                    "1920,1200,16:10,2304000\n" +
                                    "2560,1080,21:9,2764800\n" +
                                    "2048,1536,4:3,3145728\n" +
                                    "2560,1440,16:9,3686400\n" +
                                    "3840,1080,32:9,4147200\n" +
                                    "2560,1600,16:10,4096000\n" +
                                    "3440,1440,21:9,4953600\n" +
                                    "3200,1800,16:9,5760000\n" +
                                    "3840,1600,21:9,6144000\n" +
                                    "5120,1440,32:9,7372800\n" +
                                    "3840,2160,16:9,8294400\n" +
                                    "3840,2400,16:10,9216000\n" +
                                    "5120,2160,21:9,11059200\n" +
                                    "5120,2880,16:9,14745600\n" +
                                    "7680,2160,32:9,16588800\n" +
                                    "7680,4320,16:9,33177600\n";
}
#endregion
