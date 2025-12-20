using Meatcorps.Engine.Core.Enums;
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

    public bool UseDefaultKeyBindings { get; set; } = true;
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

    private GenericInput _upInput = new GenericInput(() => Raylib.IsKeyDown(KeyboardKey.Up) ? 1 : 0, nameof(_upInput)); 
    private GenericInput _downInput = new GenericInput(() => Raylib.IsKeyDown(KeyboardKey.Down) ? 1 : 0, nameof(_downInput)); 
    private GenericInput _leftInput = new GenericInput(() => Raylib.IsKeyDown(KeyboardKey.Left) ? 1 : 0, nameof(_leftInput)); 
    private GenericInput _rightInput = new GenericInput(() => Raylib.IsKeyDown(KeyboardKey.Right) ? 1 : 0, nameof(_rightInput)); 
    private GenericInput _selectionInput = new GenericInput(() => Raylib.IsKeyDown(KeyboardKey.Enter) ? 1 : 0, nameof(_selectionInput)); 
    private GenericInput _backInput = new GenericInput(() => Raylib.IsKeyDown(KeyboardKey.Escape) ? 1 : 0, nameof(_backInput)); 
    
    public Font Font { get; set; }
    public Color TextColor { get; set; } = Color.Gray;
    public Color TextColorActive { get; set; } = new Color(0, 255, 255);
    public Color TextColorValue { get; set; } = Color.White;
    public float FontScaleSize { get; set; } = 1;

    public bool IsDownPressed
    {
        get
        {
            if (UseDefaultKeyBindings && _downInput.IsPressed)
                return true;
                
            return GetButton(DownKey);
        }
    }

    public bool IsUpPressed
    {
        get
        {
            if (UseDefaultKeyBindings  && _upInput.IsPressed)
                return true;

            return GetButton(UpKey);
        }
    }

    public bool IsLeftPressed
    {
        get
        {
            if (UseDefaultKeyBindings && _leftInput.IsPressed)
                return true;

            return GetButton(LeftKey);
        }
    }

    public bool IsRightPressed
    {
        get
        {
            if (UseDefaultKeyBindings && _rightInput.IsPressed)
                return true;

            return GetButton(RightKey);
        }
    }

    public bool IsOnSelectionPressed
    {
        get
        {
            if (UseDefaultKeyBindings && _selectionInput.IsPressed)
                return true;

            return GetButton(OnSelectionPressed);
        }
    }

    public bool IsBackPressed
    {
        get
        {
            if (UseDefaultKeyBindings && _backInput.IsPressed)
                return true;

            return GetButton(BackPressed);
        }
    }

    public string DownPressedText => GetKeyText(DownKey, "DOWN");
    public string UpPressedText => GetKeyText(UpKey, "UP");
    public string LeftPressedText => GetKeyText(LeftKey, "LEFT");
    public string RightPressedText => GetKeyText(RightKey, "RIGHT");
    public string SelectionPressedText  => GetKeyText(OnSelectionPressed, "ENTER");
    public string BackPressedText  => GetKeyText(BackPressed, "ESC");

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

    public void Update(float deltaTime)
    {
        _upInput.Update();
        _downInput.Update();
        _leftInput.Update();
        _rightInput.Update();
        _selectionInput.Update();
        _backInput.Update();
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

        var state = _inputRouter.GetState(PlayerInputId, key);
        
        if (_inputRouter.InputType(PlayerInputId) is PlayerInputType.Keyboard or PlayerInputType.KeyboardMouse && UseDefaultKeyBindings)
            return false;
        
        return state.IsPressed;
    }

    private string GetKeyText(TGameInput? key, string defaultText)
    {
        if (key is null)
            return defaultText;
        
        if (_inputRouter.InputType(PlayerInputId) == PlayerInputType.KeyboardMouse && UseDefaultKeyBindings)
            return defaultText;
        
        return _inputRouter.GetState(PlayerInputId, key).Label;
    }

    private void PlaySound(TGameAudio? sound, float volume = 1)
    {
        if (_enableSound && sound is not null)
            _audioManager.Play(sound.Value, volume);
    }
}