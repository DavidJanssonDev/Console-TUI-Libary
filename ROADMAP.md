# TUI Engine — Menu Roadmap

A step-by-step build plan to take the current engine from a render loop with a `Label` and `NumberCounter`
to a fully navigable, keyboard-driven menu — following the same Unity UI Toolkit pattern already
established in the codebase.

---

## Current state (baseline)

Before touching anything, here is what exists and what is missing:

| Area | What exists | What is missing |
|---|---|---|
| Loop | `Engine.Run()` with a 16 ms sleep | `Time.DeltaTime` is never set; Escape is the only key handled |
| Input | `InputHandler`, `KeyEvent` | Neither is used anywhere; `Engine` reads the console directly |
| Rendering | `Buffer`, `Cell`, `Renderer.DiffRender` | `Cell` has no color; renderer emits no ANSI color codes |
| UI | `View`, `Component`, `Label`, `NumberCounter` | No focus, no selection, no interactive components |
| Layout | `FlexLayout` (data class only) | Layout math is hardcoded as `offsetY += 1` in `View.Render` |

---

## Phase 1 — Wire input into the engine loop

### Goal
Every component gets a chance to read keyboard input on each frame, without polling the console
themselves. The engine owns input collection; components only consume it.

### Problem with the current code
`Engine.Run()` does this:

```csharp
if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
    break;
```

This reads at most one key per frame and discards it immediately after checking for Escape.
`InputHandler` sits in `TuiEngine.Input` and is never instantiated. `Time.DeltaTime` is declared
but never written.

### What to build

**1. Fix `Engine` to collect keys and set delta time**

```csharp
// Engine.cs — new fields
private readonly InputHandler input = new();
private DateTime lastTick = DateTime.UtcNow;

// Engine.Run() — replace the existing key check
public void Run()
{
    Console.CursorVisible = false;

    while (true)
    {
        // Delta time
        var now = DateTime.UtcNow;
        Time.DeltaTime = (float)(now - lastTick).TotalSeconds;
        lastTick = now;

        // Collect ALL keys available this frame into a list
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
```

**2. Pass the key list through the update tree**

Change `UpdateRecursive` to accept `IReadOnlyList<KeyEvent>` and hand it to each component:

```csharp
private void Update(List<KeyEvent> keys)
    => UpdateRecursive(root, keys);

private void UpdateRecursive(View view, IReadOnlyList<KeyEvent> keys)
{
    if (view is Component c)
        c.OnUpdate(keys);          // <-- components now receive keys

    foreach (View child in view.Children)
        UpdateRecursive(child, keys);
}
```

**3. Update `Component.OnUpdate` signature**

```csharp
// Component.cs
public virtual void OnUpdate(IReadOnlyList<KeyEvent> keys) { }
```

Existing components (`Label`, `NumberCounter`) just ignore the parameter — no breaking change in
behavior, just a signature update.

### Why this matters
Everything from Phase 3 onward depends on components being able to read key input. Getting this
right first means the rest of the phases slot in cleanly.

---

## Phase 2 — Add color to Cell, Buffer, and Renderer

### Goal
A selected menu item needs to look different from an unselected one. Right now `Cell` holds only a
`char` — there is nowhere to store color. This phase adds color support end-to-end.

### What to build

**1. Extend `Cell`**

```csharp
// Cell.cs
public struct Cell
{
    public char Char;
    public ConsoleColor Foreground;
    public ConsoleColor Background;

    public static readonly Cell Empty = new(' ', ConsoleColor.Gray, ConsoleColor.Black);

    public Cell(char c,
                ConsoleColor fg = ConsoleColor.Gray,
                ConsoleColor bg = ConsoleColor.Black)
    {
        Char       = c;
        Foreground = fg;
        Background = bg;
    }

    public override bool Equals(object? obj)
        => obj is Cell cell
           && Char       == cell.Char
           && Foreground == cell.Foreground
           && Background == cell.Background;

    public override int GetHashCode()
        => HashCode.Combine(Char, Foreground, Background);
}
```

**2. Extend `Buffer.Set`**

```csharp
// Buffer.cs — add color overload, keep the old one for compatibility
public void Set(int x, int y, char c,
                ConsoleColor fg = ConsoleColor.Gray,
                ConsoleColor bg = ConsoleColor.Black)
{
    if (x < 0 || y < 0 || x >= Width || y >= Height) return;
    Cells[x, y] = new Cell(c, fg, bg);
}
```

`Buffer.Clear()` should fill with `Cell.Empty` so the diff renderer knows to reset color on blank
cells.

**3. Emit ANSI color codes in `Renderer`**

