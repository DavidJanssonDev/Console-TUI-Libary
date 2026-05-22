using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace TuiEngine.Rendering;

public struct Cell
{
    public char Char;

    public Cell(char c)
    {
        Char = c;
    }

    public override bool Equals(object? obj)
    {
        return obj is Cell cell && Char == cell.Char;
    }

    public override int GetHashCode()
    {
        return Char.GetHashCode();
    }
}
