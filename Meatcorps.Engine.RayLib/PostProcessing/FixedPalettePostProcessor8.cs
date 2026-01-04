using System.Numerics;
using Meatcorps.Engine.RayLib.PostProcessing.Abstractions;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.PostProcessing;

public class FixedPalettePostProcessor8 : BasePostProcessor
{
    public FixedPalettePostProcessor8()
        : base("Assets/Shaders/fixedpalette8.fx", Enumerable.Range(0, 8).Select(i => $"palette[{i}]").ToArray())
    {
    }

    public Vector3[] Palette { get; set; } = new Vector3[8];

    protected override void ApplyValues(Shader shader, Texture2D target)
    {
        for (var i = 0; i < Palette.Length; i++)
            SetValue($"palette[{i}]", Palette[i]);
    }
}