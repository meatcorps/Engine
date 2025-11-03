using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.Interfaces.Input;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Storage.Services;
using Meatcorps.Engine.Hardware.Controllers.Enums;
using Meatcorps.Engine.Hardware.Controllers.Interfaces;
using Meatcorps.Engine.SDL.Controller;

namespace Meatcorps.Engine.RayLib.Input;

public class SDLControllerDevice : IControllerDevice
{
    private SDLController _device;
    private Dictionary<ControllerInputEnum, IInput> _inputs = new();
    private readonly IInput _nothingInput = new GenericInput(() => 0, "Nothing");
    private readonly IUniversalConfig _config;
    private readonly float _deadZone;

    public int Id => _device.ControllerIndex;
    
    internal void SetControllerDevice(SDLController device)
    {
        _device = device;
        
        _inputs.Add(
            ControllerInputEnum.DPadDown, 
            new GenericInput(() => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN), "LeftFaceDown"));
        _inputs.Add(
            ControllerInputEnum.DPadLeft, 
            new GenericInput(() => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT), "LeftFaceLeft"));
        _inputs.Add(
            ControllerInputEnum.DPadRight, 
            new GenericInput(() => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT), "LeftFaceRight"));
        _inputs.Add(
            ControllerInputEnum.DPadUp, 
            new GenericInput(() => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP), "LeftFaceUp"));
        _inputs.Add(
            ControllerInputEnum.LeftStick, 
            new GenericInput(() => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSTICK), "LeftThumb"));
        _inputs.Add(
            ControllerInputEnum.LeftStickX, 
            new GenericInput(() => GetAxisWithDeadZone(device.GetAxis(SDL2.SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX) / short.MaxValue), "LeftX"));
        _inputs.Add(
            ControllerInputEnum.LeftStickY, 
            new GenericInput(() => GetAxisWithDeadZone(device.GetAxis(SDL2.SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTY) / short.MaxValue), "LeftY"));
        _inputs.Add(
            ControllerInputEnum.RightStick, 
            new GenericInput(() => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSTICK), "RightThumb"));
        _inputs.Add(
            ControllerInputEnum.RightStickX, 
            new GenericInput(() => GetAxisWithDeadZone(device.GetAxis(SDL2.SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTX) / short.MaxValue), "RightX"));
        _inputs.Add(
            ControllerInputEnum.RightStickY, 
            new GenericInput(() => GetAxisWithDeadZone(device.GetAxis(SDL2.SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTY) / short.MaxValue), "RightY"));
        _inputs.Add(
            ControllerInputEnum.AorCross, 
            new GenericInput(() => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A), "RightFaceDown"));
        _inputs.Add(
            ControllerInputEnum.BorCircle, 
            new GenericInput(() => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B), "RightFaceRight"));
        _inputs.Add(
            ControllerInputEnum.XorSquare, 
            new GenericInput(() => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X), "RightFaceLeft"));
        _inputs.Add(
            ControllerInputEnum.YorTriangle, 
            new GenericInput(() => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y), "RightFaceUp"));
        _inputs.Add(
            ControllerInputEnum.LeftTrigger, 
            new GenericInput(() => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER), "LeftTrigger1"));
        _inputs.Add(
            ControllerInputEnum.RightTrigger, 
            new GenericInput(() => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER), "RightTrigger1"));
        _inputs.Add(
            ControllerInputEnum.LeftBumper, 
            new GenericInput(() => device.GetAxis(SDL2.SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT) / short.MaxValue, "LeftTrigger"));
        _inputs.Add(
            ControllerInputEnum.RightBumper, 
            new GenericInput(() => device.GetAxis(SDL2.SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT) / short.MaxValue, "RightTrigger"));
        _inputs.Add(
            ControllerInputEnum.Middle, 
            new GenericInput(() => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_GUIDE), "Middle"));
        _inputs.Add(
            ControllerInputEnum.Start, 
            new GenericInput(() => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START), "MiddleRight"));
        _inputs.Add(
            ControllerInputEnum.Select, 
            new GenericInput(() => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK), "MiddleLeft"));
        _inputs.Add(
            ControllerInputEnum.Button1, 
            new GenericInput(() => device.GetButton(0), "Button1"));
        _inputs.Add(
            ControllerInputEnum.Button2, 
            new GenericInput(() => device.GetButton(1), "Button2"));
        _inputs.Add(
            ControllerInputEnum.Button3, 
            new GenericInput(() => device.GetButton(2), "Button3"));
        _inputs.Add(
            ControllerInputEnum.Button4, 
            new GenericInput(() => device.GetButton(3), "Button4"));
        _inputs.Add(
            ControllerInputEnum.Button5, 
            new GenericInput(() => device.GetButton(4), "Button5"));
        _inputs.Add(
            ControllerInputEnum.Button6, 
            new GenericInput(() => device.GetButton(5), "Button6"));
        _inputs.Add(
            ControllerInputEnum.Button7, 
            new GenericInput(() => device.GetButton(6), "Button7"));
        _inputs.Add(
            ControllerInputEnum.Button8, 
            new GenericInput(() => device.GetButton(7), "Button8"));
        _inputs.Add(
            ControllerInputEnum.Button9, 
            new GenericInput(() => device.GetButton(8), "Button9"));
        _inputs.Add(
            ControllerInputEnum.Button10, 
            new GenericInput(() => device.GetButton(9), "Button10"));
    }
    
    public SDLControllerDevice()
    {
        _config = GlobalObjectManager.ObjectManager.Get<IUniversalConfig>() ?? new FallbackConfig();
        _deadZone = _config.GetOrDefault("Input", "ControllerAxisDeadZone", 0.1f);
    }

    private float GetAxisWithDeadZone(float axis)
    {
        if (Math.Abs(axis) < _deadZone)
            return 0;
        return axis;
    }
    
    public IInput GetState(ControllerInputEnum input)
    {
        return _inputs.GetValueOrDefault(input, _nothingInput);
    }

    public void Rumble(float left, float right, float duration)
    {
        _device.Rumble(left, right, duration);
    }

    public ControllerType Type { get; set; }
}