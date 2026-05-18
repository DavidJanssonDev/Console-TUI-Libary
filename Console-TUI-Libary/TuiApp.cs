using System;
using System.Collections.Generic;
using System.Text;

namespace TuiEngine;

public abstract class TuiApp
{
    private bool _running;
    protected Screen Screen { get; } = new Screen();

    public void Run()
    {
        Console.CursorVisible = false;
        _running = true;

        while (_running)
        {
            HandleInput();
            Update();
            Render();

            Thread.Sleep(16); // ~60 FPS
        }

        Console.CursorVisible = true;
    }

    private void HandleInput()
    {
        if (!Input.KeyAvailable) return;

        var key = Input.ReadKey();

        if (key == ConsoleKey.Escape) 
            _running = false;

        OnKey(key);
    }

    protected abstract void OnKey(ConsoleKey key);

    protected virtual void Update() { }

    protected abstract void Render();
}
