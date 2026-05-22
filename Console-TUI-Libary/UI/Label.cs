using System;
using System.Collections.Generic;
using System.Text;

using TuiEngine.Core;

namespace TuiEngine.UI;

public class Label : View
{
    public string Text { get; set; }

    public Label(string text)
    {
        Text = text;
    }

    public override void Render(ScreenBuffer screen)
    {
        screen.WriteLine(Text);
        base.Render(screen);
    }
}