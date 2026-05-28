using System;
using System.Collections.Generic;
using System.Text;

namespace TuiEngine.UI.EngineComponents.Abstract;

public readonly struct MenuKeys
{
    public ConsoleKey Up { get; }
    public ConsoleKey Down { get; }
    public ConsoleKey Select { get; }

    public MenuKeys(ConsoleKey up, ConsoleKey down, ConsoleKey select)
    {
        Up = up;
        Down = down;
        Select = select;
    }

    // Convenience factory — the most common case
    public static MenuKeys Arrows(ConsoleKey select = ConsoleKey.Enter)
        => new(ConsoleKey.UpArrow, ConsoleKey.DownArrow, select);

    // VI keys
    public static MenuKeys Vi(ConsoleKey select = ConsoleKey.Enter)
        => new(ConsoleKey.K, ConsoleKey.J, select);

    // Numpad
    public static MenuKeys Numpad(ConsoleKey select = ConsoleKey.Enter)
        => new(ConsoleKey.NumPad8, ConsoleKey.NumPad2, select);
}
