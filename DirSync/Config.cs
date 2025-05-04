using System.Text.Json.Serialization;

namespace DirSync
{
    /// <summary>
    /// Represents the configuration settings for the DirSync application.
    /// </summary>
    public class Config
    {
        private Config() { }

        public required string SourceDirectoryPath { get; set; }
        public required string ReplicaDirectoryPath { get; set; }
        public required string LogsDirectory { get; set; }
        public int? SyncIntervalSeconds { get; set; }
        public bool? ArchiveEnabled { get; set; }

        public static Config GetDefaultConfig() => new Config
        {
            SourceDirectoryPath = string.Empty,
            ReplicaDirectoryPath = string.Empty,
            LogsDirectory = string.Empty,
            SyncIntervalSeconds = 9000,
            ArchiveEnabled = true
        };

        public static Config GetEmptyConfig() => new Config()
        {
            SourceDirectoryPath = string.Empty,
            ReplicaDirectoryPath = string.Empty,
            LogsDirectory = string.Empty
        };

        public static Config Clone(Config config)
        {
            return new Config
            {
                SourceDirectoryPath = config.SourceDirectoryPath,
                ReplicaDirectoryPath = config.ReplicaDirectoryPath,
                LogsDirectory = config.LogsDirectory,
                SyncIntervalSeconds = config.SyncIntervalSeconds,
                ArchiveEnabled = config.ArchiveEnabled
            };
        }

        [JsonConstructor]
        private Config(
        string sourceDirectoryPath,
        string replicaDirectoryPath,
        string logsDirectory,
        int? syncIntervalSeconds,
        bool? archiveEnabled)
        {
            SourceDirectoryPath = sourceDirectoryPath;
            ReplicaDirectoryPath = replicaDirectoryPath;
            LogsDirectory = logsDirectory;
            SyncIntervalSeconds = syncIntervalSeconds;
            ArchiveEnabled = archiveEnabled;
        }
    }
}
