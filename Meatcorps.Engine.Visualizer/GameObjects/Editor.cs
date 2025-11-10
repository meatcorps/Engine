using ImGuiNET;
using Meatcorps.Engine.RayLib.ImGuiTools;

namespace Meatcorps.Engine.Visualizer.GameObjects;

public class Editor: BaseImGuiGameObject
{
    public VisualItem? Item { get; set; }
    
    protected override void OnGuiInitialize()
    {
        
    }

    protected override void OnGuiUpdate(float deltaTime)
    {
        if (Item != null)
        {
            var name = Item.Name;
            var name2 = Item.Name2;
            ImGui.Begin("Editor", ImGuiWindowFlags.AlwaysAutoResize);
            ImGui.InputText("Name", ref name, 128);
            ImGui.InputText("Name2", ref name2, 128);
            var done = ImGui.Button("Done");
            ImGui.End();
            Item.Name = name;
            Item.Name2 = name2;
            if (done)
            {
                Item = null;
            }
        }
    }
}