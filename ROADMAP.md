# TUI Engine — Follow-Up Roadmap

Picks up exactly where the first roadmap left off.
The menu runs. The problems visible in the screenshot are:

- `Console.WriteLine` inside callbacks bleeds into the rendered terminal (text appears mid-screen,
  unstyled, outside the buffer)
- Menu items and their callback output appear on the same line — layout is off
- No Unity UI Toolkit feel — the developer just stacks `new MenuItem(...)` into `new Menu(...)` 
  with no config object, no style binding, no forced contract
- Tab is a global shortcut the developer did not choose — it should not exist
- Arrow keys are hardcoded; the developer has no say in what "move up" means for their game

These four phases fix all of it.

---

## What is wrong in the screenshot (Image 2)

```
=== Main Menu ===

 New game  Starting new game...
 Load game Loading game...
 Options   Opening options...
[Quit]
```

The text `Starting new game...` etc. is coming from `Console.WriteLine` inside the callbacks.
Because the buffer-based renderer does not own the full console write stream, any raw
`Console.Write/WriteLine` call bypasses the buffer entirely and appears wherever the cursor
happens to be. This is not a rendering bug — it is an architectural rule that must be enforced:
**nothing in a TUI app should ever call `Console.Write` directly after the engine starts.**
Phase 6 fixes this by giving the developer a proper output surface.

---

## Phase 6 — Kill `Console.Write` in callbacks; add a StatusBar

### The problem
The engine uses ANSI cursor positioning (`\x1b[y;xH`) to draw into a buffer.
Any raw `Console.WriteLine` call writes at the current physical cursor position,
which is wherever the last ANSI move left it. This corrupts the layout.

### What Image 1 is describing
The sketch shows two regions:

```
┌─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─┐
│                               │
│           UI area             │
│         (main content)        │
│                               │
├───────────────────────────────┤
│  Response / status bar        │
└───────────────────────────────┘
```

The response bar is a dedicated screen region the engine controls.
Callbacks write a message there — not to `Console.WriteLine`.

### What to build

**1. `StatusBar` component**

```csharp
// StatusBar.cs  (new file, TuiEngine.UI)
public class StatusBar : Component
{
    private string message = "";

    public void Post(string msg)
    {
        message = msg;
        MarkDirty();
    }

    public override void Draw(Buffer buffer, int x, int y)
    {
        // Fill the row with the bar background
        for (int i = 0; i < buffer.Width; i++)
            buffer.Set(x + i, y, ' ', ConsoleColor.Black, ConsoleColor.DarkGray);

        // Write the message text
        for (int i = 0; i < Math.Min(message.Length, buffer.Width - 2); i++)
            buffer.Set(x + 1 + i, y, message[i], ConsoleColor.White, ConsoleColor.DarkGray);
    }
}
```

**2. `Tui.Run` pins the StatusBar to the last row**

```csharp
// Tui.cs
public static class Tui
{
    public static StatusBar StatusBar { get; private set; } = new();

    public static void Run(params View[] rootChildren)
    {
        var root = new RootView();
        root.Add(rootChildren);
        root.Add(StatusBar);   // always last — engine pins it to bottom row in RootView

        var engine = new Engine(root);
        engine.Run();
    }
}
```

**3. `RootView` pins the last child to the bottom row**

```csharp
// RootView.cs
public override void Render(Buffer buffer, int x, int y)
{
    buffer.Clear();

    // Render all children except the last normally
    for (int i = 0; i < Children.Count - 1; i++)
        Children[i].Render(buffer, x, y + i);

    // Pin the last child (StatusBar) to the bottom row
    Children[^1].Render(buffer, x, buffer.Height - 1);
}
```

**4. Callbacks now call `Tui.StatusBar.Post(...)` instead of `Console.WriteLine`**

```csharp
// Program.cs
Tui.Run(
    new Label("=== Main Menu ==="),
    new Menu(
        MenuKeys.Arrows(ConsoleKey.Enter),
        new MenuItem("New game",  () => Tui.StatusBar.Post("Starting new game...")),
        new MenuItem("Load game", () => Tui.StatusBar.Post("Loading game...")),
        new MenuItem("Options",   () => Tui.StatusBar.Post("Opening options...")),
        new MenuItem("Quit",      () => Environment.Exit(0))
    )
);
```

