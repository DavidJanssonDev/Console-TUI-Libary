using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using TuiEngine.Helpers;

namespace TuiEngine.Rendering;

public static class Renderer
{
    public static void DiffRender(Buffer back, Buffer front)
    {
        var sb = new StringBuilder();

        ConsoleColor lastFg = (ConsoleColor)(-1);
        ConsoleColor lastBg = (ConsoleColor)(-1);

        for (int y = 0; y < back.Height; y++)
        for (int x = 0; x < back.Width;  x++)
        {
            Cell newCell = back.Cells[x, y];
            Cell oldCell = front.Cells[x, y];

            if (newCell.Equals(oldCell)) continue;

            // Move cursor to (x, y)
            sb.Append($"\x1b[{y + 1};{x + 1}H");

            if (newCell.Foreground != lastFg)
            {
                sb.Append(AnsiColor.Fg(newCell.Foreground));
                lastFg = newCell.Foreground;
            }

            if (newCell.Background != lastBg)
            {
                sb.Append(AnsiColor.Bg(newCell.Background));
                lastBg = newCell.Background;
            }


            sb.Append(newCell.Char);
            front.Cells[x, y] = newCell;
        }
        
        // Reset color at end of frame so the shell prompt isn't tinted
        sb.Append("\x1b[0m");
        Console.Write(sb.ToString());
    }
}
