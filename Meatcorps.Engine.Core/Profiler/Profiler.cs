using Meatcorps.Engine.Core.Interfaces.Profiler;
using Meatcorps.Engine.Core.ObjectManager;

namespace Meatcorps.Engine.Core.Profiler;

/// <summary>
/// Provides profiling capabilities for runtime performance analysis within the application.
/// The Profiler class serves as a centralized management interface for enabling, disabling,
/// and customizing profiling behavior. It interacts with implementations of the <see cref="IProfiler"/>
/// interface to perform actual profiling tasks.
/// </summary>
public static class Profiler
{
    /// <summary>
    /// Gets the current instance of the <see cref="IProfiler"/> being used for profiling tasks.
    /// </summary>
    /// <remarks>
    /// The <c>Instance</c> property allows access to the currently active implementation of the <see cref="IProfiler"/> interface.
    /// By default, it is set to a <see cref="DisabledProfiler"/>, which provides a no-op implementation. This can be replaced
    /// dynamically using the <see cref="Profiler.SetCustomProfiler"/> method or other available methods in the <see cref="Profiler"/> class.
    /// </remarks>
    public static IProfiler Instance { get; private set; } = new DisabledProfiler();

    /// <summary>
    /// Sets a custom profiler implementation for the profiling system.
    /// </summary>
    /// <param name="profiler">
    /// An instance of a class that implements the <see cref="IProfiler"/> interface.
    /// This custom profiler will replace the default <see cref="DisabledProfiler"/> and will
    /// be registered with the global object manager for further use.
    /// </param>
    public static void SetCustomProfiler(IProfiler profiler)
    {
        Instance = profiler;
        GlobalObjectManager.ObjectManager.Register(profiler);
    }

    /// <summary>
    /// Enables the profiling system by setting the global profiler instance
    /// to an active implementation of the <see cref="EnabledProfiler"/> class.
    /// This allows runtime performance data to be collected and managed.
    /// </summary>
    public static void EnableProfiler()
    {
        if (Instance is EnabledProfiler)
            return;
        
        Instance = new EnabledProfiler();
        GlobalObjectManager.ObjectManager.Register(Instance);
    }

    /// <summary>
    /// Disables the current profiler by setting the profiler instance to a default implementation of <see cref="DisabledProfiler"/>.
    /// </summary>
    /// <remarks>
    /// If the current profiler instance is already a <see cref="DisabledProfiler"/>, no further action is taken.
    /// The new <see cref="DisabledProfiler"/> instance is also registered with the global object manager for subsequent use.
    /// </remarks>
    public static void DisableProfiler()
    {
        if (Instance is DisabledProfiler)
            return;

        Instance = new DisabledProfiler();
        GlobalObjectManager.ObjectManager.Register(Instance);
    }
}