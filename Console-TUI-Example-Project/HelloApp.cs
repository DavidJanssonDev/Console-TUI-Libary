using TuiEngine.Core;
using TuiEngine.UI;

using System;
using System.Collections.Generic;
using System.Text;

namespace TuiEngineExample.App;

public class HelloApp : TuiApp
{
    private int counter = 0;
    private Label label;

    protected override void Initialize()
    {
        Root.AddChild(new Label("Hello TuiEngine!"));

        label = new Label("Counter: 0");
        Root.AddChild(label);
    }

    protected override void HandleInput()
    {
        if (!Console.KeyAvailable) return;

        var key = Console.ReadKey(true).Key;

        if (key == ConsoleKey.Spacebar)
        {
            counter++;
            label.Text = $"Counter: {counter}";
        }
    }
}