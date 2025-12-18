using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Resources;

public class FileResourceLoader : IRaylibResource
{
    private bool _isResourceManagerLoaded;
    private ResourceManager _resourceManager = null!;

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

    public async Task<Image> LoadImage(string path)
    {
        LoadResourceManager();
        Image image = default;
        await _resourceManager.AddTaskToMainThread(() => image = Raylib.LoadImage(path));
        return image;
    }

    public async Task<Texture2D> LoadTexture(string path)
    {
        LoadResourceManager();
        Texture2D texture2D = default;
        await _resourceManager.AddTaskToMainThread(() => texture2D = Raylib.LoadTexture(path));
        return texture2D;
    }

    public async Task<Font> LoadFontEx(string fileName, int fontSize, int[]? codepoints, int codepointCount)
    {
        LoadResourceManager();
        Font font = default;
        await _resourceManager.AddTaskToMainThread(() =>
            font = Raylib.LoadFontEx(fileName, fontSize, codepoints, codepointCount));
        return font;
    }

    public async Task<Wave> LoadWave(string path)
    {
        LoadResourceManager();
        Wave wave = default;
        await _resourceManager.AddTaskToMainThread(() => wave = Raylib.LoadWave(path));
        return wave;
    }

    public async Task<Sound> LoadSound(string path)
    {
        LoadResourceManager();
        Sound sound = default;
        await _resourceManager.AddTaskToMainThread(() => sound = Raylib.LoadSound(path));
        return sound;
    }

    public async Task<Music> LoadMusic(string path)
    {
        LoadResourceManager();
        Music music = default;
        await _resourceManager.AddTaskToMainThread(() => music = Raylib.LoadMusicStream(path));
        return music;
    }

    public async Task<Model> LoadModel(string path)
    {
        LoadResourceManager();
        Model model = default;
        await _resourceManager.AddTaskToMainThread(() => model = Raylib.LoadModel(path));
        return model;
    }

    public async Task<Shader> LoadShader(string? vertexPath, string? fragmentPath)
    {
        LoadResourceManager();
        Shader shader = default;
        await _resourceManager.AddTaskToMainThread(() => shader = Raylib.LoadShader(
            string.IsNullOrWhiteSpace(vertexPath) ? null : vertexPath,
            string.IsNullOrWhiteSpace(fragmentPath) ? null : fragmentPath
        ));
        return shader;
    }

    public string LoadText(string path)
    {
        return File.ReadAllText(path);
    }

    public byte[] LoadBytes(string path)
    {
        return File.ReadAllBytes(path);
    }

    private void LoadResourceManager()
    {
        if (_isResourceManagerLoaded)
            return;

        _isResourceManagerLoaded = true;

        _resourceManager = GlobalObjectManager.ObjectManager.Get<ResourceManager>()!;
    }
}