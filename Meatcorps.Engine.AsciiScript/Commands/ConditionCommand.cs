using Meatcorps.Engine.AsciiScript.Data;
using Meatcorps.Engine.AsciiScript.Enums;
using Meatcorps.Engine.AsciiScript.Interfaces;
using Meatcorps.Engine.AsciiScript.Services;

namespace Meatcorps.Engine.AsciiScript.Commands;

public class ConditionCommand: IAsciiScriptCommand
{
    private readonly Func<bool> _condition;
    public AsciiScriptItemType ScriptType => AsciiScriptItemType.Condition;
    public string Command { get; }
    private int _ifElseLineNumber = -1;
    private int _ifEndLineNumber = -1;

    public ConditionCommand(string command, Func<bool> condition)
    {
        _condition = condition;
        Command = command;
    }
    
    public bool Accept(AsciiScriptItem scriptItem)
    {
        return true;
    }

    public void Initialize(AsciiScriptItem scriptItem, AsciiScriptParser parser)
    {
        parser.Reader?.SearchAfter((item, i) =>
        {
            if (item.Type == AsciiScriptItemType.ConditionElse)
                _ifElseLineNumber = i;
            else if (item.Type == AsciiScriptItemType.ConditionEnd)
                _ifEndLineNumber = i;
            
            return item.Type == AsciiScriptItemType.ConditionEnd;
        });
    }

    public void Execute(AsciiScriptItem scriptItem, AsciiScriptParser parser, int totalRuns)
    {
        if (!_condition())
        {
            if (_ifElseLineNumber > 0)
                parser.JumpToLine(_ifElseLineNumber + 1);
            else
                parser.JumpToLine(_ifEndLineNumber + 1);
        }
    }
}