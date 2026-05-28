using System;
using System.Collections.Generic;
using System.Text;

namespace TuiEngine.UI.Manager;

public static class FocusManager
{
    private static readonly List<Component> focusable = new();
    private static int index = 0;

    public static Component? Focused
        => focusable.Count > 0 ? focusable[index] : null;

    public static void Register(Component component)
    {
        if (!focusable.Contains(component)) focusable.Add(component);
    }

    public static void Unregister(Component component)
        => focusable.Remove(component);

    public static void CycleNext()
    {
        if (focusable.Count == 0) return;

        Focused?.OnBlur();
        index = (index + 1 ) % focusable.Count;
        Focused?.OnFocus();
    }

}
