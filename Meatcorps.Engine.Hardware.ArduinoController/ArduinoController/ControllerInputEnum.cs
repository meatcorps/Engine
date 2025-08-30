namespace Meatcorps.Engine.Hardware.ArduinoController.ArduinoController;

[Flags]
public enum ControllerInputEnum
{
    Up = 1,
    Down = 2,
    Right = 4,
    Left = 8,
    Button1 = 16,
    Button2 = 32,
    Button3 = 64,
    Button4 = 128,
    Button5 = 256,
    Button6 = 512
}