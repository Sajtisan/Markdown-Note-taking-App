# 📝 Cross-Platform Markdown Studio

[![Framework](https://img.shields.io/badge/Framework-Avalonia-blue)](https://avaloniaui.net/)
[![Language](https://img.shields.io/badge/Language-C%23_14-purple)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Runtime](https://img.shields.io/badge/.NET-10.0-512bd4)](https://dotnet.microsoft.com/download/dotnet/10.0)

A Markdown editor built with a hybrid local/cloud architecture and GPU-accelerated aesthetics.

---

## 🛠 Tech Stack

| Layer | Technology |
| :--- | :--- |
| **Frontend** | **Avalonia UI** (Cross-platform XAML framework) |
| **Runtime** | **.NET 10** (Utilizing the latest C# features) |
| **Graphics** | **SkiaSharp** (Custom SKSL Shaders for background FX) |
| **Server DB** | **PostgreSQL** (Containerized via Docker) |
| **Local DB** | **SQLite** (Offline-first data persistence via EF Core) |
| **Parsing** | **Markdown.Avalonia** (Real-time XAML rendering) |

---

## 🚀 Key Features

* **Hybrid Sync:** Architected with an offline-first approach. Uses SQLite for zero-latency local edits and automatically reconciles with a PostgreSQL cloud instance using .NET 10's optimized networking stack.
* **GPU Visuals:** Implements low-level SKSL (Skia Shading Language) to render 60 FPS animated backgrounds directly on the GPU, ensuring zero CPU overhead for typing.
* **Reactive Workspace:** A fluid, responsive UI that automatically collapses sidebars into "Distraction-Free Mode" when notes are selected.
* **Modern Auth:** JWT-based Bearer authentication for secure cloud communication.



---

## 🗺 Roadmap

### ⚙️ Core Enhancements
- [ ] **Adaptive Settings:** Fully functional theme switcher (Dark, Dracula, Light).
- [ ] **GPU Toggle:** Add a "Battery Saver" mode to disable background shaders.
- [ ] **UI/UX Polish:** Refine corner radii, frosted glass opacity, and depth shadows.
- [ ] **Conflict Resolution:** Implement smart-merging for multi-device synchronization using .NET 10's high-performance JSON logic.

### 🌐 Expansion
- [ ] **Mobile Support:** Port the existing logic to Android and iOS using Avalonia's mobile backends.
- [ ] **Notes Hub:** A community workshop for sharing, discovering, and forking public notes.
- [ ] **AI Integration:** Local LLM integration for markdown summarization and formatting.

---

## 🔧 Installation & Setup

## 🔧 Installation & Setup

### 1. Prerequisites
* **.NET 10 SDK** (Verify via `dotnet --version`)
* **Docker Desktop** (For hosting the PostgreSQL instance)
* **Visual Studio 2022 / VS Code** with C# Dev Kit

### 2. Infrastructure Setup (PostgreSQL)
The application uses a containerized PostgreSQL database to ensure a consistent environment across different machines.

```bash
# Navigate to the project root
cd Markdown-Note-taking-App/server
```

# Start the database container
```bash
docker-compose up -d
```

*This spins up a persistent PostgreSQL instance on `localhost:5432`. Ensure Docker Desktop is running before executing.*

### 3. Backend API Setup
The API acts as the secure bridge between your local SQLite data and the PostgreSQL cloud.

1.  **Navigate to the Server Project:**
    ```bash
    cd server/MarkdownNotesClient.Api
    ```
2.  **Apply Database Migrations:** Ensure your connection string in `appsettings.json` points to your Docker container, then run:
    ```bash
    dotnet ef database update
    ```
3.  **Start the Service:**
    ```bash
    dotnet run
    ```
    *The API service will now be active at `http://localhost:5080`.*

### 4. Client Application Setup
1.  **Navigate to the Client Directory:**
    ```bash
    cd client/MarkdownNotesClient
    ```
2.  **Restore and Launch:**
    ```bash
    dotnet restore
    dotnet run
    ```

---

## 🗺 Roadmap

### ⚙️ Core Enhancements
- [ ] **Adaptive Settings:** Integrated theme switcher (Dark, Dracula, Light) and GPU-toggle to conserve battery.
- [ ] **Conflict Resolution:** Smart-merging for notes edited on multiple devices simultaneously.
- [ ] **UI Refinement:** Further polishing of frosted glass effects, corner radii, and depth shadows.

### 🌐 Expansion & AI
- [ ] **AI Quiz Generation:** Automatically generate Multiple Choice or Active Recall quizzes from your Markdown notes using LLM context processing.
- [ ] **Mobile & Web:** Leverage Avalonia's cross-platform nature to port the app to Android, iOS, and WebAssembly.
- [ ] **Community Hub:** A workshop for users to share and discover public notes and templates.

### 🗺️ To-do / Reminder

- [ ] **Settings Enhancements**
  - [ ] Implement functional theme switcher (Dark, Dracula, Light)
  - [ ] Add toggle for background shader effects (Battery Saver mode)
  - [ ] General settings configuration persistence
- [ ] **UI/UX Refinement**
  - [ ] Adjust color palettes for better contrast
  - [ ] Fine-tune corner radii for a modern look
  - [ ] Enhance visibility of container boxes and borders
- [ ] **Platform Expansion**
  - [ ] Optimize for mobile (Android/iOS)
  - [ ] WebAssembly (WASM) support
- [ ] **Community Features**
  - [ ] **Note Hub:** Create a central gallery for users to share and discover note templates (similar to Steam Workshop)