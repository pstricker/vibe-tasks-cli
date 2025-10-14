using System.Security.Cryptography;
using System.Text;

namespace VibeTasks.Core;

public static class Utils
{
    public static string TodayIso(AppConfig cfg) => DateTime.Now.ToString("yyyy-MM-dd");
    public static string IsoDate(DateTime dt) => dt.ToString("yyyy-MM-dd");

    public static string ShortId(int length = 4)
    {
        Span<byte> buffer = stackalloc byte[length];
        RandomNumberGenerator.Fill(buffer);
        const string alphabet = "0123456789abcdefghijklmnopqrstuvwxyz";
        var sb = new StringBuilder(length);
        foreach (var b in buffer)
            sb.Append(alphabet[b % alphabet.Length]);
        return sb.ToString();
    }

    public static string[] NormalizeTags(IEnumerable<string> tags)
        => tags.Select(t => t.Trim().ToLowerInvariant()).Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToArray();

    public static string GetEditor()
    {
        var editor = Environment.GetEnvironmentVariable("EDITOR");
        if (!string.IsNullOrWhiteSpace(editor)) return editor;
        if (OperatingSystem.IsWindows()) return "notepad";
        return "nano";
    }

    public static void OpenEditorForFile(string path)
    {
        var editor = GetEditor();
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = editor,
            ArgumentList = { path },
            UseShellExecute = false
        };
        var p = System.Diagnostics.Process.Start(psi);
        p?.WaitForExit();
    }
}
