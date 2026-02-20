namespace Meatcorps.Engine.SDL.Controller;
using SDL2;

internal sealed class SDLController : IDisposable
{
    public readonly int ControllerIndex;
    private readonly IntPtr _controller;
    private readonly IntPtr _haptic;
    private bool _disposed;
    private readonly int _effectId = int.MinValue;
    private readonly bool _isJoystick;
    public bool IsActive => _controller != IntPtr.Zero;
    public IntPtr Handle => _controller;
    
    public SDLController(int controllerIndex)
    {
        ControllerIndex = controllerIndex;
        _isJoystick = SDL.SDL_IsGameController(controllerIndex) == SDL.SDL_bool.SDL_FALSE;
        _controller = _isJoystick 
            ? SDL.SDL_JoystickOpen(controllerIndex) 
            : SDL.SDL_GameControllerOpen(controllerIndex);

        if (!_isJoystick)
            _haptic = SDL.SDL_HapticOpen(controllerIndex);
    }

    public string? GetSerial()
    {
        if (_isJoystick)
        {
            var serial = SDL.SDL_JoystickGetSerial(_controller) ?? Guid.NewGuid().ToString(); // Most random controllers don't come with a serial...
            return serial;
        }

        return SDL.SDL_GameControllerGetSerial(_controller);
    }

    public void Rumble(float leftRumble, float rightRumble, float duration)
    {
        if (!IsActive || _isJoystick)
            return;
        
        /*if (_hapticSupported)
        {
            ApplyMagnitude(leftRumble, rightRumble);
            UseHapticRumble(leftRumble, rightRumble);
            return;
        }*/
        
        UseDefaultRumble(leftRumble, rightRumble, duration);
    }

    private void UseDefaultRumble(float leftRumble, float rightRumble, float duration)
    {
        SDL.SDL_GameControllerRumble(_controller, 
            (ushort)(ushort.MaxValue * Math.Clamp(leftRumble, 0, 1)), 
            (ushort)(ushort.MaxValue * Math.Clamp(rightRumble, 0, 1)), 
            (uint)(duration * 1000));
    }

    public ushort GetVendorId()
    {
        if (_isJoystick)
            return SDL.SDL_JoystickGetVendor(_controller);
            
        var vendorId = SDL.SDL_GameControllerGetVendor(_controller);
        return vendorId;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_effectId >= 0)
            SDL.SDL_HapticDestroyEffect(_haptic, _effectId);
        
        try
        {
            if (_isJoystick)
            {
                SDL.SDL_JoystickClose(_controller);   
            }
            else
            {
                SDL.SDL_HapticClose(_haptic);
                SDL.SDL_GameControllerClose(_controller);
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public byte GetButton(int index)
    {
        if (_isJoystick)
            return SDL.SDL_JoystickGetButton(_controller, index);
        return SDL.SDL_GameControllerGetButton(_controller, (SDL.SDL_GameControllerButton)index);
    }

    public bool IsAttached()
    {
        if (_isJoystick)
            return SDL.SDL_JoystickGetAttached(_controller) == SDL.SDL_bool.SDL_TRUE;
        return SDL.SDL_GameControllerGetAttached(_controller) == SDL.SDL_bool.SDL_TRUE;
    }
    
    public byte GetButton(SDL.SDL_GameControllerButton button)
    {
        if (_isJoystick && button is SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT
            or SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN
            or SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT
            or SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP)
        {
            var hat = SDL.SDL_JoystickGetHat(_controller, 0);

            if (button == SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT)
                return (byte)((hat & SDL.SDL_HAT_LEFT) != 0 ? 1 : 0);
            if (button == SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT)
                return (byte)((hat & SDL.SDL_HAT_RIGHT) != 0 ? 1 : 0);
            if (button == SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP)
                return (byte)((hat & SDL.SDL_HAT_UP) != 0 ? 1 : 0);

            return (byte)((hat & SDL.SDL_HAT_DOWN) != 0 ? 1 : 0);
        }

        if (_isJoystick)
        {
            return button switch
            {
                SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A => SDL.SDL_JoystickGetButton(_controller, 1),
                SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B => SDL.SDL_JoystickGetButton(_controller, 2),
                SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X => SDL.SDL_JoystickGetButton(_controller, 0),
                SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y => SDL.SDL_JoystickGetButton(_controller, 3),
                SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK => SDL.SDL_JoystickGetButton(_controller, 8),
                SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START => SDL.SDL_JoystickGetButton(_controller, 9),
                SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER => SDL.SDL_JoystickGetButton(_controller, 4),
                SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER => SDL.SDL_JoystickGetButton(_controller, 5),
                _ => 0
            };
        }
        
        return SDL.SDL_GameControllerGetButton(_controller, button);
    }

    public float GetAxis(SDL.SDL_GameControllerAxis axis)
    {
        if (_isJoystick)
            return SDL.SDL_JoystickGetAxis(_controller, (int)axis);
        
        return SDL.SDL_GameControllerGetAxis(_controller, axis);
    }
}