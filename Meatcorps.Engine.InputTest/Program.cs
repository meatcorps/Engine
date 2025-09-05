// See https://aka.ms/new-console-template for more information

using System.Runtime.Loader;
using Meatcorps.Engine.Hardware.ArduinoController.ArduinoController;

var running = true;

Console.CancelKeyPress += (sender, eventArgs) =>
{
    running = false;
};

AssemblyLoadContext.Default.Unloading += context =>
{
    running = false;
};

AppDomain.CurrentDomain.ProcessExit += (s, e) =>
{
    running = false;
};

var controller = new ArduinoControllerCommunication("/dev/cu.usbserial-1130");
ControllerInputEnum previousState1 = 0;
ControllerInputEnum previousState2 = 0;
ButtonLightsEnum lightState1 = 0;
ButtonLightsEnum lightState2 = 0;

while (running)
{
    if (previousState1 != controller.ControllerState1 || previousState2 != controller.ControllerState2)
    {
        Console.Write("INPUT ENABLED: ");
        foreach (var type in Enum.GetValues<ControllerInputEnum>())
        {
            if (controller.ControllerState1.HasFlag(type))
                Console.Write("1:" + type.ToString() + " ");
            if (controller.ControllerState2.HasFlag(type))
                Console.Write("2:" + type.ToString() + " ");
        }
        Console.WriteLine();

        setLights(ref lightState1, controller.ControllerState1, [
            ControllerInputEnum.Button1,
            ControllerInputEnum.Button2,
            ControllerInputEnum.Button3,
            ControllerInputEnum.Button4,
            ControllerInputEnum.Button5,
            ControllerInputEnum.Button6
        ],
        [
            ButtonLightsEnum.Button1,
            ButtonLightsEnum.Button2,
            ButtonLightsEnum.Button3,
            ButtonLightsEnum.Button4,
            ButtonLightsEnum.Button5,
            ButtonLightsEnum.Button6
        ]);
        setLights(ref lightState2, controller.ControllerState2, [
            ControllerInputEnum.Button1,
            ControllerInputEnum.Button2,
            ControllerInputEnum.Button3,
            ControllerInputEnum.Button4,
            ControllerInputEnum.Button5,
            ControllerInputEnum.Button6
        ],
        [
            ButtonLightsEnum.Button1,
            ButtonLightsEnum.Button2,
            ButtonLightsEnum.Button3,
            ButtonLightsEnum.Button4,
            ButtonLightsEnum.Button5,
            ButtonLightsEnum.Button6
        ]);
        controller.SetButtonLights(lightState1, lightState2);
        
        Console.Write("LIGHT ENABLED: ");
        foreach (var type in Enum.GetValues<ButtonLightsEnum>())
        {
            if (lightState1.HasFlag(type))
                Console.Write("1:" + type.ToString() + " ");
            if (lightState2.HasFlag(type))
                Console.Write("2:" + type.ToString() + " ");
        }
        Console.WriteLine();
    }
    
    previousState1 = controller.ControllerState1;
    previousState2 = controller.ControllerState2;
    
    await Task.Delay(10);
}
Console.WriteLine("Hello, World!");

void setLights(ref ButtonLightsEnum state, ControllerInputEnum input, Enum[] targets, Enum[] mapsTo)
{
    state = 0;
    for (var i = 0; i < targets.Length; i++)
    {
        var target = targets[i];
        var map = mapsTo[i];
        
        if (input.HasFlag(target))
            state |= (ButtonLightsEnum)map;
    }
}