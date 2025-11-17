using System.Numerics;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Camera;
using Meatcorps.Engine.RayLib.GameObjects.UI;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.RemixIcons;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Engine.RayLib.Text;
using Meatcorps.Engine.Visualizer.Data;
using Meatcorps.Engine.Visualizer.Enums;
using Meatcorps.Engine.Visualizer.GameObjects;
using Meatcorps.Engine.Visualizer.Services;
using Meatcorps.Engine.Visualizer.VisualItems;
using Raylib_cs;

namespace Meatcorps.Engine.Visualizer.Scenes;

public class MainScene: BaseScene
{
    public Font Font => _font.GetFont();
    public Font IconFont => _font.GetFont(FontEnum.Icons);
    public VisualData VisualData { get; set; } = new();
    public bool BlockTheEditor => _editor.BlockTheEditor;
    private Vector2 _mousePosition;
    private ICameraFixedWidthAndHeight _camera = null!;
    private Editor _editor = null!;
    private Toolbox _toolbox;
    private CameraControllerGameObject _cameraController = null!;
    private UIMessageEmitter _uiMessage = null!;
    public bool EditorIsOpen => VisualData.EditItem != null;
    public DataLoaderService DataLoaderService { get; } = new DataLoaderService();
    public string MouseText { get; set; } = "";
    public bool HideUI = false;
    private TextManager<FontEnum> _font;
    public bool ValidMove => true;
    
    public Vector2 MousePosition
    {
        get => _mousePosition;
        private set
        {
            _mousePosition = value;
            MousePositionGrid = Vector2ToGrid(value);
        }
    }

    public Vector2 MousePositionGrid { get; private set; }
    
    protected override void OnInitialize()
    {
        if (GlobalObjectManager.ObjectManager.Get<ICamera>()! is ICameraFixedWidthAndHeight camera)
            _camera = camera;
        _cameraController = AddGameObject(new CameraControllerGameObject(GlobalObjectManager.ObjectManager.Get<ICamera>()));
        _uiMessage = AddGameObject(new UIMessageEmitter(
            TextKitStyles.HudDefault(GlobalObjectManager.ObjectManager.Get<IDefaultFont>()!.GetFont())));
        _toolbox = AddGameObject(new Toolbox());
        _editor = AddGameObject(new Editor());
        _font = GlobalObjectManager.ObjectManager.Get<TextManager<FontEnum>>()!;
        AddGameObject(new MainGameObject());
        SetupToolBox();
    }

    private void SetupToolBox()
    {
        _toolbox.Items.Add(new ToolboxItem
        {
            Highlight = () => VisualData.VisualType == VisualType.Node,
            Icon = RemixIcon.t_box_fill,
            Action = () => VisualData.VisualType = VisualType.Node,
            Name = "Add Node"
        });
        _toolbox.Items.Add(new ToolboxItem
        {
            Highlight = () => VisualData.VisualType == VisualType.Line,
            Icon = RemixIcon.separator,
            Action = () => VisualData.VisualType = VisualType.Line,
            Name = "Add Line"
        });
        _toolbox.Items.Add(new ToolboxItem
        {
            Icon = null
        });
        _toolbox.Items.Add(new ToolboxItem
        {
            Icon = RemixIcon.text_snippet,
            Action = () => _editor.EditName(),
            Name = "Rename document"
        });
        _toolbox.Items.Add(new ToolboxItem
        {
            Icon = RemixIcon.upload_2_fill,
            Action = () => _editor.OpenFile(),
            Name = "Load document"
        });
        _toolbox.Items.Add(new ToolboxItem
        {
            Icon = RemixIcon.save_2_fill,
            Action = SaveData,
            Name = "Save document"
        });
        _toolbox.Items.Add(new ToolboxItem
        {
            Icon = null
        });
        _toolbox.Items.Add(new ToolboxItem
        {
            Icon = RemixIcon.focus_mode,
            Action = () =>
            {
                _cameraController.SetZoom(0);
                _cameraController.SetPosition(new Vector2(0, 0));
            },
            Name = "Reset zoom and position"
        });
    }

    protected override void OnPreUpdate(float deltaTime)
    {
        MouseText = "";
        MousePosition = _camera.ScreenToWorld(Raylib.GetMousePosition() /
                              ((float)GameHost.Width / _camera.TargetWidth));
    }
    
    protected override void OnUpdate(float deltaTime)
    {
        foreach (var item in VisualData.Data)
            item.Update(deltaTime);
        
        
        _toolbox.Enabled = !HideUI;
    }
    
    
    public bool IsKeyDown(KeyboardKey key, bool ignoreEditor = false)
    {
        if ((VisualData.EditType == EditType.DataEnter || _editor.BlockTheEditor) && !ignoreEditor)
            return false;

        return Raylib.IsKeyDown(key);
    }
    
    public bool IsKeyPressed(KeyboardKey key, bool ignoreEditor = false)
    {
        if ((VisualData.EditType == EditType.DataEnter || _editor.BlockTheEditor) && !ignoreEditor)
            return false;

        return Raylib.IsKeyPressed(key);
    }

    public bool IsMouseDown(MouseButton button)
    {
        if (VisualData.EditType == EditType.DataEnter || _editor.BlockTheEditor || _toolbox.IsMouseOverToolbox)
            return false;

        return Raylib.IsMouseButtonDown(button);
    }

    public bool IsMouseUp(MouseButton button)
    {
        if (VisualData.EditType == EditType.DataEnter || _editor.BlockTheEditor)
            return false;

        return Raylib.IsMouseButtonUp(button);
    }
    
    private Vector2 Vector2ToGrid(Vector2 vector)
    {
        return new Vector2(MathF.Floor(vector.X / 10f), MathF.Floor(vector.Y / 10f)) * 10;
    }
    
    public void LoadData(IEnumerable<IVisualItem>? items)
    {
        if (items is not null)
        {
            VisualData.Data.Clear();

            foreach (var item in items)
                item.OnInitialize(this);

            VisualData.Data.AddRange(items);

            _cameraController.SetZoom(0);
            _cameraController.SetPosition(new Vector2(0, 0));

            _uiMessage.Show("Data loaded!");
        }
    }

    public void SaveData()
    {
        DataLoaderService.SaveFile(VisualData.Name, VisualData.Data);
        _uiMessage.Show("Data saved!");
    }
    
    protected override void OnDispose()
    {
    }
}