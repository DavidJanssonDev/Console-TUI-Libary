using System;
using System.Collections.Generic;
using System.Text;

namespace TuiEngine.Input;

public class InputHandler
{
    public ConsoleKey? ReadKey()
    {
        if (!Console.KeyAvailable)
            return null;

        return Console.ReadKey(true).Key;
    }
}