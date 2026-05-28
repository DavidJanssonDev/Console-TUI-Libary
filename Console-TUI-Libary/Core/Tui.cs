using TuiEngine.Rendering;
using System;
using System.Collections.Generic;
using System.Text;
using TuiEngine.UI;
using TuiEngine.UI.EngineComponents.Normal;

namespace TuiEngine.Core;

public static class Tui
{
    public static StatusBar StatusBar { get; private set; } = new StatusBar();
    public static void Run(params View[] rootChildren)
    {
        var root = new RootView();
        root.Add(rootChildren);
        root.Add(StatusBar);

        var engine = new Engine(root);
        engine.Run();
    }
}