using System.Numerics;

namespace Meatcorps.Engine.Core.Input;

public class GenericAxisInput<T> where T : Enum
{
    private readonly GenericMapper<T> _mapper;
    private readonly int _player;
    private Dictionary<T, Vector2> _axisDirection = new();

    public GenericAxisInput(GenericMapper<T> mapper, int player, T left, T right, T top, T bottom)
    {
        _axisDirection[left] = new Vector2(-1, 0);
        _axisDirection[right] = new Vector2(1, 0);
        _axisDirection[top] = new Vector2(0, -1);
        _axisDirection[bottom] = new Vector2(0, 1);
        _mapper = mapper;
        _player = player;
    }

    public Vector2 GetAxis()
    {
        var direction = Vector2.Zero;
        foreach (var (key, value) in _axisDirection)
        {
            direction += value * _mapper.GetState(_player, key).Normalized;
        }
        return direction;
    }
}