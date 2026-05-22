using System;
using System.Collections.Generic;
using System.Text;
using TuiEngine.Rendering;
using TuiEngine.UI;
using Buffer = TuiEngine.Rendering.Buffer;
namespace TuiEngine.Core;

internal class Engine
{
    private readonly View root;

    private Buffer back;
    private Buffer front;

    public Engine(View root)
    {
        this.root = root;

        int width = Console.WindowWidth;
        int height = Console.WindowHeight;

        back = new Buffer(width, height);
        front = new Buffer(width, height);
    }

    public void Run()
    {
        while (true)
        {
            if (Console.KeyAvailable &&
                Console.ReadKey(true).Key == ConsoleKey.Escape)
                break;

            Update();
            Render();
            Thread.Sleep(16);

        }
    }

    private void Render()
    {
        back.Clear();

        root.Render(back, 0, 0);

        Renderer.DiffRender(back, front);
    }

    private void Update()
    {
        UpdateRecursive(root);
    }

    private void UpdateRecursive(View view)
    {
        if (view is Component c)
            c.OnUpdate();

        foreach (View child in view.Children) 
            UpdateRecursive(child);
    }
}
