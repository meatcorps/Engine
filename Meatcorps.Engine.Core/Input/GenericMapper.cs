using System.Numerics;
using Meatcorps.Engine.Core.Interfaces.Input;
using Meatcorps.Engine.Core.Interfaces.Services;

namespace Meatcorps.Engine.Core.Input;

public class GenericMapper<T>: IInputMapper<T>, IBackgroundService where T : Enum
{
    private Dictionary<int, Dictionary<T, GenericInput>> _inputMap = new();
    private Dictionary<int, Dictionary<int, GenericAxisInput<T>>> _inputAxisMap = new();

    public IReadOnlyDictionary<T, GenericInput> GetInputs(int player)
    {
        if (!_inputMap.TryGetValue(player, out var playerInputs))
            throw new InvalidOperationException($"No input map for player {player}");
        return playerInputs;
    }
    
    public GenericMapper<T> AddInput(int player, T input, GenericInput inputState)
    {
        if (!_inputMap.TryGetValue(player, out var playerInputs))
            _inputMap[player] = playerInputs = new Dictionary<T, GenericInput>();
        playerInputs[input] = inputState;
        return this;
    }
    
    public GenericMapper<T> AddInput(int player, T input, string label, Func<float> pressedFunc)
    {
        AddInput(player, input, new GenericInput(pressedFunc, label));
        return this;
    }
    
    public GenericMapper<T> AddInput(int player, T input, string label, Func<bool> pressedFunc)
    {
        AddInput(player, input, new GenericInput(() => pressedFunc() ? 1 : 0, label));
        return this;
    }
    
    public GenericMapper<T> AddAxis(int player, int axis, T left, T right, T top, T bottom)
    {
        if (!_inputAxisMap.TryGetValue(player, out var playerIndexSet))
            _inputAxisMap[player] = playerIndexSet = new Dictionary<int, GenericAxisInput<T>>();
        playerIndexSet[axis] = new GenericAxisInput<T>(this, player, left, right, top, bottom);
        return this;
    }
    
    public IInput GetState(int player, T input)
    {
        if (!_inputMap.TryGetValue(player, out var playerInputs))
            throw new InvalidOperationException($"No input map for player {player}");
        if (!playerInputs.TryGetValue(input, out var inputState))
            throw new InvalidOperationException($"No input state for input {input} on player {player}");
        return inputState;
    }

    public Vector2 GetAxis(int player, int axis = 1)
    {
        if (!_inputAxisMap.TryGetValue(player, out var playerIndexSet))
            throw new InvalidOperationException($"No input map for player {player}");
        if (!playerIndexSet.TryGetValue(axis, out var inputState))
            throw new InvalidOperationException($"No input state for axis {axis} on player {player}");
        
        return inputState.GetAxis();
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
}