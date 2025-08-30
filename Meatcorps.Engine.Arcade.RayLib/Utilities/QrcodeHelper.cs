using System.Collections;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.UI;
using Meatcorps.Engine.RayLib.UI.Data;
using QRCoder;
using Raylib_cs;

namespace Meatcorps.Engine.Arcade.RayLib.Utilities;

public static class QrcodeHelper
{
    /// <summary>
    /// Create a Raylib Texture2D containing a QR code for the given text.
    /// </summary>
    /// <param name="text">Payload to encode.</param>
    /// <param name="scale">Pixel size of a single QR module (cell).</param>
    /// <param name="quietZone">Border around the QR in modules (spec recommends 4).</param>
    /// <param name="fg">Foreground (black) color for modules.</param>
    /// <param name="bg">Background (white) color.</param>
    public static Texture2D CreateTexture(
        string text,
        int scale = 8,
        int quietZone = 4,
        Color? fg = null,
        Color? bg = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("QR text cannot be empty.", nameof(text));
        if (scale <= 0) throw new ArgumentOutOfRangeException(nameof(scale));

        // 1) Generate module matrix (true = black module)
        var modules = GenerateMatrix(text, QRCodeGenerator.ECCLevel.Q);
        var coreSize = modules.GetLength(0);
        var totalModules = coreSize + quietZone * 2;
        var imgW = totalModules * scale;
        var imgH = totalModules * scale;

        var fgColor = fg ?? Color.Black;
        var bgColor = bg ?? Color.White;

        // 2) Build Raylib Image and paint the modules
        var img = Raylib.GenImageColor(imgW, imgH, bgColor); // background already filled

        var offset = quietZone * scale; // pixels
        for (var y = 0; y < coreSize; y++)
        {
            for (var x = 0; x < coreSize; x++)
            {
                if (!modules[y, x]) continue; // leave background

                // draw one module as a filled rect
                Raylib.ImageDrawRectangle(
                    ref img,
                    offset + x * scale,
                    offset + y * scale,
                    scale,
                    scale,
                    fgColor);
            }
        }

        // 3) Upload to GPU and free CPU-side image
        var tex = Raylib.LoadTextureFromImage(img);
        Raylib.UnloadImage(img);
        return tex;
    }

    /// <summary>
    /// Generate a square bool-matrix for the QR modules.
    /// </summary>
    private static bool[,] GenerateMatrix(string text, QRCodeGenerator.ECCLevel ecc)
    {
        using var gen = new QRCodeGenerator();
        using var data = gen.CreateQrCode(text, ecc);

        // QRCoder exposes a list of BitArray rows; convert to bool[,]
        var rows = data.ModuleMatrix.Select(row => ((BitArray)row).Cast<bool>().ToArray()).ToArray();
        var n = rows.Length;
        var result = new bool[n, n];
        for (var y = 0; y < n; y++)
        for (var x = 0; x < n; x++)
            result[y, x] = rows[y][x];

        return result;
    }

    public static InlineRender AddQrCode(
        this InlineRender ir,
        string id,
        string text,
        int scale = 8,
        int quietZone = 4,
        Color? fg = null,
        Color? bg = null,
        Insets? margin = null,
        HAlign hAlign = HAlign.Left,
        VAlign vAlign = VAlign.Middle)
    {
        Texture2D? tex = null;
        PointInt size = default;

        ir.Register(new InlineItem
        {
            Identifier = id,
            HAlign = hAlign,
            VAlign = vAlign,
            CacheSize = true,
            Margin = margin ?? Insets.Zero,
            Initialize = (_, __) =>
            {
                tex = CreateTexture(text, scale, quietZone, fg, bg);
                size = new PointInt(tex.Value.Width, tex.Value.Height);
            },
            GetSize = (_, __) => size,
            Draw = (_, __, rect) =>
            {
                if (tex.HasValue)
                    Raylib.DrawTexturePro(tex.Value,
                        new Rectangle(0, 0, tex.Value.Width, tex.Value.Height),
                        new Rectangle(rect.X, rect.Y, rect.Width, rect.Height),
                        System.Numerics.Vector2.Zero, 0f, Color.White);
            },
            Destroy = (_, __) =>
            {
                if (tex.HasValue) Raylib.UnloadTexture(tex.Value);
            }
        });

        return ir;
    }
}