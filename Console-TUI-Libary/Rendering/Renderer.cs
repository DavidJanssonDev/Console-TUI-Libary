using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;
using System.Text;

namespace TuiEngine.Rendering;

public static class Renderer
{
    public static void DiffRender(Buffer back, Buffer front)
    {
        var sb = new StringBuilder();

        for (int y = 0; y < back.Height; y++)
        {
            for (int x = 0; x < back.Width; x++)
            {
                Cell newCell = back.Cells[x, y];
                Cell oldCell = front.Cells[x, y];

                if (newCell.Equals(oldCell))
                    continue;

                sb.Append($"\x1b[{y + 1};{x + 1}H");
                sb.Append(newCell.Char);

                front.Cells[x, y] = newCell;
            }
        }

        Console.Write(sb.ToString());
    }
}
