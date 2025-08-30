using Meatcorps.Engine.RayLib.Modules;

namespace Meatcorps.Engine.RayLib.PostProcessing.Extensions;

public static class ColorGradingExtension
{
    /// <summary>
    /// Adds a final-pass 3D LUT color grading. Expects a 16x16x16 LUT packed as 256x16 image.
    /// </summary>
    public static RayLibModule SetupProcessingColorGrading(this RayLibModule module, string lutPath)
    {
        module.SetProcessing(new ColorGradingPostProcessor(lutPath));
        return module;
    }
}