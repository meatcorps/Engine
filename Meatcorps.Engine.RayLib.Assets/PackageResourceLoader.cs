using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Meatcorps.Engine.Assets.Manager;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Assets;

public class PackageResourceLoader: IRaylibResource
{
    private readonly AssetPackageManager _packageManager;

    public PackageResourceLoader(AssetPackageManager packageManager)
    {
        _packageManager = packageManager;
        _packageManager.Load();
    }
    
    public bool Exists(string path)
    {
        var exist =  _packageManager.Exists(path);
        return exist;
    }

    public bool DirectoryExists(string path)
    {
        var exist = _packageManager.DirectoryExists(path);
        return exist;
    }

    public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
    {
        var files =  _packageManager.GetFiles(path).ToArray();
        return files;
    }

    public string LoadText(string path)
    {
        if (!_packageManager.Data(out var data, path))
            return string.Empty;
        
        return Encoding.UTF8.GetString(data);
    }

    public byte[] LoadBytes(string path)
    {
        if (!_packageManager.Data(out var data, path))
            return Array.Empty<byte>();
        
        return data;
    }

    public Image LoadImage(string path)
    {
        var byteData = LoadBytes(path);
        return Raylib.LoadImageFromMemory(Path.GetExtension(path), byteData);
    }

    public Texture2D LoadTexture(string path)
    {
        var image = LoadImage(path);
        var texture2D = Raylib.LoadTextureFromImage(image);
        Raylib.UnloadImage(image);
        return texture2D;
    }

    public Font LoadFontEx(string fileName, int fontSize, int[]? codepoints, int codepointCount)
    {
        var byteData = LoadBytes(fileName);
        return Raylib.LoadFontFromMemory(Path.GetExtension(fileName),  byteData, fontSize, codepoints, codepointCount);
    }

    public Wave LoadWave(string path)
    {
        var byteData = LoadBytes(path);
        return Raylib.LoadWaveFromMemory(Path.GetExtension(path), byteData);
    }

    public Sound LoadSound(string path)
    {
        var wave = LoadWave(path);
        var sound = Raylib.LoadSoundFromWave(wave);
        Raylib.UnloadWave(wave);
        return sound;
    }

    public Music LoadMusic(string path)
    {
        var byteData = LoadBytes(path);
        GCHandle.Alloc(byteData, GCHandleType.Pinned);
        return Raylib.LoadMusicStreamFromMemory(Path.GetExtension(path), byteData);
    }

    public Model LoadModel(string path)
    {
        throw new NotImplementedException();
    }

    public Shader LoadShader(string? vertexPath, string? fragmentPath)
    {
        string? vertexData = null;
        string? fragmentData = null;

        if (vertexPath != null)
        {
            var byteData = LoadBytes(vertexPath);
            vertexData = Encoding.UTF8.GetString(byteData);
        }
        
        if (fragmentPath != null)
        {
            var byteData = LoadBytes(fragmentPath);
            fragmentData = Encoding.UTF8.GetString(byteData);
        }
            
        return Raylib.LoadShaderFromMemory(vertexData, fragmentData);
    }
    
    
}