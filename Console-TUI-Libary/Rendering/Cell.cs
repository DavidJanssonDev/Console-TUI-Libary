using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace TuiEngine.Rendering;

public struct Cell
{
    public char Char;
    public ConsoleColor Foreground;
    public ConsoleColor Background;

    public static readonly Cell Empty = new(' ', ConsoleColor.Gray, ConsoleColor.Black);

    public Cell(char c, ConsoleColor fg = ConsoleColor.Gray, ConsoleColor bg = ConsoleColor.Black)
    {
        Char = c;
        Foreground = fg;
        Background = bg;
    }

    public override bool Equals(object? obj)
           => obj is Cell cell
              && Char == cell.Char
              && Foreground == cell.Foreground
              && Background == cell.Background;

    public override int GetHashCode()
            => HashCode.Combine(Char, Foreground, Background);
}
