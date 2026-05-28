using System;
using System.Collections.Generic;
using System.Text;
using TuiEngine.Input;
using Buffer = TuiEngine.Rendering.Buffer;
namespace TuiEngine.UI.EngineComponents.Normal;

public class Menu : Component
{
    private readonly List<MenuItem> items;
    private int selectedIndex = 0;

    public Menu(params MenuItem[] menuItems)
    {
        items = new List<MenuItem>(menuItems);
        // Register items as children so View.Render traverses them
        foreach (MenuItem item in items)
            Add(item);
    }

    public override void OnFocus()
    {
        // When the menu gains focus, highlit the current item
        SyncSelection();
    }

    public override void OnUpdate(IReadOnlyList<KeyEvent> keys)
    {
        foreach (KeyEvent key in keys)
        {
            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    selectedIndex = (selectedIndex - 1 + items.Count) % items.Count;
                    SyncSelection();
                    break;

                case ConsoleKey.DownArrow:
                    selectedIndex = (selectedIndex + 1) % items.Count;
                    SyncSelection();
                    break;

                case ConsoleKey.Enter:
                    items[selectedIndex].OnClick?.Invoke();
                    break;

            }
        }
        MarkDirty();
    }

    public override void Draw(Buffer buffer, int x, int y)
    {
        // Menu itself draws nothing — each MenuItem draws itself via View.Render's child traversal
    }


    private void SyncSelection()
    {
        // Mirror focus state onto items without going thorugh FocusManager
        // (the Menu holds focus; items are visual-only children)
        for (int i = 0; i < items.Count; i++)
            items[i].ForceSelected(i == selectedIndex);
    }
}
