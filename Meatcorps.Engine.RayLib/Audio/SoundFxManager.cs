using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.Resources;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Audio;

public sealed class SoundFxManager<TSfx> : IBackgroundService, IMasterVolume, IConfigChangeTracker, IDisposable
    where TSfx : struct, Enum
{
    private readonly int _poolSizePerSfx;
    private readonly HashSet<nint> _reserved = new();
    private readonly Dictionary<TSfx, string> _soundLocations = new();
    private readonly List<OneSoundManager> _soundManagers = new();
    private readonly Dictionary<TSfx, List<Sound>> _soundPools = new();
    private bool _isDisposed;
    private object _lock;

    public SoundFxManager(int poolSizePerSfx = 4, string name = "SoundEffects")
    {
        Name = name;
        _poolSizePerSfx = Math.Max(1, poolSizePerSfx);
        GlobalObjectManager.ObjectManager.Add<IConfigChangeTracker>(this);
    }

    public void PreUpdate(float deltaTime)
    {
    }

    public void Update(float deltaTime)
    {
        foreach (var manager in _soundManagers)
            manager.Update();
    }

    public void LateUpdate(float deltaTime)
    {
    }

    public void ConfigChanged(string group, string key, object value)
    {
        if (group != "Audio" && key != "SoundFxVolume")
            return;

        SetMasterVolume(Convert.ToSingle(value));
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        foreach (var manager in _soundManagers)
        {
            if (manager.UnRegisterFromReservation(out var pointer))
                _reserved.Remove(pointer);
            manager.Dispose();
        }

        foreach (var pool in _soundPools.Values)
        {
            var count = 0;
            foreach (var s in pool)
            {
                // Stop before unload just in case
                Raylib.StopSound(s);
                if (count == 0)
                    Raylib.UnloadSound(s);
                else
                    Raylib.UnloadSoundAlias(s);

                count++;
            }
        }

        _soundManagers.Clear();
        _soundPools.Clear();
        _isDisposed = true;
    }

    public string Name { get; }
    public float MasterVolume { get; private set; } = 1f;

    public void SetMasterVolume(float volume)
    {
        MasterVolume = Math.Clamp(volume, 0f, 1f);
    }

    public async Task Load(TSfx key, string filePath, Action? afterLoad = null)
    {
        _soundLocations.Add(key, filePath);
        if (!_soundPools.ContainsKey(key))
        {
            var pool = new List<Sound>(_poolSizePerSfx);
            for (var i = 0; i < _poolSizePerSfx; i++)
            {
                Sound s = default;

                if (i == 0)
                    s = await GlobalObjectManager.ObjectManager.Get<IRaylibResource>()!.LoadSound(_soundLocations[key]);
                else
                    await GlobalObjectManager.ObjectManager.Get<ResourceManager>()!.AddTaskToMainThread(() =>
                        s = Raylib.LoadSoundAlias(pool[0]));

                if (!Raylib.IsSoundValid(s))
                    throw new Exception($"Failed to load sound {filePath}");

                pool.Add(s);
            }

            afterLoad?.Invoke();

            _soundPools[key] = pool;
        }
    }

    public SoundFxManager<TSfx> Unload(TSfx key)
    {
        if (_soundPools.TryGetValue(key, out var pool))
        {
            foreach (var s in pool) Raylib.UnloadSound(s);

            _soundPools.Remove(key);
        }

        return this;
    }

    public SoundFxManager<TSfx> SetMasterVolumeSetup(float volume)
    {
        SetMasterVolume(volume);
        return this;
    }

    public SoundFxManager<TSfx> Play(TSfx key, float? volumeOverride = null, float? pitch = null)
    {
        if (!GetSound(key, out var s))
            return this;

        var vol = Math.Clamp(volumeOverride ?? MasterVolume, 0f, MasterVolume);

        if (pitch.HasValue)
            Raylib.SetSoundPitch(s, pitch.Value);

        Raylib.SetSoundVolume(s, vol);
        Raylib.PlaySound(s);
        return this;
    }

    public bool IsPlaying(TSfx key)
    {
        if (!_soundPools.TryGetValue(key, out var pool))
            return false;

        foreach (var sound in pool)
            if (Raylib.IsSoundPlaying(sound))
                return true;

        return false;
    }

    public OneSoundManager GetOneSoundManager(TSfx key, float volume = 1, bool fromSoundPool = true)
    {
        Sound sound;
        if (fromSoundPool)
        {
            GetSound(key, out sound);
            _reserved.Add(sound.Stream.Buffer);
        }
        else
        {
            GetSound(key, out var soundOriginal);
            sound = Raylib.LoadSoundAlias(soundOriginal);
            //   sound = GlobalObjectManager.ObjectManager.Get<IRaylibResource>()!.LoadSound(_soundLocations[key]);
        }

        var manager = new OneSoundManager(this, sound, fromSoundPool, volume);
        _soundManagers.Add(manager);
        return manager;
    }

    public void RemoveOneSoundManager(OneSoundManager manager)
    {
        manager.Dispose();
        if (manager.UnRegisterFromReservation(out var pointer))
            _reserved.Remove(pointer);

        _soundManagers.Remove(manager);
    }

    private bool GetSound(TSfx key, out Sound sound)
    {
        sound = default;
        if (!_soundPools.TryGetValue(key, out var pool)) return false;

        // Find a sound not currently playing
        foreach (var s in pool)
            if (!Raylib.IsSoundPlaying(s) && !_reserved.Contains(s.Stream.Buffer))
            {
                sound = s;
                return true;
            }

        // If all are busy, force-play the first one
        var first = pool[0];
        if (_reserved.Contains(first.Stream.Buffer))
            return false;

        Raylib.StopSound(first);
        sound = first;
        return true;
    }

    public bool Has(TSfx key)
    {
        return _soundPools.ContainsKey(key);
    }

    public SoundFxManager<TSfx> Stop(TSfx key)
    {
        if (!_soundPools.TryGetValue(key, out var s)) return this;

        foreach (var sound in s)
            Raylib.StopSound(sound);
        return this;
    }
}