using System;
using System.Collections.Generic;
using System.Text;

namespace TuiEngine.Rendering;

public class Buffer
{
    public int Width;
    public int Height;

    public Cell[,] Cells;

    public Buffer(int width, int height)
    {
        Width = width;
        Height = height;
        Cells = new Cell[width, height];
        
        Clear();
    }

    public void Clear()
    {
        for (int y = 0; y < Height; y++)
        for (int x = 0; x < Width; x++)
            Cells[x, y] = new Cell(' ');
    }

    public void Set(int x, int y, char c)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height)
            return;

        Cells[x, y] = new Cell(c);
    }
}
