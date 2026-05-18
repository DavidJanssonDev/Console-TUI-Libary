using System;
using System.Collections.Generic;
using System.Text;
using TuiEngine;

namespace Console_TUI_Example_Project;

class DemoApp : TuiApp
{
    private int counter = 0;
    protected override void OnKey(ConsoleKey key)
    {
        switch (key)
        {
            case ConsoleKey.Spacebar:
                counter++;
                break;

            case ConsoleKey.R:
                counter = 0;
                break;
        }
    }

    protected override void Render()
    {
        Screen.Clear();

        Screen.WriteLine("╔══════════════════════╗");
        Screen.WriteLine("║     TUI DEMO         ║");
        Screen.WriteLine("╠══════════════════════╣");
        Screen.WriteLine($"║ Counter: {counter,-12}║");
        Screen.WriteLine("║                      ║");
        Screen.WriteLine("║ SPACE = +1           ║");
        Screen.WriteLine("║ R = reset            ║");
        Screen.WriteLine("║ ESC = quit           ║");
        Screen.WriteLine("╚══════════════════════╝");

        Screen.RenderToConsole();
    }
}