### Rule to document for your project
> After `Tui.Run()` is called, `Console.Write` and `Console.WriteLine` are forbidden.
> All output must go through `Buffer`, `StatusBar.Post`, or a dedicated UI component.

---

## Phase 7 — Developer-defined keybinds; remove Tab entirely

### The problem
Tab is a global engine shortcut the developer never asked for. Arrow keys are hardcoded in `Menu`.
A game developer building a VI-style interface wants `hjkl`. A developer targeting a numpad wants
`8` and `2`. They currently have no say.

### What to build

**1. `MenuKeys` — a required config struct**

```csharp
// MenuKeys.cs  (new file, TuiEngine.UI)
public readonly struct MenuKeys
{
    public ConsoleKey Up     { get; }
    public ConsoleKey Down   { get; }
    public ConsoleKey Select { get; }

    public MenuKeys(ConsoleKey up, ConsoleKey down, ConsoleKey select)
    {
        Up     = up;
        Down   = down;
        Select = select;
    }

    // Convenience factory — the most common case
    public static MenuKeys Arrows(ConsoleKey select = ConsoleKey.Enter)
        => new(ConsoleKey.UpArrow, ConsoleKey.DownArrow, select);

    // VI keys
    public static MenuKeys Vi(ConsoleKey select = ConsoleKey.Enter)
        => new(ConsoleKey.K, ConsoleKey.J, select);

    // Numpad
    public static MenuKeys Numpad(ConsoleKey select = ConsoleKey.Enter)
        => new(ConsoleKey.NumPad8, ConsoleKey.NumPad2, select);
}
```

**2. `Menu` constructor requires `MenuKeys` — it cannot be created without them**

```csharp
// Menu.cs — updated constructor signature
public class Menu : Component
{
    private readonly MenuKeys keys;
    private readonly List<MenuItem> items;
    private int selectedIndex = 0;

    // The developer MUST pass keys — no default, no overload without them
    public Menu(MenuKeys keys, params MenuItem[] menuItems)
    {
        this.keys = keys;
        items = new List<MenuItem>(menuItems);
        foreach (var item in items) Add(item);
    }

    public override void OnUpdate(IReadOnlyList<KeyEvent> keyEvents)
    {
        foreach (var e in keyEvents)
        {
            if      (e.Key == keys.Up)     MoveSelection(-1);
            else if (e.Key == keys.Down)   MoveSelection(+1);
            else if (e.Key == keys.Select) items[selectedIndex].OnClick?.Invoke();
        }
        MarkDirty();
    }

    private void MoveSelection(int delta)
    {
        selectedIndex = (selectedIndex + delta + items.Count) % items.Count;
        SyncSelection();
    }
    // ... rest unchanged
}
```

**3. Remove Tab from `FocusManager` and `Engine` completely**

```csharp
// Engine.cs — delete the Tab intercept block entirely
private void Update(List<KeyEvent> keys)
{
    // No more Tab handling here.
    // Focus is driven entirely by directional input (Phase 8).
    UpdateRecursive(root, keys);
}
```

`FocusManager.CycleNext()` can stay on the class for programmatic use, but the engine never
calls it automatically anymore.

### What the developer API looks like now

```csharp
// Arrow keys + Enter  (most common)
new Menu(MenuKeys.Arrows(), ...)

// VI-style
new Menu(MenuKeys.Vi(), ...)

// Custom
new Menu(new MenuKeys(ConsoleKey.W, ConsoleKey.S, ConsoleKey.Spacebar), ...)
```

The compiler enforces the contract. A `Menu` without keybinds does not compile.

---

## Phase 8 — Input-driven focus (enter menu on first arrow press)

### The problem
Right now focus only moves via Tab (which Phase 7 removes) or by explicit `FocusManager` calls.
The desired behavior from your notes:

> "If from the start the first down or up option will react to the first menu — as in you are
> going into the menu instead of using other elements."

This means: **pressing a navigation key that matches any registered menu should automatically
focus that menu**, without the developer or user needing to do anything first.

### Design

