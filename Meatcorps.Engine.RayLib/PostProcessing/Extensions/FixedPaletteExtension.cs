using System.Numerics;
using Meatcorps.Engine.RayLib.Modules;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.PostProcessing.Extensions;

public static class FixedPaletteExtension
{
    public static RayLibModule SetupProcessingFixedPalette8(this RayLibModule module, params Vector3[] palette01)
    {
        var fx = new FixedPalettePostProcessor8();
        // fill up to 8 entries; extra are ignored, missing are repeated from last
        for (var i = 0; i < fx.Palette.Length; i++)
        {
            fx.Palette[i] = i < palette01.Length
                ? palette01[i]
                : (i == 0 ? new Vector3(0, 0, 0) : fx.Palette[i - 1]);
        }

        module.SetProcessing(fx);

        return module;
    }

    public static RayLibModule SetupProcessingFixedPalette8(this RayLibModule module, params Color[] colors255)
        => module.SetupProcessingFixedPalette8(colors255.Select(c => new Vector3(c.R / 255f, c.G / 255f, c.B / 255f))
            .ToArray());

    public static RayLibModule SetupProcessingPaletteGameBoy(this RayLibModule module)
        => module.SetupProcessingFixedPalette8(
            new Color(15, 56, 15, 255), // Dark green
            new Color(48, 98, 48, 255), // Medium green
            new Color(139, 172, 15, 255), // Light green
            new Color(155, 188, 15, 255) // Pale green
        );

    public static RayLibModule SetupProcessingPaletteRed(this RayLibModule module)
        => module.SetupProcessingFixedPalette8(
            new Color(0, 0, 0, 255), // Dark green
            new Color(64, 0, 0, 255), // Dark green
            new Color(128, 0, 0, 255), // Dark green
            new Color(192, 0, 0, 255), // Dark green
            new Color(255, 0, 0, 255), // Pale green
            new Color(255, 255, 255, 255) // Pale green
        );
    
    public static RayLibModule SetupProcessingPaletteCGA1(this RayLibModule module)
        => module.SetupProcessingFixedPalette8(
            new Color(0, 0, 0, 255), // Black
            new Color(85, 255, 255, 255), // Cyan
            new Color(255, 85, 255, 255), // Magenta
            new Color(255, 255, 255, 255) // White
        );

    public static RayLibModule SetupProcessingPaletteCGA2(this RayLibModule module)
        => module.SetupProcessingFixedPalette8(
            new Color(0, 0, 0, 255), // Black
            new Color(255, 85, 85, 255), // Red
            new Color(85, 255, 85, 255), // Green
            new Color(255, 255, 85, 255) // Yellow
        );

    public static RayLibModule SetupProcessingPaletteAmiga(this RayLibModule module)
        => module.SetupProcessingFixedPalette8(
            new Color(0, 0, 0, 255),
            new Color(85, 85, 85, 255),
            new Color(170, 170, 170, 255),
            new Color(255, 255, 255, 255)
        );

    public static RayLibModule SetupProcessingPaletteEGA8(this RayLibModule module)
        => module.SetupProcessingFixedPalette8(
            new Color(0, 0, 0, 255), // Black
            new Color(0, 0, 170, 255), // Blue
            new Color(0, 170, 0, 255), // Green
            new Color(0, 170, 170, 255), // Cyan
            new Color(170, 0, 0, 255), // Red
            new Color(170, 0, 170, 255), // Magenta
            new Color(170, 85, 0, 255), // Brown
            new Color(170, 170, 170, 255) // Light gray
        );

    // C64 expanded palette (based on 8 key colors)
    public static RayLibModule SetupProcessingPaletteC64_8(this RayLibModule module)
        => module.SetupProcessingFixedPalette8(
            new Color(0, 0, 0, 255), // Black
            new Color(255, 255, 255, 255), // White
            new Color(136, 0, 0, 255), // Red
            new Color(170, 255, 238, 255), // Cyan
            new Color(204, 68, 204, 255), // Purple
            new Color(0, 204, 85, 255), // Green
            new Color(0, 0, 170, 255), // Blue
            new Color(238, 238, 119, 255) // Yellow
        );

    // Amiga Workbench-style palette
    public static RayLibModule SetupProcessingPaletteAmiga8(this RayLibModule module)
        => module.SetupProcessingFixedPalette8(
            new Color(0, 0, 0, 255), // Black
            new Color(85, 85, 85, 255), // Dark gray
            new Color(170, 170, 170, 255), // Light gray
            new Color(255, 255, 255, 255), // White
            new Color(0, 85, 170, 255), // Dark blue
            new Color(0, 170, 255, 255), // Light blue
            new Color(85, 170, 85, 255), // Green
            new Color(255, 170, 0, 255) // Orange
        );

    // Pico-8 palette (8 picked for a balanced range)
    public static RayLibModule SetupProcessingPalettePico8_8(this RayLibModule module)
        => module.SetupProcessingFixedPalette8(
            new Color(0, 0, 0, 255), // Black
            new Color(29, 43, 83, 255), // Navy
            new Color(126, 37, 83, 255), // Maroon
            new Color(0, 135, 81, 255), // Dark green
            new Color(171, 82, 54, 255), // Brown
            new Color(95, 87, 79, 255), // Dark gray
            new Color(194, 195, 199, 255), // Light gray
            new Color(255, 241, 232, 255) // Off-white
        );

