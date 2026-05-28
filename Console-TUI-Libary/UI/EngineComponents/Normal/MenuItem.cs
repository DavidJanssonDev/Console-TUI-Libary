using System;
using System.Collections.Generic;
using System.Text;
using Buffer = TuiEngine.Rendering.Buffer;
namespace TuiEngine.UI.EngineComponents.Normal;

public class MenuItem : Selectable
{
    public string Label { get; set; }
    public bool isSelected = false;

    public MenuItem(string label, Action? onClick = null)
    {
        Label = label;
        OnClick = onClick;
    }


    public void ForceSelected(bool value)
    {
        isSelected = value;
        MarkDirty();
    }

    public override void Draw(Buffer buffer, int x, int y)
    {
        var fg = isSelected ? FocusedFg : NormalFg;
        var bg = isSelected ? FocusedBg : NormalBg;

        for (int i = 0; i < Label.Length + 2; i++)
            buffer.Set(x + i, y, ' ', fg, bg);

        for (int i = 0; i < Label.Length; i++)
            buffer.Set(x + 1 + i, y, Label[i], fg, bg);
    }
}

