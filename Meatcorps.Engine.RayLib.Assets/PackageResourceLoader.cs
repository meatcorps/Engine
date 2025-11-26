using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Meatcorps.Engine.Assets.Manager;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.Resources;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Assets;

public class PackageResourceLoader: IRaylibResource
{
    private readonly AssetPackageManager _packageManager;
    private ResourceManager _resourceManager = null!;
    private bool _isResourceManagerLoaded = false;
    
    public PackageResourceLoader(AssetPackageManager packageManager)
    {
        _packageManager = packageManager;
        _packageManager.Load();
    }

    private void LoadResourceManager()
    {
        if (_isResourceManagerLoaded)
            return;

        _isResourceManagerLoaded = true;
        
        _resourceManager = GlobalObjectManager.ObjectManager.Get<ResourceManager>()!;
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

    public async Task<Image> LoadImage(string path)
    {
        LoadResourceManager();
        var byteData = LoadBytes(path);
        Image image = default;
        await _resourceManager.AddTaskToMainThread(() => image = Raylib.LoadImageFromMemory(Path.GetExtension(path), byteData));
        return image;
    }

    public async Task<Texture2D> LoadTexture(string path)
    {
        LoadResourceManager();
        var image = await LoadImage(path);
        Texture2D texture2D = default;
        await _resourceManager.AddTaskToMainThread(() =>
        {
            texture2D = Raylib.LoadTextureFromImage(image);
            Raylib.UnloadImage(image);
        });
        return texture2D;
    }

    public async Task<Font> LoadFontEx(string fileName, int fontSize, int[]? codepoints, int codepointCount)
    {
        LoadResourceManager();
        var byteData = LoadBytes(fileName);
        Font font = default;
        await _resourceManager.AddTaskToMainThread(() => font = Raylib.LoadFontFromMemory(Path.GetExtension(fileName),  byteData, fontSize, codepoints, codepointCount));
        return font; 
    }

    public async Task<Wave> LoadWave(string path)
    {
        LoadResourceManager();
        var byteData = LoadBytes(path);
        Wave wave = default;
        await _resourceManager.AddTaskToMainThread(() => wave = Raylib.LoadWaveFromMemory(Path.GetExtension(path), byteData));
        return wave;
    }

    public async Task<Sound> LoadSound(string path)
    {
        LoadResourceManager();
        var wave = await LoadWave(path);
        Sound sound = default;
        
        await _resourceManager.AddTaskToMainThread(() =>
        {
            sound = Raylib.LoadSoundFromWave(wave);
            Raylib.UnloadWave(wave);
        });
        
        return sound;
    }

    public async Task<Music> LoadMusic(string path)
    {
        LoadResourceManager();
        var byteData = LoadBytes(path);
        GCHandle.Alloc(byteData, GCHandleType.Pinned);
        
        Music music = default;
        await _resourceManager.AddTaskToMainThread(() =>
        {
            music = Raylib.LoadMusicStreamFromMemory(Path.GetExtension(path), byteData);
        });
        
        return music;
    }

    public Task<Model> LoadModel(string path)
    {
        throw new NotImplementedException();
    }

    public async Task<Shader> LoadShader(string? vertexPath, string? fragmentPath)
    {
        LoadResourceManager();
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

        Shader shader = default; 
        await _resourceManager.AddTaskToMainThread(() =>
        {
            shader = Raylib.LoadShaderFromMemory(vertexData, fragmentData);
        });
        return shader;
    }
    
    
}