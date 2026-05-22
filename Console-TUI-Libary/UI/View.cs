using System;
using System.Collections.Generic;
using System.Text;
using TuiEngine.Core;
using Buffer = TuiEngine.Rendering.Buffer;

namespace TuiEngine.UI;

public abstract class View
{
    public List<View> Children { get; } = new();

    public void Add(params View[] views)
    {
        foreach (var v in views)
            Children.Add(v);
    }

    public virtual void Render(Buffer buffer, int x, int y)
    {
        if (this is Component c)
        {
            c.Draw(buffer, x, y);
            c.ClearDirty();
        }

        int offsetY = 1;

        foreach (var child in Children)
        {
            child.Render(buffer, x, y + offsetY);
            offsetY += 1;
        }
    }
}