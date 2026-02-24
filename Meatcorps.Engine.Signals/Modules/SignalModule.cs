using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Signals.Enums;
using Meatcorps.Engine.Signals.Interfaces;
using Meatcorps.Engine.Signals.Services;

namespace Meatcorps.Engine.Signals.Modules;

public static class SignalModule
{
    /// <summary>
    /// Loads the SignalModule, registering and adding signal-related components in the provided
    /// or global ObjectManager. This initializes necessary signal handling mechanics. It also
    /// registers by default the "SignalDefault.Internal" group for internal use.
    /// </summary>
    /// <param name="objectManager">
    /// The ObjectManager instance used for registration. If null, the global ObjectManager
    /// will be used as a default.
    /// </param>
    public static void Load(ObjectManager? objectManager = null)
    {
        objectManager ??= GlobalObjectManager.ObjectManager;
        var tracker = new InternalSignalValueEvent<SignalDefault>(SignalDefault.Internal);
        objectManager.RegisterSet<ISignalValueEvent<SignalDefault>>();
        objectManager.Add<ISignalValueEvent<SignalDefault>>(tracker);
    }
}