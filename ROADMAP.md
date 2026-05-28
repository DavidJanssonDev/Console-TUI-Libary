# TUI Engine — Menu Roadmap (v2)

A step-by-step build plan from the current render loop to a fully navigable,
keyboard-driven menu in the Unity UI Toolkit style — with explicit keybinds,
no universal Tab navigation, and a two-zone layout (UI area + status bar).

---

## What changed from v1

| Topic | v1 | v2 |
|---|---|---|
| Focus cycling | Universal `Tab` key | Removed. No Tab anywhere. |
| Navigation keys | Hardcoded `UpArrow / DownArrow / Enter` inside `Menu` | Developer **must** pass a `MenuKeys` struct — compile error if omitted |
| Entering a menu | Required Tab to reach the Menu first | First `Up` or `Down` press auto-focuses the nearest Menu |
| Layout | Single flat list of children | Two-zone layout: content area + status bar (from the sketch) |
| Callback side-effects | `Console.WriteLine` bleeds into the render buffer | Callbacks are deferred and routed to the status bar instead |

---

## Current state (baseline)

| Area | What exists | What is missing |
|---|---|---|
| Loop | `Engine.Run()` 16 ms sleep | `Time.DeltaTime` never set; only Escape handled |
| Input | `InputHandler`, `KeyEvent` | Neither connected to `Engine` |
| Rendering | `Buffer`, `Cell`, `Renderer.DiffRender` | `Cell` has no color; no ANSI color codes emitted |
| UI | `View`, `Component`, `Label`, `NumberCounter` | No focus, no selection, no interactive components |
| Layout | `FlexLayout` data class | Layout math hardcoded as `offsetY += 1` in `View.Render` |

---

## Phase 1 — Wire input into the engine loop

### Goal
The engine collects every key pressed this frame into a list and passes it down
the component tree. No component polls the console directly.

### Problem in the current code
```csharp
// Engine.Run() today — reads ONE key and throws it away
if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
    break;
```
`InputHandler` and `KeyEvent` exist but are never used. `Time.DeltaTime` is
declared but never written.

### What to build

**1. Engine collects all keys per frame and sets delta time**

```csharp
// Engine.cs
private DateTime lastTick = DateTime.UtcNow;

public void Run()
{
    Console.CursorVisible = false;

    while (true)
    {
        var now = DateTime.UtcNow;
        Time.DeltaTime = (float)(now - lastTick).TotalSeconds;
        lastTick = now;

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

**2. Pass key list down the update tree**

```csharp
private void Update(List<KeyEvent> keys) => UpdateRecursive(root, keys);

private void UpdateRecursive(View view, IReadOnlyList<KeyEvent> keys)
{
    if (view is Component c)
        c.OnUpdate(keys);

    foreach (var child in view.Children)
        UpdateRecursive(child, keys);
}
```

**3. Update `Component.OnUpdate` signature**

```csharp
// Component.cs
public virtual void OnUpdate(IReadOnlyList<KeyEvent> keys) { }
```

`Label` and `NumberCounter` ignore the parameter — no behavior change.

---

## Phase 2 — Add color to Cell, Buffer, and Renderer

### Goal
`Cell` currently holds only a `char`. A selected menu item needs inverted
colors, so color support must exist at the lowest layer before any interactive
component can use it.

### What to build

**1. `Cell` gets Foreground and Background**

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
        Char = c; Foreground = fg; Background = bg;
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

**2. `Buffer.Set` gains color overload**

```csharp
// Buffer.cs
public void Set(int x, int y, char c,
                ConsoleColor fg = ConsoleColor.Gray,
                ConsoleColor bg = ConsoleColor.Black)
{
    if (x < 0 || y < 0 || x >= Width || y >= Height) return;
    Cells[x, y] = new Cell(c, fg, bg);
}
```

`Buffer.Clear()` must fill with `Cell.Empty` (not `new Cell(' ')`), so the
diff compares color changes on blank cells too.

**3. `Renderer` emits ANSI color codes**

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
        Cell n = back.Cells[x, y];
        Cell o = front.Cells[x, y];
        if (n.Equals(o)) continue;

        sb.Append($"\x1b[{y + 1};{x + 1}H");

        if (n.Foreground != lastFg) { sb.Append(AnsiColor.Fg(n.Foreground)); lastFg = n.Foreground; }
        if (n.Background != lastBg) { sb.Append(AnsiColor.Bg(n.Background)); lastBg = n.Background; }

        sb.Append(n.Char);
        front.Cells[x, y] = n;
    }

    sb.Append("\x1b[0m"); // reset color — stops the shell prompt inheriting colors
    Console.Write(sb.ToString());
}
```

