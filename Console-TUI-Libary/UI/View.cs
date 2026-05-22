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

    public virtual void Update() { }

    public void Render(Buffer buffer, int x, int y)
    {
        Draw(buffer, x, y);

        int offsetY = 1;

        foreach (var child in Children)
        {
            child.Render(buffer, x, y + offsetY);
            offsetY += 1;
        }
    }

    protected abstract void Draw(Buffer buffer, int x, int y);
}