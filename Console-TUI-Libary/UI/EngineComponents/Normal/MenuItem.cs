using System;
using System.Collections.Generic;
using System.Text;

namespace TuiEngine.UI.EngineComponents.Normal;

public class MenuItem : Selectable
{
    public string Label { get; set; }

    public MenuItem(string label, Action? onClick = null)
    {
        Label = label;
        OnClick = onClick;
    }

    public override void Draw(Rendering.Buffer buffer, int x, int y)
    {
        (ConsoleColor fg, ConsoleColor bg) = CurrentColors;

        // Fill the Full item width with backgroundcolor so selection highligt is solid

        for (int i = 0; i < Label.Length + 2; i++)
            buffer.Set(x + i, y, ' ', fg, bg);

        // Draw Label with a leading space for padding
        for (int i = 0; i < Label.Length; i++)
            buffer.Set(x + 1 + i, y, Label[i], fg, bg);

    }
}
