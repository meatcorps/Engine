using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.Hardware.Controllers.Interfaces;
using Meatcorps.Engine.Hardware.Controllers.Mapper;

namespace Meatcorps.Engine.SDL.Controller;

public class SDLControllerDeviceManager: IControllerDeviceManager, IDisposable
{
    private readonly List<SDLControllerDevice> _devices = new();
    private readonly FixedTimer _checkTimer = new(1000f);
    private readonly Dictionary<int, int> _assignedDevices = new();
    private readonly SDLControllerWatcher _watcher = new (true);
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
            if (deviceId == device)
                return playerId;
        }
        return -1;
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