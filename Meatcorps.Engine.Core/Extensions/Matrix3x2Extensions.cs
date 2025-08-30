using System.Numerics;

namespace Meatcorps.Engine.Core.Extensions;

public static class Matrix3x2Extensions
{
    public static Vector2 Transform(this Matrix3x2 matrix, Vector2 v)
    {
        return new Vector2(
            v.X * matrix.M11 + v.Y * matrix.M21 + matrix.M31,
            v.X * matrix.M12 + v.Y * matrix.M22 + matrix.M32
        );
    }

    public static void TransformRectangle(ref Vector2 center, ref Vector2 halfExtents, ref Matrix3x2 matrix)
    {
        center = matrix.Transform(center);
        var x = halfExtents.X;
        var y = halfExtents.Y;

        halfExtents.X = x * MathF.Abs(matrix.M11) + y * MathF.Abs(matrix.M12);
        halfExtents.Y = x * MathF.Abs(matrix.M21) + y * MathF.Abs(matrix.M22);
    }

    public static void TransformOrientedRectangle(ref Vector2 center, ref Matrix3x2 orientation, ref Matrix3x2 matrix)
    {
        center = matrix.Transform(center);
        orientation *= matrix;
        orientation.M31 = 0.0f;
        orientation.M32 = 0.0f;
    }
}