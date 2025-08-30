using System.Drawing;
using System.Numerics;
using Meatcorps.Engine.Core.Data;

namespace Meatcorps.Engine.Core.Extensions;

public static class Vector2Extensions
{
    public static Vector2 Min(this Vector2 a, Vector2 b) =>
        new Vector2(MathF.Min(a.X, b.X), MathF.Min(a.Y, b.Y));

    public static Vector2 Max(this Vector2 a, Vector2 b) =>
        new Vector2(MathF.Max(a.X, b.X), MathF.Max(a.Y, b.Y));

    public static Vector2 Clamp(this Vector2 value, Vector2 min, Vector2 max) =>
        new Vector2(
            MathF.Max(min.X, MathF.Min(max.X, value.X)),
            MathF.Max(min.Y, MathF.Min(max.Y, value.Y))
        );

    public static Vector2 Abs(this Vector2 value) =>
        new Vector2(MathF.Abs(value.X), MathF.Abs(value.Y));

    public static Vector2 Lerp(this Vector2 start, Vector2 end, float amount) =>
        new Vector2(
            start.X + (end.X - start.X) * amount,
            start.Y + (end.Y - start.Y) * amount
        );

    public static Vector2 LerpClamped(this Vector2 start, Vector2 end, float amount) =>
        start.Lerp(end, Math.Clamp(amount, 0f, 1f));

    public static Vector2 Normal(this Vector2 self)
    {
        return self.PerpendicularClockwise().NormalizedCopy();
    }

    public static Vector2 NormalizedCopy(this Vector2 value)
    {
        var result = Vector2.Normalize(value);
        if (result.IsNaN())
            return Vector2.Zero;
        return result;
    }

    public static void Normalize(this Vector2 value)
    {
        value = Vector2.Normalize(value);
    }

    public static Vector2 PerpendicularClockwise(this Vector2 value)
    {
        return new Vector2(value.Y, -value.X);
    }

    public static Vector2 PerpendicularCounterClockwise(this Vector2 value)
    {
        return new Vector2(-value.Y, value.X);
    }

    public static float Cross(this Vector2 self, Vector2 other)
    {
        return (self.X * other.Y) - (self.Y * other.X);
    }

    public static Vector2 ReflectVelocity(this Vector2 velocity, LineF line)
    {
        // Calculate the line vector
        var lineVector = line.End - line.Start;

        // Calculate the normal to the line (perpendicular vector)
        var normal = new Vector2(-lineVector.Y, lineVector.X);
        normal = Vector2.Normalize(normal); // Normalize to unit vector

        // Reflect the velocity
        var dotProduct = Vector2.Dot(velocity, normal);
        var reflectedVelocity = velocity - 2 * dotProduct * normal;

        return reflectedVelocity;
    }

    public static Vector2 FromAngle(float angle, float length = 1f)
    {
        var rotationOffset = MathF.PI / 2;
        return new Vector2(MathF.Cos(angle - rotationOffset), MathF.Sin(angle - rotationOffset)) * length;
    }

    public static float ToDistance(this Vector2 direction)
    {
        return Vector2.Distance(Vector2.Zero, direction);
    }

    public static float ToDistance(this Vector2 direction, Vector2 offset)
    {
        return Vector2.Distance(offset, direction);
    }

    public static Vector2 DoRotate(this Vector2 self, float angle)
    {
        self.Rotate(angle);
        return self;
    }

    public static void Rotate(this Vector2 self, float radians)
    {
        var num1 = MathF.Cos(radians);
        var num2 = MathF.Sin(radians);
        var x = self.X;
        self.X = (self.X * num1 - self.Y * num2);
        self.Y = (x * num2 + self.Y * num1);
    }

    public static bool IsNaN(this Vector2 self)
    {
        return float.IsNaN(self.X) || float.IsNaN(self.Y);
    }

    public static int GetLineInVectors(this Vector2 startPoint, Vector2 endPoint, Span<Vector2> buffer)
    {
        if (endPoint.IsNaN())
            return 0;

        var x = (int)MathF.Floor(startPoint.X);
        var y = (int)MathF.Floor(startPoint.Y);
        var x2 = (int)MathF.Floor(endPoint.X);
        var y2 = (int)MathF.Floor(endPoint.Y);

        var w = x2 - x;
        var h = y2 - y;
        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;

        if (w < 0) dx1 = -1;
        else if (w > 0) dx1 = 1;

        if (h < 0) dy1 = -1;
        else if (h > 0) dy1 = 1;

        if (w < 0) dx2 = -1;
        else if (w > 0) dx2 = 1;

        var longest = Math.Abs(w);
        var shortest = Math.Abs(h);

        if (!(longest > shortest))
        {
            longest = Math.Abs(h);
            shortest = Math.Abs(w);

            if (h < 0) dy2 = -1;
            else if (h > 0) dy2 = 1;

            dx2 = 0;
        }

        var numerator = longest >> 1;

        if (longest > buffer.Length)
            longest = buffer.Length;

        for (var i = 0; i <= longest; i++)
        {
            buffer[i] = new Vector2(x, y);
            numerator += shortest;
            if (!(numerator < longest))
            {
                numerator -= longest;
                x += dx1;
                y += dy1;
            }
            else
            {
                x += dx2;
                y += dy2;
            }
        }

        return longest + 1;
    }

