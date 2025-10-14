# 🧭 Vibe Tasks CLI

[![Build](https://github.com/pstricker/vibe-tasks-cli/actions/workflows/dotnet.yml/badge.svg)](https://github.com/pstricker/vibe-tasks-cli/actions/workflows/dotnet.yml)
![License](https://img.shields.io/github/license/pstricker/vibe-tasks-cli)
![.NET](https://img.shields.io/badge/.NET-8.0-blue)

**Vibe Tasks CLI** is a fast, developer-friendly tool for tracking your daily tasks directly from the command line.  
Each day’s work is stored in its own JSON file, incomplete tasks automatically roll forward, and you can instantly generate plain-text standup summaries.  
It’s built for engineers who want a seamless, keyboard-first workflow without switching tools.

---

## ✨ Features

- ✅ Add, update, remove, or archive tasks quickly  
- 🌀 Automatic roll-forward of incomplete tasks  
- 🏷️ Tag tasks with normalized lowercase tags  
- 🧩 Task statuses: `todo`, `inprogress`, `complete`, `blocked`, `skipped`  
- 📝 Inline or `--edit` multiline note editing  
- 🔍 Search by substring, regex, fuzzy match, or tag  
- 🧠 SQLite full-text index for cross-day search  
- 🧾 “Yesterday / Today” standup summaries  
- 💾 Optional Git auto-commit on every change  
- 🎨 Colorized console output and `--json` for scripting  

---

## 🚀 Installation

### 1. Clone and build

```bash
git clone https://github.com/pstricker/vibe-tasks-cli.git
cd vibe-tasks-cli
dotnet build
```

### 2. Publish as a self-contained executable

```bash
dotnet publish src/VibeTasks.Cli -c Release -r osx-arm64 --self-contained true /p:PublishSingleFile=true
```

If you’re on Intel-based macOS, use:
```bash
dotnet publish src/VibeTasks.Cli -c Release -r osx-x64 --self-contained true /p:PublishSingleFile=true
```

For Windows or Linux:
```bash
dotnet publish src/VibeTasks.Cli -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
dotnet publish src/VibeTasks.Cli -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true
```

The resulting binary will be located in:
```
src/VibeTasks.Cli/bin/Release/net8.0/<runtime>/publish/task
```

Make it executable and move it to your PATH (macOS/Linux):
```bash
chmod +x src/VibeTasks.Cli/bin/Release/net8.0/osx-arm64/publish/task
sudo mv src/VibeTasks.Cli/bin/Release/net8.0/osx-arm64/publish/task /usr/local/bin/task
```

Now you can run it from anywhere:
```bash
task --help
```

---

## 🧭 Add to PATH manually (optional)

If you prefer not to move it, add this line to your `~/.zshrc` or `~/.bash_profile`:

```bash
export PATH="$PATH:/Users/philstricker/Projects/vibe-tasks-cli/src/VibeTasks.Cli/bin/Release/net8.0/osx-arm64/publish"
```

Apply changes:
```bash
source ~/.zshrc
```

Confirm it’s available:
```bash
which task
```

---

## 🧠 Usage Examples

### Add a task
```bash
task add "Investigate cache stampede" -t perf,infra --note "Repro in staging"
```

### Update task status
```bash
task status 1 inprogress
task status 1 complete
```

### Edit or append notes
```bash
task note 1 --append "Checked connection pool"
task note 1 --edit
```

### List tasks
```bash
task list              # today
task list --open       # only incomplete tasks
task list --json       # machine-readable
```

### Search tasks
```bash
task search "cache"          # substring
task search "cache" --regex  # regex
task search "cache" --fuzzy  # fuzzy
task search -t perf          # by tag
```

### Generate standup summary
```bash
task standup
```

Example output:
```
Yesterday:
- Investigate cache stampede
Today:
- Improve connection pool retry
```

### Rebuild SQLite index
```bash
task reindex
```

---

## ⚙️ Configuration

Settings are stored in:
```
~/.vibe-tasks/config.json
```

Show configuration:
```bash
task config --list
```

Change configuration:
```bash
task config --set UseSqliteIndex=true
task config --set GitAutoCommit=false
task config --set DataDir="/Users/philstricker/Projects/vibe-tasks-data"
```

---

## 📁 Data Layout

Default directory:
```
~/.vibe-tasks/
```

Typical contents:
```
├── 2025-10-12.json
├── 2025-10-13.json
├── vibetasks-index.sqlite
└── config.json
```

Each JSON file represents a single day’s work. Incomplete tasks automatically roll forward.  
The SQLite index is used for fast global search.

To customize the location:
```bash
task config --set DataDir="/path/to/tasks"
```

---

## 🧪 Development

Build and test:
```bash
dotnet restore
dotnet build
dotnet test
```

Run interactively:
```bash
dotnet run --project src/VibeTasks.Cli -- --help
```

---

## 🔧 Continuous Integration

Add this GitHub Actions workflow at `.github/workflows/dotnet.yml`:

```yaml
name: .NET Build & Test

on: [push, pull_request]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 8.0.x
      - name: Restore
        run: dotnet restore
      - name: Build
        run: dotnet build --configuration Release --no-restore
      - name: Test
        run: dotnet test --no-build --verbosity normal
```

This ensures all pushes and pull requests are automatically built and tested.

---

## 🧰 Tech Stack

- [.NET 8](https://dotnet.microsoft.com/) — Runtime and SDK  
- [Spectre.Console.Cli](https://spectreconsole.net/) — CLI framework  
- [Microsoft.Data.Sqlite](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/) — Full-text search index  
- [System.Text.Json](https://learn.microsoft.com/en-us/dotnet/api/system.text.json) — Storage and serialization  
- [xUnit](https://xunit.net/) — Unit testing  

---

## 🧾 License

MIT © [Phil Stricker](https://github.com/pstricker)

---

## ✅ Repository Setup Checklist

- [x] Add this `README.md`  
- [x] Add an MIT `LICENSE` file  
- [x] Enable GitHub Actions (see workflow above)  
- [x] Add build & license badges (already included)  
- [x] Tag your first release  
  ```bash
  git tag -a v1.0.0 -m "Initial release"
  git push origin v1.0.0
  ```
- [x] (Optional) Add Homebrew tap for `brew install pstricker/vibe-tasks-cli`

---

## 🏁 Summary

| Property | Description |
|-----------|-------------|
| **Project** | Vibe Tasks CLI |
| **Purpose** | Daily task tracking & standup automation |
| **Storage** | One JSON file per day, auto roll-forward |
| **Indexing** | SQLite full-text search |
| **Key Commands** | `add`, `status`, `note`, `list`, `search`, `standup`, `reindex`, `config` |
| **Platform** | Cross-platform (.NET 8) |
| **License** | MIT |
| **Author** | [Phil Stricker](https://github.com/pstricker) |

---

> 🧩 **Vibe Tasks CLI** — a simple, scriptable, and lightning-fast daily workflow tracker for developers who live in the terminal.