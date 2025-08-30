using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Interfaces.Input;
using Meatcorps.Engine.Core.Utilities;

namespace Meatcorps.Engine.Core.Input;

public class GenericInput: IInput
{
    public string Label { get; }
    public bool Enable { get; set; } = true;
    public bool Down { get; private set; }
    public bool Up { get; private set; }
    public bool IsPressed { get; private set; }
    public float Normalized { get; private set; }
    public IButtonAnimation? Animation { get; set; }
    public bool EnableLightWhenPressed { get; set; }
    public bool EnableLightWhenUnpressed { get; set; }
    
    private readonly Func<float> _pressedFunc;
    private readonly EdgeDetector _edgeDetector = new();
    
    public GenericInput(Func<float> pressedFunc, string label)
    {
        Label = label;
        _pressedFunc = pressedFunc;
    }

    public void Update()
    {
        if (!Enable)
            return;
        
        Normalized = _pressedFunc();
        var state = Normalized;
        _edgeDetector.Update(state.EqualsSafe(1));
        IsPressed = _edgeDetector.IsRisingEdge;
        Down = Normalized < 0.5f;
        Up = Normalized > 0.5f;
        Animation?.Update(this);
    }
}