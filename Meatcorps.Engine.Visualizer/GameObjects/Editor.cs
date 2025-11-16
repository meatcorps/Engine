using System.Numerics;
using ImGuiNET;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.ImGuiTools;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.Visualizer.Services;
using Meatcorps.Engine.Visualizer.VisualItems;
using Raylib_cs;

namespace Meatcorps.Engine.Visualizer.GameObjects;

public class Editor: BaseImGuiGameObject
{
    public IVisualItem? Item { get; set; }
    public bool BlockTheEditor => _editName || _openFile || Item != null;

    public string Name { get; set; } = Guid.NewGuid().ToString();
    private bool _editName = false;
    private bool _openFile = false;

    private MainGameObject _mainGameObject;

    private string[] _fileItems = [];
    private int _currentFileIndex = 0;
    private ICameraFixedWidthAndHeight _camera;

    public void SetDataLoaderService(MainGameObject mainGameObject) 
        => _mainGameObject = mainGameObject;
    
    protected override void OnGuiInitialize()
    {
        if (GlobalObjectManager.ObjectManager.Get<ICamera>()! is ICameraFixedWidthAndHeight camera)
            _camera = camera;
    }

    public void OpenFile()
    {
        _fileItems = _mainGameObject.DataLoaderService.GetFiles().ToArray();
        _openFile = true;
        _currentFileIndex = 0;
    }
    
    public void EditName()
    {
        _editName = true;
    }

    protected override void OnGuiUpdate(float deltaTime)
    {
        if (Raylib.IsKeyDown(KeyboardKey.LeftShift) && Raylib.IsKeyPressed(KeyboardKey.F4) && !BlockTheEditor)
            OpenFile();

        if (Raylib.IsKeyPressed(KeyboardKey.F4) && !BlockTheEditor)
            EditName();

        if (_openFile)
        {
            ImGui.Begin("Editor", ImGuiWindowFlags.AlwaysAutoResize);
            var name = Name;

            ImGui.Selectable("Select file");
            
            name = name.Replace("/", "").Replace("\\", "");
            Name = name;

            if (ImGui.Combo("Select item", ref _currentFileIndex, _fileItems, _fileItems.Length))
            {
                //
            }
            
            var cancel = ImGui.Button("Cancel");
            var done = ImGui.Button("Done");
            ImGui.End();

            if (cancel)
            {
                _openFile = false;
            }
            
            if (done)
            {
                Name = _fileItems[_currentFileIndex];
                _mainGameObject.LoadData(_mainGameObject.DataLoaderService.LoadFile(_fileItems[_currentFileIndex]));
                _openFile = false;
            }

            return;  
        } 
        
        if (_editName)
        {
            ImGui.Begin("Editor", ImGuiWindowFlags.AlwaysAutoResize);
            var name = Name;
            
            ImGui.InputText("Name", ref name, 128);
            name = name.Replace("/", "").Replace("\\", "");
            Name = name;
            
            var done = ImGui.Button("Done");
            ImGui.End();

            if (done)
                _editName = false;
            
            return;  
        } 
        
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

    protected override void OnDraw()
    {
        base.OnDraw();
        
        if (BlockTheEditor)
        {
            var mousePosition = Raylib.GetMousePosition() /
                             ((float) Scene.GameHost.Width / _camera.TargetWidth);

            Raylib.DrawTriangleLines(mousePosition, mousePosition + new Vector2(5, 30),
                mousePosition + new Vector2(30, 15), Color.White);
            Raylib.DrawTriangleLines(mousePosition + new Vector2(1, 1), mousePosition + new Vector2(6, 31),
                mousePosition + new Vector2(31, 16), Color.White);

        }
    }
}