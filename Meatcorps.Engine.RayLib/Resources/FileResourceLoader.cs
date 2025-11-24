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

    public Task<Image> LoadImage(string path)
    {
        throw new NotImplementedException();
        //return Raylib.LoadImage(path);
    }

    public Task<Texture2D> LoadTexture(string path)
    {
        throw new NotImplementedException();
        //return Raylib.LoadTexture(path);
    }

    public Task<Font> LoadFontEx(string fileName, int fontSize, int[]? codepoints, int codepointCount)
    {
        throw new NotImplementedException();
        //return Raylib.LoadFontEx(fileName, fontSize, codepoints, codepointCount);;
    }

    public Task<Wave> LoadWave(string path)
    {
        
        throw new NotImplementedException();
        //return Raylib.LoadWave(path);
    }

    public Task<Sound> LoadSound(string path)
    {
        throw new NotImplementedException();
        //return Raylib.LoadSound(path);
    }

    public Task<Music> LoadMusic(string path)
    {
        
        throw new NotImplementedException();
        //return Raylib.LoadMusicStream(path);
    }

    public Task<Model> LoadModel(string path)
    {
        throw new NotImplementedException();
        //return Raylib.LoadModel(path);
    }

    public Task<Shader> LoadShader(string? vertexPath, string? fragmentPath)
    {
        throw new NotImplementedException();
        
        /*return Raylib.LoadShader(
            string.IsNullOrWhiteSpace(vertexPath) ? null : vertexPath,
            string.IsNullOrWhiteSpace(fragmentPath) ? null : fragmentPath
        );*/
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