using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Hardware.ArduinoController.ArduinoController;

namespace Meatcorps.Engine.Hardware.ArduinoController.Services;

public class ArduinoControllerService : IBackgroundService
{
    private readonly ArduinoControllerManager _arduinoController;

    public ArduinoControllerService(ArduinoControllerManager arduinoController)
    {
        _arduinoController = arduinoController;
    }
    
    public void PreUpdate(float deltaTime)
    {
        _arduinoController.Update();
    }

    public void Update(float deltaTime)
    {
    }

    public void LateUpdate(float deltaTime)
    {
    }
}