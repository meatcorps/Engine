using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Interfaces;

namespace Meatcorps.Engine.RayLib.Audio;

public sealed class VolumeManager : IBackgroundService
{
    private bool _initialized;
    private readonly Dictionary<string, IMasterVolume> _buses =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, float> _cache =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, bool> _muted =
        new(StringComparer.OrdinalIgnoreCase);

    private bool _muteAll;
    private bool _muteAllCache;

    public bool MuteAll
    {
        get => _muteAll;
        set
        {
            if (!_initialized)
            {
                _muteAllCache = value;
                return;
            }

            if (_muteAll == value)
                return;

            foreach (var (name, _) in _buses)
                MuteMasterVolume(name, value);

            _muteAll = value;
        }
    }

    public VolumeManager(bool muteAll = false)
    {
        _muteAllCache = muteAll;
    }

    public void ToggleMuteAll() => MuteAll = !MuteAll;

    public void PreUpdate(float deltaTime)
    {
        if (_initialized)
            return;

        var buses = GlobalObjectManager.ObjectManager.GetList<IMasterVolume>();
        if (buses != null)
        {
            foreach (var bus in buses)
            {
                if (!_buses.TryAdd(bus.Name, bus))
                    continue;

                _muted[bus.Name] = false;
                _cache[bus.Name] = Math.Clamp(bus.MasterVolume, 0f, 1f);
            }
        }

        _initialized = true;
        MuteAll = _muteAllCache; // apply cached global mute once
    }

    public IEnumerable<(string name, float volume, bool muted)> Enumerate()
    {
        foreach (var (name, vol) in _cache)
            yield return (name, vol, _muted[name]);
    }

    public void SetMasterVolume(string name, float volume)
    {
        if (!_buses.TryGetValue(name, out var bus))
            throw new Exception($"No master volume named '{name}'");

        volume = Math.Clamp(volume, 0f, 1f);
        _cache[name] = volume;

        if (!_muteAll && !_muted[name])
            bus.SetMasterVolume(volume); // bus will lerp internally
    }

    public float GetMasterVolume(string name)
    {
        if (!_cache.TryGetValue(name, out var v))
            throw new Exception($"No master volume named '{name}'");
        return v;
    }

    public void MuteMasterVolume(string name, bool mute)
    {
        if (!_buses.TryGetValue(name, out var bus))
            throw new Exception($"No master volume named '{name}'");

        _muted[name] = mute;
        if (mute)
        {
            _cache[name] = bus.MasterVolume;
            bus.SetMasterVolume(0f);
        }
        else
        {
            bus.SetMasterVolume(_cache[name]); // bus lerps back up
        }
    }

    public bool IsMuted(string name)
    {
        if (!_muted.TryGetValue(name, out var m))
            throw new Exception($"No master volume named '{name}'");
        return m;
    }

    public void ToggleMute(string name) => MuteMasterVolume(name, !IsMuted(name));

    // Convenience Try* helpers
    public bool TrySetMasterVolume(string name, float volume)
    {
        if (!_buses.ContainsKey(name)) return false;
        SetMasterVolume(name, volume);
        return true;
    }
    
    public bool TryGetMasterVolume(string name, out float volume)
    {
        if (!_cache.TryGetValue(name, out volume)) return false;
        return true;
    }
    
    public bool TryMuteMasterVolume(string name, bool mute)
    {
        if (!_buses.ContainsKey(name)) return false;
        MuteMasterVolume(name, mute);
        return true;
    }
    
    public bool TryIsMuted(string name, out bool isMuted)
    {
        if (!_muted.TryGetValue(name, out isMuted)) return false;
        return true;
    }
    
    public bool TryToggleMute(string name)
    {
        if (!_muted.ContainsKey(name)) return false;
        ToggleMute(name);
        return true;
    }

    // Simple duck scope (temporary dip)
    public IDisposable Duck(string name, float toVolume)
    {
        var prev = GetMasterVolume(name);
        SetMasterVolume(name, toVolume);
        return new Scope(() => SetMasterVolume(name, prev));
    }

    public void Update(float deltaTime) { }
    public void LateUpdate(float deltaTime) { }

    private sealed class Scope : IDisposable
    {
        private Action? _onDispose;
        public Scope(Action onDispose) { _onDispose = onDispose; }
        public void Dispose() { _onDispose?.Invoke(); _onDispose = null; }
    }
}