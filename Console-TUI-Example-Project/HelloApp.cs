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
        base.HandleInput();

        if (!Console.KeyAvailable) return;

        var key = Console.ReadKey(true).Key;

        if (key == ConsoleKey.Spacebar)
        {
            counter++;
        }
    }

    protected override void Update()
    {
        counterLabel.Text = $"Counter: {counter}";

        Root.Update();
    }
}