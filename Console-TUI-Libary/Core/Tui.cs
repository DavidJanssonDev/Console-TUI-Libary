using TuiEngine.Rendering;
using System;
using System.Collections.Generic;
using System.Text;
using TuiEngine.UI;
using Buffer = TuiEngine.Rendering.Buffer;

namespace TuiEngine.Core;

public static class Tui
{
    public static void Run(params View[] rootChildren)
    {
        var root = new RootView();
        root.Add(rootChildren);

        var engine = new Engine(root);
        engine.Run();

    }
}