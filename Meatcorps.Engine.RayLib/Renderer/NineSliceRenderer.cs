using System.Numerics;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Renderer;

public class NineSliceRenderer
{
    private readonly Texture2D _texture;
    private readonly int _left, _right, _top, _bottom;
    private readonly Rectangle _sourceRect;

    private readonly Rectangle[] _src = new Rectangle[9];
    private readonly Rectangle[] _dst = new Rectangle[9];

    public NineSliceRenderer(Texture2D texture, int left = 2, int right = 2, int top = 2, int bottom = 2, Rectangle? sourceRect = null)
    {
        _texture = texture;
        _left = left;
        _right = right;
        _top = top;
        _bottom = bottom;
        _sourceRect = sourceRect ?? new Rectangle(0, 0, texture.Width, texture.Height);

        CalculateSourceRects();
    }

    private void CalculateSourceRects()
    {
        float sx = _sourceRect.X;
        float sy = _sourceRect.Y;
        float sw = _sourceRect.Width;
        float sh = _sourceRect.Height;

        float innerW = sw - _left - _right;
        float innerH = sh - _top - _bottom;

        // Top row
        _src[0] = new Rectangle(sx, sy, _left, _top);
        _src[1] = new Rectangle(sx + _left, sy, innerW, _top);
        _src[2] = new Rectangle(sx + sw - _right, sy, _right, _top);

        // Middle row
        _src[3] = new Rectangle(sx, sy + _top, _left, innerH);
        _src[4] = new Rectangle(sx + _left, sy + _top, innerW, innerH);
        _src[5] = new Rectangle(sx + sw - _right, sy + _top, _right, innerH);

        // Bottom row
        _src[6] = new Rectangle(sx, sy + sh - _bottom, _left, _bottom);
        _src[7] = new Rectangle(sx + _left, sy + sh - _bottom, innerW, _bottom);
        _src[8] = new Rectangle(sx + sw - _right, sy + sh - _bottom, _right, _bottom);
    }

    public void Draw(Rectangle target, Color tint)
    {
        float x = target.X;
        float y = target.Y;
        float w = target.Width;
        float h = target.Height;

        // Sizes
        float centerW = w - _left - _right;
        float centerH = h - _top - _bottom;

        // Top row
        _dst[0] = new Rectangle(x, y, _left, _top);
        _dst[1] = new Rectangle(x + _left, y, centerW, _top);
        _dst[2] = new Rectangle(x + w - _right, y, _right, _top);

        // Middle row
        _dst[3] = new Rectangle(x, y + _top, _left, centerH);
        _dst[4] = new Rectangle(x + _left, y + _top, centerW, centerH);
        _dst[5] = new Rectangle(x + w - _right, y + _top, _right, centerH);

        // Bottom row
        _dst[6] = new Rectangle(x, y + h - _bottom, _left, _bottom);
        _dst[7] = new Rectangle(x + _left, y + h - _bottom, centerW, _bottom);
        _dst[8] = new Rectangle(x + w - _right, y + h - _bottom, _right, _bottom);

        for (int i = 0; i < 9; i++)
        {
            Raylib.DrawTexturePro(_texture, _src[i], _dst[i], Vector2.Zero, 0f, tint);
        }
    }
}