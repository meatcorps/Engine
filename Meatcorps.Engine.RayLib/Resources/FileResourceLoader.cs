using System.Runtime.InteropServices;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Resources;

public class FileResourceLoader: IRaylibResource
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

    public Image LoadImage(string path)
    {
        return Raylib.LoadImage(path);
    }

    public Texture2D LoadTexture(string path)
    {
        return Raylib.LoadTexture(path);
    }

    public Font LoadFontEx(string fileName, int fontSize, int[]? codepoints, int codepointCount)
    {
        return Raylib.LoadFontEx(fileName, fontSize, codepoints, codepointCount);;
    }

    public Wave LoadWave(string path)
    {
        return Raylib.LoadWave(path);
    }

    public Sound LoadSound(string path)
    {
        return Raylib.LoadSound(path);
    }

    public Music LoadMusic(string path)
    {
        return Raylib.LoadMusicStream(path);
    }

    public Model LoadModel(string path)
    {
        return Raylib.LoadModel(path);
    }

    public Shader LoadShader(string? vertexPath, string? fragmentPath)
    {
        return Raylib.LoadShader(
            string.IsNullOrWhiteSpace(vertexPath) ? null : vertexPath,
            string.IsNullOrWhiteSpace(fragmentPath) ? null : fragmentPath
        );
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