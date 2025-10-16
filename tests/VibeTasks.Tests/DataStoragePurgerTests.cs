using System;
using System.IO;
using System.IO.Compression;
using VibeTasks.Cli.Services;
using Xunit;

namespace VibeTasks.Tests
{
    public class DataStorePurgerTests : IDisposable
    {
        private readonly string _root;

        public DataStorePurgerTests()
        {
            _root = Path.Combine(Path.GetTempPath(), $"vt-purge-{Guid.NewGuid():N}");
            Directory.CreateDirectory(_root);
        }

        [Fact]
        public void Purge_All_WithBackup_DeletesTargets_AndCreatesZip()
        {
            // Arrange
            var json1 = Path.Combine(_root, "2025-10-10.json");
            var json2 = Path.Combine(_root, "2025-10-11.json");
            File.WriteAllText(json1, "{}");
            File.WriteAllText(json2, "{}");

            var sqlite = Path.Combine(_root, "vibetasks-index.sqlite");
            File.WriteAllText(sqlite, "db");

            var sut = new DataStorePurger();

            // Act
            var res = sut.Purge(new PurgeOptions
            {
                DataDir = _root,
                PurgeJson = true,
                PurgeSqlite = true,
                CreateBackup = true
            });

            // Assert
            Assert.True(res.Success);
            Assert.True(!File.Exists(json1) && !File.Exists(json2));
            Assert.False(File.Exists(sqlite));
            Assert.False(string.IsNullOrEmpty(res.BackupPath));
            Assert.True(File.Exists(res.BackupPath!));

            using var zip = ZipFile.OpenRead(res.BackupPath!);
            // backup should include only files that existed
            Assert.True(zip.Entries.Count >= 1);
        }

        [Fact]
        public void Purge_JsonOnly_DoesNotDeleteSqlite()
        {
            var json = Path.Combine(_root, "2025-10-10.json");
            File.WriteAllText(json, "{}");
            var sqlite = Path.Combine(_root, "vibetasks-index.sqlite");
            File.WriteAllText(sqlite, "db");

            var sut = new DataStorePurger();
            var res = sut.Purge(new PurgeOptions
            {
                DataDir = _root,
                PurgeJson = true,
                PurgeSqlite = false
            });

            Assert.True(res.Success);
            Assert.False(File.Exists(json));
            Assert.True(File.Exists(sqlite));
        }

        [Fact]
        public void Purge_SqliteOnly_DoesNotDeleteJson()
        {
            var json = Path.Combine(_root, "2025-10-10.json");
            File.WriteAllText(json, "{}");
            var sqlite = Path.Combine(_root, "vibetasks-index.sqlite");
            File.WriteAllText(sqlite, "db");

            var sut = new DataStorePurger();
            var res = sut.Purge(new PurgeOptions
            {
                DataDir = _root,
                PurgeJson = false,
                PurgeSqlite = true
            });

            Assert.True(res.Success);
            Assert.True(File.Exists(json));
            Assert.False(File.Exists(sqlite));
        }

        [Fact]
        public void Purge_WithBackupDir_UsesProvidedPath()
        {
            var json = Path.Combine(_root, "2025-10-10.json");
            File.WriteAllText(json, "{}");

            var customBackup = Path.Combine(_root, "bk");
            Directory.CreateDirectory(customBackup);

            var sut = new DataStorePurger();
            var res = sut.Purge(new PurgeOptions
            {
                DataDir = _root,
                PurgeJson = true,
                PurgeSqlite = false,
                CreateBackup = true,
                BackupDir = customBackup
            });

            Assert.True(res.Success);
            Assert.StartsWith(customBackup, res.BackupPath!);
            Assert.True(File.Exists(res.BackupPath!));
        }

        [Fact]
        public void Purge_NoTargets_NoBackupCreated()
        {
            var sut = new DataStorePurger();
            var res = sut.Purge(new PurgeOptions
            {
                DataDir = _root,
                PurgeJson = true,
                PurgeSqlite = true,
                CreateBackup = true
            });

            Assert.True(res.Success);
            Assert.True(string.IsNullOrEmpty(res.BackupPath));
        }

        public void Dispose()
        {
            try { Directory.Delete(_root, true); } catch { /* ignore */ }
        }
    }
}