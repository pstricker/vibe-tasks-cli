# VibeTasks — Daily Task CLI

A fast, text-first .NET 8 CLI to track your day-to-day tasks, grouped by day in **one JSON file per day**. Supports quick add, status changes (`todo|inprogress|blocked|skipped|complete`), notes (inline or `$EDITOR`), tags, search (substring / regex / fuzzy), **auto roll-forward** of open tasks, and a **plain-text standup** generator.

## Install

```bash
# Restore & build
dotnet build

# Or publish single-file binaries (adjust RID)
dotnet publish -c Release -r osx-arm64 --self-contained true /p:PublishSingleFile=true
```

## NuGet packages

- Spectre.Console & Spectre.Console.Cli — rich console & CLI framework
- FuzzySharp — fuzzy search
- TextCopy — clipboard for standup (`--clipboard`)

## Usage

The executable name is `task` (via `dotnet run --project src/VibeTasks.Cli` or after publishing).

### Add & basic flow

```bash
task add "Investigate cache stampede" -t perf,infra --note "Repro in staging"
task status 9xn2 inprogress
task note 9xn2 --append "Checked connection pool"
task list --open
task rm 9xn2           # remove from *today* only
task archive 9xn2      # stop rolling forward
```

### Standup

```
Yesterday:
- Task 1
- Task 2
Today:
- Task 3
- Task 4
```

Run:
```bash
task standup --date 2025-10-13 --clipboard
```

### Search

```bash
task search "cache" -t perf --from 2025-10-01 --to 2025-10-31
task search "stampeed" --fuzzy
task search "cache.*stampede" --regex
```

### Config

Config lives at `~/.vibe-tasks/config.json`

```bash
task config                  # show full JSON
task config --get dataDir
task config --set dataDir=/path/to/dir
task config --set gitAutoCommit=false
```

### Data format

`~/.vibe-tasks/YYYY-MM-DD.json`

```json
{
  "date": "2025-10-12",
  "timezone": "Pacific/Honolulu",
  "tasks": [ { "id": "9xn2", "description": "…", "tags": ["perf"], "status": "inprogress", "note": "", "history": [] } ]
}
```

### Roll-forward rules

- Triggered on first command of the day if today’s file doesn’t exist.
- Carry tasks from the most recent prior day where `status != complete` and `archived == false`.
- **Fallback:** if none match, include tasks **edited yesterday** (not archived), even if complete.
- Preserves stable IDs and appends a `rollforward` history event.

### Git auto-commit

When enabled (`gitAutoCommit: true`), any write will run `git init`, `git add -A`, `git commit -m "…"`. Failures are ignored.

## Statuses

`todo | inprogress | blocked | skipped | complete`

## Tests

```bash
dotnet test
```

## Publish quickstart

```bash
# macOS Apple Silicon
dotnet publish src/VibeTasks.Cli -c Release -r osx-arm64 --self-contained true /p:PublishSingleFile=true
# Linux x64
dotnet publish src/VibeTasks.Cli -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true
# Windows x64
dotnet publish src/VibeTasks.Cli -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

## Roadmap ideas

- Move tasks between days explicitly (`task move <id> --to YYYY-MM-DD`)
- Export/import NDJSON
- Richer filtering & grouping in `list`
- Optional SQLite index for faster cross-day searching
