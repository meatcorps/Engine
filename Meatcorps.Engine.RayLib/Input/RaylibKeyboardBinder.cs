using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Input;
using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.Interfaces.Input;
using Meatcorps.Engine.Core.ObjectManager;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Input;

public class RaylibKeyboardBinder<T> : IGenericInputMapSaver<T> where T : Enum
{
    private readonly IUniversalConfig _config;
    private readonly Dictionary<string, Func<float>> _keyBinds = new();
    private readonly Dictionary<string, KeyboardKey> _keyToString = new();
    private readonly Dictionary<string, GenericInput> _originalMappings = new();

    public int TotalProfiles { get; private set; }

    public RaylibKeyboardBinder()
    {
        foreach (var key in Enum.GetValues<KeyboardKey>())
        {
            _keyBinds[key.ToString()] = () => Raylib.IsKeyDown(key) ? 1 : 0;
            _keyToString[key.ToString()] = key;
        }

        _config = GlobalObjectManager.ObjectManager.Get<IUniversalConfig>()!;
    }

    public GenericInput LoadFromConfig(int profile, T input, GenericInput map)
    {
        TotalProfiles = Math.Max(profile + 1, TotalProfiles);
        _originalMappings[input.ToString() + profile.ToString()] = map;
        var configBinding = _config.GetOrDefault("KeyboardBindings", input.ToString() + "_" + profile.ToString(), map.Label, false);
        
        if (configBinding == map.Label)
            return map;

        return new GenericInput(GetKeyBind(configBinding), configBinding);
    }

    public void SaveToConfig(int profile, T input, GenericInput map)
    {
        _config.Set("KeyboardBindings", input + "_" + profile, map.Label);
    }

    public GenericInput? DefaultMap(int profile, T input)
    {
        return _originalMappings.ContainsKey(input.ToString() + profile.ToString())
            ? _originalMappings[input.ToString() + profile.ToString()]
            : null;
    }

    public Func<float> GetKeyBind(string key)
    {
        return _keyBinds[key];
    }

    public Func<float> GetKeyBind(KeyboardKey key)
    {
        return _keyBinds[key.ToString()];
    }

    public KeyboardKey GetKeyFromString(string key)
    {
        return _keyToString[key];
    }

    public GenericInput? IsAnyKeyPressed()
    {
        foreach (var (key, binding) in _keyBinds)
            if (binding().EqualsSafe(1))
                return new GenericInput(binding, key);

        return null;
    }
}