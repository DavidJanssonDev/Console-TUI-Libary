using TuiEngine.Core;
using TuiEngine.UI;
using TuiEngine.UI.EngineComponents.Normal;

Tui.Run(
    new Label("Main menu"),
    new MenuItem("Start game", () => Console.WriteLine("Starting...")),
    new MenuItem("Options", () => { }),
    new MenuItem("Quit", () => Environment.Exit(0))
);
