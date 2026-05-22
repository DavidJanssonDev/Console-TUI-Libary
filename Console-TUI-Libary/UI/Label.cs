using System;
using System.Collections.Generic;
using System.Text;
using TuiEngine.Core;
using Buffer = TuiEngine.Rendering.Buffer;

namespace TuiEngine.UI;

public class Label : Component
{
    private string text;

    public string Text
    {
        get => text;

        set
        {
            if (text != value)
            {
                text = value;
                MarkDirty();
            }
        }
    }

    public Label(string text)
    {
        Text = text;
    }
    
    public override void OnUpdate()
    {
        // static label → nothing needed
    }

    public override void Draw(Buffer buffer, int x, int y)
    {
        for (int i = 0; i < Text.Length; i++)
        {
            buffer.Set(x + i, y, Text[i]);
        }
    }
}