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

        // Render all children except the last normally
        for (int i = 0; i < Children.Count - 1; i++)
            Children[i].Render(buffer, x, y + 1);

        // Pin the last child (StatusBar) to the bottom row
        Children[^1].Render(buffer, x, buffer.Height - 1);
    }
}