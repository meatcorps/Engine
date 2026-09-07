using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

// ReSharper disable NotAccessedField.Local

namespace Meatcorps.Engine.RayLib.Audio;

public sealed class MusicManager<TTrack> : IBackgroundService, IMasterVolume, IConfigChangeTracker, IDisposable
    where TTrack : struct, Enum
{
    public string Name => "Music";
    public float MasterVolume { get; private set; }
    public bool CrossFade { get; set; } = true;
    
    public bool IsPlaying => _current.Valid && _current.State == MusicState.Play;
    
    private MusicState _nextState;
    private bool _stateChanged;

    public void SetMasterVolume(float volume)
    {
        MasterVolume = Math.Clamp(volume, 0f, 1f);
        
        if (_next.Valid)
            _next.SetVolumeDirect(MasterVolume);
        else if (_current.Valid)
            _current.SetVolumeDirect(MasterVolume);
    }

    private readonly Dictionary<TTrack, Music> _tracks = new();
    private MusicHandle _current;
    private MusicHandle _next;
    private TTrack? _currentTrack = null;
    private TTrack? _nextTrack = null;
    
    private float _volume = 1f;

    private bool _isDisposed;

    public MusicManager()
    {
        GlobalObjectManager.ObjectManager.Add<IConfigChangeTracker>(this);
    }

    public MusicHandle GetHandle()
    {
        if (_next.Valid)
            return _next;
        return _current;
    }

    public MusicHandle CreateMusicHandle(TTrack key, float fadeTime = 1f, float startAtSeconds = 0f)
    {
        var handle = new MusicHandle(_tracks[key], fadeTime, startAtSeconds);
        handle.SetVolumeDirect(MasterVolume);
        return handle;
    }

    public void PreUpdate(float deltaTime)
    {
    }

    public void Update(float deltaTime)
    {
        if (_isDisposed) 
            return;

        if (_current.Valid)
            _current.Update(deltaTime);
        
        if (_next.Valid)
        {
            _next.Update(deltaTime);
            
            if ((!_current.Valid || _current.DoneFading))
            {
                _current.Dispose();
                _current = _next;
                _current.Volume = MasterVolume;
                _next = new MusicHandle();
                _current.State = _nextState;
                _currentTrack = _nextTrack;
                Console.WriteLine($"Switching to next song: {_nextState} {_currentTrack}");
                if (!Raylib.IsMusicValid(_current.Handle))
                    Console.WriteLine($"WARNING SONG NOT VALID: {_currentTrack}");
                    
                _nextTrack = null;
            }
        }
    }

    public void LateUpdate(float deltaTime)
    {
    }

    public void ConfigChanged(string group, string key, object value)
    {
        if (group != "Audio" || key != "MusicVolume")
            return;

        SetMasterVolume(Convert.ToSingle(value));
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
        PlayAt(key, 0f, fadeSpeed);
        return this;
    }

    public MusicManager<TTrack> PlayAt(TTrack key, float startAtSeconds, float fadeSpeed = 1f)
    {
        if (key.Equals(_currentTrack) && _current.Valid)
        {
            _current.State = MusicState.Play;
            return this;
        }
        if (key.Equals(_nextTrack) && _next.Valid)
        {
            _nextState = MusicState.Play;
            if (CrossFade)
                _next.State = MusicState.Play;
            return this;
        }
        _next = new MusicHandle(_tracks[key], fadeSpeed, startAtSeconds);
        _next.SetVolumeDirect(0);
        
        if (_current.Valid)
            _current.Volume = 0;
        
        _next.Volume = MasterVolume;
        _nextState = MusicState.Play;
        
        _next.State = CrossFade ? MusicState.Play : MusicState.Stop;
        _nextTrack = key;
        return this;
    }

    public MusicManager<TTrack> Stop()
    {
        if (_next.Valid)
        {
            if (CrossFade)
                _next.State = MusicState.Stop;
            
            _nextState = MusicState.Stop;
        }
        else
            _current.State = MusicState.Stop;
        
        return this;
    }

    public MusicManager<TTrack> Pause()
    {
        if (_next.Valid)
        {
            if (CrossFade)
                _next.State = MusicState.Paused;
            _nextState = MusicState.Paused;
        }
        else 
            _current.State = MusicState.Paused;

        return this;
    }

    public MusicManager<TTrack> Resume()
    {
        if (_next.Valid)
        {
            if (CrossFade)
                _next.State = MusicState.Resume;
            _nextState = MusicState.Resume;
        }
        else 
            _current.State = MusicState.Resume;
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

    public void Dispose()
    {
        if (_isDisposed) return;

        if (_current.Valid)
            _current.Dispose();

        foreach (var kv in _tracks) 
            Raylib.UnloadMusicStream(kv.Value);

        _tracks.Clear();
        _isDisposed = true;
    }
}

public struct MusicHandle : IDisposable
{
    public Music Handle;
    public MusicState State;
    private bool _isDisposed;
    private bool _initalized;
 
    public float TotalTime { get; private set; }

    public float CurrentTime
    {
        get => Raylib.GetMusicTimePlayed(Handle);
        private set => Raylib.SeekMusicStream(Handle, Math.Clamp(value, 0f, TotalTime));
    }

    public float Volume
    {
        get => _volume.RealValue;
        set => _volume.RealValue = value;
    }
    
    public bool DoneFading => _volume.IsAtRealValue;
    
    public float CurrentVolume => _volume.DisplayValue;
    
    public float CurrentTimeNormalized
    {
        get => Raylib.GetMusicTimePlayed(Handle) / TotalTime;
        private set => Raylib.SeekMusicStream(Handle, Math.Clamp(value * TotalTime, 0f, TotalTime));
    }

    private float _pan;
    public float Pan
    {
        get => _pan;
        set
        {
            _pan = Math.Clamp(value, -1f, 1f);
            Raylib.SetMusicPan(Handle, _pan);
        }
    }

    private float _pitch;
    public float Pitch
    {
        get => _pitch;
        set
        {
            _pitch = value;
            Raylib.SetMusicPitch(Handle, value);
            TotalTime = Raylib.GetMusicTimeLength(Handle);
        }
    }
    
    public bool Valid => _initalized && !_isDisposed;
    
    private SmoothValue _volume;
    private float _currentVolume;
    private bool _isPaused;

    public MusicHandle()
    {
        _initalized = false;
        _isDisposed = true;
        //
    }
    
    public MusicHandle(Music handle, float fadeSpeed, float startFrom = 0f)
    {
        Handle = handle;
        _volume = new SmoothValue(fadeSpeed);
        TotalTime = Raylib.GetMusicTimeLength(Handle);
        CurrentTime = startFrom;
        _initalized = true;
        Pitch = 1f;
        Pan = 1f;
    }

    public void SetVolumeDirect(float volume)
    {
        _volume.RealValue = volume;
        _volume.SnapToReal();
    }

    public void Update(float deltaTime)
    {
        if (!Raylib.IsMusicValid(Handle) || _isDisposed)
            return;
        
        Raylib.UpdateMusicStream(Handle);
        
        if (State is MusicState.Play or MusicState.Resume)
            _volume.Update(deltaTime);
        
        _volume.RealValue = Math.Clamp(Volume, 0f, 1f);

        if (!_currentVolume.EqualsSafe(_volume.DisplayValue))
        {
            _currentVolume = _volume.DisplayValue;
            Raylib.SetMusicVolume(Handle, _currentVolume);
        }
        
        switch (State)
        {
            case MusicState.Play:
                if (!Raylib.IsMusicStreamPlaying(Handle))
                {
                    Raylib.PlayMusicStream(Handle);
                    Raylib.SetMusicVolume(Handle, _currentVolume);
                }

                _isPaused = false;
                break;
            case MusicState.Resume:
                if (!Raylib.IsMusicStreamPlaying(Handle))
                {
                    if (_isPaused)
                        Raylib.ResumeMusicStream(Handle);
                    else
                        Raylib.PlayMusicStream(Handle);
                    
                    Raylib.SetMusicVolume(Handle, _currentVolume);
                }

                _isPaused = false;
                break;
            case MusicState.Paused:
                _isPaused = true;
                if (Raylib.IsMusicStreamPlaying(Handle))
                    Raylib.PauseMusicStream(Handle);
                break;
            case MusicState.Stop:
                
                if (Raylib.IsMusicStreamPlaying(Handle))
                    Raylib.StopMusicStream(Handle);
                
                _isPaused = false;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void Dispose()
    {
        _isDisposed = true;
        if (Raylib.IsMusicValid(Handle))
            Raylib.StopMusicStream(Handle);
    }
}

public enum MusicState
{
    Play,
    Resume,
    Paused,
    Stop
}