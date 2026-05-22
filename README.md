# TuiEngine 🖥️

A modern, Unity-inspired Terminal User Interface (TUI) library for .NET that brings declarative UI development to the console.

[![.NET](https://img.shields.io/badge/.NET-6.0%2B-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey)](https://github.com/yourusername/tuiengine)

## 🎯 Vision

TuiEngine aims to bring the intuitive, component-based architecture of Unity's UI Toolkit to terminal applications. Build rich, interactive console interfaces with the same declarative patterns you'd use for modern GUIs.

## ✨ Features

### Current (v0.1)
- ✅ **Simple App Framework** - Get started quickly with `TuiApp` base class
- ✅ **Event-Driven Input** - Clean keyboard event handling
- ✅ **Screen Buffer Management** - Efficient rendering abstraction
- ✅ **Cross-Platform Foundation** - Works on Windows, Linux, and macOS

### Planned
- 🔄 **Component System** - Reusable UI components with hierarchy
- 🔄 **Flexbox Layouts** - Responsive, automatic positioning and sizing
- 🔄 **Mouse Support** - Click, drag, and scroll interactions
- 🔄 **Data Binding** - Automatic UI updates when data changes
- 🔄 **Styling System** - Separate structure from presentation
- 🔄 **Rich Widget Library** - Buttons, inputs, lists, dialogs, and more
- 🔄 **Feature Detection** - Graceful degradation for limited terminals
- 🔄 **Theming** - Color schemes and visual customization
- 🔄 **Animation** - Smooth transitions and effects

## 🚀 Quick Start

### Installation

```bash
# Clone the repository
git clone https://github.com/yourusername/tuiengine.git

# Navigate to the project
cd tuiengine

# Build the library
dotnet build
```

### Your First App

```csharp
using System;
using TuiEngine;

namespace MyTuiApp;

class HelloApp : TuiApp
{
    private int counter = 0;

    protected override void OnKey(ConsoleKey key)
    {
        switch (key)
        {
            case ConsoleKey.Spacebar:
                counter++;
                break;
            case ConsoleKey.R:
                counter = 0;
                break;
        }
    }

    protected override void Render()
    {
        Screen.Clear();
        
        Screen.WriteLine("╔══════════════════════╗");
        Screen.WriteLine("║   Hello TuiEngine!   ║");
        Screen.WriteLine("╠══════════════════════╣");
        Screen.WriteLine($"║ Counter: {counter,-12}║");
        Screen.WriteLine("║                      ║");
        Screen.WriteLine("║ SPACE = +1           ║");
        Screen.WriteLine("║ R = reset            ║");
        Screen.WriteLine("║ ESC = quit           ║");
        Screen.WriteLine("╚══════════════════════╝");
        
        Screen.RenderToConsole();
    }
}

// Run it
new HelloApp().Run();
```

Run with:
```bash
dotnet run
```

## 📚 Documentation

- [Getting Started Guide](docs/getting-started.md) *(Coming Soon)*
- [Architecture Overview](docs/architecture.md) *(Coming Soon)*
- [Component System](docs/components.md) *(Coming Soon)*
- [Layout System](docs/layouts.md) *(Coming Soon)*
- [API Reference](docs/api/README.md) *(Coming Soon)*

## 🏗️ Architecture

TuiEngine is inspired by Unity's UI Toolkit and modern web frameworks, implementing:

### Core Principles

1. **Separation of Concerns** - Structure (components), presentation (styles), and logic (code) are decoupled
2. **Component Hierarchy** - Everything is a `View` that can contain child views
3. **Declarative UI** - Describe what you want, not how to draw it
4. **Event-Driven** - Input flows through an event system with bubbling/capturing
5. **Graceful Degradation** - Features degrade smoothly on limited terminals

### Current Architecture

```
TuiApp (Your Application)
  ├── Input (Keyboard handling)
  ├── Screen (Buffer management)
  └── Render Loop (~60 FPS)
```

### Target Architecture (v1.0)

```
TuiApp
  ├── TerminalDriver (Platform abstraction)
  │   ├── WindowsDriver
  │   ├── CursesDriver
  │   └── BasicDriver (Fallback)
  │
  ├── LayoutEngine (Flexbox-inspired)
  │   ├── FlexLayout
  │   └── AbsoluteLayout
  │
  ├── View Tree
  │   ├── Container
  │   ├── Button
  │   ├── TextInput
  │   ├── ListView
  │   └── Custom Components
  │
  ├── Event System
  │   ├── Keyboard Events
  │   ├── Mouse Events
  │   └── Focus Management
  │
  └── Styling System
      ├── Theme
      └── StyleSheet
```

## 🛣️ Roadmap

See [ROADMAP](ROADMAP.md) for detailed milestones and progress tracking.

### High-Level Timeline

- **Q2 2026** - Phase 1: Component System & Layouts
- **Q3 2026** - Phase 2: Mouse Support & Terminal Drivers
- **Q4 2026** - Phase 3: Advanced Features & Widget Library
- **Q1 2027** - Phase 4: v1.0 Release

## 🤝 Contributing

Contributions are welcome! Please read our [Contributing Guidelines](CONTRIBUTING.md) *(Coming Soon)* before submitting PRs.

### Development Setup

```bash
# Clone the repo
git clone https://github.com/yourusername/tuiengine.git
cd tuiengine

# Install dependencies
dotnet restore

# Run tests
dotnet test

# Run the demo
cd Console-TUI-Example-Project
dotnet run
```

## 📖 Examples

Check out the [examples](examples/) directory for sample applications:

- [Counter App](Console-TUI-Example-Project/) - Basic demonstration (current)
- Text Editor *(Coming Soon)*
- File Browser *(Coming Soon)*
- Dashboard *(Coming Soon)*
- Game UI *(Coming Soon)*

## 🔧 Requirements

- .NET 6.0 or higher
- Supported terminals:
  - Windows Terminal
  - Windows Console (ConHost)
  - iTerm2 (macOS)
  - GNOME Terminal (Linux)
  - Any xterm-compatible terminal

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

TuiEngine is inspired by and learns from:

- **Unity UI Toolkit** - Component-based architecture and Flexbox layouts
- **Terminal.Gui** - C# terminal UI patterns and cross-platform support
- **Spectre.Console** - Advanced rendering techniques
- **React** - Declarative UI and component model
- **Yoga** - Flexbox layout algorithm

## 🌟 Star History

If you find TuiEngine useful, please consider giving it a star! ⭐

## 📬 Contact

- GitHub Issues: [Report bugs or request features](https://github.com/yourusername/tuiengine/issues)
- Discussions: [Ask questions or share ideas](https://github.com/yourusername/tuiengine/discussions)

---

**Built with ❤️ for the terminal**