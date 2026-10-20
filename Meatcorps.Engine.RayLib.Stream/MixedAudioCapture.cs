using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Stream;

public sealed class MixedAudioCapture : IDisposable
{
    private const int ChunkFrames = 4096;
    private const int SlotCount = 64;

    private sealed class Chunk
    {
        public readonly float[] Samples =
            new float[ChunkFrames * 2];

        public int Frames;
        public ulong Timestamp;
    }

    private readonly VideoStream _stream;
    private readonly int _sampleRate;
    private readonly Chunk[] _chunks = new Chunk[SlotCount];

    private readonly object _gate = new();
    private readonly AutoResetEvent _wake = new(false);
    private readonly Thread _worker;
    private readonly AudioCallback<float> _callback;

    private int _read;
    private int _write;
    private int _count;
    private int _dropped;

    private volatile bool _running = true;
    private Exception? _workerError;
    private bool _disposed;

    public int DroppedChunks =>
        Volatile.Read(ref _dropped);

    public Exception? WorkerError => _workerError;

    public MixedAudioCapture(VideoStream stream, int sampleRate = 48000)
    {
        _stream = stream;
        _sampleRate = sampleRate;

        for (int i = 0; i < SlotCount; i++)
            _chunks[i] = new Chunk();

        _callback = OnAudioMixed;

        _worker = new Thread(Work)
        {
            IsBackground = true,
            Name = "Meatcorps Audio Stream"
        };

        _worker.Start();

        Raylib.AttachAudioMixedProcessor(_callback);
    }

    private void OnAudioMixed(Span<float> samples)
    {
        // Raylib audio thread. Never wait for GStreamer.
        if (!_running || samples.Length < 2)
            return;

        // A single callback timestamp, shared with video.
        if (!_stream.IsStarted)
            return;
        ulong timestamp = _stream.TimeNanoseconds();

        if (!Monitor.TryEnter(_gate))
        {
            Interlocked.Increment(ref _dropped);
            return;
        }

        bool queued = false;

        try
        {
            int totalFrames = samples.Length / 2;
            int offset = 0;

            while (offset < totalFrames)
            {
                if (_count == SlotCount)
                {
                    Interlocked.Increment(ref _dropped);
                    break;
                }

                int frames = Math.Min(
                    ChunkFrames, totalFrames - offset
                );

                Chunk chunk = _chunks[_write];

                samples.Slice(
                    offset * 2, frames * 2
                ).CopyTo(chunk.Samples);

                chunk.Frames = frames;

                chunk.Timestamp = timestamp +
                    (ulong)offset * 1_000_000_000UL /
                    (ulong)_sampleRate;

                _write = (_write + 1) % SlotCount;
                _count++;

                offset += frames;
                queued = true;
            }
        }
        finally
        {
            Monitor.Exit(_gate);
        }

        if (queued)
            _wake.Set();
    }

    private void Work()
    {
        // Worker owns this buffer, not the audio callback.
        float[] buffer = new float[ChunkFrames * 2];

        while (true)
        {
            int frames = 0;
            ulong timestamp = 0;

            lock (_gate)
            {
                if (_count > 0)
                {
                    Chunk chunk = _chunks[_read];

                    frames = chunk.Frames;
                    timestamp = chunk.Timestamp;

                    Array.Copy(
                        chunk.Samples,
                        buffer,
                        frames * 2
                    );

                    _read = (_read + 1) % SlotCount;
                    _count--;
                }
                else if (!_running)
                {
                    break;
                }
            }

            if (frames == 0)
            {
                _wake.WaitOne(20);
                continue;
            }

            try
            {
                _stream.SubmitAudio(
                    buffer.AsSpan(0, frames * 2),
                    timestamp
                );
            }
            catch (Exception ex)
            {
                _workerError = ex;
                _running = false;
                break;
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        // Detach while Raylib's audio device still exists.
        Raylib.DetachAudioMixedProcessor(_callback);

        _running = false;
        _wake.Set();

        // Drain the queue before stopping the stream.
        _worker.Join();

        _wake.Dispose();
    }
}
