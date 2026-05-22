using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleTUILibary.Input;

public static class Input
{
    public static bool KeyAvailable => Console.KeyAvailable;

    public static ConsoleKey ReadKey()
    {
        return Console.ReadKey(true).Key;
    }
}
