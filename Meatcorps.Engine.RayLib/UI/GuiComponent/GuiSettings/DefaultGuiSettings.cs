using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Audio;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.UI.GuiComponent.GuiSettings;

public class DefaultGuiSettings<TGameInput, TGameAudio> : IGuiSettings, IResourceLoadOnInit
    where TGameAudio : struct, Enum
    where TGameInput : Enum
{
    private readonly bool _enableSound = true;
    private SoundFxManager<TGameAudio> _audioManager = null!;
    private PlayerInputRouter<TGameInput> _inputRouter = null!;

    public DefaultGuiSettings()
    {
        GlobalObjectManager.ObjectManager.Register<IGuiSettings>(this);
    }

    public TGameAudio? SelectionSound { get; set; } = null;
    public TGameAudio? NavigationSound { get; set; } = null;
    public TGameAudio? ErrorSound { get; set; } = null;
    public TGameAudio? NotificationSound { get; set; } = null;

    public int PlayerInputId { get; set; } = 1;
    public TGameInput? UpKey { get; set; } = default;
    public TGameInput? DownKey { get; set; } = default;
    public TGameInput? LeftKey { get; set; } = default;
    public TGameInput? RightKey { get; set; } = default;
    public TGameInput? OnSelectionPressed { get; set; } = default;
    public TGameInput? BackPressed { get; set; } = default;

    public Font Font { get; set; }

    public bool IsDownPressed => GetButton(DownKey);
    public bool IsUpPressed => GetButton(UpKey);
    public bool IsLeftPressed => GetButton(LeftKey);
    public bool IsRightPressed => GetButton(RightKey);
    public bool IsOnSelectionPressed => GetButton(OnSelectionPressed);
    public bool IsBackPressed => GetButton(BackPressed);

    public void PlaySelectionSound(float volume = 1)
    {
        PlaySound(SelectionSound, volume);
    }

    public void PlayNavigationSound(float volume = 1)
    {
        PlaySound(NavigationSound, volume);
    }

    public void PlayErrorSound(float volume = 1)
    {
        PlaySound(ErrorSound, volume);
    }

    public void PlayNotificationSound(float volume = 1)
    {
        PlaySound(NotificationSound, volume);
    }

    public int TotalResources => 0;
    public int ResourcesLoaded => 0;

    public Task Load()
    {
        _audioManager = GlobalObjectManager.ObjectManager.Get<SoundFxManager<TGameAudio>>()!;
        _inputRouter = GlobalObjectManager.ObjectManager.Get<PlayerInputRouter<TGameInput>>()!;
        return Task.CompletedTask;
    }

    private bool GetButton(TGameInput? key)
    {
        if (key is null)
            return false;

        return _inputRouter.GetState(PlayerInputId, key).IsPressed;
    }

    private void PlaySound(TGameAudio? sound, float volume = 1)
    {
        if (_enableSound && sound is not null)
            _audioManager.Play(sound.Value, volume);
    }
}