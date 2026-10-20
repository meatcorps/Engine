using System.Runtime.InteropServices;
using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Modules;

namespace Meatcorps.Engine.RayLib.Stream;

public static class StreamModule
{
    public static RayLibModule WithStream(this RayLibModule module, VideoStreamOptions? streamOptions = null, bool autoStart = true, bool captureEndResult = true,
        string tag = "default")
    {
        streamOptions ??= LoadOptionsFromConfig();
        if (GlobalObjectManager.ObjectManager.Get<VideoStreamOptions>(tag) != null)
            throw new ArgumentException($"Cannot register stream! the tag '{tag}' already exist");
        
        GlobalObjectManager.ObjectManager.Register(streamOptions, tag);
        _ = new StreamGameTask(20, autoStart, captureEndResult, tag);
        return module;
    }

    public static VideoStreamOptions LoadOptionsFromConfig()
    {
        var config = GlobalObjectManager.ObjectManager.Get<IUniversalConfig>()!;
        const string GROUP = "Stream";
        var encoder = GetEncoderBasedOnPlatform();
        return new VideoStreamOptions
        {
            AudioEnabled = config.GetOrDefault(GROUP, "AudioEnabled", false, false),
            AudioBitrateKbps = config.GetOrDefault(GROUP, "AudioBitrateKbps", 128, false),
            AudioSampleRate = config.GetOrDefault(GROUP, "AudioSampleRate", 48000, false),
            Width = config.GetOrDefault(GROUP, "Width", 1920, false),
            Height = config.GetOrDefault(GROUP, "Height", 1080, false),
            Fps = config.GetOrDefault(GROUP, "Fps", 60, false),
            BitrateKbps = config.GetOrDefault(GROUP, "BitrateKbps", 8000, false),
            Encoder = GetEnumFromString<VideoEncoder>(config.GetOrDefault(GROUP, "Encoder", encoder.ToString(), false)),
            Codec = GetEnumFromString<VideoCodec>(
                config.GetOrDefault(GROUP, "Codec", VideoCodec.H265.ToString(), false)),
            Output = GetEnumFromString<VideoOutput>(config.GetOrDefault(GROUP, "Output", VideoOutput.Rtsp.ToString(),
                false)),
            FilePath = config.GetOrDefault(GROUP, "FilePath", "video.mp4", false),
            RtspPath = config.GetOrDefault(GROUP, "RtspPath", "/stream", false),
            Port = (ushort)config.GetOrDefault(GROUP, "Port", 8554, false),
        };
    }

    public static VideoEncoder GetEncoderBasedOnPlatform()
    {
        // TODO Maby this is really needed but for now we will just use auto. It works fine
        return VideoEncoder.Auto;
    }

    private static T GetEnumFromString<T>(string name) where T : struct, Enum
    {
        if (Enum.TryParse<T>(name, true, out var result))
            return result;

        var def = (T)default;

        return def;
    }
}