using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.Interfaces.Input;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Storage.Services;
using Meatcorps.Engine.Hardware.Controllers;
using Meatcorps.Engine.Hardware.Controllers.Constants;
using Meatcorps.Engine.Hardware.Controllers.Enums;
using Meatcorps.Engine.Hardware.Controllers.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Input;

public class RayLibControllerDevice : IControllerDevice
{
    private readonly float _deadZone;
    private readonly Dictionary<ControllerInputEnum, IInput> _inputs = new();
    private readonly IInput _nothingInput = new GenericInput(() => 0, "Nothing");

    public RayLibControllerDevice(int id)
    {
        var config = GlobalObjectManager.ObjectManager.Get<IUniversalConfig>() ?? new FallbackConfig();
        _deadZone = config.GetOrDefault("Input", "ControllerAxisDeadZone", 0.1f);
        Id = id;
        _inputs.Add(
            ControllerInputEnum.DPadDown,
            new ControllerInput(() => Type, () => Raylib.IsGamepadButtonDown(id, GamepadButton.LeftFaceDown) ? 1 : 0,
                ButtonConstants.DpadDown));
        _inputs.Add(
            ControllerInputEnum.DPadLeft,
            new ControllerInput(() => Type, () => Raylib.IsGamepadButtonDown(id, GamepadButton.LeftFaceLeft) ? 1 : 0,
                ButtonConstants.DpadLeft));
        _inputs.Add(
            ControllerInputEnum.DPadRight,
            new ControllerInput(() => Type, () => Raylib.IsGamepadButtonDown(id, GamepadButton.LeftFaceRight) ? 1 : 0,
                ButtonConstants.DpadRight));
        _inputs.Add(
            ControllerInputEnum.DPadUp,
            new ControllerInput(() => Type, () => Raylib.IsGamepadButtonDown(id, GamepadButton.LeftFaceUp) ? 1 : 0,
                ButtonConstants.DPadUp));
        _inputs.Add(
            ControllerInputEnum.LeftStick,
            new ControllerInput(() => Type, () => Raylib.IsGamepadButtonDown(id, GamepadButton.LeftThumb) ? 1 : 0,
                ButtonConstants.LeftJoyConClick));
        _inputs.Add(
            ControllerInputEnum.LeftStickX,
            new ControllerInput(() => Type,
                () => GetAxisWithDeadZone(Raylib.GetGamepadAxisMovement(id, GamepadAxis.LeftX)),
                ButtonConstants.LeftX));
        _inputs.Add(
            ControllerInputEnum.LeftStickY,
            new ControllerInput(() => Type,
                () => GetAxisWithDeadZone(Raylib.GetGamepadAxisMovement(id, GamepadAxis.LeftY)),
                ButtonConstants.LeftY));
        _inputs.Add(
            ControllerInputEnum.RightStick,
            new ControllerInput(() => Type, () => Raylib.IsGamepadButtonDown(id, GamepadButton.RightThumb) ? 1 : 0,
                ButtonConstants.RightJoyConClick));
        _inputs.Add(
            ControllerInputEnum.RightStickX,
            new ControllerInput(() => Type,
                () => GetAxisWithDeadZone(Raylib.GetGamepadAxisMovement(id, GamepadAxis.RightX)),
                ButtonConstants.RightX));
        _inputs.Add(
            ControllerInputEnum.RightStickY,
            new ControllerInput(() => Type,
                () => GetAxisWithDeadZone(Raylib.GetGamepadAxisMovement(id, GamepadAxis.RightY)),
                ButtonConstants.RightY));
        _inputs.Add(
            ControllerInputEnum.AorCross,
            new ControllerInput(() => Type, () => Raylib.IsGamepadButtonDown(id, GamepadButton.RightFaceDown) ? 1 : 0,
                ButtonConstants.RightFaceDown));
        _inputs.Add(
            ControllerInputEnum.BorCircle,
            new ControllerInput(() => Type, () => Raylib.IsGamepadButtonDown(id, GamepadButton.RightFaceRight) ? 1 : 0,
                ButtonConstants.RightFaceRight));
        _inputs.Add(
            ControllerInputEnum.XorSquare,
            new ControllerInput(() => Type, () => Raylib.IsGamepadButtonDown(id, GamepadButton.RightFaceLeft) ? 1 : 0,
                ButtonConstants.RightFaceLeft));
        _inputs.Add(
            ControllerInputEnum.YorTriangle,
            new ControllerInput(() => Type, () => Raylib.IsGamepadButtonDown(id, GamepadButton.RightFaceUp) ? 1 : 0,
                ButtonConstants.RightFaceUp));
        _inputs.Add(
            ControllerInputEnum.LeftBumper,
            new ControllerInput(() => Type, () => Raylib.IsGamepadButtonDown(id, GamepadButton.LeftTrigger1) ? 1 : 0,
                ButtonConstants.LeftBumper));
        _inputs.Add(
            ControllerInputEnum.RightBumper,
            new ControllerInput(() => Type, () => Raylib.IsGamepadButtonDown(id, GamepadButton.RightTrigger1) ? 1 : 0,
                ButtonConstants.RightBumper));
        _inputs.Add(
            ControllerInputEnum.LeftTrigger,
            new ControllerInput(() => Type, () => (Raylib.GetGamepadAxisMovement(id, GamepadAxis.LeftTrigger) + 1) / 2,
                ButtonConstants.LeftTrigger));
        _inputs.Add(
            ControllerInputEnum.RightTrigger,
            new ControllerInput(() => Type, () => (Raylib.GetGamepadAxisMovement(id, GamepadAxis.RightTrigger) + 1) / 2,
                ButtonConstants.RightTrigger));
        _inputs.Add(
            ControllerInputEnum.Middle,
            new ControllerInput(() => Type, () => Raylib.IsGamepadButtonDown(id, GamepadButton.Middle) ? 1 : 0,
                ButtonConstants.Middle));
        _inputs.Add(
            ControllerInputEnum.Start,
            new ControllerInput(() => Type, () => Raylib.IsGamepadButtonDown(id, GamepadButton.MiddleRight) ? 1 : 0,
                ButtonConstants.MiddleRight));
        _inputs.Add(
            ControllerInputEnum.Select,
            new ControllerInput(() => Type, () => Raylib.IsGamepadButtonDown(id, GamepadButton.MiddleLeft) ? 1 : 0,
                ButtonConstants.MiddleLeft));
    }

    public int Id { get; }

    public IInput GetState(ControllerInputEnum input)
    {
        return _inputs.GetValueOrDefault(input, _nothingInput);
    }

    public void Rumble(float left, float right, float duration)
    {
        Raylib.SetGamepadVibration(Id, left, right, duration);
    }

    public ControllerType Type { get; set; }

    public bool IsConnected => Raylib.IsGamepadAvailable(Id);

    private float GetAxisWithDeadZone(float axis)
    {
        if (Math.Abs(axis) < _deadZone)
            return 0;
        return axis;
    }
}