using Meatcorps.Engine.RayLib.PostProcessing.Abstractions;

namespace Meatcorps.Engine.RayLib.PostProcessing;

public class GrayscalePostProcessor : BasePostProcessor
{
    public GrayscalePostProcessor() 
        : base("Assets/Shaders/grayscale.fx", Array.Empty<string>()) { }
}