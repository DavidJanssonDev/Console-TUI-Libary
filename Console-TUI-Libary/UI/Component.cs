using System;
using System.Collections.Generic;
using System.Text;

namespace TuiEngine.UI;
public abstract class Component : View
{
    public bool IsDirty { get; private set; } = true;

    public virtual void OnMount() { }

    public virtual void OnUpdate() { }

    protected void MarkDirty()
    {
        IsDirty = true;
    }

    public void ClearDirty()
    {
        IsDirty = false;
    }

    public abstract void Draw(Rendering.Buffer buffer, int x, int y);
}