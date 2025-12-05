using ImGuiNET;
using Meatcorps.Engine.RayLib.ImGuiTools;
using Meatcorps.Engine.RayLib.ImGuiTools.Controllers.IconFonts;

namespace Meatcorps.Engine.Raylib.Examples.GameObjects;

// Change the MainScene.cs load this example

public class ImGuiTest: BaseImGuiGameObject
{
    protected override void OnGuiInitialize()
    {
        //
    }

    protected override void OnGuiUpdate(float deltaTime)
    {
        //ImGui.ShowDemoWindow();
        ImGui.Begin("Simple Window");
        ImGui.Text("Hello, world!");
        ImGui.End();
    }
}