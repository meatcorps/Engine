using Meatcorps.Engine.Hardware.Controllers.Enums;

namespace Meatcorps.Engine.Hardware.Controllers.Mapper;

public static class ControllerVendorToTypeMapping
{
    public static ControllerType Get(int vendorId)
    {
        return vendorId switch
        {
            0x045E => ControllerType.XBox,        // Microsoft
            0x054C => ControllerType.PlayStation, // Sony
            0x057E => ControllerType.Nintendo,    // Nintendo (Switch, etc.)
            0x28DE => ControllerType.Steam,       // Valve (Steam Controller / Deck)
            _ => ControllerType.Other
        };
    }
}