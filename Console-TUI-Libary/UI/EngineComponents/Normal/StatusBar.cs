using System;
using System.Collections.Generic;
using System.Text;
using Buffer = TuiEngine.Rendering.Buffer;
namespace TuiEngine.UI.EngineComponents.Normal;

public class StatusBar : Component
{
    public string message = "";

    public void Post(string msg)
    {
        message = msg;
        MarkDirty();
    }


    public override void Draw(Buffer buffer, int x, int y)
    {
        // Fill the row with the bar background
        for (int i = 0; i < buffer.Width; i++)
            buffer.Set(x + i, y, ' ', ConsoleColor.Black, ConsoleColor.DarkGray);

        // Write the message text
        for (int i = 0; i < Math.Min(message.Length, buffer.Width - 2); i++)
            buffer.Set(x + 1 + i, y, message[i], ConsoleColor.White, ConsoleColor.DarkGray);
    }
}
