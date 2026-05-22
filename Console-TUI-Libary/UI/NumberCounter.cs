using System;
using System.Collections.Generic;
using System.Text;
using Buffer = TuiEngine.Rendering.Buffer;

namespace TuiEngine.UI;

public class NumberCounter : Component
{
    private int value = 0;

    private string cachedText = "";

    public override void OnUpdate()
    {
        // simple demo logic: increment every frame
        value++;

        cachedText = $"Counter: {value}";
    }

    protected override void Draw(Buffer buffer, int x, int y)
    {
        for (int i = 0; i < cachedText.Length; i++)
        {
            buffer.Set(x + i, y, cachedText[i]);
        }
    }
}