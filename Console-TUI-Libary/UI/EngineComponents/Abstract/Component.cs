using System;
using System.Collections.Generic;
using System.Text;
using TuiEngine.Input;
using TuiEngine.UI.Manager;

namespace TuiEngine.UI;
public abstract class Component : View
{
    public bool IsFocused => FocusManager.Focused == this;
    public bool IsDirty { get; private set; } = true;

    public virtual void OnFocus() { }
    public virtual void OnBlur() { }

    public virtual void OnMount() 
    {
        FocusManager.Register(this);
    }

    public virtual void OnUpdate(IReadOnlyList<KeyEvent> keys) { }

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