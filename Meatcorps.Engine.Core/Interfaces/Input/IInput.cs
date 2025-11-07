namespace Meatcorps.Engine.Core.Interfaces.Input;

public interface IInput
{
    string Label { get; }
    bool Enable { get; set; }
    bool Down { get; }
    bool Up { get; }
    bool IsPressed { get; }
    float Normalized { get; }
    IButtonAnimation? Animation { get; set; }
    bool EnableLightWhenPressed { get; set; }
    bool EnableLightWhenUnpressed { get; set; }
}