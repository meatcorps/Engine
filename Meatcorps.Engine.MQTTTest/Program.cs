using System.Runtime.Loader;
using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Storage.Data;
using Meatcorps.Engine.Logging.Module;
using Meatcorps.Engine.MQTT.Enums;
using Meatcorps.Engine.MQTT.Modules;
using Meatcorps.Engine.MQTTTest;
using Meatcorps.Engine.Signals.Data;

LoggingModule.Load();
CoreModule.Load();
BasicConfig.Load();

MQTTModule.Load()
    .RegisterComplexObject("arcade/test", true, false, new TestObject
    {
        Test = "Hello world2!",
        Value = 100.0f
    })
    .Create();

var obj = new SignalValue<TestObject, MQTTGroup>(MQTTGroup.Exchange, "arcade/test");
Console.WriteLine("Start value: " + obj.Value.Test);
obj.ValueChanged += (value) => Console.WriteLine("ValueChanged! " + value.Test);
obj.IncomingValue += (value) => Console.WriteLine("Incoming! " + value.Test);
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

Console.WriteLine("Added hallow!");
obj.Value.List.Add("Hellow!");
await Task.Delay(2000);
obj.Value.List.Add("Hellow3!");
await Task.Delay(2000);
obj.Value.List.Add("Hellow4!");
Console.WriteLine("Added!");
await Task.Delay(2000);
obj.Push();
Console.WriteLine("Pushed!");
while (running)
{
    if (obj.Value != null)
        Console.WriteLine(">" + obj.Value.Test);
    else
        Console.WriteLine("> Empty value.");
    await Task.Delay(1000);
}

await Task.Delay(10000);
GlobalObjectManager.ObjectManager.Dispose();    
    