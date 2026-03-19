using ImGuiNET;
using Meatcorps.Engine.RayLib.ImGuiTools.Interfaces;

namespace Meatcorps.Engine.RayLib.ImGuiTools;

public class ImGuiGenericInitialize: IImGuiInitializeModule
{
    private readonly Action<ImGuiIOPtr> _initialize;
    private readonly Action? _cleanup;

    public ImGuiGenericInitialize(Action<ImGuiIOPtr> initialize, Action? cleanup = null)
    {
        _initialize = initialize;
        _cleanup = cleanup;
    }
    
    public void Initialize(ImGuiIOPtr io)
    {
        _initialize(io);
    }

    public void Cleanup()
    {
        _cleanup?.Invoke();
    }
}