    // ZX Spectrum-inspired 8 colors
    public static RayLibModule SetupProcessingPaletteZXSpectrum8(this RayLibModule module)
        => module.SetupProcessingFixedPalette8(
            new Color(0, 0, 0, 255), // Black
            new Color(0, 0, 205, 255), // Blue
            new Color(205, 0, 0, 255), // Red
            new Color(205, 0, 205, 255), // Magenta
            new Color(0, 205, 0, 255), // Green
            new Color(0, 205, 205, 255), // Cyan
            new Color(205, 205, 0, 255), // Yellow
            new Color(205, 205, 205, 255) // White
        );

    /// <summary>
    /// Sets up a fixed-palette post processor with texture-based palette lookup and ordered dithering.
    /// </summary>
    /// <param name="module">The RayLib module instance</param>
    /// <param name="ditherStrength">Strength of 4x4 Bayer dithering (default 1/255)</param>
    /// <param name="palette">One or more RGB colors in 0..1 space</param>
    public static RayLibModule SetupProcessingFixedPaletteTex(
        this RayLibModule module,
        float ditherStrength = 1f / 255f,
        float ditherScale = 2,
        params Vector3[] palette)
    {
        if (palette == null || palette.Length == 0)
            throw new ArgumentException("Palette must have at least one color.");

        var fx = new FixedPalettePostProcessorTex
        {
            PaletteCount = Math.Min(palette.Length, 32),
            DitherStrength = ditherStrength,
            DitherScale = ditherScale,
        };

        // Fill palette entries; repeat last color if fewer than 32
        for (var i = 0; i < fx.Palette.Length; i++)
        {
            fx.Palette[i] = (i < palette.Length)
                ? palette[i]
                : palette[palette.Length - 1];
        }

        module.SetProcessing(fx);
        return module;
    }

    public static RayLibModule SetupProcessingPaletteC64(this RayLibModule module)
        => module.SetupProcessingFixedPaletteTex(
            1f / 255f,
            2,
            new Vector3(0f, 0f, 0f),
            new Vector3(1f, 1f, 1f),
            new Vector3(0.65f, 0.33f, 0.26f),
            new Vector3(0.64f, 0.78f, 0.94f),
            new Vector3(0.79f, 0.37f, 0.79f),
            new Vector3(0.37f, 0.69f, 0.31f),
            new Vector3(0.29f, 0.27f, 0.71f),
            new Vector3(0.85f, 0.85f, 0.64f),
            new Vector3(0.75f, 0.54f, 0.33f),
            new Vector3(0.53f, 0.33f, 0.09f),
            new Vector3(0.94f, 0.78f, 0.76f),
            new Vector3(0.53f, 0.53f, 0.53f),
            new Vector3(0.73f, 0.73f, 0.73f),
            new Vector3(0.54f, 0.78f, 0.38f),
            new Vector3(0.47f, 0.44f, 0.82f),
            new Vector3(0.78f, 0.71f, 0.94f)
        );

    public static RayLibModule SetupProcessingPaletteEGA16(this RayLibModule module)
        => module.SetupProcessingFixedPaletteTex(
            1f / 255f,
            2,
            new Vector3(0f, 0f, 0f),
            new Vector3(0f, 0f, 0.66f),
            new Vector3(0f, 0.66f, 0f),
            new Vector3(0f, 0.66f, 0.66f),
            new Vector3(0.66f, 0f, 0f),
            new Vector3(0.66f, 0f, 0.66f),
            new Vector3(0.66f, 0.33f, 0f),
            new Vector3(0.66f, 0.66f, 0.66f),
            new Vector3(0.33f, 0.33f, 0.33f),
            new Vector3(0.33f, 0.33f, 1f),
            new Vector3(0.33f, 1f, 0.33f),
            new Vector3(0.33f, 1f, 1f),
            new Vector3(1f, 0.33f, 0.33f),
            new Vector3(1f, 0.33f, 1f),
            new Vector3(1f, 1f, 0.33f),
            new Vector3(1f, 1f, 1f)
        );

    public static RayLibModule SetupProcessingPalettePico8(this RayLibModule module)
        => module.SetupProcessingFixedPaletteTex(
            1f / 255f,
            2,
            new Vector3(0f, 0f, 0f),
            new Vector3(0.125f, 0.125f, 0.125f),
            new Vector3(0.5f, 0.125f, 0.125f),
            new Vector3(0.75f, 0.25f, 0.25f),
            new Vector3(0.875f, 0.5f, 0.5f),
            new Vector3(0.25f, 0.5f, 0.25f),
            new Vector3(0.5f, 0.75f, 0.5f),
            new Vector3(0.75f, 1f, 0.75f),
            new Vector3(0.25f, 0.25f, 0.5f),
            new Vector3(0.5f, 0.5f, 0.75f),
            new Vector3(0.75f, 0.75f, 1f),
            new Vector3(1f, 1f, 0.5f),
            new Vector3(1f, 0.75f, 0.5f),
            new Vector3(0.75f, 0.5f, 0.25f),
            new Vector3(0.5f, 0.25f, 0f),
            new Vector3(1f, 1f, 1f)
        );

