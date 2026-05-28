using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace TuiEngine.Helpers;

internal static class AnsiColor
{
    // Maps ConsoleColor enum values to standard 4-bit ANSI codes
    private static readonly int[] FgMap =
    {
        30, // Black
        31, // DarkRed
        32, // DarkGreen
        33, // DarkYellow
        34, // DarkBlue
        35, // DarkMagenta
        36, // DarkCyan
        37, // Gray
        90, // DarkGray
        91, // Red
        92, // Green
        93, // Yellow
        94, // Blue
        95, // Magenta
        96, // Cyan
        97  // White
    };
    private static readonly int[] BgMap =
    {
        40, // Black
        41, // DarkRed
        42, // DarkGreen
        43, // DarkYellow
        44, // DarkBlue
        45, // DarkMagenta
        46, // DarkCyan
        47, // Gray
        100, // DarkGray
        101, // Red
        102, // Green
        103, // Yellow
        104, // Blue
        105, // Magenta
        106, // Cyan
        107  // White
    };

    public static string Fg(ConsoleColor color) => $"\x1b[{FgMap[(int) color]}m";
    public static string Bg(ConsoleColor color) => $"\x1b[{BgMap[(int) color]}m";

}
