using Meatcorps.Engine.AsciiScript.Enums;
using Meatcorps.Engine.AsciiScript.Services;

namespace Meatcorps.Engine.AsciiScript.Data;

public struct AsciiScriptItem
{
    public AsciiScriptItemType Type;
    public string Command;
    public string Value;
    public int LineNumber;
}