using System.Numerics;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.Interfaces.Input;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Storage.Services;
using Meatcorps.Engine.Hardware.Controllers.Enums;
using Meatcorps.Engine.Hardware.Controllers.Interfaces;

namespace Meatcorps.Engine.Hardware.Controllers.Mapper;

public class ControllerInputMapper<T> : IBackgroundService, IConfigChangeTracker, IInputMapper<T> where T : Enum
{
    private readonly IControllerDeviceManager _manager;
    private readonly Dictionary<T, ControllerInputEnum> _mapping = new Dictionary<T, ControllerInputEnum>();
    private readonly GenericInput _nothingInput = new GenericInput(() => 0, "Unknown");
    private bool _dPadIsAxis;
    private bool _initialized;
    private readonly List<int> _profiles = new();
    private bool _enableRumble = true;
    private readonly IUniversalConfig _config;

    public ControllerInputMapper(IControllerDeviceManager manager)
    {
        _manager = manager;
        _manager.Initialize();
        _config = GlobalObjectManager.ObjectManager.Get<IUniversalConfig>() ?? new FallbackConfig();
        _enableRumble = _config.GetOrDefault("Input", "Rumble", true);
        GlobalObjectManager.ObjectManager.Add<IConfigChangeTracker>(this);
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
            var stateX = device.GetState(ControllerInputEnum.LeftStickX);
            var stateY = device.GetState(ControllerInputEnum.LeftStickY);
            var value = new Vector2(stateX.Normalized, stateY.Normalized);
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
        if (!_manager.IsDeviceAssigned(player) || !_enableRumble)
            return;

        var device = _manager.GetDevice(player);
        device?.Rumble(left, right, duration);
    }

    public void AssignProfile(int profileId, int player)
    {
        _manager.AssignDevice(player, profileId);
    }

    public void UnassignProfile(int player)
    {
        _manager.UnassignDevice(player);
    }

    public bool IsAssigned(int player)
    {
        return _manager.IsDeviceAssigned(player);
    }

    public bool IsConnected(int player)
    {
        if (!_manager.IsDeviceAssigned(player))
        {
            return false;
        }
        
        var device = _manager.GetDevice(player);
        return device?.IsConnected ?? false;
    }

    public bool AnyInputPressed(out int profileId, out int player)
    {
        var deviceId = 0;
        foreach (var device in _manager.GetDevices())
        {
            // Exception because this is common to use on the controller
            if (device.GetState(ControllerInputEnum.LeftStickX).IsPressed || device.GetState(ControllerInputEnum.LeftStickY).IsPressed)
            {
                profileId = deviceId;
                player = _manager.WhichPlayerOnDevice(profileId);
                return true;
            }
                
            foreach (var button in _mapping.Values)
            {
                if (device.GetState(button).IsPressed)
                {
                    profileId = deviceId;
                    player = _manager.WhichPlayerOnDevice(profileId);
                    return true;
                }
            }
            deviceId++;
        }
        profileId = -1;
        player = -1;
        return false;
    }

    public PlayerInputType InputType(int _) => PlayerInputType.GamePad;
    
    public IReadOnlyList<int> GetAvailableProfiles()
    {
        _profiles.Clear();
        for (var i = 0; i < _manager.TotalDevices; i++)
        {
            _profiles.Add(i);
        }
        return _profiles;
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
            foreach (var mapping in Enum.GetValues<ControllerInputEnum>())
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

    public void ConfigChanged(string group, string key, object value)
    {
        if (group == "Input" && key == "Rumble")
            _enableRumble = _config.GetOrDefault("Input", "Rumble", true);
    }
}