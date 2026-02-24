using System.Numerics;
using Meatcorps.Engine.Core.Interfaces.Input;

namespace Meatcorps.Engine.Core.Input;

/// <summary>
/// Derives a Vector2 axis from four directional IInput bindings (left, right, up, down). Accumulates normalized contributions from each direction to produce a combined axis value.
/// </summary>
public class GenericAxisInput<T> where T : Enum
{
    private readonly IInputMapper<T> _mapper;
    private readonly Dictionary<T, Vector2> _axisDirection = new();

    /// <param name="mapper">The input mapper used to retrieve per-direction IInput state.</param>
    /// <param name="left">The input enum value mapped to the left direction.</param>
    /// <param name="right">The input enum value mapped to the right direction.</param>
    /// <param name="top">The input enum value mapped to the up direction.</param>
    /// <param name="bottom">The input enum value mapped to the down direction.</param>
    public GenericAxisInput(IInputMapper<T> mapper, T left, T right, T top, T bottom)
    {
        _axisDirection[left] = new Vector2(-1, 0);
        _axisDirection[right] = new Vector2(1, 0);
        _axisDirection[top] = new Vector2(0, -1);
        _axisDirection[bottom] = new Vector2(0, 1);
        _mapper = mapper;
    }

    /// <summary>Returns the accumulated directional axis for the given player.</summary>
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