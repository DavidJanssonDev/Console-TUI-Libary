using System;
using System.Collections.Generic;
using System.Text;
using TuiEngine.Input;

namespace TuiEngine.UI.EngineComponents;

public abstract class Selectable : Component
{
    public Action? OnClick { get; set; }

    protected ConsoleColor NormalFg = ConsoleColor.Gray;
    protected ConsoleColor NormalBg = ConsoleColor.Black;
    protected ConsoleColor FocusedFg = ConsoleColor.Black;
    protected ConsoleColor FocusedBg = ConsoleColor.White;   // inverted = selected look

    public override void OnUpdate(IReadOnlyList<KeyEvent> keys)
    {
        foreach (KeyEvent key in keys)
        {
            if (key.Key == ConsoleKey.Enter)
                OnClick?.Invoke();
        }
    }

    protected (ConsoleColor fg, ConsoleColor bg) CurrentColors
        => IsFocused ? (FocusedFg, FocusedBg) : (NormalFg, NormalBg);
}

