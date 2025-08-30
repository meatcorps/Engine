namespace Meatcorps.Engine.Core.Extensions;

public static class IntExtensions
{
    public static int Wrap(this int value, int size)
    {
        if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
        return ((value % size) + size) % size;   // 0..size-1
    }
}