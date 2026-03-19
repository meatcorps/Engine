using ImGuiNET;

namespace Meatcorps.Engine.RayLib.ImGuiTools.Interfaces;

public interface IImGuiInitializeModule
{
    public void Initialize(ImGuiIOPtr io);
    public void Cleanup();
}