    public static bool IsEqualsSafe(this Vector2 vec1, Vector2 other)
    {
        return vec1.X.EqualsSafe(other.X) && vec1.Y.EqualsSafe(other.Y);
    }

    public static float LookTowards(this Vector2 vec1, Vector2 other, Vector2 facingOrientation)
    {
        return MathF.Atan2(vec1.Y - other.Y, vec1.X - other.X) - MathF.Atan2(facingOrientation.Y, facingOrientation.X) +
               MathF.PI / 2;
    }

    public static float GetAngleWithOrientation(this Vector2 vec1, Vector2 facingOrientation)
    {
        return MathF.Atan2(vec1.Y, vec1.X) - MathF.Atan2(facingOrientation.Y, facingOrientation.X) + MathF.PI / 2;
    }

    public static PointInt ToPointInt(this Vector2 vec)
    {
        return new PointInt((int)vec.X, (int)vec.Y);
    }

    public static Vector2 NormalizedSafe(this Vector2 v, Vector2 fallback = default)
    {
        var lenSq = v.X * v.X + v.Y * v.Y;
        if (lenSq > 1e-12f)
        {
            var inv = 1.0f / MathF.Sqrt(lenSq);
            return new Vector2(v.X * inv, v.Y * inv);
        }

        return fallback;
    }

    /// <summary>Squared length (no sqrt) – faster for comparisons.</summary>
    public static float LengthSquaredFast(this Vector2 v)
    {
        return v.X * v.X + v.Y * v.Y;
    }

    /// <summary>Squared distance between two points (no sqrt).</summary>
    public static float DistanceSquared(this Vector2 a, Vector2 b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return dx * dx + dy * dy;
    }

    /// <summary>Clamp vector magnitude to max (aka truncate).</summary>
    public static Vector2 LimitMagnitude(this Vector2 v, float max)
    {
        var lenSq = v.X * v.X + v.Y * v.Y;
        var maxSq = max * max;

        if (lenSq > maxSq && lenSq > 0)
        {
            var inv = max / MathF.Sqrt(lenSq);
            return new Vector2(v.X * inv, v.Y * inv);
        }

        return v;
    }

    /// <summary>Returns a vector with exact length if possible; zero stays zero.</summary>
    public static Vector2 WithLength(this Vector2 v, float length)
    {
        var lenSq = v.X * v.X + v.Y * v.Y;

        if (lenSq > 1e-12f)
        {
            var inv = length / MathF.Sqrt(lenSq);
            return new Vector2(v.X * inv, v.Y * inv);
        }

        return Vector2.Zero;
    }

    /// <summary>Project vector a onto vector b (returns 0 if b is ~zero).</summary>
    public static Vector2 ProjectOn(this Vector2 a, Vector2 b)
    {
        var bLenSq = b.X * b.X + b.Y * b.Y;

        if (bLenSq < 1e-12f)
        {
            return Vector2.Zero;
        }

        var dot = a.X * b.X + a.Y * b.Y;
        var k = dot / bLenSq;
        return new Vector2(b.X * k, b.Y * k);
    }

    /// <summary>Reject (perpendicular component) of a from b.</summary>
    public static Vector2 RejectFrom(this Vector2 a, Vector2 b)
    {
        return a - a.ProjectOn(b);
    }
    
    public static Vector2 Warp(this Vector2 point, int width, int height)
    {
        return new Vector2(
            point.X.Wrap(width),
            point.Y.Wrap(height)
        );
    }

    public static Vector2 Warp(this Vector2 point, RectF bounds)
    {
        return new Vector2(
            bounds.X + (point.X - bounds.X).Wrap(bounds.Width),
            bounds.Y + (point.Y - bounds.Y).Wrap(bounds.Height)
        );
    }

    public static Vector2 Warp(this Vector2 point, Rect bounds)
    {
        return new Vector2(
            bounds.X + (point.X - bounds.X).Wrap(bounds.Width),
            bounds.Y + (point.Y - bounds.Y).Wrap(bounds.Height)
        );
    }
}