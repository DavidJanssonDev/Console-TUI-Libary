using System;
using System.Collections.Generic;
using System.Text;

namespace TuiEngine.UI;
public abstract class Component : View
{
    public virtual void OnMount() { }

    public virtual void OnUpdate() { }
}