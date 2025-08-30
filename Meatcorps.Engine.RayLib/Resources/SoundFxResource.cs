using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Audio;
using Meatcorps.Engine.RayLib.Interfaces;

namespace Meatcorps.Engine.RayLib.Resources;

public class SoundFxResource<T> : ILoadAfterRayLibInit, IAudioInitNeeded where T : struct, Enum
{
    private readonly int _poolSize;
    private string _name;
    private float _masterVolume;
    private readonly Dictionary<T, string> _sound = new();

    public SoundFxResource(int poolSize = 4, string name = "SoundFx", float masterVolume = 1f)
    {
        _poolSize = poolSize;
        _name = name;
        _masterVolume = masterVolume;
    }

    public SoundFxResource<T> AddSound(T sound, string path)
    {
        _sound[sound] = path;
        return this;
    }

    public SoundFxResource<T> SetMasterVolume(float volume)
    {
        _masterVolume = volume;
        return this;   
    }

    public SoundFxResource<T> SetName(string name)
    {
        _name = name;
        return this;   
    }

    public SoundFxResource<T> UsePlaceHoldersForMissingFiles(string path = "Assets/PlaceHolders/sound.wav")
    {
        foreach (var e in Enum.GetValues<T>())
        {
            if (!_sound.ContainsKey(e))
                _sound.Add(e, path);
        }

        return this;
    }

    public void Load()
    {
        var nonExisting = new List<string>();
        var manager = new SoundFxManager<T>(_poolSize, _name);
        foreach (var sound in _sound)
        {
            if (File.Exists(sound.Value))
                manager.Load(sound.Key, sound.Value);
            else
                nonExisting.Add($"{sound.Key} -> {sound.Value} does not map to a file");
        }

        foreach (var e in Enum.GetValues<T>())
        {
            if (!_sound.ContainsKey(e))
                nonExisting.Add($"{e} is not mapped!");
        }

        if (nonExisting.Any())
            throw new Exception("Missing sound files: \n" + string.Join("\n ", nonExisting));

        _sound.Clear();
        GlobalObjectManager.ObjectManager.RegisterList<IMasterVolume>();
        GlobalObjectManager.ObjectManager.Register<SoundFxManager<T>>(manager);
        GlobalObjectManager.ObjectManager.Add<IBackgroundService>(manager);
        GlobalObjectManager.ObjectManager.Add<IMasterVolume>(manager);
        manager.SetMasterVolume(_masterVolume);
        GlobalObjectManager.ObjectManager.Register(new VolumeManager());
    }

    public static SoundFxResource<T> Create(int poolSize = 4)
    {
        return new SoundFxResource<T>(poolSize);
    }
}