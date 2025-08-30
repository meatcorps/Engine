using System.Diagnostics;

namespace Meatcorps.Engine.Core.Utilities;

public sealed class FrameTimer
{
    private readonly long[] _samples;
    private int _i;

    public double AvgUs { get; private set; }
    public double P95Us { get; private set; }
    public double P99Us { get; private set; }

    public double AvgMs => AvgUs / 1000.0;
    public double P95Ms => P95Us / 1000.0;
    public double P99Ms => P99Us / 1000.0;

    public double AvgFps => AvgUs > 0 ? 1_000_000.0 / AvgUs : 0;
    public double P95Fps => P95Us > 0 ? 1_000_000.0 / P95Us : 0;
    public double P99Fps => P99Us > 0 ? 1_000_000.0 / P99Us : 0;

    public FrameTimer(int capacity = 600) 
    { 
        _samples = new long[capacity]; 
    }

    public ScopedScope Scope()
    {
        var start = Stopwatch.GetTimestamp();
        return new ScopedScope((elapsedTicks) =>
        {
            var idx = _i++ % _samples.Length;
            _samples[idx] = elapsedTicks;
            ComputeStats();
        }, start);
    }

    private void ComputeStats()
    {
        var freq = (double)Stopwatch.Frequency;
        var copy = _samples.ToArray();
        Array.Sort(copy);

        double sum = 0;
        foreach (var t in copy) sum += t;

        AvgUs = (sum / copy.Length) * 1_000_000.0 / freq;
        P95Us = copy[(int)(copy.Length * 0.95)] * 1_000_000.0 / freq;
        P99Us = copy[(int)(copy.Length * 0.99)] * 1_000_000.0 / freq;
    }

    public override string ToString()
    {
        return $"Avg: {AvgUs:F2} µs ({AvgFps:F0} FPS) | " +
               $"p95: {P95Us:F2} µs ({P95Fps:F0} FPS) | " +
               $"p99: {P99Us:F2} µs ({P99Fps:F0} FPS)";
    }

    public string ToCompactString()
    {
        return $"Avg: {AvgMs:F2} ms | p95: {P95Ms:F2} ms | p99: {P99Ms:F2} ms";
    }

    public readonly struct ScopedScope : IDisposable
    {
        private readonly long _start;
        private readonly Action<long> _onEnd;
        public ScopedScope(Action<long> onEnd, long start) { _onEnd = onEnd; _start = start; }
        public void Dispose()
        {
            var end = Stopwatch.GetTimestamp();
            _onEnd(end - _start);
        }
    }
}