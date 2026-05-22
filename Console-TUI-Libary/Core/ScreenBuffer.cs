using System;
using System.Collections.Generic;
using System.Text;

namespace TuiEngine.Core;

public class ScreenBuffer
{
    private readonly List<string> lines = new();

    public void Clear()
    {
        lines.Clear();
    }

    public void WriteLine(string text)
    {
        lines.Add(text);
    }

    public void Present()
    {
        Console.SetCursorPosition(0, 0);

        foreach (var line in lines)
            Console.WriteLine(line);
    }
}