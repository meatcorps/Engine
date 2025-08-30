using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Interfaces.Input;

namespace Meatcorps.Engine.Hardware.ArduinoController.ArduinoController;

public class ArduinoInput : IInput
{
    public bool Enable { get; set; }
    private readonly ControllerInputEnum _button;
    private readonly int _player;
    private readonly ButtonLightsEnum? _light;
    private bool _previousState;
    public bool Down { get; private set; }
    public bool Up { get; private set; }
    public bool IsPressed { get; private set; }
    public float Normalized => Down ? 1 : 0;

    public IButtonAnimation? Animation { get; set; }
    public bool EnableLightWhenPressed { get; set; }
    public bool EnableLightWhenUnpressed { get; set; }

    public ArduinoInput(ControllerInputEnum button, int player, bool enable, ButtonLightsEnum? light = null)
    {
        _button = button;
        _player = player;
        _light = light;
        Enable = enable;
    }

    public void Update(ControllerInputEnum input1, ControllerInputEnum input2, ref ButtonLightsEnum lights1, ref ButtonLightsEnum lights2)
    {
        if (!Enable)
            return;
        
        ProcessInputState(input1, input2);
        
        UpdateButtonLights(ref lights1, ref lights2);
    }

    private void ProcessInputState(ControllerInputEnum input1, ControllerInputEnum input2)
    {
        var input = _player == 1 ? input1 : input2;
        Down = input.HasFlag(_button);
        Up = !Down;
        IsPressed = Down && !_previousState;
        _previousState = Down;
    }

    private void UpdateButtonLights(ref ButtonLightsEnum lights1, ref ButtonLightsEnum lights2)
    {
        if (_light == null)
            return;

        if (Animation != null)
        {
            if (Animation.Update(this))
                if (_player == 1)
                    lights1 |= (ButtonLightsEnum)_light;
                else
                    lights2 |= (ButtonLightsEnum)_light;
        }

        if ((EnableLightWhenPressed && Down) || (EnableLightWhenUnpressed && Up))
            if (_player == 1)
                lights1 |= (ButtonLightsEnum)_light;
            else
                lights2 |= (ButtonLightsEnum)_light;
    }
}