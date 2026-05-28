using System;
using System.Collections.Generic;
using System.Text;
using TuiEngine.Input;
using TuiEngine.Rendering;
using TuiEngine.UI;
using Buffer = TuiEngine.Rendering.Buffer;
namespace TuiEngine.Core;

internal class Engine
{
    private readonly View root;
    private readonly InputHandler input = new();
    private DateTime lastTick = DateTime.UtcNow;

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
        Console.CursorVisible = false;

        while (true)
        {
            // Delta Time Calculation
            var now = DateTime.UtcNow;
            Time.DeltaTime = (float)(now - lastTick).TotalSeconds;
            lastTick = now;

            // Collect All Available this frame into a list
            var keys = new List<KeyEvent>();
            while (Console.KeyAvailable)
            {
                var k = Console.ReadKey(true).Key;
                if (k == ConsoleKey.Escape) return;
                keys.Add(new KeyEvent(k));
            }


            Update(keys);
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

    private void Update(List<KeyEvent> keys) => UpdateRecursive(root, keys);
    

    private void UpdateRecursive(View view, List<KeyEvent> keys)
    {
        if (view is Component c)
            c.OnUpdate(keys);

        foreach (View child in view.Children) 
            UpdateRecursive(child, keys);
    }
}
