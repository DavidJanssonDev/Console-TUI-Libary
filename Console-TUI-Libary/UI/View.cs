using System;
using System.Collections.Generic;
using System.Text;
using TuiEngine.Core;
using Buffer = TuiEngine.Rendering.Buffer;

namespace TuiEngine.UI;

public abstract class View
{
    public List<View> Children { get; } = new();

    public View? Parent { get; private set; }

    public int X { get; set; }
    public int Y { get; set; }

    public void AddChild(View child)
    {
        child.Parent = this;
        Children.Add(child);
    }

    public virtual void Update()
    {
        foreach (var child in Children)
            child.Update();
    }

    public void Render(Buffer buffer, int offsetX = 0, int offsetY = 0)
    {
        Draw(buffer, offsetX, offsetY);

        int yOffset = 0;

        foreach (var child in Children)
        {
            child.Render(buffer, offsetX, offsetY + yOffset);
            yOffset += 1; // 🔥 stack vertically
        }
    }

    protected abstract void Draw(Buffer buffer, int offsetX, int offsetY);
}