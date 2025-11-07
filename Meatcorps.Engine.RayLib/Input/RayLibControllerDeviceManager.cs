using System.Runtime.InteropServices;
using HidSharp;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.Hardware.Controllers.Enums;
using Meatcorps.Engine.Hardware.Controllers.Interfaces;
using Meatcorps.Engine.Hardware.Controllers.Mapper;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Input;

public class RayLibControllerDeviceManager: IControllerDeviceManager
{
    private List<RayLibControllerDevice> _devices = new();
    private FixedTimer _checkTimer = new(1000f);
    private Dictionary<int, int> _assignedDevices = new();
    private Dictionary<string, ControllerType> _deviceMapping = new();
    
    public IControllerDevice? GetDevice(int player)
    {
        if (!_assignedDevices.TryGetValue(player, out var device))
            return null;
        
        if (device >= _devices.Count || _devices.Count == 0)
            return null;
        
        return _devices[device];
    }

    public IEnumerable<IControllerDevice> GetDevices()
    {
        return _devices;
    }

    public int TotalDevices { get; private set; }

    public void AssignDevice(int player, int device)
    {
        _assignedDevices[player] = device;
    }

    public void UnassignDevice(int player)
    {
        _assignedDevices.Remove(player);
    }

    public bool IsDeviceAssigned(int player)
    {
        return GetDevice(player) is not null;
    }
    
    public int WhichPlayerOnDevice(int device)
    {
        foreach (var (playerId, deviceId) in _assignedDevices)
        {
            if (deviceId == device && GetDevice(playerId) is not null)
                return playerId;
        }
        return -1;
    }

    public void Initialize()
    {
        if (File.Exists("gamecontrollerdb.txt"))
        {
            var db = File.ReadAllText("gamecontrollerdb.txt");
            Raylib.SetGamepadMappings(db); 
        }
        
        UpdateTotalDevices();
    }

    public void Update(float deltaTime)
    {
        _checkTimer.Update(deltaTime);
        if (_checkTimer.Output)
            UpdateTotalDevices();
    }

    private void UpdateTotalDevices()
    {
        var oldTotal = TotalDevices;
        TotalDevices = 0;
        for (var i = 0; i <= 8; i++)
        {
            if (Raylib.IsGamepadAvailable(i))
            {
                TotalDevices++;
            }
        }
        if (TotalDevices != oldTotal)
        {
            UpdateDevices();
        }
    }

    private void UpdateDevices()
    {
        _devices.Clear();
        
        for (var i = 0; i < TotalDevices; i++)
            _devices.Add(new RayLibControllerDevice(i));

        foreach (var device in _devices)
        {
            unsafe
            {
                var gamepad = Raylib.GetGamepadName(device.Id);
                var name = Marshal.PtrToStringAnsi((IntPtr)gamepad);
                Console.WriteLine("CONTROLLER NAME: " + name);
                device.Type = ControllerType.Other;
                
                if (name is null)
                    continue;
                
                if (name.Contains("box", StringComparison.InvariantCultureIgnoreCase))
                    device.Type = ControllerType.XBox;

                if (name.Contains("dual", StringComparison.InvariantCultureIgnoreCase) || name.Contains("play", StringComparison.InvariantCultureIgnoreCase))
                    device.Type = ControllerType.PlayStation;
                
                if (name.Contains("steam", StringComparison.InvariantCultureIgnoreCase))
                    device.Type = ControllerType.Steam;
                
                if (name.Contains("nin", StringComparison.InvariantCultureIgnoreCase))
                    device.Type = ControllerType.Nintendo;
            }
        }
    }
}