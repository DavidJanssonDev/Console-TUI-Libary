using System;
using System.Collections.Generic;
using System.Text;
using TuiEngine.Core;

namespace TuiEngine.UI;

public abstract class View
{
    public List<View> Children { get; } = new();

    public View? Parent { get; private set; }

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

    public virtual void Render(ScreenBuffer screen)
    {
        foreach (var child in Children)
            child.Render(screen);
    }
}