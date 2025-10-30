using Spectre.Console.Cli;
using VibeTasks.Cli.Commands;
using VibeTasks.Cli.Services;
using VibeTasks.Commands;
using VibeTasks.Core;

namespace VibeTasks;

public static class Program
{
    public static void ConfigureCommands(IConfigurator cfg)
    {
        cfg.SetApplicationName("task");
        cfg.PropagateExceptions();

        cfg.AddCommand<AddCommand>("add")
           .WithAlias("a")
           .WithDescription("Add a new task.");
        cfg.AddCommand<RemoveCommand>("remove")
           .WithAlias("del")
           .WithDescription("Remove an existing task.");
        cfg.AddCommand<ArchiveCommand>("archive")
           .WithDescription("Archive completed or old tasks.");
        cfg.AddCommand<StatusCommand>("status")
           .WithAlias("st")
           .WithDescription("Show the status of a task.");
        cfg.AddCommand<NoteCommand>("note")
           .WithAlias("n")
           .WithDescription("Add or view notes for a task.");
        cfg.AddCommand<ListCommand>("list")
           .WithAlias("ls")
           .WithDescription("List all tasks.");
        cfg.AddCommand<SearchCommand>("search")
           .WithAlias("s")
           .WithDescription("Search for tasks by keyword.");
        cfg.AddCommand<StandupCommand>("standup")
           .WithAlias("su")
           .WithDescription("Show standup summary of tasks.");
        cfg.AddCommand<EditCommand>("edit")
           .WithAlias("e")
           .WithDescription("Edit an existing task.");
        cfg.AddCommand<ReopenCommand>("reopen")
           .WithAlias("ro")
           .WithDescription("Reopen an archived or completed task.");
        cfg.AddCommand<ConfigCommand>("config")
           .WithAlias("c")
           .WithDescription("Configure application settings.");
        cfg.AddCommand<ReindexCommand>("reindex")
           .WithAlias("ri")
           .WithDescription("Reindex the task database.");
        cfg.AddCommand<PurgeCommand>("purge").WithAlias("p")
           .WithDescription("Delete local task data (JSON and/or SQLite) with optional backup and confirmation.");
        cfg.AddCommand<VibeTasks.Cli.Commands.ShellCommand>("shell")
           .WithDescription("Start interactive shell mode.");
        cfg.AddCommand<VibeTasks.Cli.Commands.ExitCommand>("exit")
           .WithDescription("Exit shell mode.");
    }

    public static int Main(string[] args)
    {
        var config = AppConfig.Load();
        Directory.CreateDirectory(config.DataDir);

        var store = new DataStore(config);
        var roller = new RollForwardService(store);
        roller.PerformIfNeeded();

        // Shell mode: if first arg is 'shell', run shell loop
        if (args.Length > 0 && args[0].Equals("shell", StringComparison.OrdinalIgnoreCase))
        {
            ShowAsciiArt();
            var app = new CommandApp(new TypeRegistrar());
            app.Configure(cfg => ConfigureCommands(cfg));
            return app.Run(args); // ShellCommand handles the loop
        }
        else
        {
            var app = new CommandApp(new TypeRegistrar());
            app.Configure(cfg => ConfigureCommands(cfg));
            return app.Run(args);
        }
    }

    private static void ShowAsciiArt()
    {
        var art = @"
   _    ___ __       ______           __       
  | |  / (_) /_  ___/_  __/___ ______/ /_______
  | | / / / __ \/ _ \/ / / __ `/ ___/ //_/ ___/
  | |/ / / /_/ /  __/ / / /_/ (__  ) ,< (__  ) 
  |___/_/_.___/\___/_/  \__,_/____/_/|_/____/

";

        Spectre.Console.AnsiConsole.Write(new Spectre.Console.Markup($"[green]{art}[/]\n"));
    }
}