**4. `AnsiColor` helper**

```csharp
// AnsiColor.cs  (new, TuiEngine.Rendering)
internal static class AnsiColor
{
    private static readonly int[] FgMap = { 30,34,32,36,31,35,33,37,90,94,92,96,91,95,93,97 };
    private static readonly int[] BgMap = { 40,44,42,46,41,45,43,47,100,104,102,106,101,105,103,107 };

    public static string Fg(ConsoleColor c) => $"\x1b[{FgMap[(int)c]}m";
    public static string Bg(ConsoleColor c) => $"\x1b[{BgMap[(int)c]}m";
}
```

### Why the diff must compare color
`Cell.Equals` now checks `Char + Foreground + Background`. Moving the cursor
highlight from item A to item B changes only color on those two cells — the
diff catches it even though the characters are the same letters.

---

## Phase 3 — Focus system with auto-focus (no Tab)

### Goal
Only the focused component receives keys. There is no global Tab key.
Instead: if nothing is focused and the player presses a navigation key
(`Up` or `Down`), the engine automatically focuses the first `Menu` in the tree.
This means the developer never has to think about "entering" the menu — the
first keypress does it.

### Why no Tab
Tab is a web/desktop convention. In a TUI game or tool, you want the **game
developer** to define what navigation means. Tab as a universal escape hatch
breaks that contract and causes surprising behavior when the menu has its own
up/down navigation.

### What to build

**1. `FocusManager` — no Tab, exposes explicit `SetFocus`**

```csharp
// FocusManager.cs  (new, TuiEngine.UI)
public static class FocusManager
{
    private static readonly List<Component> registered = new();
    public static Component? Focused { get; private set; }

    public static void Register(Component c)
    {
        if (!registered.Contains(c)) registered.Add(c);
    }

    // Explicit focus — called by Engine or by components themselves
    public static void SetFocus(Component c)
    {
        Focused?.OnBlur();
        Focused = c;
        Focused.OnFocus();
    }

    // Find the first component of type T in registration order
    public static T? FirstOfType<T>() where T : Component
        => registered.OfType<T>().FirstOrDefault();
}
```

**2. `Component` gets focus lifecycle**

```csharp
// Component.cs
public bool IsFocused => FocusManager.Focused == this;

public virtual void OnFocus() { }
public virtual void OnBlur()  { }

public override void OnMount()
{
    FocusManager.Register(this);
}
```

**3. Engine auto-focuses the first Menu on navigation key**

```csharp
// Engine.cs — Update() replaces the previous Tab intercept
private void Update(List<KeyEvent> keys)
{
    // Auto-focus: if nothing is focused and a navigation key arrives,
    // give focus to the first Menu in the tree automatically.
    if (FocusManager.Focused == null && keys.Count > 0)
    {
        var firstMenu = FocusManager.FirstOfType<Menu>();
        if (firstMenu != null)
            FocusManager.SetFocus(firstMenu);
    }

    UpdateRecursive(root, keys);
}

private void UpdateRecursive(View view, IReadOnlyList<KeyEvent> keys)
{
    if (view is Component c)
    {
        // Non-focused components receive an empty key list
        c.OnUpdate(c.IsFocused ? keys : Array.Empty<KeyEvent>());
    }

    foreach (var child in view.Children)
        UpdateRecursive(child, keys);
}
```

**4. Mount all components on startup**

```csharp
// Engine constructor
public Engine(View root)
{
    this.root = root;
    // ... buffer init
    MountRecursive(root); // registers every Component with FocusManager
}

private void MountRecursive(View view)
{
    if (view is Component c) c.OnMount();
    foreach (var child in view.Children)
        MountRecursive(child);
}
```

On the first frame with no focus, the first `Up` or `Down` press triggers
auto-focus on the `Menu`. After that, the `Menu` handles its own navigation.

---

## Phase 4 — MenuKeys struct + Selectable

### Goal
The developer is **forced** to declare what keys drive a menu. There is no
default. If you forget `MenuKeys`, the code does not compile. This matches how
Unity forces you to assign an `InputAction` asset — the engine will not guess.

### What to build

**1. `MenuKeys` — required, no defaults**

```csharp
// MenuKeys.cs  (new, TuiEngine.UI)
public readonly struct MenuKeys
{
    public ConsoleKey Up     { get; init; }
    public ConsoleKey Down   { get; init; }
    public ConsoleKey Select { get; init; }

    // Constructor — all three fields required at the call site
    public MenuKeys(ConsoleKey up, ConsoleKey down, ConsoleKey select)
    {
        Up     = up;
        Down   = down;
        Select = select;
    }

    // Convenience preset — opt-in, not the default path
    public static MenuKeys ArrowKeys
        => new(ConsoleKey.UpArrow, ConsoleKey.DownArrow, ConsoleKey.Enter);
}
```

