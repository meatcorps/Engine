namespace Meatcorps.Engine.Core.Interfaces.Resource;

public interface IResource
{
    bool Exists(string path);
    bool DirectoryExists(string path);
    string[] GetFiles(string path, string searchPattern, SearchOption searchOption);
    string LoadText(string path);
    byte[] LoadBytes(string path);
}