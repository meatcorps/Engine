using System.Collections.Concurrent;
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

    public int TotalResources { get; private set; }
    public int ResourcesLoaded => _taskDone.Count;

    private ConcurrentBag<bool> _taskDone = new();
    
    public async Task Load()
    {
        var resource = GlobalObjectManager.ObjectManager.Get<IRaylibResource>()!;
        var nonExisting = new List<string>();
        var manager = new MusicManager<T>();
        _taskDone.Clear();
        
        var tasks = new List<Task>();
        
        foreach (var (k, v) in _music)
        {
            if (resource.Exists(v.path))
            {
                tasks.Add(Task.Run(async () =>
                {
                    await manager.Load(k, v.path);
                    _taskDone.Add(true);
                })); 
            }
            else
            {
                nonExisting.Add($"{k} -> {v.path} does not map to a file");
                tasks.Add(Task.Run(async () =>
                {
                    await manager.Load(k, Path.Combine("Assets", "PlaceHolders", "music.mp3"));
                    _taskDone.Add(true);
                })); 
            }
        }
        
        await Task.WhenAll(tasks);

        foreach (var e in Enum.GetValues<T>())
            if (!_music.ContainsKey(e)) 
                nonExisting.Add($"{e} is not mapped!");

        if (nonExisting.Any())
            Console.WriteLine("Missing music files: Using failback " + Path.Combine("Assets", "PlaceHolders", "music.mp3") + ": \n" + string.Join("\n ", nonExisting));

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
        TotalResources = _music.Count;
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
            {
                _music.Add(e, (path, 0f));
                TotalResources = _music.Count;
            }

        return this;
    }

    public static MusicResource<T> Create()
    {
        return new MusicResource<T>();
    }
}