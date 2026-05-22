using System;
using System.Collections.Generic;
using System.Text;
using TuiEngine.Core;
using Buffer = TuiEngine.Rendering.Buffer;

namespace TuiEngine.UI;

internal class RootView : View
{
    public override void Render(Buffer buffer, int x, int y)
    {
        buffer.Clear(); // ONLY HERE
        base.Render(buffer, x, y);
    }
}