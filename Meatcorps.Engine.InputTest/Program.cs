using System.Runtime.Loader;
using Meatcorps.Engine.Hardware.ArduinoController.ArduinoController;
// ReSharper disable RedundantAssignment

var running = true;

Console.CancelKeyPress += (_, _) =>
{
    running = false;
};

AssemblyLoadContext.Default.Unloading += _ =>
{
    running = false;
};

AppDomain.CurrentDomain.ProcessExit += (_, _) =>
{
    running = false;
};

var controller = new ArduinoControllerCommunication(args[0]);
ControllerInputEnum previousState1 = 0;
ControllerInputEnum previousState2 = 0;

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

        setLights(out var lightState1, controller.ControllerState1, [
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
        setLights(out var lightState2, controller.ControllerState2, [
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
return;

void setLights(out ButtonLightsEnum state, ControllerInputEnum input, Enum[] targets, Enum[] mapsTo)
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