```csharp
// Renderer.cs
public static void DiffRender(Buffer back, Buffer front)
{
    var sb = new StringBuilder();

    ConsoleColor lastFg = (ConsoleColor)(-1);
    ConsoleColor lastBg = (ConsoleColor)(-1);

    for (int y = 0; y < back.Height; y++)
    for (int x = 0; x < back.Width;  x++)
    {
        Cell newCell = back.Cells[x, y];
        Cell oldCell = front.Cells[x, y];

        if (newCell.Equals(oldCell)) continue;

        // Move cursor
        sb.Append($"\x1b[{y + 1};{x + 1}H");

        // Only emit color codes when they actually change (avoids bloating the output)
        if (newCell.Foreground != lastFg)
        {
            sb.Append(AnsiColor.Fg(newCell.Foreground));
            lastFg = newCell.Foreground;
        }
        if (newCell.Background != lastBg)
        {
            sb.Append(AnsiColor.Bg(newCell.Background));
            lastBg = newCell.Background;
        }

        sb.Append(newCell.Char);
        front.Cells[x, y] = newCell;
    }

    // Reset color at end of frame so the shell prompt isn't tinted
    sb.Append("\x1b[0m");
    Console.Write(sb.ToString());
}
```

**4. Add `AnsiColor` helper**

```csharp
// AnsiColor.cs  (new file, TuiEngine.Rendering)
internal static class AnsiColor
{
    // Maps ConsoleColor enum values to standard 4-bit ANSI codes
    private static readonly int[] FgMap =
    { 30,34,32,36,31,35,33,37,90,94,92,96,91,95,93,97 };

    private static readonly int[] BgMap =
    { 40,44,42,46,41,45,43,47,100,104,102,106,101,105,103,107 };

    public static string Fg(ConsoleColor c) => $"\x1b[{FgMap[(int)c]}m";
    public static string Bg(ConsoleColor c) => $"\x1b[{BgMap[(int)c]}m";
}
```

### Why the diff check must include color
`Cell.Equals` now compares `Char`, `Foreground`, and `Background`. If only the color changes (e.g.
the cursor moves to an item and inverts it) the diff will catch it even though the character is the
same `' '` or letter.

---

## Phase 3 — Focus system

### Goal
Only the focused component receives and acts on key input. Tab cycles focus between focusable
components. This mirrors Unity's `Focusable` class.

### What to build

**1. `FocusManager` — static registry**

```csharp
// FocusManager.cs  (new file, TuiEngine.UI)
public static class FocusManager
{
    private static readonly List<Component> focusable = new();
    private static int index = 0;

    public static Component? Focused
        => focusable.Count > 0 ? focusable[index] : null;

    public static void Register(Component c)
    {
        if (!focusable.Contains(c)) focusable.Add(c);
    }

    public static void Unregister(Component c)
        => focusable.Remove(c);

    public static void CycleNext()
    {
        if (focusable.Count == 0) return;
        Focused?.OnBlur();
        index = (index + 1) % focusable.Count;
        Focused?.OnFocus();
    }
}
```

**2. Add focus lifecycle to `Component`**

```csharp
// Component.cs — new members
public bool IsFocused => FocusManager.Focused == this;

public virtual void OnFocus() { }
public virtual void OnBlur()  { }

// Call Register in OnMount (or in a constructor override)
public virtual void OnMount()
{
    FocusManager.Register(this);
}
```

**3. Engine handles Tab before distributing keys**

```csharp
// Engine.UpdateRecursive — intercept Tab first
private void Update(List<KeyEvent> keys)
{
    // Tab is a global navigation key — handle it before any component sees it
    if (keys.Any(k => k.Key == ConsoleKey.Tab))
        FocusManager.CycleNext();

    // Only pass remaining keys to the focused component
    var filtered = keys.Where(k => k.Key != ConsoleKey.Tab).ToList();
    UpdateRecursive(root, filtered);
}

private void UpdateRecursive(View view, IReadOnlyList<KeyEvent> keys)
{
    if (view is Component c)
    {
        // Non-focused components get an empty key list
        var componentKeys = c.IsFocused ? keys : Array.Empty<KeyEvent>();
        c.OnUpdate(componentKeys);
    }

    foreach (View child in view.Children)
        UpdateRecursive(child, keys);
}
```

**4. Give focus to the first registered component on startup**

```csharp
// Engine constructor — after building the tree, set initial focus
public Engine(View root)
{
    this.root = root;
    MountRecursive(root);        // calls OnMount on all components
    FocusManager.Focused?.OnFocus();
    // ... buffer init
}

private void MountRecursive(View view)
{
    if (view is Component c) c.OnMount();
    foreach (var child in view.Children)
        MountRecursive(child);
}
```

### What this looks like in practice
A `NumberCounter` with focus logs a `*` next to its value. A `Menu` (Phase 5) only moves its
selection when it owns focus. Without focus, it ignores all input — exactly as a Unity `Selectable`
would.

---

## Phase 4 — Selectable and MenuItem

### Goal
A `Selectable` base component that knows how to render itself differently when focused or chosen,
and a `MenuItem` that pairs a label with a callback action.

### What to build

**1. `Selectable` — base for interactive components**

