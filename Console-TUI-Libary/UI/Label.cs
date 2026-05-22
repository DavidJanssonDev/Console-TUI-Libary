using System;
using System.Collections.Generic;
using System.Text;
using TuiEngine.Core;
using Buffer = TuiEngine.Rendering.Buffer;

namespace TuiEngine.UI;

public class Label : View
{
    public string Text { get; set; }

    public Label(string text)
    {
        Text = text;
    }

    protected override void Draw(Buffer buffer, int offsetX, int offsetY)
    {
        for (int i = 0; i < Text.Length; i++)
        {
            buffer.Set(offsetX + i, offsetY, Text[i]);
        }
    }
}