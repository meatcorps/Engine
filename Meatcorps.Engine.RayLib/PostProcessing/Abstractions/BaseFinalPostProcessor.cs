namespace Meatcorps.Engine.RayLib.PostProcessing.Abstractions;

public abstract class BaseFinalPostProcessor : BasePostProcessor
{
    protected BaseFinalPostProcessor(string fxFilename, string[] shaderValues, bool enabled = true) : base(fxFilename, shaderValues, enabled)
    {
    }
}