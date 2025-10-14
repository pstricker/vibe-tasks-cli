using Spectre.Console;
using Spectre.Console.Cli;
using VibeTasks.Core;
using VibeTasks.Commands;

namespace VibeTasks;

public static class Program
{
    public static int Main(string[] args)
    {
        var config = AppConfig.Load();
        Directory.CreateDirectory(config.DataDir);

        var store = new DataStore(config);
        var roller = new RollForwardService(store);
        roller.PerformIfNeeded();

        var app = new CommandApp(new TypeRegistrar());
        app.Configure(cfg =>
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
        });

        return app.Run(args);
    }
}