Usage in code:
```csharp
// Explicit — developer picks the keys
new Menu(new MenuKeys(ConsoleKey.W, ConsoleKey.S, ConsoleKey.Enter), ...)

// Or use the preset when arrows make sense
new Menu(MenuKeys.ArrowKeys, ...)
```

**2. `Selectable` — base for interactive components**

```csharp
// Selectable.cs  (new, TuiEngine.UI)
public abstract class Selectable : Component
{
    public Action? OnClick { get; set; }

    protected ConsoleColor NormalFg  = ConsoleColor.Gray;
    protected ConsoleColor NormalBg  = ConsoleColor.Black;
    protected ConsoleColor SelectedFg = ConsoleColor.Black;
    protected ConsoleColor SelectedBg = ConsoleColor.White;
}
```

`Selectable` does not handle input itself — the `Menu` that contains it does.
This way a `MenuItem` never needs to know what keys are in play.

**3. `MenuItem`**

```csharp
// MenuItem.cs  (new, TuiEngine.UI)
public class MenuItem : Selectable
{
    public string Label { get; }
    private bool isSelected;

    public MenuItem(string label, Action? onClick = null)
    {
        Label   = label;
        OnClick = onClick;
    }

    // Called by Menu — not by FocusManager
    public void SetSelected(bool value)
    {
        if (isSelected == value) return;
        isSelected = value;
        MarkDirty();
    }

    public override void Draw(Buffer buffer, int x, int y)
    {
        var fg = isSelected ? SelectedFg : NormalFg;
        var bg = isSelected ? SelectedBg : NormalBg;

        // Solid background fill for the whole item width
        for (int i = 0; i < Label.Length + 2; i++)
            buffer.Set(x + i, y, ' ', fg, bg);

        // Label with 1-char left padding
        for (int i = 0; i < Label.Length; i++)
            buffer.Set(x + 1 + i, y, Label[i], fg, bg);
    }
}
```

---

## Phase 5 — Menu component

### Goal
`Menu` owns navigation. It receives `MenuKeys` at construction (compile error
if omitted), moves the selection with the declared keys, and fires `OnClick`
on the selected item.

### What to build

**1. `Menu`**

```csharp
// Menu.cs  (new, TuiEngine.UI)
public class Menu : Component
{
    private readonly List<MenuItem> items;
    private readonly MenuKeys keys;           // <-- required at construction
    private int selectedIndex = 0;

    // MenuKeys is the first argument — impossible to forget
    public Menu(MenuKeys keys, params MenuItem[] menuItems)
    {
        this.keys  = keys;
        this.items = new List<MenuItem>(menuItems);

        foreach (var item in items)
            Add(item); // items are children so View.Render traverses them
    }

    public override void OnFocus()
    {
        // Highlight item 0 the moment the menu receives focus
        SyncSelection();
    }

    public override void OnBlur()
    {
        // Clear all highlights when focus leaves
        foreach (var item in items)
            item.SetSelected(false);
    }

    public override void OnUpdate(IReadOnlyList<KeyEvent> frameKeys)
    {
        foreach (var key in frameKeys)
        {
            if      (key.Key == keys.Up)     MoveSelection(-1);
            else if (key.Key == keys.Down)   MoveSelection(+1);
            else if (key.Key == keys.Select) items[selectedIndex].OnClick?.Invoke();
        }
    }

    public override void Draw(Buffer buffer, int x, int y)
    {
        // Menu itself is invisible — only its MenuItem children draw
    }

    private void MoveSelection(int delta)
    {
        selectedIndex = (selectedIndex + delta + items.Count) % items.Count;
        SyncSelection();
        MarkDirty();
    }

    private void SyncSelection()
    {
        for (int i = 0; i < items.Count; i++)
            items[i].SetSelected(i == selectedIndex);
    }
}
```

**2. Final usage**

```csharp
// Program.cs
Tui.Run(
    new Label("=== Main Menu ==="),
    new Menu(
        MenuKeys.ArrowKeys,                          // explicit — or pass custom keys
        new MenuItem("New game",  () => StartGame()),
        new MenuItem("Load game", () => LoadGame()),
        new MenuItem("Options",   () => OpenOptions()),
        new MenuItem("Quit",      () => Environment.Exit(0))
    )
);
```

First arrow key press → engine auto-focuses `Menu` → `OnFocus` highlights
item 0 → `Up`/`Down` move the selection → `Enter` fires the action.
No Tab required anywhere.

---

## Phase 6 — Two-zone layout (UI area + status bar)

