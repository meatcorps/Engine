using Meatcorps.Engine.Session.Data;

namespace Meatcorps.Engine.Session.Extensions;

public static class SessionDataBagExtensions
{
    public static int Inc<TEnum>(this SessionDataBag<TEnum> bag, TEnum key, int delta) where TEnum : Enum
    {
        var cur = bag.GetOrDefault<int>(key, 0) + delta;
        bag.Set(key, cur);
        return cur;
    }

    public static int ClampInt<TEnum>(this SessionDataBag<TEnum> bag, TEnum key, int min, int max) where TEnum : Enum
    {
        var cur = Math.Clamp(bag.GetOrDefault<int>(key, 0), min, max);
        bag.Set(key, cur);
        return cur;
    }

    public static SessionDataBag<TEnum> RegisterItemByValue<TEnum, T>(this SessionDataBag<TEnum> bag, TEnum type , T currentValue, string? name = null) where TEnum : Enum
    {
        bag.RegisterItem(new SessionDataItem<TEnum, T>(type, name, currentValue));
        return bag;   
    }
}