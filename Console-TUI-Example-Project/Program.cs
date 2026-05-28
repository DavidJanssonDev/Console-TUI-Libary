using TuiEngine.Core;
using TuiEngine.UI;
using TuiEngine.UI.EngineComponents.Normal;

// Program.cs
Tui.Run(
    new Label("=== Main Menu ==="),
    new Menu(
        new MenuItem("New game", () => StartGame()),
        new MenuItem("Load game", () => LoadGame()),
        new MenuItem("Options", () => OpenOptions()),
        new MenuItem("Quit", () => Environment.Exit(0))
    )
);

void OpenOptions()
{
    // Placeholder for options menu
    Console.WriteLine("Opening options...");
}

void LoadGame()
{
    // Placeholder for loading a game
    Console.WriteLine("Loading game...");
}

void StartGame()
{
    // Placeholder for starting a new game
    Console.WriteLine("Starting new game...");
}