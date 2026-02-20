using System.Numerics;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Interfaces.Input;
using Meatcorps.Engine.Core.Interfaces.Services;

namespace Meatcorps.Engine.Core.Input;

public class GenericMapper<T>: IInputMapper<T>, IBackgroundService where T : Enum
{
    private readonly Dictionary<int, Dictionary<T, GenericInput>> _inputMap = new();
    private readonly Dictionary<int, Dictionary<int, GenericAxisInput<T>>> _inputAxisMap = new();
    private readonly Dictionary<int, int> _indexProfile = new();
    private readonly GenericInput _defaultInput = new GenericInput(() => 0, "UNKNOWN");
    private readonly IGenericInputMapSaver<T>? _loaderAndSaver;

    public GenericMapper(IGenericInputMapSaver<T>? loaderAndSaver = null)
    {
        _loaderAndSaver = loaderAndSaver;
    }
    
    public IReadOnlyDictionary<T, GenericInput> GetInputs(int profileId)
    {
        if (!_inputMap.TryGetValue(profileId, out var playerInputs))
            throw new InvalidOperationException($"No input map for profile {profileId}");
        return playerInputs;
    }

    public IInput GetStateByProfile(int profileId, T input)
    {
        if (!_inputMap.TryGetValue(profileId, out var playerInputs))
            return _defaultInput;
        return playerInputs.GetValueOrDefault(input, _defaultInput);
    }
    
    public GenericMapper<T> AddInput(int profileId, T input, GenericInput inputState)
    {
        if (!_inputMap.TryGetValue(profileId, out var playerInputs))
            _inputMap[profileId] = playerInputs = new Dictionary<T, GenericInput>();
        playerInputs[input] = _loaderAndSaver?.LoadFromConfig(profileId, input, inputState) ?? inputState;
        return this;
    }

    public void SetInput(int profileId, T input, GenericInput inputState)
    {
        if (!_inputMap.TryGetValue(profileId, out var playerInputs))
            _inputMap[profileId] = playerInputs = new Dictionary<T, GenericInput>();
        CheckIfAlreadyAssigned(profileId, inputState); 
        playerInputs[input] = inputState;
        _loaderAndSaver?.SaveToConfig(profileId, input, inputState);
    }
    
    public GenericMapper<T> AddInput(int profileId, T input, string label, Func<float> pressedFunc)
    {
        AddInput(profileId, input, new GenericInput(pressedFunc, label));
        return this;
    }
    
    public GenericMapper<T> AddInput(int profileId, T input, string label, Func<bool> pressedFunc)
    {
        AddInput(profileId, input, new GenericInput(() => pressedFunc() ? 1 : 0, label));
        return this;
    }
    
    public GenericMapper<T> AddAxis(int profileId, int axis, T left, T right, T up, T down)
    {
        if (!_inputAxisMap.TryGetValue(profileId, out var playerIndexSet))
            _inputAxisMap[profileId] = playerIndexSet = new Dictionary<int, GenericAxisInput<T>>();
        playerIndexSet[axis] = new GenericAxisInput<T>(this, left, right, up, down);
        return this;
    }

    public void Reset(int profileId, T input)
    {
        if (!_inputMap.TryGetValue(profileId, out var playerInputs))
            _inputMap[profileId] = playerInputs = new Dictionary<T, GenericInput>();

        var mapping = _loaderAndSaver?.DefaultMap(profileId, input);

        if (mapping is null) 
            return;
        
        playerInputs[input] = mapping;
    }
    
    public IInput GetState(int player, T input)
    {
        if (!_indexProfile.TryGetValue(player, out var profileId))
        {
            return _defaultInput;
        }
        
        if (!_inputMap.TryGetValue(profileId, out var playerInputs))
            return _defaultInput;
            //throw new InvalidOperationException($"No input map for profile {profileId}");
        return playerInputs.GetValueOrDefault(input, _defaultInput);
            //throw new InvalidOperationException($"No input state for input {input} on profile {profileId}");
    }

    public Vector2 GetAxis(int player, int axis = 1)
    {
        if (!_indexProfile.TryGetValue(player, out var profileId))
        {
            return Vector2.Zero;
        }
        
        if (!_inputAxisMap.TryGetValue(profileId, out var playerIndexSet))
            throw new InvalidOperationException($"No input map for player {player}");
        if (!playerIndexSet.TryGetValue(axis, out var inputState))
            throw new InvalidOperationException($"No input state for axis {axis} on player {player}");
        
        return inputState.GetAxis(player);
    }

    public void Rumble(int player, float left, float right, float duration)
    {
        // No rumble support
    }

    public void AssignProfile(int profileId, int player)
    {
        var existingProfile = -1;
        foreach (var (plaId, proId) in _indexProfile)
        {
            if (proId == profileId)
            {
                existingProfile = plaId;
                break;
            }
        }
        UnassignProfile(existingProfile);
        _indexProfile[player] = profileId;
    }

    public void UnassignProfile(int player)
    {
        _indexProfile.Remove(player);
    }

    public bool IsAssigned(int player)
    {
        return _indexProfile.ContainsKey(player);
    }

    public bool IsConnected(int player)
    {
        return _indexProfile.TryGetValue(player, out _);
    }

    public bool AnyInputPressed(out int profileId, out int playerid)
    {
        foreach (var inputs in _inputMap)
        {
            foreach (var input in inputs.Value)
            {
                if (input.Value.IsPressed)
                {
                    profileId = inputs.Key;
                    playerid = -1;
                    foreach (var (plaId, proId) in _indexProfile)
                    {
                        if (proId == profileId)
                        {
                            playerid = plaId;
                            break;
                        }
                    }
                    return true;
                }   
            }
        }
        profileId = -1;
        playerid = -1;
        return false;
    }

    public PlayerInputType InputType(int _)
    {
        return PlayerInputType.KeyboardMouse; 
    } 
    
    public IReadOnlyList<int> GetAvailableProfiles()
    {
        return _inputMap.Keys.ToList();
    }

    public void PreUpdate(float deltaTime)
    {
        foreach (var (_, playerInputs) in _inputMap)
            foreach (var (_, inputState) in playerInputs)
                inputState.Update();
    }

    public void Update(float deltaTime)
    {
    }

    public void LateUpdate(float deltaTime)
    {
    }

    private void CheckIfAlreadyAssigned(int profileId, GenericInput inputState)
    {
        if (!_inputMap.TryGetValue(profileId, out var playerInputs))
            _inputMap[profileId] = playerInputs = new Dictionary<T, GenericInput>();

        foreach (var (key, binding) in playerInputs.ToArray())
        {
            if (binding.Label == inputState.Label)
                playerInputs[key] = _defaultInput;
        }
    }
}