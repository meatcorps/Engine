using ImGuiNET;
using Meatcorps.Engine.RayLib.ImGuiTools;
using Meatcorps.Engine.Visualizer.VisualItems;

namespace Meatcorps.Engine.Visualizer.GameObjects;

public class Editor: BaseImGuiGameObject
{
    public IVisualItem? Item { get; set; }
    
    protected override void OnGuiInitialize()
    {
        
    }

    protected override void OnGuiUpdate(float deltaTime)
    {
        if (Item != null)
        {
           ImGui.Begin("Editor", ImGuiWindowFlags.AlwaysAutoResize);
           Item.OnEditorDraw();
           var done = ImGui.Button("Done");
           ImGui.End();
           
           if (done)
               Item = null;
        }
    }
}