using System;
using System.Collections.Generic;
using System.Text;
using TuiEngine.Input;
using Buffer = TuiEngine.Rendering.Buffer;

namespace TuiEngine.UI;

public class NumberCounter : Component
{
    private int value = 0;

    private string? cached;

    public override void OnUpdate(IReadOnlyList<KeyEvent> _)
    {
        value++;

        cached = $"Counter: {value}";

        MarkDirty(); // 🔥 ONLY WHEN VALUE CHANGES
    }

    public override void Draw(Rendering.Buffer buffer, int x, int y)
    {
        if (cached == null) return;

        for (int i = 0; i < cached.Length; i++)
        {
            buffer.Set(x + i, y, cached[i]);
        }
    }
}