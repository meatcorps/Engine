using System.Numerics;
using System.Runtime.CompilerServices;
using Meatcorps.Engine.Core.Extensions;

namespace Meatcorps.Engine.Core.Utilities;

public class PrimitivesHelper
{
    public static bool IntersectsSlab(
        float positionCoordinate,
        float directionCoordinate,
        float slabMinimum,
        float slabMaximum,
        ref float rayMinimumDistance,
        ref float rayMaximumDistance)
    {
        if (Math.Abs(directionCoordinate) < 1.401298464324817E-45)
            return positionCoordinate >= slabMinimum && positionCoordinate <= slabMaximum;
        var num1 = (slabMinimum - positionCoordinate) / directionCoordinate;
        var num2 = (slabMaximum - positionCoordinate) / directionCoordinate;
        if (num1 > num2)
            (num1, num2) = (num2, num1);

        rayMinimumDistance = num1 > rayMinimumDistance ? num1 : rayMinimumDistance;
        rayMaximumDistance = num2 < rayMaximumDistance ? num2 : rayMaximumDistance;
        return rayMinimumDistance <= rayMaximumDistance;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void CreateRectangleFromPoints(
        IReadOnlyList<Vector2> points,
        out Vector2 minimum,
        out Vector2 maximum)
    {
        if (points == null || points.Count == 0)
        {
            minimum = Vector2.Zero;
            maximum = Vector2.Zero;
        }
        else
        {
            minimum = maximum = points[0];
            for (int index = points.Count - 1; index > 0; --index)
            {
                var point = points[index];
                minimum = minimum.Min(point);
                maximum = maximum.Max(point);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TransformRectangle(
        ref Vector2 center,
        ref Vector2 halfExtents,
        ref Matrix3x2 transformMatrix)
    {
        center = transformMatrix.Transform(center);
        float x = halfExtents.X;
        float y = halfExtents.Y;
        halfExtents.X = (float)(x * Math.Abs(transformMatrix.M11) + y * Math.Abs(transformMatrix.M12));
        halfExtents.Y = (float)(x * Math.Abs(transformMatrix.M21) + y * Math.Abs(transformMatrix.M22));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TransformOrientedRectangle(
        ref Vector2 center,
        ref Matrix3x2 orientation,
        ref Matrix3x2 transformMatrix)
    {
        center = transformMatrix.Transform(center);
        orientation *= transformMatrix;
        orientation.M31 = 0.0f;
        orientation.M32 = 0.0f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SquaredDistanceToPointFromRectangle(
        Vector2 minimum,
        Vector2 maximum,
        Vector2 point)
    {
        var pointFromRectangle = 0.0f;
        if (point.X < minimum.X)
        {
            var num = minimum.X - point.X;
            pointFromRectangle += num * num;
        }
        else if (point.X > maximum.X)
        {
            var num = maximum.X - point.X;
            pointFromRectangle += num * num;
        }

        if (point.Y < minimum.Y)
        {
            var num = minimum.Y - point.Y;
            pointFromRectangle += num * num;
        }
        else if (point.Y > maximum.Y)
        {
            var num = maximum.Y - point.Y;
            pointFromRectangle += num * num;
        }

        return pointFromRectangle;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ClosestPointToPointFromRectangle(
        Vector2 minimum,
        Vector2 maximum,
        Vector2 point,
        out Vector2 result)
    {
        result = point;
        if (result.X < minimum.X)
            result.X = minimum.X;
        else if (result.X > maximum.X)
            result.X = maximum.X;
        if (result.Y < minimum.Y)
        {
            result.Y = minimum.Y;
        }
        else
        {
            if (result.Y <= maximum.Y)
                return;
            result.Y = maximum.Y;
        }
    }
}