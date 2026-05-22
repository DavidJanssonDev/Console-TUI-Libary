using System;
using System.Collections.Generic;
using System.Text;
using TuiEngine.Core;
using Buffer = TuiEngine.Rendering.Buffer;

namespace TuiEngine.UI;

public class Label : Component
{
    public string Text;

    public Label(string text)
    {
        Text = text;
    }

    protected override void Draw(Buffer buffer, int x, int y)
    {
        if (string.IsNullOrEmpty(Text))
            return;

        for (int i = 0; i < Text.Length; i++)
        {
            buffer.Set(x + i, y, Text[i]);
        }
    }
}