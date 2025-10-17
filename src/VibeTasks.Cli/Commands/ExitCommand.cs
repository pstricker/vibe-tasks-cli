using Spectre.Console.Cli;

namespace VibeTasks.Cli.Commands;

public class ExitCommand : Command<ExitCommand.Settings>
{
    public class Settings : CommandSettings { }

    public override int Execute(CommandContext context, Settings settings)
    {
        // This command is only meaningful in shell mode, so just return a special code.
        return 9999; // Used to signal shell exit if needed
    }
}
