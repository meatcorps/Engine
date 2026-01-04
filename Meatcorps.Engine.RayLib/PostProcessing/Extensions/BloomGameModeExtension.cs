using System.Numerics;
using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.Modules;

namespace Meatcorps.Engine.RayLib.PostProcessing.Extensions;

public static class BloomGameModeExtension
{
    /// <summary>
    ///     This should be the first Final PostProcess.
    /// </summary>
    /// <param name="module"></param>
    /// <param name="threshold">Threshold: 0.75–0.85 usually feels natural.</param>
    /// <param name="knee">Knee (soft threshold): 0.1–0.2 to avoid harsh cutoffs.</param>
    /// <param name="intensity">0.4–0.8; clamp the final composite: finalColor.rgb = min(finalColor.rgb, 1.0);</param>
    /// <param name="spread"></param>
    /// <returns></returns>
    public static RayLibModule SetupProcessingBloom(
        this RayLibModule module,
        float threshold = 0.8f,
        float knee = 0.1f,
        float intensity = 0.6f,
        float spread = 1.0f)
    {
        var processors = new List<IPostProcessor>()
        {
            new BloomThresholdPostProcessor
            {
                Threshold = threshold,
                Knee = knee
            },
            new GaussianBlurPostProcessor
            {
                Direction = new Vector2(1, 0),
                Spread = spread
            },
            new GaussianBlurPostProcessor
            {
                Direction = new Vector2(0, 1),
                Spread = spread
            },
            new BloomCompositePostProcessor
            {
                Intensity = intensity
            }
        };
        
        foreach (var processor in processors)
            module.SetProcessing(processor);
        
        GlobalObjectManager.ObjectManager.Add<IConfigChangeTracker>(new BloomGameModeConfig(processors));
        
        return module;
    }
}

public class BloomGameModeConfig: IConfigChangeTracker
{
    private readonly List<IPostProcessor> _processors;
    
    public BloomGameModeConfig(List<IPostProcessor> processors)
    {
        _processors = processors;
        UpdateConfig();
    }

    private void UpdateConfig()
    {
        var enabled = GlobalObjectManager.ObjectManager.Get<IUniversalConfig>()!.GetOrDefault("Graphics", "Bloom Effect", true);
        foreach (var processor in _processors)
            processor.Enabled = enabled;
    }
    
    public void ConfigChanged(string group, string key, object value)
    {
        if (group == "Graphics" && key == "Bloom Effect")
            UpdateConfig();
    }
}