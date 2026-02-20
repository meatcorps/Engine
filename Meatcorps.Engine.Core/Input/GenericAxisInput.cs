using System.Numerics;
using Meatcorps.Engine.Core.Interfaces.Input;

namespace Meatcorps.Engine.Core.Input;

public class GenericAxisInput<T> where T : Enum
{
    private readonly IInputMapper<T> _mapper;
    private readonly Dictionary<T, Vector2> _axisDirection = new();

    public GenericAxisInput(IInputMapper<T> mapper, T left, T right, T top, T bottom)
    {
        _axisDirection[left] = new Vector2(-1, 0);
        _axisDirection[right] = new Vector2(1, 0);
        _axisDirection[top] = new Vector2(0, -1);
        _axisDirection[bottom] = new Vector2(0, 1);
        _mapper = mapper;
    }

    public Vector2 GetAxis(int player)
    {
        var direction = Vector2.Zero;
        foreach (var (key, value) in _axisDirection)
        {
            direction += value * _mapper.GetState(player, key).Normalized;
        }
        return direction;
    }
}