namespace Meatcorps.Engine.Core.Utilities;

public static class FileUtilities
{
    public static string GetFullPath(params string[] fileName)
    {
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        return Path.Combine(appDirectory, Path.Combine(fileName));
    }
}