When a key event arrives and no component is focused, the engine should ask each registered menu
"is this one of your navigation keys?" — and if so, give that menu focus immediately.

**1. Add `IKeyFocusable` interface**

```csharp
// IKeyFocusable.cs  (new file, TuiEngine.UI)
// A component that can claim focus in response to a specific key
public interface IKeyFocusable
{
    bool ClaimsKey(ConsoleKey key);
}
```

**2. `Menu` implements `IKeyFocusable`**

```csharp
// Menu.cs — add interface
public class Menu : Component, IKeyFocusable
{
    public bool ClaimsKey(ConsoleKey key)
        => key == keys.Up || key == keys.Down;
    // ...
}
```

**3. `Engine` does a focus-claim pass before distributing keys**

```csharp
// Engine.cs
private void Update(List<KeyEvent> keyEvents)
{
    // If nothing is focused and a key arrives, let any IKeyFocusable claim it
    if (FocusManager.Focused == null && keyEvents.Count > 0)
    {
        foreach (var key in keyEvents)
        {
            var claimer = FindKeyFocusable(root, key.Key);
            if (claimer != null)
            {
                FocusManager.SetFocus(claimer);
                break;   // first claimant wins
            }
        }
    }

    UpdateRecursive(root, keyEvents);
}

private Component? FindKeyFocusable(View view, ConsoleKey key)
{
    if (view is IKeyFocusable kf && view is Component c && kf.ClaimsKey(key))
        return c;

    foreach (var child in view.Children)
    {
        var result = FindKeyFocusable(child, key);
        if (result != null) return result;
    }
    return null;
}
```

**4. Add `FocusManager.SetFocus`**

```csharp
// FocusManager.cs
public static void SetFocus(Component c)
{
    Focused?.OnBlur();
    // Find and set by reference
    index = focusable.IndexOf(c);
    if (index < 0) { Register(c); index = focusable.Count - 1; }
    Focused?.OnFocus();
}
```

### What this feels like in practice

```
App starts — no component has focus.

User presses DownArrow.
→ Engine finds Menu (first IKeyFocusable that claims DownArrow).
→ FocusManager.SetFocus(menu).
→ menu.OnFocus() fires → selectedIndex = 0, first item highlights.
→ That same DownArrow is then passed to OnUpdate → moves to item 1.

User presses UpArrow.
→ Menu already has focus → moves selection up.

User presses Enter.
→ Fires selected item's OnClick.
```

No Tab. No explicit focus call. The first directional key just works.

---

## Phase 9 — Unity UI Toolkit-style declarative API

### The problem
The current call pattern:

```csharp
Tui.Run(
    new Label("=== Main Menu ==="),
    new Menu(
        MenuKeys.Arrows(),
        new MenuItem("New game", () => ...),
        new MenuItem("Quit",     () => ...)
    )
);
```

This is close, but in Unity UI Toolkit, a visual element carries a **style config** as a separate
object (USS), and data is bound to the element declaratively — you do not pass data through the
constructor. The goal is to separate **what it looks like** from **what it does** from
**what data it shows**.

### What to build

**1. `MenuStyle` — visual config separated from behavior**

```csharp
// MenuStyle.cs  (new file, TuiEngine.UI)
public class MenuStyle
{
    public ConsoleColor NormalFg    { get; init; } = ConsoleColor.Gray;
    public ConsoleColor NormalBg    { get; init; } = ConsoleColor.Black;
    public ConsoleColor SelectedFg  { get; init; } = ConsoleColor.Black;
    public ConsoleColor SelectedBg  { get; init; } = ConsoleColor.White;
    public string       Prefix      { get; init; } = " ";   // shown before each item
    public string       SelectedPrefix { get; init; } = "►"; // shown before selected item

    public static readonly MenuStyle Default = new();
}
```

**2. `Menu` accepts an optional `MenuStyle`**

```csharp
public Menu(MenuKeys keys, MenuStyle? style = null, params MenuItem[] menuItems)
{
    this.keys  = keys;
    this.style = style ?? MenuStyle.Default;
    // ...
}
```

**3. `MenuItem` uses `MenuItemConfig` — data-binding style**

Instead of passing label + callback directly, the developer defines a config object and the
component reads from it. This mirrors how Unity's `Label.bindingPath` works.

