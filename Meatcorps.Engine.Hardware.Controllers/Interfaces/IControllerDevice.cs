using Meatcorps.Engine.Core.Interfaces.Input;
using Meatcorps.Engine.Hardware.Controllers.Enums;

namespace Meatcorps.Engine.Hardware.Controllers.Interfaces;

public interface IControllerDevice
{
    public IInput GetState(ControllerInputEnum input);
    public void Rumble(float left, float right, float duration);
    public ControllerType Type { get; }
}