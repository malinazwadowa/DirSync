using System.Text;
using System.Text.Json;

namespace DirSync
{
    /// <summary>
    /// Manages configuration of the application and ensures its validity. Saves config to JSON file.
    /// </summary>
    internal class ConfigManager
    {
        private readonly string _configPath = Path.Combine(AppContext.BaseDirectory, "config.json");
        public Config CurrentConfig { get; private set; } = null!;

        public void Initialize(string[] args, bool interactiveMode)
        {
            InitializeConfig();

            if (interactiveMode)
            {
                ConfigureInteractive();
            }
            else
            {
                ConfigureNonInteractive(args);
                Console.WriteLine("Current configuration:");
                PrintConfig(CurrentConfig);

                if (PromptUtils.PromptForConfirmation("Do you want to proceed with this config?"))
                {
                    SaveConfig(CurrentConfig);
                }
                else
                {
                    Environment.Exit(0);
                }
            }
        }

        private Config InitializeConfig()
        {
            Config config = null;

            if (File.Exists(_configPath))
            {
                try
                {
                    string json = File.ReadAllText(_configPath);
                    config = JsonSerializer.Deserialize<Config>(json);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to deserialize config file: {e.Message}");
                }

                if (config != null && ValidateConfig(config,false))
                {
                    CurrentConfig = config;
                    return config;
                }
                else
                {
                    config = Config.GetDefaultConfig();
                    CurrentConfig = config;

                    return config;
                }
            }
            else
            {      
                config = Config.GetDefaultConfig();
                CurrentConfig = config;

                return config;
            }
        }

        public void ConfigureInteractive()
        {
            if (ValidateConfig(CurrentConfig, false))
            {
                Console.WriteLine("Do you want to continue with current settings?");
                Console.WriteLine();
                Console.WriteLine("==============================================");
                Console.WriteLine($"Source directory: {CurrentConfig.SourceDirectoryPath}");
                Console.WriteLine($"Replica directory: {CurrentConfig.ReplicaDirectoryPath}");
                Console.WriteLine($"Logs directory: {CurrentConfig.LogsDirectory}");
                Console.WriteLine($"Synchronization interval in seconds: {CurrentConfig.SyncIntervalSeconds}");
                Console.WriteLine($"Archiving enabled: {CurrentConfig.ArchiveEnabled}");
                Console.WriteLine("==============================================");
                Console.WriteLine();

                if (!PromptUtils.PromptForConfirmation(""))
                {
                    Config newConfig = PromptUtils.PromptForConfig();

                    if (ValidateConfig(newConfig))
                    {
                        SaveConfig(newConfig);
                        CurrentConfig = newConfig;
                    }
                }
            }
            else
            {
                Config newConfig = PromptUtils.PromptForConfig();

                if (ValidateConfig(newConfig))
                {
                    SaveConfig(newConfig);
                    CurrentConfig = newConfig;
                }
            }
        }

        private void ConfigureNonInteractive(string[] args)
        {
            Config newConfig = ParseArgumentsToConfig(args);
            Config resultingConfig = Config.Clone(CurrentConfig);

            MergeConfigInPlace(resultingConfig, newConfig);

            if (ValidateConfig(resultingConfig, true))
            {
                CurrentConfig = resultingConfig;
            }
            else
            {
                Console.WriteLine("===================================================");
                Console.WriteLine("Failed to create config. Exiting the application.");
                Environment.Exit(1);
            }
        }

        static bool ValidateConfig(Config config, bool withMessaging = true)
        {
            if (config == null)
            {
                if (withMessaging)
                    Console.WriteLine("Config is null.");
                return false;
            }

            var message = new StringBuilder();

            if (string.IsNullOrEmpty(config.SourceDirectoryPath))
                message.AppendLine(">Source directory path is not provided/empty.");
            else if (!Directory.Exists(config.SourceDirectoryPath))
                message.AppendLine($">Source path does not exist: {config.SourceDirectoryPath}");

            if (string.IsNullOrEmpty(config.ReplicaDirectoryPath))
                message.AppendLine(">Replica directory path is not provided/empty.");
            else if (!Directory.Exists(config.ReplicaDirectoryPath))
                message.AppendLine($">Replica path does not exist: {config.ReplicaDirectoryPath}");

            if (string.IsNullOrEmpty(config.LogsDirectory))
                message.AppendLine(">Logs directory path is not provided/empty.");
            else if (!Directory.Exists(config.LogsDirectory))
                message.AppendLine($">Logs directory path does not exist: {config.LogsDirectory}");

            if (config.SyncIntervalSeconds == null || config.SyncIntervalSeconds <= 0)
                message.AppendLine($">Invalid or missing SyncIntervalSeconds.{config.SyncIntervalSeconds}");

            if (config.ArchiveEnabled == null)
                message.AppendLine(">ArchiveEnabled is null.");
            
            if(message.Length == 0)
            {
                bool pathsValid = ValidatePaths(config.SourceDirectoryPath, config.ReplicaDirectoryPath, config.LogsDirectory, out StringBuilder pathMessages);
                if (!pathsValid)
                {
                    message.Append(pathMessages);
                }
            }

            if (message.Length > 0)
            {
                if (withMessaging)
                    Console.WriteLine(message.ToString());
                return false;
            }

            return true;
        }

