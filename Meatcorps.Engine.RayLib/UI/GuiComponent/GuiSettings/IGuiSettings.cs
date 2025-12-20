using Raylib_cs;

namespace Meatcorps.Engine.RayLib.UI.GuiComponent.GuiSettings;

public interface IGuiSettings
{
    public Font Font { get; set; }
    public Color TextColor { get; }
    public Color TextColorActive { get; }
    public Color TextColorValue { get; }
    public float FontScaleSize { get; }
    
    bool IsDownPressed { get; }
    bool IsUpPressed { get; }
    bool IsLeftPressed { get; }
    bool IsRightPressed { get; }
    bool IsOnSelectionPressed { get; }
    bool IsBackPressed { get; }

    string DownPressedText { get; }
    string UpPressedText { get; }
    string LeftPressedText { get; }
    string RightPressedText { get; }
    string SelectionPressedText { get; }
    string BackPressedText { get; }
    
    void PlaySelectionSound(float volume = 1);
    void PlayNavigationSound(float volume = 1);
    void PlayErrorSound(float volume = 1);
    void PlayNotificationSound(float volume = 1);
}