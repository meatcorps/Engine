using Meatcorps.Engine.Core.Interfaces.Resource;

namespace Meatcorps.Engine.Core.Resource;

public class FallbackResource: IResource
{
    public bool Exists(string path)
    {
        return File.Exists(path);
    }

    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
    {
        return Directory.GetFiles(path, searchPattern, searchOption);
    }

    public string LoadText(string path)
    {
        return File.ReadAllText(path);
    }

    public byte[] LoadBytes(string path)
    {
        return File.ReadAllBytes(path);
    }
}