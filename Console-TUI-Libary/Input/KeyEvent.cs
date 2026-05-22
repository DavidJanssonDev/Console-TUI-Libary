using System;
using System.Collections.Generic;
using System.Text;

namespace TuiEngine.Input;

public struct KeyEvent
{
    public ConsoleKey Key { get; }

    public KeyEvent(ConsoleKey key)
    {
        Key = key;
    }
}
