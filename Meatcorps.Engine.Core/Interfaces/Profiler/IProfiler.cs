using Meatcorps.Engine.Core.Utilities;

namespace Meatcorps.Engine.Core.Interfaces.Profiler;

/// <summary>
/// Defines profiling methods for measuring performance metrics.
/// </summary>
public interface IProfiler
{
    /// <summary>
    /// Starts a profiling scope for a specific operation associated with the given sender and name.
    /// </summary>
    /// <param name="sender">The type of the object initiating the profiling operation.</param>
    /// <param name="name">The name of the profiling operation.</param>
    /// <returns>A <see cref="FrameTimer.ScopedScope"/> object that represents the active profiling scope. Dispose of this object to stop the profiling.</returns>
    FrameTimer.ScopedScope StartProfile(Type sender, string name, Type? childType = null);

    /// <summary>
    /// Retrieves all active timers being tracked by the profiler along with their associated names.
    /// </summary>
    /// <returns>A collection of tuples where each tuple contains the name of the profiling operation and its associated <see cref="FrameTimer"/> instance.</returns>
    IEnumerable<(string, FrameTimer)> GetTimers();

    /// <summary>
    /// Clears all active timers currently being tracked by the profiler, resetting the profiling data.
    /// </summary>
    void ClearTimers();
}