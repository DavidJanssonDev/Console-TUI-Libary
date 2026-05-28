using System;
using System.Collections.Generic;
using System.Text;
using TuiEngine.Input;
using TuiEngine.Rendering;
using TuiEngine.UI;
using TuiEngine.UI.Manager;
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
        MountRecursive(root);
        FocusManager.Focused?.OnFocus();
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

    #region UPDATE
    private void Update(List<KeyEvent> keys)
    {
        // Tab is a global navigation key - handle it before any component sees it 
        if (keys.Any(k => k.Key == ConsoleKey.Tab))
            FocusManager.CycleNext();

        var filtered = keys.Where(k => k.Key != ConsoleKey.Tab).ToList();
        UpdateRecursive(root, filtered);
    }

    private void UpdateRecursive(View view, IReadOnlyList<KeyEvent> keys)
    {
        if (view is Component c)
        {
            var componentKeys = c.IsFocused ? keys : Array.Empty<KeyEvent>();
            c.OnUpdate(componentKeys);
        }

        foreach (View child in view.Children) 
            UpdateRecursive(child, keys);
    }
    #endregion

    #region MOUNT COMPONENTS
    private void MountRecursive(View view)
    {
        if (view is Component c) c.OnMount();
        foreach (View child in view.Children)
            MountRecursive(child);
    }
    #endregion 

}
