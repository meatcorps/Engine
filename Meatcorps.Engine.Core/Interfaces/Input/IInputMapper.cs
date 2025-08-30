using System.Numerics;

namespace Meatcorps.Engine.Core.Interfaces.Input;

public interface IInputMapper<in T> where T : Enum
{
    IInput GetState(int player, T input);
    Vector2 GetAxis(int player, int axis = 1);
}