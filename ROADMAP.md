# TuiEngine Roadmap 🛣️

This is my personal development plan for TuiEngine — a Unity-inspired terminal UI framework for .NET.

It’s not a company roadmap, just how I personally want to grow the project step by step so I can actually build it without overwhelming myself.

---

## 🎯 What I’m Trying to Build

I want TuiEngine to feel like building UI in Unity or React, but for the terminal.

Something where I can:
- create components instead of manually drawing text
- build UI trees instead of printing strings
- eventually have layout, styling, and interaction like a real UI framework

Right now it’s just a foundation, but the goal is a full terminal UI system that feels modern and structured.

---

## 🧭 Current Status (v0.1)

This is what already exists:

- TuiApp base class
- Keyboard input handling
- Screen buffer system
- Basic render loop (~60 FPS)
- Cross-platform console support foundation

📦 **v0.1.0 is basically done — this is my starting point**

---

## 🚀 My Personal Milestones

Each version below is something I can realistically build, finish, and tag as a release.

---

## 📦 v0.2 — Make UI feel like “components”

**Goal:** stop thinking in raw console drawing and start thinking in UI objects

What I want to add:
- a base `View` class
- parent/child UI structure (tree of elements)
- simple lifecycle (create, update, render)
- basic internal state per component

**End result:**
I should be able to build a UI from nested components instead of writing everything in Render()

---

## 📐 v0.3 — Layout system (so I stop positioning everything manually)

**Goal:** let the system handle positioning

What I want:
- row / column layout system
- alignment rules (left, center, right)
- padding + spacing
- simple flex-like behavior

**End result:**
UI elements should arrange themselves automatically instead of me calculating positions

---

## 🖱️ v0.4 — Make it interactive

**Goal:** move beyond keyboard polling into real UI interaction

What I want:
- mouse support (click, scroll, hover)
- focus system (what element is active)
- event bubbling (like DOM / Unity UI)
- proper input routing per component

**End result:**
I can click or interact with UI elements like buttons and lists

---

## 🧩 v0.5 — Basic widget library

**Goal:** stop rebuilding the same UI elements every time

What I want:
- Button
- TextInput
- ListView
- Container / Panel
- simple dialogs

**End result:**
I can build small apps quickly without rewriting UI primitives

---

## 🎨 v0.6 — Styling (separate look from logic)

**Goal:** stop hardcoding colors and formatting everywhere

What I want:
- theme system
- reusable styles
- inheritance (basic)
- maybe a simple CSS-like idea later

**End result:**
I can change how the UI looks without touching logic

---

## 🔗 v0.7 — Data binding (make UI react to data)

**Goal:** stop manually calling refresh all the time

What I want:
- reactive state
- automatic UI updates when data changes
- bindings between UI and variables

**End result:**
Changing a variable automatically updates the UI

---

## ✨ v0.8 — Animations (make it feel alive)

**Goal:** make transitions smoother and less static

What I want:
- simple animations (fade, move, resize)
- frame-based animation system
- non-blocking updates

**End result:**
UI doesn’t feel like static text anymore

---

## 🏁 v1.0 — First “real” version

**Goal:** something I’m actually proud to use for projects

What I want:
- stable component system
- layout + input + styling working together
- decent widget set
- cleaned up architecture
- proper documentation
- example apps

**End result:**
A usable terminal UI framework I can build real apps with

---

## 🧪 After v1.0 (just ideas for later)

- plugin system
- multi-window terminal UI
- remote rendering (over network)
- visual UI builder (maybe one day)

---

## 📊 How I’m thinking about releases

Each version should:

- actually run and be usable
- include at least one small demo app
- not break everything from previous versions if possible
- be tagged so I can look back at progress

---

**Last updated:** May 2026

