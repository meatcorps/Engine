using Meatcorps.Engine.Session.Data;
using Newtonsoft.Json;

namespace Meatcorps.Engine.Session.Extensions;

public static class SessionDataBagExtensions
{
    public static int Inc<TEnum>(this SessionDataBag<TEnum> bag, TEnum key, int delta) where TEnum : Enum
    {
        var cur = bag.GetOrDefault(key, 0) + delta;
        bag.Set(key, cur);
        return cur;
    }

    public static int ClampInt<TEnum>(this SessionDataBag<TEnum> bag, TEnum key, int min, int max) where TEnum : Enum
    {
        var cur = Math.Clamp(bag.GetOrDefault(key, 0), min, max);
        bag.Set(key, cur);
        return cur;
    }

    public static SessionDataBag<TEnum> RegisterItemByValue<TEnum, T>(this SessionDataBag<TEnum> bag, TEnum type , T currentValue, string? name = null) where TEnum : Enum
    {
        bag.RegisterItem(new SessionDataItem<TEnum, T>(type, name, currentValue));
        return bag;   
    }
    
    public static SessionDataBag<TEnum> RegisterComplexItem<TEnum, T>(this SessionDataBag<TEnum> bag, TEnum type , T currentValue, JsonSerializerSettings? settings = null, string? name = null) where TEnum : Enum where T : class
    {
        name ??= type.ToString();
        settings ??= new JsonSerializerSettings();
        bag.RegisterItem(new SessionDataItemComplex<TEnum, T>(type, name, currentValue, settings));
        return bag;   
    }
}