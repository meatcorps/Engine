using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Storage.Data;
using Meatcorps.Engine.RayLib.Audio;
using Meatcorps.Engine.RayLib.Interfaces;

namespace Meatcorps.Engine.RayLib.Resources;

public class MusicResource<T> : IResourceLoadOnInit, IAudioInitNeeded where T : struct, Enum
{
    private readonly Dictionary<T, (string path, float volume)> _music = new();
    private float _masterVolume;

    public MusicResource()
    {
        var config = GlobalObjectManager.ObjectManager.Get<IUniversalConfig>() ?? new BasicConfig();
        _masterVolume = config.GetOrDefault("Audio", "MusicVolume", 1f);
    }

    public int TotalResources => _music.Count;
    public int ResourcesLoaded { get; private set; }

    public async Task Load()
    {
        var resource = GlobalObjectManager.ObjectManager.Get<IRaylibResource>()!;
        var nonExisting = new List<string>();
        var manager = new MusicManager<T>();
        foreach (var (k, v) in _music)
            if (resource.Exists(v.path))
            {
                await manager.Load(k, v.path);
                ResourcesLoaded++;
                manager.SetMasterVolume(v.volume);
            }
            else
            {
                nonExisting.Add($"{k} -> {v.path} does not map to a file");
            }

        foreach (var e in Enum.GetValues<T>())
            if (!_music.ContainsKey(e))
                nonExisting.Add($"{e} is not mapped!");

        if (nonExisting.Any())
            throw new Exception("Missing music files: \n" + string.Join("\n ", nonExisting));

        _music.Clear();
        GlobalObjectManager.ObjectManager.RegisterList<IMasterVolume>();
        GlobalObjectManager.ObjectManager.Register(manager);
        GlobalObjectManager.ObjectManager.Add<IBackgroundService>(manager);
        GlobalObjectManager.ObjectManager.Add<IMasterVolume>(manager);
        manager.SetMasterVolume(_masterVolume);

        GlobalObjectManager.ObjectManager.Register(new VolumeManager());
    }

    public MusicResource<T> AddMusic(T k, string path, float volume = 1f)
    {
        _music[k] = (path, volume);
        return this;
    }

    public MusicResource<T> SetMasterVolume(float volume)
    {
        _masterVolume = volume;
        return this;
    }

    public MusicResource<T> UsePlaceHoldersForMissingFiles(string path = "Assets/PlaceHolders/music.mp3")
    {
        foreach (var e in Enum.GetValues<T>())
            if (!_music.ContainsKey(e))
                _music.Add(e, (path, 0f));

        return this;
    }

    public static MusicResource<T> Create()
    {
        return new MusicResource<T>();
    }
}