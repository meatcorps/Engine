using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Signals.Enums;
using Meatcorps.Engine.Signals.Interfaces;
using Meatcorps.Engine.Signals.Services;

namespace Meatcorps.Engine.Signals.Modules;

public static class SignalModule
{
    public static void Load(ObjectManager? objectManager = null)
    {
        objectManager ??= GlobalObjectManager.ObjectManager;
        var tracker = new InternalSignalValueEvent<SignalDefault>(SignalDefault.Internal);
        objectManager.RegisterSet<ISignalValueEvent<SignalDefault>>();
        objectManager.Add(tracker);
    }
}