### Goal
Match the sketch: a main UI zone at the top and a single-line status bar at
the bottom that shows the result of the last action — without callback output
bleeding into the render buffer via `Console.WriteLine`.

### The problem with raw Console.WriteLine in callbacks
When a `MenuItem` callback calls `Console.WriteLine`, it writes directly to
stdout during the engine loop. The next `DiffRender` call overwrites that
line with whatever the buffer says should be there — or worse, the text
appears mid-frame and is then overwritten incorrectly. The fix is to never
write to the console from a callback. Instead, callbacks post a message to a
`StatusBar` component that owns that region of the buffer.

### What to build

**1. `StatusBar` component**

```csharp
// StatusBar.cs  (new, TuiEngine.UI)
public class StatusBar : Component
{
    private string message = string.Empty;

    // Static so callbacks can post to it without a reference chain
    public static StatusBar? Instance { get; private set; }

    public StatusBar()
    {
        Instance = this;
    }

    public void Post(string msg)
    {
        message = msg;
        MarkDirty();
    }

    public override void Draw(Buffer buffer, int x, int y)
    {
        // Clear the row first
        for (int i = 0; i < buffer.Width; i++)
            buffer.Set(x + i, y, ' ', ConsoleColor.Black, ConsoleColor.DarkGray);

        // Write message
        for (int i = 0; i < Math.Min(message.Length, buffer.Width - x); i++)
            buffer.Set(x + i, y, message[i], ConsoleColor.White, ConsoleColor.DarkGray);
    }
}
```

**2. Callbacks post to StatusBar instead of writing to console**

```csharp
// Program.cs — callbacks use StatusBar.Instance, not Console.WriteLine
Tui.Run(
    new Label("=== Main Menu ==="),
    new Menu(
        MenuKeys.ArrowKeys,
        new MenuItem("New game",  () => StatusBar.Instance?.Post("Starting new game...")),
        new MenuItem("Load game", () => StatusBar.Instance?.Post("Loading game...")),
        new MenuItem("Options",   () => StatusBar.Instance?.Post("Opening options...")),
        new MenuItem("Quit",      () => Environment.Exit(0))
    ),
    new StatusBar()
);
```

**3. Two-zone layout in `View.Render`**

Right now all children stack with `offsetY += 1`. For the status bar to sit
at the bottom, `View.Render` needs to know the terminal height. The minimal
change: give `RootView` two named slots — content and footer:

```csharp
// RootView.cs
internal class RootView : View
{
    public View Content { get; }    // renders top-down from y = 0
    public StatusBar? Footer { get; set; }  // renders at Terminal.Height - 1

    public RootView(View content)
    {
        Content = content;
        Add(content);
    }

    public override void Render(Buffer buffer, int x, int y)
    {
        buffer.Clear();
        Content.Render(buffer, x, 0);

        if (Footer != null)
            Footer.Render(buffer, x, buffer.Height - 1);
    }
}
```

`Tui.Run` separates the last `StatusBar` child out into `RootView.Footer`
automatically:

```csharp
// Tui.cs
public static void Run(params View[] rootChildren)
{
    var content = new View(); // anonymous grouping view
    StatusBar? footer = null;

    foreach (var child in rootChildren)
    {
        if (child is StatusBar sb) footer = sb;
        else content.Add(child);
    }

    var root = new RootView(content);
    root.Footer = footer;

    new Engine(root).Run();
}
```

---

## Summary — build order and files changed

| Phase | New files | Changed files |
|---|---|---|
| 1 — Input routing | — | `Engine.cs`, `Component.cs` |
| 2 — Color | `AnsiColor.cs` | `Cell.cs`, `Buffer.cs`, `Renderer.cs` |
| 3 — Focus (no Tab) | `FocusManager.cs` | `Component.cs`, `Engine.cs` |
| 4 — MenuKeys + Selectable | `MenuKeys.cs`, `Selectable.cs`, `MenuItem.cs` | — |
| 5 — Menu | `Menu.cs` | — |
| 6 — Layout + StatusBar | `StatusBar.cs` | `RootView.cs`, `Tui.cs` |

### Test at each phase
- After Phase 1: `NumberCounter` now increments correctly with real delta time.
- After Phase 2: Pass a color to any `Label` — it renders with that color.
- After Phase 3: First `Down` press auto-focuses the `Menu`. No Tab needed.
- After Phase 4: `new Menu(MenuKeys.ArrowKeys, ...)` compiles; omitting `MenuKeys` does not.
- After Phase 5: Full arrow key navigation + Enter fires actions.
- After Phase 6: Action results appear in the status bar at the bottom; no raw `Console.WriteLine` in the tree.