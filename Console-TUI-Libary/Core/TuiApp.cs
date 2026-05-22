using System;
using System.Collections.Generic;
using System.Text;
using TuiEngine.UI;

namespace TuiEngine.Core;

public abstract class TuiApp
{
    protected ScreenBuffer Screen { get; private set; }
    protected RootView Root { get; private set; }

    private bool running = true;

    protected TuiApp()
    {
        Screen = new ScreenBuffer();
        Root = new RootView();
    }

    public void Run()
    {
        Initialize();

        while (running)
        {
            HandleInput();
            Update();

            if (Root.ConsumeDirty())
            {
                Render();
            }

            Thread.Sleep(16);
        }
    }

    protected virtual void Initialize() { }

    protected virtual void Update()
    {
        Root.Update();
    }

    protected virtual void Render()
    {
        Screen.Clear();
        Root.Render(Screen);
        Screen.Present();
    }

    protected void RequestRender()
    {
        Render();
    }

    protected virtual void HandleInput()
    {
        if (!Console.KeyAvailable) return;

        var key = Console.ReadKey(true).Key;

        if (key == ConsoleKey.Escape)
            running = false;
    }
}