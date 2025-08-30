using System.Globalization;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Storage.Abstractions;

namespace Meatcorps.Game.Snake.Data;

[Serializable]
public class GameConfig<T> : BaseConfig<GameConfig<T>> where T : Enum
{
    public string Name { get; set; } = "Snake";
    private const string GROUP = "General";
    protected override void DoRegisterDefaultValues()
    {
        GetOrDefault("General", "Name", "Snake");
    }

    protected override GameConfig<T> Instance => this;
    
    public string GetOrDefault(T key, string? defaultValue)
    {
        if (defaultValue is null)
        {
            if (!SystemSettings.TryGetValue(GROUP, out var setting))
                throw new InvalidOperationException("Group does not exist");
            if (!setting.ContainsKey(key.ToString()))
                throw new InvalidOperationException("key does not exist");
        }
        else
        {
            if (!SystemSettings.ContainsKey(GROUP))
                SystemSettings.Add(GROUP, new Dictionary<string, string>());
            SystemSettings[GROUP][key.ToString()] = defaultValue;
        }

        defaultValue ??= SystemSettings[GROUP][key.ToString()];

        return GetOrDefault(GROUP, key.ToString(), defaultValue);
    }

    public void Set(T key, string value)
    {
        if (!SystemSettings.TryGetValue(GROUP, out var setting))
            throw new InvalidOperationException("Group does not exist");
        if (!setting.ContainsKey(key.ToString()))
            throw new InvalidOperationException("key does not exist");

        Set(GROUP, key.ToString(), value);
    }

    public int GetOrDefault(T key, int? defaultValue)
    {
        int.TryParse(GetOrDefault(key, defaultValue?.ToString(CultureInfo.InvariantCulture)),
            CultureInfo.InvariantCulture, out var result);
        return result;
    }

    public void Set(T key, int value)
    {
        Set(key, value.ToString(CultureInfo.InvariantCulture));
    }

    public float GetOrDefault(T key, float? defaultValue)
    {
        float.TryParse(GetOrDefault(key, defaultValue?.ToString(CultureInfo.InvariantCulture)), CultureInfo.InvariantCulture, out var result);
        return result;
    }

    public void Set(T key, float value)
    {
        Set(key, value.ToString(CultureInfo.InvariantCulture));
    }

    public bool GetOrDefault(T key, bool? defaultValue)
    {
        return GetOrDefault(key, defaultValue.ToString()) == "True";
    }

    public void Set(T key, bool value)
    {
        Set(key, value.ToString());
    }
    
    public static GameConfig<T> Create()
    {
        var config = GlobalObjectManager.ObjectManager.Get<GameConfig<T>>();
        if (config != null) 
            return config;
        return new GameConfig<T>();
    }
}