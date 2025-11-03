using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Interfaces.Input;
using Meatcorps.Engine.Core.Utilities;

namespace Meatcorps.Engine.Core.Input;

public class GenericInput: BaseInput
{
    private readonly Func<float> _pressedFunc;
    
    public GenericInput(Func<float> pressedFunc, string label): base(label)
    {
        _pressedFunc = pressedFunc;
    }

    protected override float PressedState()
    {
        return _pressedFunc();
    }
}