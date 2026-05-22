using System;
using System.Collections.Generic;
using System.Text;

namespace TuiEngine.Rendering;

public static class Renderer
{
    public static void DiffRender(Buffer back, Buffer front)
    {
        Console.SetCursorPosition(0, 0);

        for (int y = 0; y < back.Height; y++)
        {
            for (int x = 0; x < back.Width; x++)
            {
                char b = back.Cells[x, y].Char;
                char f = front.Cells[x, y].Char;

                if (b == f)
                    continue;

                Console.SetCursorPosition(x, y);
                Console.Write(b);

                front.Cells[x, y] = back.Cells[x, y];
            }
        }

        Console.SetCursorPosition(0, 0);
    }
}
