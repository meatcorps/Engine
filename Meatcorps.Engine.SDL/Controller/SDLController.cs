namespace Meatcorps.Engine.SDL.Controller;
using SDL2;

internal sealed class SDLController : IDisposable
{
    public readonly int ControllerIndex;
    private readonly IntPtr _controller;
    private readonly bool _hapticSupported;
    private readonly IntPtr _haptic;
    private SDL2.SDL.SDL_HapticEffect _effect;
    private bool _disposed;
    private int _effectId = int.MinValue;
    private bool _isJoystick;
    public bool IsActive => _controller != IntPtr.Zero;
    public IntPtr Handle => _controller;
    
    public SDLController(int controllerIndex)
    {
        ControllerIndex = controllerIndex;
        _isJoystick = SDL.SDL_IsGameController(controllerIndex) == SDL.SDL_bool.SDL_FALSE;
        if (_isJoystick)
            _controller = SDL2.SDL.SDL_JoystickOpen(controllerIndex);
        else 
            _controller = SDL2.SDL.SDL_GameControllerOpen(controllerIndex);

        if (!_isJoystick)
        {
            _haptic = SDL2.SDL.SDL_HapticOpen(controllerIndex);
            _hapticSupported = _haptic != IntPtr.Zero && SDL2.SDL.SDL_HapticRumbleSupported(_haptic) == 1;
            _effect.type = SDL2.SDL.SDL_HAPTIC_LEFTRIGHT;
            _effect.leftright.length = SDL2.SDL.SDL_HAPTIC_INFINITY;
        }
    }

    public string? GetSerial()
    {
        if (_isJoystick)
        {
            var serial = SDL2.SDL.SDL_JoystickGetSerial(_controller);
            if (serial is null)
                serial = Guid.NewGuid().ToString(); // Most random controllers don't come with a serial...
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

    private void ApplyMagnitude(float leftRumble, float rightRumble)
    {
        _effect.leftright.large_magnitude = (ushort)(0xFFFF * Math.Clamp(leftRumble, 0, 1));
        _effect.leftright.large_magnitude = (ushort)(0xFFFF * Math.Clamp(rightRumble, 0, 1));
    }

    private void UseDefaultRumble(float leftRumble, float rightRumble, float duration)
    {
        SDL2.SDL.SDL_GameControllerRumble(_controller, 
            (ushort)(ushort.MaxValue * Math.Clamp(leftRumble, 0, 1)), 
            (ushort)(ushort.MaxValue * Math.Clamp(rightRumble, 0, 1)), 
            (uint)(duration * 1000));
    }
    
    private void UseHapticRumble(float leftRumble, float rightRumble)
    {
        if (leftRumble < 0.01f && rightRumble < 0.01f && _effectId != int.MinValue)
        {
            SDL2.SDL.SDL_HapticStopEffect(_haptic, _effectId);
            return;
        }

        if (_effectId == int.MinValue)
            _effectId = SDL2.SDL.SDL_HapticNewEffect(_haptic, ref _effect);

        if (_effectId >= 0)
            SDL2.SDL.SDL_HapticRunEffect(_haptic, _effectId, SDL2.SDL.SDL_HAPTIC_INFINITY);
    }

    public ushort GetVendorId()
    {
        if (_isJoystick)
            return SDL2.SDL.SDL_JoystickGetVendor(_controller);
            
        var vendorId = SDL2.SDL.SDL_GameControllerGetVendor(_controller);
        return vendorId;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_effectId >= 0)
            SDL2.SDL.SDL_HapticDestroyEffect(_haptic, _effectId);
        
        try
        {
            if (_isJoystick)
            {
                SDL.SDL_JoystickClose(_controller);   
            }
            else
            {
                SDL2.SDL.SDL_HapticClose(_haptic);
                SDL2.SDL.SDL_GameControllerClose(_controller);
            }
        }
        catch (Exception)
        {
            
        }
    }

    public byte GetButton(int index)
    {
        if (_isJoystick)
            return SDL2.SDL.SDL_JoystickGetButton(_controller, index);
        return SDL.SDL_GameControllerGetButton(_controller, (SDL.SDL_GameControllerButton)index);
    }

    public bool IsAttached()
    {
        if (_isJoystick)
            return SDL2.SDL.SDL_JoystickGetAttached(_controller) == SDL.SDL_bool.SDL_TRUE;
        return SDL2.SDL.SDL_GameControllerGetAttached(_controller) == SDL.SDL_bool.SDL_TRUE;
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
            if (button == SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN)
                return (byte)((hat & SDL.SDL_HAT_DOWN) != 0 ? 1 : 0);
        }

        if (_isJoystick)
        {
            switch (button)
            {
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A:
                    return SDL2.SDL.SDL_JoystickGetButton(_controller, 1);  
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B:
                    return SDL2.SDL.SDL_JoystickGetButton(_controller, 2);
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X:
                    return SDL2.SDL.SDL_JoystickGetButton(_controller, 0);
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y:
                    return SDL2.SDL.SDL_JoystickGetButton(_controller, 3);
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK:
                    return SDL2.SDL.SDL_JoystickGetButton(_controller, 8);
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START:
                    return SDL2.SDL.SDL_JoystickGetButton(_controller, 9);
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER:
                    return SDL2.SDL.SDL_JoystickGetButton(_controller, 4);
                case SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER:
                    return SDL2.SDL.SDL_JoystickGetButton(_controller, 5);
            }
            return 0;   
        }
        
        return SDL.SDL_GameControllerGetButton(_controller, button);
    }

    public float GetAxis(SDL.SDL_GameControllerAxis axis)
    {
        if (_isJoystick)
            return SDL2.SDL.SDL_JoystickGetAxis(_controller, (int)axis);
        
        return SDL.SDL_GameControllerGetAxis(_controller, axis);
    }
}