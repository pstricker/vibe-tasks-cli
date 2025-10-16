using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using VibeTasks.Cli.Services;
using VibeTasks.Core;

namespace VibeTasks.Cli.Commands
{
    public class PurgeCommand : Command<PurgeCommand.Settings>
    {
        private readonly AppConfig _cfg;
        public PurgeCommand(AppConfig cfg) { _cfg = cfg; }
        public class Settings : CommandSettings
        {
            [Description("Skip the confirmation prompt.")]
            [CommandOption("--yes|-y")]
            public bool Yes { get; set; }

            [Description("Only purge daily JSON files.")]
            [CommandOption("--json-only")]
            public bool JsonOnly { get; set; }

            [Description("Only purge the SQLite index file.")]
            [CommandOption("--sqlite-only")]
            public bool SqliteOnly { get; set; }

            [Description("Create a backup ZIP before deletion (stored in DataDir/backups unless --backup-dir is set).")]
            [CommandOption("--backup")]
            public bool Backup { get; set; }

            [Description("Directory to store the backup ZIP (optional).")]
            [CommandOption("--backup-dir <PATH>")]
            public string? BackupDir { get; set; }
        }

        public override int Execute(CommandContext ctx, Settings s)
        {
            var dataDir = _cfg.DataDir;
            if (!Directory.Exists(dataDir))
            {
                AnsiConsole.MarkupLine($"[yellow]No data directory found at:[/] [italic]{dataDir}[/]. Nothing to purge.");
                return 0;
            }

            if (s.JsonOnly && s.SqliteOnly)
            {
                AnsiConsole.MarkupLine("[red]--json-only and --sqlite-only are mutually exclusive.[/]");
                return -1;
            }

            var willPurgeJson = s.JsonOnly || (!s.JsonOnly && !s.SqliteOnly);   // default includes JSON
            var willPurgeSqlite = s.SqliteOnly || (!s.JsonOnly && !s.SqliteOnly); // default includes SQLite

            // Build a human-friendly summary
            var targetSummary = (willPurgeJson, willPurgeSqlite) switch
            {
                (true, true) => "all task data (daily JSON files and the SQLite index)",
                (true, false) => "all daily JSON files",
                (false, true) => "the SQLite index",
                _ => "nothing"
            };

            if (!s.Yes)
            {
                var prompt = new ConfirmationPrompt(
                    $"[red]This will permanently delete {targetSummary}[/] in [bold]{dataDir}[/].{(s.Backup ? " A backup ZIP will be created first." : "")} Continue?"
                )
                {
                    DefaultValue = false
                };

                if (!AnsiConsole.Prompt(prompt))
                {
                    AnsiConsole.MarkupLine("[gray]Purge cancelled.[/]");
                    return 0;
                }
            }

            var purger = new DataStorePurger();
            var result = purger.Purge(new PurgeOptions
            {
                DataDir = dataDir,
                PurgeJson = willPurgeJson,
                PurgeSqlite = willPurgeSqlite,
                CreateBackup = s.Backup,
                BackupDir = s.BackupDir
            });

            if (!result.Success)
            {
                AnsiConsole.MarkupLine($"[red]{result.Message}[/]");
                return -1;
            }

            if (!string.IsNullOrEmpty(result.BackupPath))
            {
                AnsiConsole.MarkupLine($"[green]Backup created:[/] [italic]{result.BackupPath}[/]");
            }

            if (result.DeletedJsonCount > 0)
                AnsiConsole.MarkupLine($"[green]Deleted JSON files:[/] {result.DeletedJsonCount}");

            if (result.DeletedSqlite)
                AnsiConsole.MarkupLine($"[green]Deleted SQLite index:[/] vibetasks-index.sqlite");

            if (result.DeletedJsonCount == 0 && !result.DeletedSqlite)
                AnsiConsole.MarkupLine("[yellow]No matching files to delete.[/]");

            AnsiConsole.MarkupLine("[bold]Done.[/]");
            return 0;
        }
    }
}