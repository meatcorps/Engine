using System.Runtime.CompilerServices;

namespace Meatcorps.Engine.Core.Extensions;

public static class EnumExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasFlagFast<T>(this T value, T flag) where T : struct, Enum
    {
        var v = Unsafe.As<T, ulong>(ref value);
        var f = Unsafe.As<T, ulong>(ref flag);
        return (v & f) == f;
    }
}