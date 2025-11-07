using Meatcorps.Engine.Hardware.Controllers.Enums;

namespace Meatcorps.Engine.Hardware.Controllers.Interfaces;

public interface IControllerDeviceManager
{
    public IControllerDevice? GetDevice(int player);
    public IEnumerable<IControllerDevice> GetDevices();
    public int TotalDevices { get; }
    public void AssignDevice(int player, int device);
    public void UnassignDevice(int player);
    public bool IsDeviceAssigned(int player);
    public int WhichPlayerOnDevice(int device);
    public void Initialize();
    public void Update(float deltaTime);
}