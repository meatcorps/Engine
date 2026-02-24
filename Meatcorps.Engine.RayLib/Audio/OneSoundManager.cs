using Meatcorps.Engine.Core.Tween;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Audio;

public class OneSoundManager : IDisposable
{
    private readonly IMasterVolume _masterVolume;
    private readonly bool _sharedSound;
    private readonly Sound _sound;
    private bool _isDisposed;
    private bool _isPaused;
    private float _pitch = 1;
    private float _targetVolume;
    private float _volume;

    public OneSoundManager(IMasterVolume masterVolume, Sound sound, bool sharedSound, float volume = 1f)
    {
        _masterVolume = masterVolume;
        _sound = sound;
        _sharedSound = sharedSound;
        _volume = volume;
        _targetVolume = volume;
    }

    public float Volume
    {
        get => _volume;
        set
        {
            if (_isDisposed)
                return;
            _volume = Math.Clamp(value, 0, 1);
            Raylib.SetSoundVolume(_sound, Tween.Lerp(0, _masterVolume.MasterVolume, _volume));
        }
    }

    public float Pitch
    {
        get => _pitch;
        set
        {
            if (_isDisposed)
                return;
            _pitch = Math.Clamp(value, 0.1f, 4f);
            Raylib.SetSoundPitch(_sound, _pitch);
        }
    }

    public bool Pause
    {
        get => _isPaused;
        set
        {
            if (_isDisposed)
                return;
            _isPaused = value;
            if (_isPaused)
                Raylib.PauseSound(_sound);
            else
                Raylib.ResumeSound(_sound);
        }
    }

    public bool Repeat { get; set; }

    public bool IsPlaying
    {
        get
        {
            if (_isDisposed)
                return false;
            return Raylib.IsSoundPlaying(_sound);
        }
    }


    public void Dispose()
    {
        if (_isDisposed) return;

        // Stop first (safe on non‑playing too)
        Raylib.StopSound(_sound);

        if (!_sharedSound) Raylib.UnloadSoundAlias(_sound);

        _isDisposed = true;
    }

    public void SetVolumeSmooth(float v)
    {
        _targetVolume = Math.Clamp(v, 0f, 1f);
    }

    public bool UnRegisterFromReservation(out nint pointer)
    {
        if (!_sharedSound)
        {
            pointer = 0;
            return false;
        }

        pointer = _sound.Stream.Buffer;
        return true;
    }

    public void Play(bool? loop = null)
    {
        if (_isDisposed)
            return;

        if (loop.HasValue)
            Repeat = loop.Value;

        Raylib.SetSoundVolume(_sound, Tween.Lerp(0, _masterVolume.MasterVolume, _volume));
        Raylib.SetSoundPitch(_sound, _pitch);
        Raylib.PlaySound(_sound);
    }

    public void Stop()
    {
        if (_isDisposed)
            return;
        Raylib.StopSound(_sound);
    }


    public void Update(float deltaTime = 0f)
    {
        if (_isDisposed)
            return;

        var isPlaying = IsPlaying;

        if (isPlaying)
        {
            if (_volume <= 0.001f && IsPlaying)
                Raylib.PauseSound(_sound);

            if (Math.Abs(_targetVolume - _volume) > 0.001f)
                Volume = Tween.Lerp(_volume, _targetVolume, Math.Clamp(deltaTime * 8f, 0f, 1f));
        }
        else
        {
            if (Repeat && !Pause)
                Play();
        }
    }
}