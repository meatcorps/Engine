using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Interfaces.Input;
using Meatcorps.Engine.Core.Utilities;

namespace Meatcorps.Engine.Core.Input;

public abstract class BaseInput : IInput
{
    public virtual string Label { get; }
    public bool Enable { get; set; } = true;
    public bool Down { get; private set; }
    public bool Up { get; private set; }
    public bool IsPressed { get; private set; }
    public float Normalized { get; private set; }
    public IButtonAnimation? Animation { get; set; }
    public bool EnableLightWhenPressed { get; set; }
    public bool EnableLightWhenUnpressed { get; set; }
    
    private readonly EdgeDetector _edgeDetector = new();
    
    public BaseInput(string label)
    {
        Label = label;
    }

    protected abstract float PressedState();

    public void Update()
    {
        if (!Enable)
            return;
        
        Normalized = Math.Clamp(PressedState(), -1, 1);
        var state = Normalized;
        Down = Math.Abs(Normalized) > 0.5f;
        Up = Math.Abs(Normalized) < 0.5f;
        _edgeDetector.Update(Down);
        IsPressed = _edgeDetector.IsRisingEdge;
        Animation?.Update(this);
    }
}