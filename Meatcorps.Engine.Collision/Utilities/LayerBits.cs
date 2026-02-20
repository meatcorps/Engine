using Meatcorps.Engine.Core.Settings;

namespace Meatcorps.Engine.Collision.Utilities;

public static class LayerBits
{
    // 1 << enum value (supports negative-check & >= 32 guard in debug)
    public static uint Bit<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        var i = Convert.ToInt32(value);

        if (MeatcorpsEngineLibSettings.IsDebug)
        {
            if (i < 0 || i > 31)
                throw new ArgumentOutOfRangeException(nameof(value), $"Layer index {i} must be in [0,31].");
        }

        return 1u << i;
    }

    // Combine many enum values into one mask
    public static uint MaskOf<TEnum>(params TEnum[] values) where TEnum : struct, Enum
    {
        var mask = 0u;

        if (values.Length == 0)
            return mask;

        foreach (var t in values)
        {
            mask |= Bit(t);
        }

        return mask;
    }

    // Mutators
    public static uint Add(this uint mask, uint bits)
    {
        return mask | bits;
    }

    public static uint Remove(this uint mask, uint bits)
    {
        return mask & ~bits;
    }

    public static bool Has(this uint mask, uint bits)
    {
        return (mask & bits) != 0u;
    }

    // Nice-to-have: decode mask back to enum names (for debugging / tooling)
    public static IEnumerable<string> NamesFromMask<TEnum>(uint mask) where TEnum : struct, Enum
    {
        var names = Enum.GetNames(typeof(TEnum));
        var values = (TEnum[])Enum.GetValues(typeof(TEnum));

        for (var i = 0; i < values.Length; i++)
        {
            var bit = Bit(values[i]);

            if ((mask & bit) != 0u)
                yield return names[i];
        }
    }
}