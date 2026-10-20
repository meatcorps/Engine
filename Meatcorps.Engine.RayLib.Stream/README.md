# Meatcorps.Engine.RayLib.Stream — RTSP + H.264/H.265 (macOS experimental)

C# owns a Raylib `RenderTexture2D`; the native wrapper receives only the texture ID.
The macOS backend performs an OpenGL GPU blit into four private IOSurface-backed
`CVPixelBuffer`s, encodes with hardware-only VideoToolbox (H.264 or H.265), and
passes *compressed* Annex-B access units to shared GStreamer output code.

The build system selects exactly one platform implementation. Windows and Linux are
explicit placeholder backends. C# has no platform setting.

## Build (macOS, Homebrew)

```sh
brew install cmake pkg-config gstreamer
pkg-config --modversion gstreamer-1.0 gstreamer-app-1.0 gstreamer-rtsp-server-1.0
cmake -S . -B build -DCMAKE_BUILD_TYPE=Debug
cmake --build build -j
```

Copy `build/libmeatcorps_stream.dylib` alongside the .NET executable or configure
the native library search path. Keep Homebrew GStreamer available at runtime.

## Live endpoint (default)

```csharp
using var stream = new Meatcorps.VideoStreaming.VideoStream(new()
{
    Width = 1920, Height = 1080, Fps = 60, BitrateKbps = 8000,
    Codec = Meatcorps.VideoStreaming.VideoCodec.H265,
    Output = Meatcorps.VideoStreaming.VideoOutput.Rtsp,
    Port = 8554, RtspPath = "/meatcorps"
});
stream.Start(); // after Raylib.InitWindow

// RenderTexture filled independently by the application:
stream.Update(streamTarget.Texture.Id); // after EndTextureMode

stream.Stop(); // before Raylib.CloseWindow
```

VLC on the streaming host: `rtsp://127.0.0.1:8554/meatcorps`.
A remote Pi connects to `rtsp://<HOST_IP>:8554/meatcorps`.
One shared RTSP media factory serves multiple clients; encode once.
RTSP stops publishing until a client connects; the encoder keeps running.

Switch to MP4 by setting `Output = VideoOutput.Mp4` and `FilePath = "recording.mp4"`.
Both codecs use the same `mp4mux` output and platform encoder.

## License

MIT License  
See `LICENSE` for details.