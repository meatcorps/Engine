using System.Runtime.InteropServices;

namespace Meatcorps.Engine.RayLib.Stream;

public enum VideoEncoder { Auto = 0, VideoToolbox = 1, Nvenc = 2, Vaapi = 3, Amf = 4, Qsv = 5 }
public enum VideoCodec { H264 = 0, H265 = 1 }
public enum VideoOutput { Rtsp = 0, Mp4 = 1 }
public enum SubmitResult { Submitted = 0, Skipped = 1, Dropped = 2 }

public sealed record VideoStreamOptions
{
    public int Width { get; init; } = 1920;
    public int Height { get; init; } = 1080;
    public int Fps { get; init; } = 60;
    public int BitrateKbps { get; init; } = 8000;
    public VideoEncoder Encoder { get; init; } = VideoEncoder.Auto;
    public VideoCodec Codec { get; init; } = VideoCodec.H265;
    public VideoOutput Output { get; init; } = VideoOutput.Rtsp;
    public string RtspPath { get; init; } = "/meatcorps";
    public ushort Port { get; init; } = 8554;
    public string FilePath { get; init; } = "recording.mp4";
    public bool AudioEnabled { get; init; } = false;
    public int AudioSampleRate { get; init; } = 48000;
    public int AudioBitrateKbps { get; init; } = 128;
}

public sealed class VideoStream : IDisposable
{
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeConfig
    {
        public int Width;
        public int Height;
        public int Fps;
        public int BitrateKbps;
        public VideoEncoder Encoder;
        public VideoCodec Codec;
        public VideoOutput Output;
        public nint Location;
        public ushort Port;
        public int AudioSampleRate;
        public int AudioBitrateKbps;
    }

    private static class Native
    {
        private const string Library = "meatcorps_stream";

        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int mc_stream_create(in NativeConfig config, out nint handle);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int mc_stream_start(nint handle);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int mc_stream_submit_texture(nint handle, uint texture, int flipY);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int mc_stream_stop(nint handle);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void mc_stream_destroy(nint handle);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern nint mc_stream_last_error(nint handle);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern ulong mc_stream_time_ns(nint handle);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int mc_stream_submit_audio_f32(nint handle, ref float samples, uint frames, ulong ptsNanoseconds);
    }

    private nint _handle;
    private bool _started;
    
    public bool IsStarted => _started;

    public VideoStream(VideoStreamOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        string location = options.Output == VideoOutput.Rtsp
            ? options.RtspPath : options.FilePath;
        var nativeLocation = Marshal.StringToCoTaskMemUTF8(location);
        try
        {
            var config = new NativeConfig
            {
                Width = options.Width,
                Height = options.Height,
                Fps = options.Fps,
                BitrateKbps = options.BitrateKbps,
                Encoder = options.Encoder,
                Codec = options.Codec,
                Output = options.Output,
                Location = nativeLocation,
                Port = options.Port,
                AudioSampleRate = options.AudioEnabled ? options.AudioSampleRate : 0,
                AudioBitrateKbps = options.AudioEnabled ? options.AudioBitrateKbps : 0
            };
            int code = Native.mc_stream_create(in config, out _handle);
            if (code < 0 || _handle == 0)
                throw new InvalidOperationException($"mc_stream_create failed: {code}");
        }
        finally
        {
            Marshal.FreeCoTaskMem(nativeLocation); // C copies the string.
        }
    }

    public void Start()
    {
        EnsureValid();
        Check(Native.mc_stream_start(_handle));
        _started = true;
    }

    // Call from the Raylib rendering thread, after Raylib.EndTextureMode().
    public SubmitResult Update(uint textureId, bool flipY)
    {
        EnsureValid();
        if (!_started)
            throw new InvalidOperationException("Start() must be called first");
        return (SubmitResult)Check(Native.mc_stream_submit_texture(_handle, textureId, flipY ? 1 : 0));
    }
    
    public ulong TimeNanoseconds()
    {
        EnsureValid();

        if (!_started)
            throw new InvalidOperationException(
                "Stream has not been started"
            );

        return Native.mc_stream_time_ns(_handle);
    }

    public void SubmitAudio(ReadOnlySpan<float> samples, ulong ptsNanoseconds)
    {
        EnsureValid();

        if (!_started)
            throw new InvalidOperationException(
                "Stream has not been started"
            );

        if (samples.Length % 2 != 0)
            throw new ArgumentException(
                "Expected interleaved stereo samples",
                nameof(samples)
            );

        if (samples.IsEmpty)
            return;

        // P/Invoke copies PCM into a native GstBuffer
        // before the call returns.
        Check(Native.mc_stream_submit_audio_f32(
            _handle,
            ref MemoryMarshal.GetReference(samples),
            checked((uint)(samples.Length / 2)),
            ptsNanoseconds
        ));
    }

    public void Stop()
    {
        if (_handle == 0 || !_started) return;
        try { Check(Native.mc_stream_stop(_handle)); }
        finally { _started = false; }
    }

    // Dispose before Raylib.CloseWindow(), while the GL context still exists.
    public void Dispose()
    {
        if (_handle == 0) return;
        try { Stop(); }
        finally
        {
            Native.mc_stream_destroy(_handle);
            _handle = 0;
        }
        GC.SuppressFinalize(this);
    }

    private int Check(int code)
    {
        if (code >= 0) return code;
        string? error = Marshal.PtrToStringUTF8(Native.mc_stream_last_error(_handle));
        throw new InvalidOperationException($"Streaming error {code}: {error}");
    }

    private void EnsureValid()
    {
        if (_handle == 0) throw new ObjectDisposedException(nameof(VideoStream));
    }
}
