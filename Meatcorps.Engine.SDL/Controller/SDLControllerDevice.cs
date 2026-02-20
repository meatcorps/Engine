using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.Interfaces.Input;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Storage.Services;
using Meatcorps.Engine.Hardware.Controllers;
using Meatcorps.Engine.Hardware.Controllers.Constants;
using Meatcorps.Engine.Hardware.Controllers.Enums;
using Meatcorps.Engine.Hardware.Controllers.Interfaces;

namespace Meatcorps.Engine.SDL.Controller;

public class SDLControllerDevice : IControllerDevice
{
    private SDLController _device = null!;
    private readonly Dictionary<ControllerInputEnum, IInput> _inputs = new();
    private readonly IInput _nothingInput = new GenericInput(() => 0, "Nothing");
    private readonly float _deadZone;

    public int Id => _device.ControllerIndex;
    
    internal void SetControllerDevice(SDLController device)
    {
        _device = device;
        
        _inputs.Add(
            ControllerInputEnum.DPadDown, 
            new ControllerInput(() => Type, () => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN), ButtonConstants.DpadDown));
        _inputs.Add(
            ControllerInputEnum.DPadLeft, 
            new ControllerInput(() => Type, () => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT), ButtonConstants.DpadLeft));
        _inputs.Add(
            ControllerInputEnum.DPadRight, 
            new ControllerInput(() => Type, () => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT), ButtonConstants.DpadRight));
        _inputs.Add(
            ControllerInputEnum.DPadUp, 
            new ControllerInput(() => Type, () => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP), ButtonConstants.DPadUp));
        _inputs.Add(
            ControllerInputEnum.LeftStick, 
            new ControllerInput(() => Type, () => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSTICK), ButtonConstants.LeftJoyConClick));
        _inputs.Add(
            ControllerInputEnum.LeftStickX, 
            new ControllerInput(() => Type, () => GetAxisWithDeadZone(device.GetAxis(SDL2.SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX) / short.MaxValue), ButtonConstants.LeftX));
        _inputs.Add(
            ControllerInputEnum.LeftStickY, 
            new ControllerInput(() => Type, () => GetAxisWithDeadZone(device.GetAxis(SDL2.SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTY) / short.MaxValue), ButtonConstants.LeftY));
        _inputs.Add(
            ControllerInputEnum.RightStick, 
            new ControllerInput(() => Type, () => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSTICK), ButtonConstants.RightJoyConClick));
        _inputs.Add(
            ControllerInputEnum.RightStickX, 
            new ControllerInput(() => Type, () => GetAxisWithDeadZone(device.GetAxis(SDL2.SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTX) / short.MaxValue), ButtonConstants.RightX));
        _inputs.Add(
            ControllerInputEnum.RightStickY, 
            new ControllerInput(() => Type, () => GetAxisWithDeadZone(device.GetAxis(SDL2.SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTY) / short.MaxValue), ButtonConstants.RightY));
        _inputs.Add(
            ControllerInputEnum.AorCross, 
            new ControllerInput(() => Type, () => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A), ButtonConstants.RightFaceDown));
        _inputs.Add(
            ControllerInputEnum.BorCircle, 
            new ControllerInput(() => Type, () => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B), ButtonConstants.RightFaceRight));
        _inputs.Add(
            ControllerInputEnum.XorSquare, 
            new ControllerInput(() => Type, () => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X), ButtonConstants.RightFaceLeft));
        _inputs.Add(
            ControllerInputEnum.YorTriangle, 
            new ControllerInput(() => Type, () => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y), ButtonConstants.RightFaceUp));
        _inputs.Add(
            ControllerInputEnum.LeftBumper, 
            new ControllerInput(() => Type, () => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER), ButtonConstants.LeftBumper));
        _inputs.Add(
            ControllerInputEnum.RightBumper, 
            new ControllerInput(() => Type, () => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER), ButtonConstants.RightBumper));
        _inputs.Add(
            ControllerInputEnum.LeftTrigger, 
            new ControllerInput(() => Type, () => device.GetAxis(SDL2.SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT) / short.MaxValue, ButtonConstants.LeftTrigger));
        _inputs.Add(
            ControllerInputEnum.RightTrigger, 
            new ControllerInput(() => Type, () => device.GetAxis(SDL2.SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT) / short.MaxValue, ButtonConstants.RightTrigger));
        _inputs.Add(
            ControllerInputEnum.Middle, 
            new ControllerInput(() => Type, () => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_GUIDE), ButtonConstants.Middle));
        _inputs.Add(
            ControllerInputEnum.Start, 
            new ControllerInput(() => Type, () => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START), ButtonConstants.MiddleRight));
        _inputs.Add(
            ControllerInputEnum.Select, 
            new ControllerInput(() => Type, () => device.GetButton(SDL2.SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK), ButtonConstants.MiddleLeft));
        _inputs.Add(
            ControllerInputEnum.Button1, 
            new ControllerInput(() => Type, () => device.GetButton(0), ButtonConstants.Button1));
        _inputs.Add(
            ControllerInputEnum.Button2, 
            new ControllerInput(() => Type, () => device.GetButton(1), ButtonConstants.Button2));
        _inputs.Add(
            ControllerInputEnum.Button3, 
            new ControllerInput(() => Type, () => device.GetButton(2), ButtonConstants.Button3));
        _inputs.Add(
            ControllerInputEnum.Button4, 
            new ControllerInput(() => Type, () => device.GetButton(3), ButtonConstants.Button4));
        _inputs.Add(
            ControllerInputEnum.Button5, 
            new ControllerInput(() => Type, () => device.GetButton(4), ButtonConstants.Button5));
        _inputs.Add(
            ControllerInputEnum.Button6, 
            new ControllerInput(() => Type, () => device.GetButton(5), ButtonConstants.Button6));
        _inputs.Add(
            ControllerInputEnum.Button7, 
            new ControllerInput(() => Type, () => device.GetButton(6), ButtonConstants.Button7));
        _inputs.Add(
            ControllerInputEnum.Button8, 
            new ControllerInput(() => Type, () => device.GetButton(7), ButtonConstants.Button8));
        _inputs.Add(
            ControllerInputEnum.Button9, 
            new ControllerInput(() => Type, () => device.GetButton(8), ButtonConstants.Button9));
        _inputs.Add(
            ControllerInputEnum.Button10, 
            new ControllerInput(() => Type, () => device.GetButton(9), ButtonConstants.Button10));
    }
    
    public SDLControllerDevice()
    {
        var config = GlobalObjectManager.ObjectManager.Get<IUniversalConfig>() ?? new FallbackConfig();
        _deadZone = config.GetOrDefault("Input", "ControllerAxisDeadZone", 0.1f);
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
    
    public bool IsConnected => _device.IsAttached();
}