using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.Interfaces.Input;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Hardware.ArduinoController.ArduinoController;
using Meatcorps.Engine.Hardware.ArduinoController.Services;

namespace Meatcorps.Engine.Hardware.ArduinoController.Modules;

public class ArduinoControllerModule
{
    private List<ControllerInputEnum> _inputsEnabled = new();
    private bool _player2Enabled = false;
    
    public static ArduinoControllerModule Setup()
    {
        return new ArduinoControllerModule();
    }

    public ArduinoControllerModule EnableInput(ControllerInputEnum input)
    {
        if (_inputsEnabled.Contains(input))
            return this;
        
        _inputsEnabled.Add(input);
        return this;
    }

    public ArduinoControllerModule EnableJoystick(bool upDown = true, bool leftRight = true)
    {
        if (upDown)
        {
            EnableInput(ControllerInputEnum.Up);
            EnableInput(ControllerInputEnum.Down);
        }

        if (leftRight)
        {
            EnableInput(ControllerInputEnum.Left);
            EnableInput(ControllerInputEnum.Right);
        }
        return this;
    }

    public ArduinoControllerModule EnableButtons(int total)
    {
        if (total >= 1)
            EnableInput(ControllerInputEnum.Button1);
        if (total >= 2)
            EnableInput(ControllerInputEnum.Button2);
        if (total >= 3)
            EnableInput(ControllerInputEnum.Button3);
        if (total >= 4)
            EnableInput(ControllerInputEnum.Button4);
        if (total >= 5)
            EnableInput(ControllerInputEnum.Button5);
        if (total == 6)
            EnableInput(ControllerInputEnum.Button6);
        return this;
    }

    public ArduinoControllerModule EnablePlayer2()
    {
        _player2Enabled = true;
        return this;
    }

    public ArduinoControllerModule EnableAll()
    {
        EnableJoystick();
        EnableButtons(6);
        EnablePlayer2();
        return this;
    }
    
    public ArduinoControllerModule SetupRouter<T>(IInputMapper<T> inputMapper, PlayerInputRouter<T>? router = null) where T : Enum
    {
        if (router == null)
        {
            router = new PlayerInputRouter<T>();
            GlobalObjectManager.ObjectManager.Register(router);
        }
        
        router.AssignMapper(1, inputMapper);
        if (_player2Enabled)
            router.AssignMapper(2, inputMapper);

        return this;
    } 
    
    public ArduinoControllerModule Load(string comPort)
    {
        var controllerCommunicator = new ArduinoControllerCommunication(comPort);
        var controller = new ArduinoControllerManager(controllerCommunicator,
            _inputsEnabled.ToArray(), _player2Enabled);
        
        GlobalObjectManager.ObjectManager.Register(controllerCommunicator);
        GlobalObjectManager.ObjectManager.Register(controller);
        GlobalObjectManager.ObjectManager.Add<IBackgroundService>(new ArduinoControllerService(controller));
        
        return this;
    }
}