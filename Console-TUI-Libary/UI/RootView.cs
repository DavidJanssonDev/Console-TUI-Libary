using System;
using System.Collections.Generic;
using System.Text;

using TuiEngine.Core;

namespace TuiEngine.UI;

public class RootView : View
{
    private bool isDirty = true;

    public void MarkDirty()
    {
        isDirty = true;
    }

    public bool ConsumeDirty()
    {
        if (!isDirty)
            return false;

        isDirty = false;
        return true;
    }
}