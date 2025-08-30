using System.Numerics;

namespace Meatcorps.Engine.Hardware.ArduinoController.ArduinoController;

public class ArduinoControllerManager
{
    private readonly ArduinoControllerCommunication _arduinoControllerCommunication;
    private Dictionary<ControllerInputEnum, ArduinoInput> _inputsPlayer1 { get; } = new ();
    private Dictionary<ControllerInputEnum, ArduinoInput> _inputsPlayer2 { get; } = new ();
    private ButtonLightsEnum _previousButtonLights1 = 0;
    private ButtonLightsEnum _previousButtonLights2 = 0;
    private Vector2 _axisPlayer1 = Vector2.Zero;
    private Vector2 _axisPlayer2 = Vector2.Zero;

    public ArduinoControllerManager(ArduinoControllerCommunication arduinoControllerCommunication, ControllerInputEnum[] enabledInputs, bool player2Enabled)
    {
        _arduinoControllerCommunication = arduinoControllerCommunication;
        foreach (var target in Enum.GetValues<ControllerInputEnum>())
        {
            _inputsPlayer1.Add(target, new ArduinoInput(target, 1, enabledInputs.Contains(target), MapToButtonLights(target)));
            _inputsPlayer2.Add(target, new ArduinoInput(target, 2, enabledInputs.Contains(target) && player2Enabled, MapToButtonLights(target)));
        }
    }

    public ArduinoInput GetState(int player, ControllerInputEnum input)
    {
        return player == 1 ? _inputsPlayer1[input] : _inputsPlayer2[input];
    }
    
    public Vector2 GetAxis(int player) => player == 1 ? _axisPlayer1 : _axisPlayer2;

    public IEnumerable<ArduinoInput> GetInputs(int player)
    {
        return player == 1 
            ? _inputsPlayer1.Values.Where(x => x.Enable) 
            : _inputsPlayer2.Values.Where(x => x.Enable);
    }
    
    public void Update()
    {
        _axisPlayer1 = Vector2.Zero;
        _axisPlayer2 = Vector2.Zero;
        ButtonLightsEnum buttonLights1 = 0;
        ButtonLightsEnum buttonLights2 = 0;
        
        foreach (var target in Enum.GetValues<ControllerInputEnum>())
        {
            var targetPlayer1 = _inputsPlayer1[target];
            var targetPlayer2 = _inputsPlayer2[target];
            
            targetPlayer1.Update(
                _arduinoControllerCommunication.ControllerState1, 
                _arduinoControllerCommunication.ControllerState2,
                ref buttonLights1,
                ref buttonLights2);
            targetPlayer2.Update(
                _arduinoControllerCommunication.ControllerState1, 
                _arduinoControllerCommunication.ControllerState2,
                ref buttonLights1,
                ref buttonLights2);

            if (target == ControllerInputEnum.Down && targetPlayer1.Down)
                _axisPlayer1.Y = 1;
            if (target == ControllerInputEnum.Up && targetPlayer1.Down)
                _axisPlayer1.Y = -1;
            if (target == ControllerInputEnum.Right && targetPlayer1.Down)
                _axisPlayer1.X = 1;
            if (target == ControllerInputEnum.Left && targetPlayer1.Down)
                _axisPlayer1.X = -1;
            if (target == ControllerInputEnum.Down && targetPlayer2.Down)
                _axisPlayer2.Y = 1;
            if (target == ControllerInputEnum.Up && targetPlayer2.Down)
                _axisPlayer2.Y = -1;
            if (target == ControllerInputEnum.Right && targetPlayer2.Down)
                _axisPlayer2.X = 1;
            if (target == ControllerInputEnum.Left && targetPlayer2.Down)
                _axisPlayer2.X = -1;
        }
        
        if (_previousButtonLights1 != buttonLights1 || _previousButtonLights2 != buttonLights2)
            _arduinoControllerCommunication.SetButtonLights(buttonLights1, buttonLights2);
        
        _previousButtonLights1 = buttonLights1;
        _previousButtonLights2 = buttonLights2;
    }


    private ButtonLightsEnum? MapToButtonLights(ControllerInputEnum input)
    {
        return input switch
        {
            ControllerInputEnum.Button1 => ButtonLightsEnum.Button1,
            ControllerInputEnum.Button2 => ButtonLightsEnum.Button2,
            ControllerInputEnum.Button3 => ButtonLightsEnum.Button3,
            ControllerInputEnum.Button4 => ButtonLightsEnum.Button4,
            ControllerInputEnum.Button5 => ButtonLightsEnum.Button5,
            ControllerInputEnum.Button6 => ButtonLightsEnum.Button6,
            _ => null
        };
    }
}