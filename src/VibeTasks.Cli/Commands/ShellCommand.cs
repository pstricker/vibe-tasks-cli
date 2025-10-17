using Spectre.Console;
using Spectre.Console.Cli;
using VibeTasks.Core;

namespace VibeTasks.Cli.Commands;

public class ShellCommand : Command<ShellCommand.Settings>
{
    public class Settings : CommandSettings { }

    public override int Execute(CommandContext context, Settings settings)
    {
        AnsiConsole.MarkupLine("[green]Entering shell mode. Type commands or 'exit' to quit.[/]");
        var historyFile = GetHistoryFilePath();
        var history = LoadHistory(historyFile, 25);
        int historyIndex = history.Count;
        while (true)
        {
            string input = ReadLineWithHistory(history, ref historyIndex);
            if (string.IsNullOrWhiteSpace(input)) continue;
            if (input.Trim().ToLower() == "exit") break;
            // Only add to history if not a duplicate of previous
            if (history.Count == 0 || history[history.Count - 1] != input)
            {
                history.Add(input);
            }
            historyIndex = history.Count;
            try
            {
                // Split input into args, respecting quoted strings
                var args = ParseArguments(input);
                var app = new CommandApp(new TypeRegistrar());
                app.Configure(cfg => VibeTasks.Program.ConfigureCommands(cfg));
                app.Run(args);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
            }
        }
        SaveHistory(historyFile, history);
        AnsiConsole.MarkupLine("[red]Exiting shell mode.[/]");
        return 0;

        // Get history file path (in AppConfig.DataDir)
        static string GetHistoryFilePath()
        {
            var config = AppConfig.Load();
            var dir = config.DataDir ?? AppConfig.GetDefaultDataDir();
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "shell-history.txt");
        }

        // Load last N lines from history file
        static List<string> LoadHistory(string filePath, int maxLines)
        {
            if (!File.Exists(filePath)) return new List<string>();
            var lines = File.ReadAllLines(filePath);
            return lines.Reverse().Take(maxLines).Reverse().ToList();
        }

        // Save all history to file
        static void SaveHistory(string filePath, List<string> history)
        {
            try
            {
                File.WriteAllLines(filePath, history);
            }
            catch { /* ignore errors */ }
        }

        // Custom input loop supporting history navigation
        static string ReadLineWithHistory(List<string> history, ref int historyIndex)
        {
            var buffer = new System.Text.StringBuilder();
            int cursor = 0;
            int origCursorLeft = Console.CursorLeft;
            int origCursorTop = Console.CursorTop;
            string prompt = "> ";
            Console.Write(prompt);
            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (cursor > 0)
                    {
                        buffer.Remove(cursor - 1, 1);
                        cursor--;
                        RedrawInput(prompt, buffer.ToString(), cursor);
                    }
                }
                else if (key.Key == ConsoleKey.LeftArrow)
                {
                    if (cursor > 0)
                    {
                        cursor--;
                        Console.SetCursorPosition(origCursorLeft + prompt.Length + cursor, origCursorTop);
                    }
                }
                else if (key.Key == ConsoleKey.RightArrow)
                {
                    if (cursor < buffer.Length)
                    {
                        cursor++;
                        Console.SetCursorPosition(origCursorLeft + prompt.Length + cursor, origCursorTop);
                    }
                }
                else if (key.Key == ConsoleKey.UpArrow)
                {
                    if (history.Count > 0 && historyIndex > 0)
                    {
                        historyIndex--;
                        buffer.Clear();
                        buffer.Append(history[historyIndex]);
                        cursor = buffer.Length;
                        RedrawInput(prompt, buffer.ToString(), cursor);
                    }
                }
                else if (key.Key == ConsoleKey.DownArrow)
                {
                    if (history.Count > 0 && historyIndex < history.Count - 1)
                    {
                        historyIndex++;
                        buffer.Clear();
                        buffer.Append(history[historyIndex]);
                        cursor = buffer.Length;
                        RedrawInput(prompt, buffer.ToString(), cursor);
                    }
                    else if (historyIndex == history.Count - 1)
                    {
                        historyIndex++;
                        buffer.Clear();
                        cursor = 0;
                        RedrawInput(prompt, buffer.ToString(), cursor);
                    }
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    buffer.Insert(cursor, key.KeyChar);
                    cursor++;
                    RedrawInput(prompt, buffer.ToString(), cursor);
                }
            }
            return buffer.ToString();
        }

        static void RedrawInput(string prompt, string input, int cursor)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.BufferWidth));
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(prompt + input);
            Console.SetCursorPosition(prompt.Length + cursor, Console.CursorTop);
        }

        // Local function to parse arguments with quotes
        static string[] ParseArguments(string commandLine)
        {
            var args = new List<string>();
            var current = new System.Text.StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < commandLine.Length; i++)
            {
                char c = commandLine[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }
                if (char.IsWhiteSpace(c) && !inQuotes)
                {
                    if (current.Length > 0)
                    {
                        args.Add(current.ToString());
                        current.Clear();
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
            if (current.Length > 0)
                args.Add(current.ToString());
            return args.ToArray();
        }
    }
}
