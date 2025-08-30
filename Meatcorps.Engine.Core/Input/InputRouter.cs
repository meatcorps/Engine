namespace Meatcorps.Engine.Core.Input;

using System.Numerics;
using Meatcorps.Engine.Core.Interfaces.Input;

public class PlayerInputRouter<T> : IInputMapper<T> where T : Enum
{
    private readonly Dictionary<int, IInputMapper<T>> _playerMappers = new();

    public void AssignMapper(int player, IInputMapper<T> mapper)
    {
        _playerMappers[player] = mapper;
    }

    public bool HasMapper(int player)
    {
        return _playerMappers.ContainsKey(player);
    }

    public bool TryGetMapper(int player, out IInputMapper<T> mapper)
    {
        return _playerMappers.TryGetValue(player, out mapper!);
    }

    public bool IsMapperType<TMapper>(int player) where TMapper : class, IInputMapper<T>
    {
        return _playerMappers.TryGetValue(player, out var mapper) && mapper is TMapper;
    }

    public bool IsMapperWithManager<TManager>(int player)
    {
        return _playerMappers.TryGetValue(player, out var mapper)
               && mapper is IInputMapperWithManager<T, TManager>;
    }

    public bool TryGetManager<TManager>(int player, out TManager manager)
    {
        manager = default!;
        if (!_playerMappers.TryGetValue(player, out var mapper))
            return false;

        if (mapper is IInputMapperWithManager<T, TManager> withMgr)
        {
            manager = withMgr.Manager;
            return true;
        }
        return false;
    }

    public IInput GetState(int player, T input)
    {
        if (_playerMappers.TryGetValue(player, out var mapper))
            return mapper.GetState(player, input);

        throw new InvalidOperationException($"No input mapper assigned for player {player}");
    }

    public Vector2 GetAxis(int player, int axis = 1)
    {
        if (_playerMappers.TryGetValue(player, out var mapper))
            return mapper.GetAxis(player, axis);

#if DEBUG
        throw new InvalidOperationException($"No input mapper assigned for player {player}");
#else
        return Vector2.Zero;
#endif
    }
}