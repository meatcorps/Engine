using System.Runtime.InteropServices;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.Hardware.Controllers.Enums;
using Meatcorps.Engine.Hardware.Controllers.Interfaces;
using Meatcorps.Engine.Hardware.Controllers.Mapper;
using Meatcorps.Engine.SDL.Controller;

namespace Meatcorps.Engine.RayLib.Input;

public class SDLControllerDeviceManager: IControllerDeviceManager, IDisposable
{
    private List<SDLControllerDevice> _devices = new();
    private FixedTimer _checkTimer = new(1000f);
    private Dictionary<int, int> _assignedDevices = new();
    private Dictionary<string, ControllerType> _deviceMapping = new();
    private SDLControllerWatcher _watcher = new SDLControllerWatcher(true);
    private int _totalDevices;
    
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

    public int TotalDevices => _watcher.ControllerCount;

    public void AssignDevice(int player, int device)
    {
        _assignedDevices[player] = device;
    }

    public bool IsDeviceAssigned(int player)
    {
        return GetDevice(player) is not null;
    }

    public void Initialize()
    {
        _watcher.Initialize();
        
        if (File.Exists("gamecontrollerdb.txt"))
        {
            var db = File.ReadAllText("gamecontrollerdb.txt");
            SDL2.SDL.SDL_GameControllerAddMapping(db);
        }
        
        UpdateTotalDevices();
    }

    public void Update(float deltaTime)
    {
        _watcher.Update();
        _checkTimer.Update(deltaTime);
        UpdateTotalDevices();
    }

    private void UpdateTotalDevices()
    {
        if (_totalDevices != TotalDevices)
            UpdateDevices();
        
        _totalDevices = TotalDevices;
    }

    private void UpdateDevices()
    {
        _devices.Clear();

        foreach (var device in _watcher.GetControllers())
        {
            var controller = new SDLControllerDevice();
            controller.SetControllerDevice(device);
            controller.Type = ControllerVendorToTypeMapping.Get(device.GetVendorId());
            _devices.Add(controller);
        }
    }

    public void Dispose()
    {
        _watcher.Dispose();
    }
}