    public static RayLibModule SetupProcessingPaletteDesaturatedSurvivalHorror(this RayLibModule module)
        => module.SetupProcessingFixedPaletteTex(
            2f / 255f,
            2,
            new Vector3(0.733f, 0.314f, 0.259f),
            new Vector3(0.502f, 0.224f, 0.204f),
            new Vector3(0.302f, 0.180f, 0.180f),
            new Vector3(0.161f, 0.122f, 0.125f),
            new Vector3(0.643f, 0.553f, 0.263f),
            new Vector3(0.447f, 0.451f, 0.231f),
            new Vector3(0.259f, 0.278f, 0.188f),
            new Vector3(0.161f, 0.180f, 0.133f),
            new Vector3(0.408f, 0.620f, 0.518f),
            new Vector3(0.322f, 0.439f, 0.400f),
            new Vector3(0.235f, 0.290f, 0.282f),
            new Vector3(0.145f, 0.169f, 0.169f),
            new Vector3(0.439f, 0.427f, 0.631f),
            new Vector3(0.349f, 0.349f, 0.471f),
            new Vector3(0.255f, 0.259f, 0.310f),
            new Vector3(0.157f, 0.161f, 0.180f),
            new Vector3(0.718f, 0.306f, 0.749f),
            new Vector3(0.506f, 0.286f, 0.549f),
            new Vector3(0.337f, 0.239f, 0.380f),
            new Vector3(0.208f, 0.176f, 0.239f),
            new Vector3(0.733f, 0.553f, 0.451f),
            new Vector3(0.502f, 0.396f, 0.349f),
            new Vector3(0.341f, 0.294f, 0.286f),
            new Vector3(0.216f, 0.192f, 0.192f),
            new Vector3(0.737f, 0.824f, 0.831f),
            new Vector3(0.588f, 0.627f, 0.639f),
            new Vector3(0.427f, 0.443f, 0.451f),
            new Vector3(0.290f, 0.302f, 0.302f),
            new Vector3(0.000f, 0.000f, 0.000f)
        );
    
    public static RayLibModule SetupProcessingPaletteVGA32(this RayLibModule module)
    => module.SetupProcessingFixedPaletteTex(
        16f / 255f, // dither strength (change if needed)
        1,
        new Vector3(0f, 0f, 0f),             // 0  black
        new Vector3(0f, 0f, 0.666f),         // 1  blue
        new Vector3(0f, 0.666f, 0f),         // 2  green
        new Vector3(0f, 0.666f, 0.666f),     // 3  cyan
        new Vector3(0.666f, 0f, 0f),         // 4  red
        new Vector3(0.666f, 0f, 0.666f),     // 5  magenta
        new Vector3(0.666f, 0.333f, 0f),     // 6  brown
        new Vector3(0.666f, 0.666f, 0.666f), // 7  light gray
        new Vector3(0.333f, 0.333f, 0.333f), // 8  dark gray
        new Vector3(0.333f, 0.333f, 1f),     // 9  bright blue
        new Vector3(0.333f, 1f, 0.333f),     // 10 bright green
        new Vector3(0.333f, 1f, 1f),         // 11 bright cyan
        new Vector3(1f, 0.333f, 0.333f),     // 12 bright red
        new Vector3(1f, 0.333f, 1f),         // 13 bright magenta
        new Vector3(1f, 1f, 0.333f),         // 14 yellow
        new Vector3(1f, 1f, 1f),             // 15 white
        new Vector3(0f, 0f, 0.333f),         // 16 dark blue 2
        new Vector3(0f, 0.333f, 0f),         // 17 dark green 2
        new Vector3(0f, 0.333f, 0.333f),     // 18 dark cyan 2
        new Vector3(0.333f, 0f, 0f),         // 19 dark red 2
        new Vector3(0.333f, 0f, 0.333f),     // 20 dark magenta 2
        new Vector3(0.333f, 0.166f, 0f),     // 21 dark brown
        new Vector3(0.2f, 0.2f, 0.2f), // 22 dark gray repeat (keeps 32 count consistent)
        new Vector3(0.5f, 0.5f, 0.5f),       // 23 medium gray
        new Vector3(0.5f, 0.5f, 1f),         // 24 pastel blue
        new Vector3(0.5f, 1f, 0.5f),         // 25 pastel green
        new Vector3(0.5f, 1f, 1f),           // 26 pastel cyan
        new Vector3(1f, 0.5f, 0.5f),         // 27 pastel red
        new Vector3(1f, 0.5f, 1f),           // 28 pastel magenta
        new Vector3(1f, 1f, 0.5f),           // 29 pastel yellow
        new Vector3(0.833f, 0.833f, 0.833f)  // 30 light pastel gray
    );
}