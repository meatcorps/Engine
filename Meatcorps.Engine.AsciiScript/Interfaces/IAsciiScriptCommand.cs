using Meatcorps.Engine.AsciiScript.Data;
using Meatcorps.Engine.AsciiScript.Enums;
using Meatcorps.Engine.AsciiScript.Services;

namespace Meatcorps.Engine.AsciiScript.Interfaces;

public interface IAsciiScriptCommand
{
    AsciiScriptItemType ScriptType { get; }
    string Command { get; }
    bool Accept(AsciiScriptItem scriptItem);
    void Initialize(AsciiScriptItem scriptItem, AsciiScriptParser parser);
    void Execute(AsciiScriptItem scriptItem, AsciiScriptParser parser, int totalRuns);
}