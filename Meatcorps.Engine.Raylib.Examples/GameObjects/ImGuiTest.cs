using ImGuiNET;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.ImGuiTools;
using Meatcorps.Engine.RayLib.ImGuiTools.Controllers;
using Meatcorps.Engine.RayLib.Resources;

namespace Meatcorps.Engine.Raylib.Examples.GameObjects;

// Change the MainScene.cs load this example

public class ImGuiTest : BaseImGuiGameObject
{
    protected override void OnGuiInitialize()
    {
        //
    }

    protected override void OnGuiUpdate(float deltaTime)
    {
        //ImGui.ShowDemoWindow();
        ImGui.Begin("Simple Window");
        rlImGui.Image(GlobalObjectManager.ObjectManager.Get<OneTexture>("BGPIC")!.Texture);
        ImGui.Text("Hello, world!");
        ImGui.End();
    }
}