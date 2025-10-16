using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace VibeTasks.Cli.Services
{
    public sealed class PurgeOptions
    {
        public string DataDir { get; init; } = "";
        public bool PurgeJson { get; init; } = true;
        public bool PurgeSqlite { get; init; } = true;
        public bool CreateBackup { get; init; } = false;
        public string? BackupDir { get; init; } // if null, defaults to <DataDir>/backups
    }

    public sealed class PurgeResult
    {
        public bool Success { get; init; }
        public string? BackupPath { get; init; }
        public int DeletedJsonCount { get; init; }
        public bool DeletedSqlite { get; init; }
        public string? Message { get; init; }
    }

    public sealed class DataStorePurger
    {
        public PurgeResult Purge(PurgeOptions options)
        {
            var result = new PurgeResult();
            if (string.IsNullOrWhiteSpace(options.DataDir))
                return Fail("Data directory was not provided.");

            if (!Directory.Exists(options.DataDir))
                return Ok("Data directory does not exist. Nothing to purge.");

            if (!options.PurgeJson && !options.PurgeSqlite)
                return Fail("Nothing selected to purge: both --json-only and --sqlite-only are false.");

            try
            {
                string? backupPath = null;

                if (options.CreateBackup)
                {
                    backupPath = CreateBackupZip(options);
                }

                int jsonDeleted = 0;
                bool sqliteDeleted = false;

                if (options.PurgeJson)
                {
                    var jsonFiles = Directory.GetFiles(options.DataDir, "*.json", SearchOption.TopDirectoryOnly);
                    foreach (var file in jsonFiles)
                    {
                        File.Delete(file);
                        jsonDeleted++;
                    }
                }

                if (options.PurgeSqlite)
                {
                    var sqlitePath = Path.Combine(options.DataDir, "vibetasks-index.sqlite");
                    if (File.Exists(sqlitePath))
                    {
                        File.Delete(sqlitePath);
                        sqliteDeleted = true;
                    }
                }

                return new PurgeResult
                {
                    Success = true,
                    BackupPath = backupPath,
                    DeletedJsonCount = jsonDeleted,
                    DeletedSqlite = sqliteDeleted,
                    Message = "Purge completed."
                };
            }
            catch (Exception ex)
            {
                return Fail($"Error purging data: {ex.Message}");
            }
        }

        private static PurgeResult Fail(string msg) => new PurgeResult { Success = false, Message = msg };
        private static PurgeResult Ok(string msg) => new PurgeResult { Success = true, Message = msg };

        private static string CreateBackupZip(PurgeOptions options)
        {
            var timestamp = DateTimeOffset.Now.ToString("yyyyMMdd-HHmmss");
            var backupDir = options.BackupDir ?? Path.Combine(options.DataDir, "backups");
            Directory.CreateDirectory(backupDir);

            // Only include files we might purge: *.json and vibetasks-index.sqlite
            var tempDir = Path.Combine(backupDir, $".backup-staging-{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDir);

            try
            {
                if (options.PurgeJson)
                {
                    foreach (var f in Directory.GetFiles(options.DataDir, "*.json", SearchOption.TopDirectoryOnly))
                    {
                        File.Copy(f, Path.Combine(tempDir, Path.GetFileName(f)), overwrite: true);
                    }
                }

                if (options.PurgeSqlite)
                {
                    var sqlitePath = Path.Combine(options.DataDir, "vibetasks-index.sqlite");
                    if (File.Exists(sqlitePath))
                    {
                        File.Copy(sqlitePath, Path.Combine(tempDir, "vibetasks-index.sqlite"), overwrite: true);
                    }
                }

                var hasAnything = Directory.EnumerateFiles(tempDir).Any();
                if (!hasAnything)
                {
                    // No relevant files—don’t emit an empty backup
                    return string.Empty;
                }

                var zipPath = Path.Combine(backupDir, $"vibe-tasks-backup-{timestamp}.zip");
                ZipFile.CreateFromDirectory(tempDir, zipPath, CompressionLevel.Optimal, includeBaseDirectory: false);
                return zipPath;
            }
            finally
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { /* ignore */ }
            }
        }
    }
}