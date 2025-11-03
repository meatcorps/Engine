using System.Numerics;
using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.Interfaces.Input;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Hardware.Controllers.Enums;
using Meatcorps.Engine.Hardware.Controllers.Interfaces;

namespace Meatcorps.Engine.Hardware.Controllers.Mapper;

public class ControllerInputMapper<T> : IBackgroundService, IInputMapper<T> where T : Enum
{
    private readonly IControllerDeviceManager _manager;
    private Dictionary<T, ControllerInputEnum> _mapping = new Dictionary<T, ControllerInputEnum>();
    private readonly GenericInput _nothingInput = new GenericInput(() => 0, "Unknown");
    private bool _dPadIsAxis = false;
    private bool _initialized = false;

    public ControllerInputMapper(IControllerDeviceManager manager)
    {
        _manager = manager;
        _manager.Initialize();
    }

    public ControllerInputMapper<T> SetDPadIsAxis(bool isAxis)
    {
        _dPadIsAxis = isAxis;
        return this;
    }

    public ControllerInputMapper<T> Map(T input, ControllerInputEnum controllerInput)
    {
        _mapping.Add(input, controllerInput);
        return this;
    }

    public IInput GetState(int player, T input)
    {
        if (!_manager.IsDeviceAssigned(player))
            return _nothingInput;

        if (!_mapping.TryGetValue(input, out var controllerInput))
            throw new Exception("Input not mapped");

        var device = _manager.GetDevice(player);
        return device?.GetState(controllerInput) ?? _nothingInput;
    }

    public Vector2 GetAxis(int player, int axis = 1)
    {
        if (!_manager.IsDeviceAssigned(player))
            return Vector2.Zero;

        var device = _manager.GetDevice(player);

        if (device == null)
            return Vector2.Zero;

        if (axis == 1)
        {
            var value = new Vector2(device.GetState(ControllerInputEnum.LeftStickX).Normalized,
                device.GetState(ControllerInputEnum.LeftStickY).Normalized);
            if (_dPadIsAxis)
            {
                if (device.GetState(ControllerInputEnum.DPadUp).Down)
                    value.Y = -1;
                if (device.GetState(ControllerInputEnum.DPadDown).Down)
                    value.Y = 1;
                if (device.GetState(ControllerInputEnum.DPadLeft).Down)
                    value.X = -1;
                if (device.GetState(ControllerInputEnum.DPadRight).Down)
                    value.X = 1;
            }

            return value;
        }

        if (axis == 2)
            return new Vector2(device.GetState(ControllerInputEnum.RightStickX).Normalized,
                device.GetState(ControllerInputEnum.RightStickY).Normalized);

        return Vector2.Zero;
    }

    public void Rumble(int player, float left, float right, float duration)
    {
        if (!_manager.IsDeviceAssigned(player))
            return;

        var device = _manager.GetDevice(player);
        device?.Rumble(left, right, duration);
    }

    public void PreUpdate(float deltaTime)
    {
        if (!_initialized)
        {
            _manager.Initialize();
            _initialized = true;
        }

        _manager.Update(deltaTime);

        foreach (var device in _manager.GetDevices())
        {
            foreach (var mapping in _mapping.Values)
            {
                if (device.GetState(mapping) is BaseInput input)
                    input.Update();
            }
        }
    }

    public void Update(float deltaTime)
    {
    }

    public void LateUpdate(float deltaTime)
    {
    }
}