```csharp
// MenuItemConfig.cs  (new file, TuiEngine.UI)
public class MenuItemConfig
{
    public string Label   { get; set; } = "";
    public Action? OnClick { get; set; }
}

// MenuItem.cs — bind to a config
public class MenuItem : Selectable
{
    private readonly MenuItemConfig config;

    public MenuItem(MenuItemConfig config)
    {
        this.config = config;
        OnClick = () => config.OnClick?.Invoke();
    }

    // Convenience — lets you still write new MenuItem("Quit", () => ...)
    public MenuItem(string label, Action? onClick = null)
        : this(new MenuItemConfig { Label = label, OnClick = onClick }) { }

    public string Label => config.Label;
}
```

Now a developer can hold a reference to the config and mutate it at runtime — the component
automatically reflects the change on the next draw (because `MarkDirty` is called when the
buffer is written):

```csharp
var saveConfig = new MenuItemConfig { Label = "Continue", OnClick = ContinueGame };

Tui.Run(
    new Menu(
        MenuKeys.Arrows(),
        MenuStyle.Default,
        new MenuItem(saveConfig),
        new MenuItem("New game",  () => Tui.StatusBar.Post("New game")),
        new MenuItem("Quit",      () => Environment.Exit(0))
    )
);

// Later — mutate the config; the menu re-draws automatically
saveConfig.Label   = "Continue (slot 3)";
saveConfig.OnClick = () => LoadSlot(3);
```

This is the Unity data-binding pattern: **the config is the model, the component is the view**.

**4. Full usage showing all four phases together**

```csharp
// Program.cs
var saveSlot = new MenuItemConfig { Label = "Continue", OnClick = () => Load() };

Tui.Run(
    new Label("=== Main Menu ==="),
    new Menu(
        keys:  MenuKeys.Arrows(ConsoleKey.Enter),
        style: new MenuStyle
        {
            SelectedFg     = ConsoleColor.Yellow,
            SelectedBg     = ConsoleColor.DarkBlue,
            SelectedPrefix = "► "
        },
        new MenuItem(saveSlot),
        new MenuItem("New game",  () => Tui.StatusBar.Post("Starting...")),
        new MenuItem("Options",   () => Tui.StatusBar.Post("Options...")),
        new MenuItem("Quit",      () => Environment.Exit(0))
    )
);
```

---

## Summary — what each phase gives you

| Phase | Removes | Adds | Key file |
|---|---|---|---|
| 6 — StatusBar | `Console.WriteLine` in callbacks | `StatusBar`, pinned bottom row in `RootView` | `StatusBar.cs` |
| 7 — MenuKeys | Global Tab, hardcoded arrow keys | `MenuKeys` struct, required constructor param | `MenuKeys.cs` |
| 8 — Input focus | Manual focus bootstrap | `IKeyFocusable`, auto-claim on first arrow press | `IKeyFocusable.cs` |
| 9 — Declarative API | Constructor-heavy style | `MenuStyle`, `MenuItemConfig` data binding | `MenuStyle.cs`, `MenuItemConfig.cs` |

### New files added across both roadmaps

```
TuiEngine/
  Core/
    Engine.cs          (modified every phase)
    Time.cs
    Tui.cs             (Phase 6: StatusBar property)
  Input/
    InputHandler.cs
    KeyEvent.cs
  Rendering/
    AnsiColor.cs       (Roadmap 1 Phase 2)
    Buffer.cs
    Cell.cs
    Renderer.cs
    Terminal.cs
  UI/
    Component.cs
    FocusManager.cs    (Roadmap 1 Phase 3)
    IKeyFocusable.cs   ← Phase 8
    Label.cs
    Menu.cs            ← Phase 5 + updated each phase
    MenuItemConfig.cs  ← Phase 9
    MenuKeys.cs        ← Phase 7
    MenuItem.cs        ← Phase 4 + updated
    MenuStyle.cs       ← Phase 9
    NumberCounter.cs
    RootView.cs        (Phase 6: pin bottom row)
    Selectable.cs      (Roadmap 1 Phase 4)
    StatusBar.cs       ← Phase 6
    View.cs
```