using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Interfaces.Services;

namespace Meatcorps.Engine.Core.Input;

using System.Numerics;
using Meatcorps.Engine.Core.Interfaces.Input;

public class PlayerInputRouter<T> : IBackgroundService, IInputMapper<T> where T : Enum
{
    private readonly Dictionary<int, IInputMapper<T>> _playerMappers = new();
    private List<IInputMapper<T>> _inputMappers = new();
    private List<int> _profileIds = new();
    public bool AutoAssign { get; set; } = false;
    private int _useAutoMapper = 0;
    
    public void AssignMapper(int player, IInputMapper<T> mapper)
    {
        AddMapper(mapper);
        _playerMappers[player] = mapper;
    }

    public void AddMapper(IInputMapper<T> mapper)
    {
        foreach (var mapperToCheck in _inputMappers)
        {
            if (mapperToCheck.GetType() == mapper.GetType())
                return;
        }
        _inputMappers.Add(mapper);
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
        if (AutoAssign)
            return _inputMappers[_useAutoMapper].GetState(1, input);
        
        if (_playerMappers.TryGetValue(player, out var mapper))
            return mapper.GetState(player, input);

        throw new InvalidOperationException($"No input mapper assigned for player {player}");
    }

    public Vector2 GetAxis(int player, int axis = 1)
    {
        if (AutoAssign)
            return _inputMappers[_useAutoMapper].GetAxis(1, axis);
        
        if (_playerMappers.TryGetValue(player, out var mapper))
            return mapper.GetAxis(player, axis);

#if DEBUG
        throw new InvalidOperationException($"No input mapper assigned for player {player}");
#else
        return Vector2.Zero;
#endif
    }

    public void Rumble(int player, float left, float right, float duration)
    {
        if (AutoAssign)
            _inputMappers[_useAutoMapper].Rumble(1, left, right, duration);
        
        if (_playerMappers.TryGetValue(player, out var mapper))
            mapper.Rumble(player, left, right, duration);
    }

    public void AssignProfile(int profileId, int player)
    {
        var internalId = (int)MathF.Floor(profileId / 1000f);
        var profile = profileId % 1000;
        
        _playerMappers[player] = _inputMappers[internalId];
        _inputMappers[internalId].AssignProfile(profile, player);
        if (_playerMappers.TryGetValue(player, out var mapper))
            mapper.AssignProfile(profile, player);
        else
            throw new InvalidOperationException($"No input mapper assigned for internalId {internalId}");
    }

    public void UnassignProfile(int player)
    {
        foreach (var mapper in _playerMappers)
            mapper.Value.UnassignProfile(player);
    }

    public bool IsAssigned(int player)
    {
        foreach (var mapper in _playerMappers)
        {
            if (mapper.Value.IsAssigned(player))
                return true;
        }
        return false;
    }

    public bool IsConnected(int player)
    {
        if (_playerMappers.TryGetValue(player, out var mapper))
            return mapper.IsConnected(player);
        
        return false;
    }

    public bool AnyInputPressed(out int profileId, out int player)
    {
        var counter = 0;
        foreach (var inputMapper in _inputMappers)
        {
            if (inputMapper.AnyInputPressed(out profileId, out player))
            {
                profileId += counter * 1000;
                return true;
            }
            counter++;
        }
        profileId = -1;
        player = -1;
        return false;
    }

    public PlayerInputType InputType(int player)
    {
        if (_playerMappers.TryGetValue(player, out var mapper))
        {
            return mapper.InputType(player);
        }
        
        return PlayerInputType.Unknown;
    }
    
    public IReadOnlyList<int> GetAvailableProfiles()
    {
        _profileIds.Clear();
        var counter = 0;
        foreach (var mapper in _inputMappers)
        {
            foreach (var profileId in mapper.GetAvailableProfiles())
            {
                _profileIds.Add(profileId + counter * 1000);
            }
            counter++;
        }
        return _profileIds;
    }

    public void PreUpdate(float deltaTime)
    {
        if (!AutoAssign)
            return;

        if (_inputMappers.Count == 1)
            return;
        
        var counter = 0;
        foreach (var mapper in _inputMappers)
        {
            if (mapper.AnyInputPressed(out var profileId, out var playerId))
            {
                mapper.AssignProfile(profileId, 1);
                _useAutoMapper = counter;
                return;
            }

            counter++;
        }
    }

    public void Update(float deltaTime)
    {
    }

    public void LateUpdate(float deltaTime)
    {
        
    }
}