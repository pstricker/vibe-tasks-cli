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

        cfg.AddCommand<AddCommand>("add").WithAlias("a");
        cfg.AddCommand<RemoveCommand>("remove").WithAlias("del");
        cfg.AddCommand<ArchiveCommand>("archive");
        cfg.AddCommand<StatusCommand>("status");
        cfg.AddCommand<NoteCommand>("note");
        cfg.AddCommand<ListCommand>("list").WithAlias("ls");
        cfg.AddCommand<SearchCommand>("search").WithAlias("s");
        cfg.AddCommand<StandupCommand>("standup");
        cfg.AddCommand<EditCommand>("edit");
        cfg.AddCommand<ReopenCommand>("reopen");
        cfg.AddCommand<ConfigCommand>("config");
        cfg.AddCommand<ReindexCommand>("reindex");
        cfg.AddCommand<PurgeCommand>("purge")
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
}
