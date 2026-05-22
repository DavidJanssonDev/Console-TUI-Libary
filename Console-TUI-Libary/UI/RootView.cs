using System;
using System.Collections.Generic;
using System.Text;
using TuiEngine.Core;
using Buffer = TuiEngine.Rendering.Buffer;

namespace TuiEngine.UI;

public class RootView : View
{
    protected override void Draw(Buffer buffer, int offsetX, int offsetY)
    {
        buffer.Clear();
    }
}