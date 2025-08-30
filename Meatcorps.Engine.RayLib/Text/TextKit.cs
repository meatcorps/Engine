using System.Numerics;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.UI;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Text;

public static class TextKit
{
    // -------- Measure --------

    public static Vector2 MeasureLine(ref TextStyle s, string line)
        => Raylib.MeasureTextEx(s.Font, line, s.Size, s.Spacing);

    public static Vector2 Measure(TextStyle s, string text)
    {
        var lines = text.Split('\n');
        var w = 0f;
        var h = 0f;
        var advance = s.Size * s.LineHeight;
        foreach (var ln in lines)
        {
            var sz = MeasureLine(ref s, ln);
            if (sz.X > w) w = sz.X;
            h += (lines.Length == 1) ? sz.Y : advance;
        }

        return new Vector2(w, h <= 0 ? s.Size : h);
    }

    // -------- Word wrap --------

    public static List<string> Wrap(ref TextStyle s, string text, float maxWidth)
    {
        var result = new List<string>();
        foreach (var paragraph in text.Split('\n'))
        {
            var words = paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
            {
                result.Add("");
                continue;
            }

            var line = words[0];
            for (var i = 1; i < words.Length; i++)
            {
                var test = line + " " + words[i];
                var size = Raylib.MeasureTextEx(s.Font, test, s.Size, s.Spacing);
                if (size.X <= maxWidth) line = test;
                else
                {
                    result.Add(line);
                    line = words[i];
                }
            }

            result.Add(line);
        }

        return result;
    }

    // -------- Bounds helpers --------

    public static Rectangle GetBounds(ref TextStyle s, string text, Vector2 position)
    {
        var sz = Measure(s, text);
        return new Rectangle(position.X, position.Y, sz.X, sz.Y);
    }

    public static Vector2 AlignPosition(Rectangle rect, Vector2 size, HAlign h, VAlign v, bool pixelSnap = true)
    {
        var x = h switch
        {
            HAlign.Left => rect.X,
            HAlign.Center => rect.X + (rect.Width - size.X) * 0.5f,
            HAlign.Right => rect.X + (rect.Width - size.X),
            _ => rect.X
        };
        var y = v switch
        {
            VAlign.Top => rect.Y,
            VAlign.Middle => rect.Y + (rect.Height - size.Y) * 0.5f,
            VAlign.Bottom => rect.Y + (rect.Height - size.Y),
            _ => rect.Y
        };
        if (pixelSnap)
        {
            x = MathF.Round(x);
            y = MathF.Round(y);
        }

        return new Vector2(x, y);
    }

    // -------- Draw (single/multi-line) --------
    public static void Draw(ref TextStyle s, string text, Vector2 position, Anchor anchor = Anchor.TopLeft, bool pixelSnap = true)
    {
        if (pixelSnap) position = new Vector2(MathF.Round(position.X), MathF.Round(position.Y));

        if (anchor != Anchor.TopLeft)
        {
            var size = Measure(s, text);
            position -= UIAnchorHelper.ResolveAnchorPixel(anchor, Vector2.Zero, (int)size.X, (int)size.Y);
        }
        
        var lines = text.Split('\n');
        float y = position.Y;
        float advance = s.Size * s.LineHeight;

        foreach (var ln in lines)
        {
            Vector2 pos = new Vector2(position.X, y);

            if (s.UseShadow)
                Raylib.DrawTextEx(s.Font, ln, pos + s.ShadowOffset, s.Size, s.Spacing, s.ShadowColor);

            if (s.UseOutline)
                DrawOutlineText(ref s, ln, pos);

            Raylib.DrawTextEx(s.Font, ln, pos, s.Size, s.Spacing, s.Color);
            y += (lines.Length == 1) ? MeasureLine(ref s, ln).Y : advance;
        }
    }

    public static void DrawWrapped(ref TextStyle s, string text, Rectangle rect, HAlign h = HAlign.Left,
        VAlign v = VAlign.Top, bool pixelSnap = true)
    {
        var lines = Wrap(ref s, text, rect.Width);
        var advance = s.Size * s.LineHeight;
        var totalH = lines.Count == 0
            ? 0
            : (lines.Count == 1 ? MeasureLine(ref s, lines[0]).Y : lines.Count * advance);

        var blockW = 0f;
        foreach (var ln in lines) blockW = MathF.Max(blockW, MeasureLine(ref s, ln).X);
        Vector2 origin = AlignPosition(rect, new Vector2(blockW, totalH), h, v, pixelSnap);

        var y = origin.Y;
        foreach (var ln in lines)
        {
            var lineSize = MeasureLine(ref s, ln);
            var x = h switch
            {
                HAlign.Left => origin.X,
                HAlign.Center => origin.X + (blockW - lineSize.X) * 0.5f,
                HAlign.Right => origin.X + (blockW - lineSize.X),
                _ => origin.X
            };
            if (pixelSnap)
            {
                x = MathF.Round(x);
                y = MathF.Round(y);
            }

            var pos = new Vector2(x, y);

            if (s.UseShadow)
                Raylib.DrawTextEx(s.Font, ln, pos + s.ShadowOffset, s.Size, s.Spacing, s.ShadowColor);

            if (s.UseOutline)
                DrawOutlineText(ref s, ln, pos);

            Raylib.DrawTextEx(s.Font, ln, pos, s.Size, s.Spacing, s.Color);
            y += (lines.Count == 1) ? lineSize.Y : advance;
        }
    }

    private static void DrawOutlineText(ref TextStyle s, string line, Vector2 pos)
    {
        if (s.PixelOutline)
            DrawPixelOutline(ref s, line, pos);
        else
            DrawSoftOutline(ref s, line, pos);
    }

    private static void DrawSoftOutline(ref TextStyle s, string line, Vector2 pos)
    {
        var size = Math.Max(1, s.OutlineSize);

        for (var dy = -size; dy <= size; dy++)
        for (var dx = -size; dx <= size; dx++)
        {
            if (dx == 0 && dy == 0) continue;
            Raylib.DrawTextEx(s.Font, line, pos + new Vector2(dx, dy), s.Size, s.Spacing, s.OutlineColor);
        }
    }

    private static void DrawPixelOutline(ref TextStyle s, string line, Vector2 pos)
    {
        var size = Math.Max(1, s.OutlineSize);
        // Only draw pure axis-aligned offsets (NES/arcade look)
        for (var i = -size; i <= size; i++)
        {
            if (i == 0)
                continue;

            // Horizontal
            Raylib.DrawTextEx(s.Font, line, pos + new Vector2(i, 0), s.Size, s.Spacing, s.OutlineColor);
            // Vertical
            Raylib.DrawTextEx(s.Font, line, pos + new Vector2(0, i), s.Size, s.Spacing, s.OutlineColor);
        }

        // Diagonals for square-cornered look
        for (var dx = -size; dx <= size; dx += size * 2)
        {
            for (var dy = -size; dy <= size; dy += size * 2)
            {
                Raylib.DrawTextEx(s.Font, line, pos + new Vector2(dx, dy), s.Size, s.Spacing, s.OutlineColor);
            }
        }
    }
}