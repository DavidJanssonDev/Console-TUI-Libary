using TuiEngine.Core;
using TuiEngine.UI.EngineComponents.Abstract;
using TuiEngine.UI.EngineComponents.Normal;

// Program.cs
Tui.Run(
    new Label("=== Main Menu ==="),
    new Menu(
        MenuKeys.Arrows(ConsoleKey.Enter),
        new MenuItem("New game", () => Tui.StatusBar.Post("Starting new game...")),
        new MenuItem("Load game", () => Tui.StatusBar.Post("Loading game...")),
        new MenuItem("Options", () => Tui.StatusBar.Post("Opening options...")),
        new MenuItem("Quit", () => Environment.Exit(0))
    )
);

