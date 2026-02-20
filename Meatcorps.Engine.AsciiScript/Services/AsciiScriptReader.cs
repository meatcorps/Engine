using System.Text;
using Meatcorps.Engine.AsciiScript.Data;
using Meatcorps.Engine.AsciiScript.Enums;
using Meatcorps.Engine.Core.Interfaces.Resource;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Resource;

namespace Meatcorps.Engine.AsciiScript.Services;

public class AsciiScriptReader
{
    private readonly string[] _blocks;
    private readonly string[] _commands;
    private readonly string[] _variables;
    private readonly StringBuilder _data = new StringBuilder();
    private readonly List<AsciiScriptItem> _items = new();
    private int _lineNumber;
    private readonly IResource _resource;
    
    public AsciiScriptReader(string[] blocks, string[] commands, string[] variables)
    {
        _blocks = blocks;
        _commands = commands;
        _variables = variables;
        _resource = GlobalObjectManager.ObjectManager.Get<IResource>() ?? new FallbackResource();
    }

    public void LoadFromFileAndParse(string path)
    {
        if (!_resource.Exists(path))
            throw new FileNotFoundException();
        _items.Clear();
        Reset();
        Read(_resource.LoadText(path));
    }
    
    public void LoadFromStringAndParse(string data)
    {
        _items.Clear();
        Reset();
        Read(data);
    }

    public void Reset()
    {
        _lineNumber = 0;
    }
    
    public bool ReadNext(out AsciiScriptItem item)
    {
        if (_lineNumber >= _items.Count)
        {
            item = default;
            return false;
        }
        item = _items[_lineNumber];
        _lineNumber++;
        return true;
    }

    public int SearchAll(Func<AsciiScriptItem, bool> condition)
    {
        var line = 0;
        foreach (var item in _items)
        {
            if (condition(item))
                return line;
                
            line++;
        }
        return -1;
    }

    public void SearchAfter(Func<AsciiScriptItem, int, bool> condition)
    {
        var line = _lineNumber;
        foreach (var item in _items.Skip(_lineNumber))
        {
            if (condition(item, line))
                return;
                
            line++;
        }
    }

    public void Read(string data)
    {
        var lines = data.Replace("\r", "").Split("\n").Select(line => line.TrimEnd()).ToArray();
        var isBlock = false;
        var blockName = "";
        var currentLineNumber = 0;
        var conditionDepth = 0;
        foreach (var line in lines)
        {
            currentLineNumber++;
            if (line.Length > 2 && line.StartsWith("//"))
                continue;

            if (line.StartsWith("IF "))
            {
                conditionDepth++;
                
                _items.Add(new AsciiScriptItem
                {
                    Type = AsciiScriptItemType.Condition,
                    Command = line[2..].Trim(),
                    Value = "",
                    LineNumber = currentLineNumber
                });
            }
            else if (line.StartsWith("ELSE") && conditionDepth > 0)
            {
                _items.Add(new AsciiScriptItem
                {
                    Type = AsciiScriptItemType.ConditionElse,
                    Command = "",
                    Value = "",
                    LineNumber = currentLineNumber
                });
                continue;
            }
            else if (line.StartsWith("ENDIF") &&  conditionDepth > 0)
            {
                conditionDepth--;
                _items.Add(new AsciiScriptItem
                {
                    Type = AsciiScriptItemType.ConditionEnd,
                    Command = "",
                    Value = "",
                    LineNumber = currentLineNumber
                });
            }
            else if (line.StartsWith("GOTO "))
            {
                _items.Add(new AsciiScriptItem
                {
                    Type = AsciiScriptItemType.Goto,
                    Command = "",
                    Value = line[5..].Trim(),
                    LineNumber = currentLineNumber
                });
            }
            else if (line.StartsWith("LABEL "))
            {
                _items.Add(new AsciiScriptItem
                {
                    Type = AsciiScriptItemType.GotoLabel,
                    Command = "",
                    Value = line[5..].Trim(),
                    LineNumber = currentLineNumber
                });
            }
            else if (line.EndsWith(":"))
            {
                var name = line[..^1]; // without trailing ':'
                if (_blocks.Contains(name, StringComparer.Ordinal))
                {
                    isBlock = true;
                    blockName = name;
                    _data.Clear();
                    continue;
                }
            }
            else if (isBlock) // Block end
            {
                if (line == "END" + blockName)
                {
                    _items.Add(new AsciiScriptItem
                    {
                        Type = AsciiScriptItemType.Block,
                        Command = blockName.Trim(),
                        Value = _data.ToString(),
                        LineNumber = currentLineNumber
                    });
                    isBlock = false;
                    _data.Clear();
                }
                else
                {
                    _data.AppendLine(line);
                }
                continue;
            } else if (_commands.Contains(line, StringComparer.Ordinal))
            {
                _items.Add(new AsciiScriptItem
                {
                    Type = AsciiScriptItemType.Command,
                    Command = line.Trim(),
                    Value = "",
                    LineNumber = currentLineNumber
                });
                continue;
            }

            var idx = line.IndexOf('=');
            if (idx > 0)
            {
                var key = line[..idx];
                var val = line[(idx + 1)..];
                
                if (_variables.Contains(key, StringComparer.Ordinal))
                    _items.Add(new AsciiScriptItem
                    {
                        Type = AsciiScriptItemType.Variable, 
                        Command = key, 
                        Value = val,
                        LineNumber = currentLineNumber
                    });
            }
        }
        
        if (isBlock)
            throw new Exception("Block not closed");
        if (conditionDepth != 0)
            throw new Exception("Condition not closed");
    }
}