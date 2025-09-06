using System.Runtime.Loader;
using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Storage.Data;
using Meatcorps.Engine.Logging.Module;
using Meatcorps.Engine.MQTT.Enums;
using Meatcorps.Engine.MQTT.Modules;
using Meatcorps.Engine.Signals.Data;

ConsoleLoggingModule.Load();
CoreModule.Load();
BasicConfig.Load();

MQTTModule.Load()
    .RegisterComplexObject<TestObject>("arcade/test", true, new TestObject
    {
        test = "Hello world2!",
        value = 100.0f
    })
    .Create();

var obj = new SignalValue<TestObject, MQTTGroup>(MQTTGroup.Exchange, "arcade/test", new TestObject());

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

while (running)
{
    if (obj.Value != null)
        Console.WriteLine(">" + obj.Value.test);
    else
        Console.WriteLine("> Empty value.");
    await Task.Delay(1000);
}

await Task.Delay(10000);
GlobalObjectManager.ObjectManager.Dispose();    
    