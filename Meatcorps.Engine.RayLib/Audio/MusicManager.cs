using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Tween;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Audio;

public sealed class MusicManager<TTrack> : IBackgroundService, IMasterVolume, IConfigChangeTracker, IDisposable
    where TTrack : struct, Enum
{
    private readonly Stack<Snapshot> _snapshotStack = new();

    private readonly Dictionary<TTrack, Music> _tracks = new();
    private Music? _current;
    private TTrack? _currentKey;
    private float _fadeSpeed = 1f;
    private bool _isDisposed;
    private bool _isFadingOut;
    private TTrack? _pendingKey;
    private float? _pendingSeekSeconds; // seek after swap if provided

    private float _volume = 1f;

    public MusicManager()
    {
        GlobalObjectManager.ObjectManager.Add<IConfigChangeTracker>(this);
    }

    public void PreUpdate(float deltaTime)
    {
    }

    public void Update(float deltaTime)
    {
        if (_current == null || _isDisposed) return;

        Raylib.UpdateMusicStream(_current.Value);

        if (_isFadingOut)
        {
            _volume -= _fadeSpeed * deltaTime;

            if (_volume <= 0f)
            {
                // swap to pending
                InternalStop();
                _isFadingOut = false;

                if (_pendingKey.HasValue && _tracks.TryGetValue(_pendingKey.Value, out var next))
                {
                    _current = next;
                    _currentKey = _pendingKey.Value;
                    _pendingKey = default;
                    _volume = 0f;

                    Raylib.PlayMusicStream(_current.Value);

                    if (_pendingSeekSeconds.HasValue)
                    {
                        Raylib.SeekMusicStream(_current.Value, _pendingSeekSeconds.Value);
                        _pendingSeekSeconds = null;
                    }

                    InternalSetVolume(_volume);
                }

                return;
            }

            InternalSetVolume(_volume);
            return;
        }

        if (_volume < 1f)
        {
            _volume += _fadeSpeed * deltaTime;

            if (_volume > 1f) _volume = 1f;

            InternalSetVolume(_volume);
        }
    }

    public void LateUpdate(float deltaTime)
    {
    }

    public void ConfigChanged(string group, string key, object value)
    {
        if (group != "Audio" && key != "MusicVolume")
            return;

        SetMasterVolume(Convert.ToSingle(value));
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        // Stop fades/overrides to avoid late updates tickling unloaded resources
        _isFadingOut = false;
        _pendingKey = default;
        _pendingSeekSeconds = null;
        _snapshotStack.Clear();

        if (_current != null)
        {
            Raylib.StopMusicStream(_current.Value);
            _current = null;
            _currentKey = default;
        }

        foreach (var kv in _tracks) Raylib.UnloadMusicStream(kv.Value);

        _tracks.Clear();
        _isDisposed = true;
    }

    public string Name => "Music";
    public float MasterVolume { get; private set; }

    public void SetMasterVolume(float volume)
    {
        MasterVolume = Math.Clamp(volume, 0f, 1f);

        InternalSetVolume();
    }

    // -----------------------
    // Public API
    // -----------------------

    public async Task Load(TTrack key, string filePath)
    {
        if (!_tracks.ContainsKey(key))
        {
            var m = await GlobalObjectManager.ObjectManager.Get<IRaylibResource>()!.LoadMusic(filePath);
            _tracks[key] = m;
        }
    }

    public MusicManager<TTrack> Play(TTrack key, float fadeSpeed = 1f)
    {
        return PlayInternal(key, fadeSpeed, null);
    }

    public MusicManager<TTrack> PlayAt(TTrack key, float startAtSeconds, float fadeSpeed = 1f)
    {
        return PlayInternal(key, fadeSpeed, startAtSeconds);
    }

    public MusicManager<TTrack> Stop()
    {
        InternalStop();
        _currentKey = default;
        _pendingKey = default;
        _pendingSeekSeconds = null;
        _isFadingOut = false;
        _volume = 0f;
        return this;
    }

    private void InternalStop()
    {
        if (_current != null) Raylib.StopMusicStream(_current.Value);

        _current = null;
    }

    public MusicManager<TTrack> Pause()
    {
        if (_current != null) Raylib.PauseMusicStream(_current.Value);

        return this;
    }

    public MusicManager<TTrack> Resume()
    {
        if (_current != null) Raylib.ResumeMusicStream(_current.Value);

        return this;
    }

    public MusicManager<TTrack> SetupSetMasterVolume(float volume)
    {
        SetMasterVolume(volume);

        return this;
    }

    public bool Has(TTrack key)
    {
        return _tracks.ContainsKey(key);
    }

    // -----------------------
    // Scoped override feature
    // -----------------------

    public IDisposable Override(TTrack tempKey, float fadeSpeed = 1f)
    {
        // snapshot current (if any)
        if (_current != null && _currentKey.HasValue)
        {
            var position = Raylib.GetMusicTimePlayed(_current.Value); // seconds
            var snap = new Snapshot
            {
                Key = _currentKey.Value,
                PositionSeconds = position,
                Volume = _volume
            };
            _snapshotStack.Push(snap);
        }

        PlayInternal(tempKey, fadeSpeed, null);
        return new RevertHandle(this, fadeSpeed);
    }

    private void InternalSetVolume(float? volume = null)
    {
        if (_current != null) Raylib.SetMusicVolume(_current.Value, Tween.Lerp(0, MasterVolume, volume ?? _volume));
    }

    private void RestoreTop(float fadeSpeed)
    {
        if (_snapshotStack.Count == 0) return;

        var snap = _snapshotStack.Pop();

        // Play previous and seek to exact moment we paused
        PlayInternal(snap.Key, fadeSpeed, snap.PositionSeconds);

        // Optionally restore the previous volume curve’s target;
        // current fade routine will ease us back toward 1.0 anyway.
    }

    // -----------------------
    // Internals
    // -----------------------

    private MusicManager<TTrack> PlayInternal(TTrack key, float fadeSpeed, float? startAtSeconds)
    {
        _fadeSpeed = MathF.Max(0.0001f, fadeSpeed);

        if (!_tracks.TryGetValue(key, out var next)) return this;

        if (_current == null)
        {
            _current = next;
            _currentKey = key;
            _volume = 0f;
            Raylib.PlayMusicStream(_current.Value);
            if (startAtSeconds.HasValue) Raylib.SeekMusicStream(_current.Value, startAtSeconds.Value);

            InternalSetVolume(_volume);
            return this;
        }

        if (_currentKey.HasValue && EqualityComparer<TTrack>.Default.Equals(_currentKey.Value, key))
        {
            // If caller asks to start at a specific time on the same track, seek immediately.
            if (startAtSeconds.HasValue) Raylib.SeekMusicStream(_current.Value, startAtSeconds.Value);

            return this;
        }

        _pendingKey = key;
        _pendingSeekSeconds = startAtSeconds;
        _isFadingOut = true;
        return this;
    }

    private struct Snapshot
    {
        public TTrack Key;
        public float PositionSeconds;
        public float Volume;
    }

    private sealed class RevertHandle : IDisposable
    {
        private readonly float _fadeSpeed;
        private MusicManager<TTrack>? _owner;

        public RevertHandle(MusicManager<TTrack> owner, float fadeSpeed)
        {
            _owner = owner;
            _fadeSpeed = fadeSpeed;
        }

        public void Dispose()
        {
            if (_owner == null) return;

            _owner.RestoreTop(_fadeSpeed);
            _owner = null;
        }
    }
}