        static bool ValidatePaths(string originPath, string targetPath, string logsPath, out StringBuilder message)
        {
            message = new StringBuilder();

            //Checking path uniquness
            if (string.Equals(originPath, targetPath, StringComparison.OrdinalIgnoreCase))
            {
                message.AppendLine(">Origin and Target directories cannot be the same.");
            }
            if (string.Equals(originPath, logsPath, StringComparison.OrdinalIgnoreCase))
            {
                message.AppendLine(">Logs directory cannot be the same as Origin directory.");
            }
            if (string.Equals(targetPath, logsPath, StringComparison.OrdinalIgnoreCase))
            {
                message.AppendLine(">Logs directory cannot be the same as Target directory.");
            }

            //Ensuring paths are not inside each other
            if (IsSubPathOf(originPath, targetPath) || IsSubPathOf(targetPath, originPath))
            {
                message.AppendLine(">Target directory cannot be inside the Origin directory or vice versa.");
            }
            if (IsSubPathOf(originPath, logsPath))
            {
                message.AppendLine(">Logs directory cannot be inside the Origin directory.");
            }
            if (IsSubPathOf(targetPath, logsPath))
            {
                message.AppendLine(">Logs directory cannot be inside the Target directory.");
            }

            if (message.Length > 0)
            {
                return false;
            }

            return true;
        }

        static bool IsSubPathOf(string outerPath, string innerPath)
        {
            try
            {
                var outerFullPath = Path.GetFullPath(outerPath);
                var innerFullPath = Path.GetFullPath(innerPath);
                return innerFullPath.StartsWith(outerFullPath, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private bool SaveConfig(Config config)
        {
            try
            {
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configPath, json);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to save new config file: {e.Message}");
                Console.WriteLine("Exiting the application...");
                Environment.Exit(1);
                return false;
            }
        }

        public void MergeConfigInPlace(Config baseConfig, Config overrideConfig)
        {
            if (!string.IsNullOrEmpty(overrideConfig.SourceDirectoryPath))
                baseConfig.SourceDirectoryPath = overrideConfig.SourceDirectoryPath;

            if (!string.IsNullOrEmpty(overrideConfig.ReplicaDirectoryPath))
                baseConfig.ReplicaDirectoryPath = overrideConfig.ReplicaDirectoryPath;

            if (!string.IsNullOrEmpty(overrideConfig.LogsDirectory))
                baseConfig.LogsDirectory = overrideConfig.LogsDirectory;

            if (overrideConfig.SyncIntervalSeconds != null)
                baseConfig.SyncIntervalSeconds = overrideConfig.SyncIntervalSeconds;

            if (overrideConfig.ArchiveEnabled != null)
                baseConfig.ArchiveEnabled = overrideConfig.ArchiveEnabled;
        }

        private Config ParseArgumentsToConfig(string[] args)
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>();
            Config config = Config.GetEmptyConfig();

            foreach (var arg in args)
            {
                var parts = arg.Split(new[] { '=' }, 2);
                if (parts.Length == 2)
                {
                    arguments[parts[0].ToLower()] = parts[1];
                }
                else
                {
                    Console.WriteLine($">>>Invalid argument format: {arg}. Correct format is --key=value");
                    Environment.Exit(1);
                }
            }

            if (arguments.TryGetValue("--source", out var source))
            {
                if (!Directory.Exists(source))
                {
                    Console.WriteLine(">>Provided source directory path is invalid.");
                }
                config.SourceDirectoryPath = Path.GetFullPath(source);
            }

            if (arguments.TryGetValue("--replica", out var replicaPath))
            {
                if (!Directory.Exists(replicaPath))
                {
                    Console.WriteLine(">>Provided replica directory path is invalid.");
                }
                config.ReplicaDirectoryPath = Path.GetFullPath(replicaPath);
            }

            if (arguments.TryGetValue("--logs", out var logsPath))
            {
                if (!Directory.Exists(logsPath))
                {
                    Console.WriteLine(">>Provided logs folder directory path is invalid");
                }
                config.LogsDirectory = Path.GetFullPath(logsPath);
            }

            if (arguments.ContainsKey("--interval"))
            {
                if (!int.TryParse(arguments["--interval"], out int interval) || interval <= 0)
                {
                    Console.WriteLine(">>Provided value interval is invalid. Must be a positive integer.");
                }
                config.SyncIntervalSeconds = interval;
            }

            if (arguments.ContainsKey("--archive"))
            {
                if (!bool.TryParse(arguments["--archive"], out bool archive))
                {
                    Console.WriteLine(">>Invalid value for --archive. Use true or false.");
                }
                config.ArchiveEnabled = archive;
            }

            return config;
        }

        private static void PrintConfig(Config config)
        {
            Console.WriteLine($"  SourceDirectoryPath:   {config.SourceDirectoryPath}");
            Console.WriteLine($"  ReplicaDirectoryPath:  {config.ReplicaDirectoryPath}");
            Console.WriteLine($"  LogsDirectory:         {config.LogsDirectory}");
            Console.WriteLine($"  SyncIntervalSeconds:   {config.SyncIntervalSeconds}");
            Console.WriteLine($"  ArchiveEnabled:        {config.ArchiveEnabled}");
            Console.WriteLine("");
        }
    }
}