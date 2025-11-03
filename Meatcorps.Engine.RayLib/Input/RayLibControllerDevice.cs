using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.Interfaces.Input;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Storage.Services;
using Meatcorps.Engine.Hardware.Controllers.Enums;
using Meatcorps.Engine.Hardware.Controllers.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Input;

public class RayLibControllerDevice : IControllerDevice
{
    public int Id { get; private set; }
    private Dictionary<ControllerInputEnum, IInput> _inputs = new();
    private readonly IInput _nothingInput = new GenericInput(() => 0, "Nothing");
    private readonly IUniversalConfig _config;
    private readonly float _deadZone;

    public RayLibControllerDevice(int id)
    {
        _config = GlobalObjectManager.ObjectManager.Get<IUniversalConfig>() ?? new FallbackConfig();
        _deadZone = _config.GetOrDefault("Input", "ControllerAxisDeadZone", 0.1f);
        Id = id;
        _inputs.Add(
            ControllerInputEnum.DPadDown, 
            new GenericInput(() => Raylib.IsGamepadButtonDown(id, GamepadButton.LeftFaceDown) ? 1 : 0, "LeftFaceDown"));
        _inputs.Add(
            ControllerInputEnum.DPadLeft, 
            new GenericInput(() => Raylib.IsGamepadButtonDown(id, GamepadButton.LeftFaceLeft) ? 1 : 0, "LeftFaceLeft"));
        _inputs.Add(
            ControllerInputEnum.DPadRight, 
            new GenericInput(() => Raylib.IsGamepadButtonDown(id, GamepadButton.LeftFaceRight) ? 1 : 0, "LeftFaceRight"));
        _inputs.Add(
            ControllerInputEnum.DPadUp, 
            new GenericInput(() => Raylib.IsGamepadButtonDown(id, GamepadButton.LeftFaceUp) ? 1 : 0, "LeftFaceUp"));
        _inputs.Add(
            ControllerInputEnum.LeftStick, 
            new GenericInput(() => Raylib.IsGamepadButtonDown(id, GamepadButton.LeftThumb) ? 1 : 0, "LeftThumb"));
        _inputs.Add(
            ControllerInputEnum.LeftStickX, 
            new GenericInput(() => GetAxisWithDeadZone(Raylib.GetGamepadAxisMovement(id, GamepadAxis.LeftX)), "LeftX"));
        _inputs.Add(
            ControllerInputEnum.LeftStickY, 
            new GenericInput(() => GetAxisWithDeadZone(Raylib.GetGamepadAxisMovement(id, GamepadAxis.LeftY)), "LeftY"));
        _inputs.Add(
            ControllerInputEnum.RightStick, 
            new GenericInput(() => Raylib.IsGamepadButtonDown(id, GamepadButton.RightThumb) ? 1 : 0, "RightThumb"));
        _inputs.Add(
            ControllerInputEnum.RightStickX, 
            new GenericInput(() => GetAxisWithDeadZone(Raylib.GetGamepadAxisMovement(id, GamepadAxis.RightX)), "RightX"));
        _inputs.Add(
            ControllerInputEnum.RightStickY, 
            new GenericInput(() => GetAxisWithDeadZone(Raylib.GetGamepadAxisMovement(id, GamepadAxis.RightY)), "RightY"));
        _inputs.Add(
            ControllerInputEnum.AorCross, 
            new GenericInput(() => Raylib.IsGamepadButtonDown(id, GamepadButton.RightFaceDown) ? 1 : 0, "RightFaceDown"));
        _inputs.Add(
            ControllerInputEnum.BorCircle, 
            new GenericInput(() => Raylib.IsGamepadButtonDown(id, GamepadButton.RightFaceRight) ? 1 : 0, "RightFaceRight"));
        _inputs.Add(
            ControllerInputEnum.XorSquare, 
            new GenericInput(() => Raylib.IsGamepadButtonDown(id, GamepadButton.RightFaceLeft) ? 1 : 0, "RightFaceLeft"));
        _inputs.Add(
            ControllerInputEnum.YorTriangle, 
            new GenericInput(() => Raylib.IsGamepadButtonDown(id, GamepadButton.RightFaceUp) ? 1 : 0, "RightFaceUp"));
        _inputs.Add(
            ControllerInputEnum.LeftTrigger, 
            new GenericInput(() => Raylib.IsGamepadButtonDown(id, GamepadButton.LeftTrigger1) ? 1 : 0, "LeftTrigger1"));
        _inputs.Add(
            ControllerInputEnum.RightTrigger, 
            new GenericInput(() => Raylib.IsGamepadButtonDown(id, GamepadButton.RightTrigger1) ? 1 : 0, "RightTrigger1"));
        _inputs.Add(
            ControllerInputEnum.LeftBumper, 
            new GenericInput(() => (Raylib.GetGamepadAxisMovement(id, GamepadAxis.LeftTrigger) + 1) / 2, "LeftTrigger"));
        _inputs.Add(
            ControllerInputEnum.RightBumper, 
            new GenericInput(() => (Raylib.GetGamepadAxisMovement(id, GamepadAxis.RightTrigger) + 1) / 2, "RightTrigger"));
        _inputs.Add(
            ControllerInputEnum.Middle, 
            new GenericInput(() => Raylib.IsGamepadButtonDown(id, GamepadButton.Middle) ? 1 : 0, "Middle"));
        _inputs.Add(
            ControllerInputEnum.Start, 
            new GenericInput(() => Raylib.IsGamepadButtonDown(id, GamepadButton.MiddleRight) ? 1 : 0, "MiddleRight"));
        _inputs.Add(
            ControllerInputEnum.Select, 
            new GenericInput(() => Raylib.IsGamepadButtonDown(id, GamepadButton.MiddleLeft) ? 1 : 0, "MiddleLeft"));
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
        Raylib.SetGamepadVibration(Id, left, right, duration);
    }

    public ControllerType Type { get; set; }
}