using ImGuiNET;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.ImGuiTools;
using Meatcorps.Engine.RayLib.ImGuiTools.Controllers;
using Meatcorps.Engine.RayLib.Resources;

namespace Meatcorps.Engine.Raylib.Examples.GameObjects;

// Change the MainScene.cs load this example

public class ImGuiTest : BaseImGuiTool
{

    protected override void DoDraw(float deltaTime)
    {
        using (ImGuiTextManager<DefaultFont>.UnsafeUseFont(DefaultFont.Default, 16))
        {
            ImGui.Begin("Simple Window");
            rlImGui.Image(GlobalObjectManager.ObjectManager.Get<OneTexture>("BGPIC")!.Texture);
            ImGui.Text("Hello, world!");
            ImGui.End();
        }
        base.DoDraw(deltaTime);
    }
    
}