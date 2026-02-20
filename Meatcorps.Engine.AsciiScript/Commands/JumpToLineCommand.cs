using Meatcorps.Engine.AsciiScript.Data;
using Meatcorps.Engine.AsciiScript.Enums;
using Meatcorps.Engine.AsciiScript.Interfaces;
using Meatcorps.Engine.AsciiScript.Services;

namespace Meatcorps.Engine.AsciiScript.Commands;

internal class JumpToLineCommand : IAsciiScriptCommand
{
    public AsciiScriptItemType ScriptType => AsciiScriptItemType.GotoLabel;
    public string Command => "JumpToLine";
    private readonly int _targetLineNumber;
    
    public JumpToLineCommand(int targetLineNumber)
    {
        _targetLineNumber = targetLineNumber;
    }
    
    public bool Accept(AsciiScriptItem scriptItem)
    {
        return true;
    }

    public void Initialize(AsciiScriptItem scriptItem, AsciiScriptParser parser)
    {
    }

    public void Execute(AsciiScriptItem scriptItem, AsciiScriptParser parser, int runs)
    {
        parser.JumpToLine(_targetLineNumber);
    }
}