using TuiEngine.Core;
using TuiEngine.UI;

using System;
using System.Collections.Generic;
using System.Text;

namespace TuiEngineExample.App;

public class HelloApp : TuiApp
{
    private int counter = 0;
    private Label counterLabel;

    protected override void Initialize()
    {
        Root.AddChild(new Label("Hello TuiEngine!"));

        counterLabel = new Label("Counter: 0");
        Root.AddChild(counterLabel);
    }

    protected override void HandleInput()
    {
        if (!Console.KeyAvailable) return;

        var key = Console.ReadKey(true).Key;

        if (key == ConsoleKey.Escape)
            Environment.Exit(0);

        if (key == ConsoleKey.Spacebar)
        {
            counter++;
            counterLabel.Text = $"Counter: {counter}";

            Root.MarkDirty(); // 🔥 THIS is the ONLY trigger needed
        }
    }
}