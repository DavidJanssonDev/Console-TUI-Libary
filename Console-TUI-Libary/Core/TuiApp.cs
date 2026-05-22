using TuiEngine.Rendering;
using System;
using System.Collections.Generic;
using System.Text;
using TuiEngine.UI;
using Buffer = TuiEngine.Rendering.Buffer;

namespace TuiEngine.Core;

public abstract class TuiApp
{
    protected Buffer BackBuffer;
    protected Buffer FrontBuffer;
    protected RootView Root;

    private bool running = true;

    protected TuiApp()
    {
        int width = Console.WindowWidth;
        int height = Console.WindowHeight;

        BackBuffer = new Buffer(width, height);
        FrontBuffer = new Buffer(width, height);
        Root = new RootView();
    }

    public void Run()
    {
        Initialize();

        while (running)
        {
            HandleInput();
            Update();

            Render();
        }
    }

    protected virtual void Initialize() { }

    protected virtual void Update()
    {
        Root.Update();
    }

    protected virtual void HandleInput()
    {
        if (!Console.KeyAvailable) return;

        var key = Console.ReadKey(true).Key;

        if (key == ConsoleKey.Escape)
            running = false;
    }

    private void Render()
    {
        BackBuffer.Clear();

        Root.Render(BackBuffer, 0, 0);

        Renderer.DiffRender(BackBuffer, FrontBuffer);
    }
}