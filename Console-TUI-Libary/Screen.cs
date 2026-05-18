using System;
using System.Collections.Generic;
using System.Text;

namespace TuiEngine;

public class Screen
{
    private readonly StringBuilder _sb = new ();

    public void Clear()
    {
        _sb.Clear();
    }

    public void WriteLine(string text)
    {
        _sb.AppendLine(text);
    }

    public void RenderToConsole()
    {
        Console.SetCursorPosition(0, 0);
        Console.WriteLine(_sb.ToString());
    }
}