```csharp
// Selectable.cs  (new file, TuiEngine.UI)
public abstract class Selectable : Component
{
    public Action? OnClick { get; set; }

    protected ConsoleColor NormalFg   = ConsoleColor.Gray;
    protected ConsoleColor NormalBg   = ConsoleColor.Black;
    protected ConsoleColor FocusedFg  = ConsoleColor.Black;
    protected ConsoleColor FocusedBg  = ConsoleColor.White;   // inverted = selected look

    public override void OnUpdate(IReadOnlyList<KeyEvent> keys)
    {
        foreach (var key in keys)
        {
            if (key.Key == ConsoleKey.Enter)
                OnClick?.Invoke();
        }
    }

    protected (ConsoleColor fg, ConsoleColor bg) CurrentColors
        => IsFocused ? (FocusedFg, FocusedBg) : (NormalFg, NormalBg);
}
```

**2. `MenuItem`**

```csharp
// MenuItem.cs  (new file, TuiEngine.UI)
public class MenuItem : Selectable
{
    public string Label { get; set; }

    public MenuItem(string label, Action? onClick = null)
    {
        Label   = label;
        OnClick = onClick;
    }

    public override void Draw(Buffer buffer, int x, int y)
    {
        var (fg, bg) = CurrentColors;

        // Fill the full item width with background color so selection highlight is solid
        for (int i = 0; i < Label.Length + 2; i++)
            buffer.Set(x + i, y, ' ', fg, bg);

        // Draw label with a leading space for padding
        for (int i = 0; i < Label.Length; i++)
            buffer.Set(x + 1 + i, y, Label[i], fg, bg);
    }
}
```

### Usage — matches the existing pattern
```csharp
Tui.Run(
    new Label("Main menu"),
    new MenuItem("Start game", () => Console.WriteLine("Starting...")),
    new MenuItem("Options",    () => { }),
    new MenuItem("Quit",       Environment.Exit)
);
```

This already works at this point — Tab moves focus between items, Enter fires the action. The only
thing missing is grouping them into a navigable list automatically, which is Phase 5.

---

## Phase 5 — Menu component

### Goal
A `Menu` component that owns a list of `MenuItem` children, moves selection with arrow keys, and
fires the selected item's `OnClick` with Enter — all as a single reusable component.

### What to build

**1. `Menu`**

```csharp
// Menu.cs  (new file, TuiEngine.UI)
public class Menu : Component
{
    private readonly List<MenuItem> items;
    private int selectedIndex = 0;

    public Menu(params MenuItem[] menuItems)
    {
        items = new List<MenuItem>(menuItems);
        // Register items as children so View.Render traverses them
        foreach (var item in items)
            Add(item);
    }

    public override void OnFocus()
    {
        // When the menu gains focus, highlight the current item
        SyncSelection();
    }

    public override void OnUpdate(IReadOnlyList<KeyEvent> keys)
    {
        foreach (var key in keys)
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
        // Mirror focus state onto items without going through FocusManager
        // (the Menu holds focus; items are visual-only children)
        for (int i = 0; i < items.Count; i++)
            items[i].ForceSelected(i == selectedIndex);
    }
}
```

**2. Add `ForceSelected` to `MenuItem`**

Because the `Menu` owns navigation (not `FocusManager`), items need a way to be marked
"visually selected" without actually holding keyboard focus:

```csharp
// MenuItem.cs — add field
private bool isSelected = false;

public void ForceSelected(bool value)
{
    isSelected = value;
    MarkDirty();
}

public override void Draw(Buffer buffer, int x, int y)
{
    var fg = isSelected ? FocusedFg : NormalFg;
    var bg = isSelected ? FocusedBg : NormalBg;

    for (int i = 0; i < Label.Length + 2; i++)
        buffer.Set(x + i, y, ' ', fg, bg);

    for (int i = 0; i < Label.Length; i++)
        buffer.Set(x + 1 + i, y, Label[i], fg, bg);
}
```

**3. Final usage**

```csharp
// Program.cs
Tui.Run(
    new Label("=== Main Menu ==="),
    new Menu(
        new MenuItem("New game",  () => StartGame()),
        new MenuItem("Load game", () => LoadGame()),
        new MenuItem("Options",   () => OpenOptions()),
        new MenuItem("Quit",      () => Environment.Exit(0))
    )
);
```

Arrow keys move the highlight. Enter fires the action. Tab moves focus to the next focusable
sibling (if there is one). Escape exits the engine.

---

## Summary — build order and files changed

| Phase | New files | Changed files |
|---|---|---|
| 1 — Input routing | — | `Engine.cs`, `Component.cs` |
| 2 — Color | `AnsiColor.cs` | `Cell.cs`, `Buffer.cs`, `Renderer.cs` |
| 3 — Focus | `FocusManager.cs` | `Component.cs`, `Engine.cs` |
| 4 — Selectable + MenuItem | `Selectable.cs`, `MenuItem.cs` | — |
| 5 — Menu | `Menu.cs` | `MenuItem.cs` |

Each phase is independently testable. After Phase 1, key output appears in `NumberCounter`.
After Phase 2, you can pass colors into any `Label`. After Phase 3, Tab cycling works.
After Phase 4, individual `MenuItem` components respond to Enter. After Phase 5, the full menu
is done.