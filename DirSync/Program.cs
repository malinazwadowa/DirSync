namespace DirSync
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Program program = new Program();
            program.Run(args);
        }

        private void Run(string[] args)
        {
            if (HelpRequested(args))
            {
                ShowHelp();
                Environment.Exit(0);
            }

            ConfigManager configManager = new ConfigManager();
            configManager.Initialize(args, IsInteractiveModeRequested(args));
            Config myConfig = configManager.CurrentConfig;

            // Initialize Variables 
            string originPath = myConfig.SourceDirectoryPath;
            string targetPath = myConfig.ReplicaDirectoryPath;
            bool useArchiving = myConfig.ArchiveEnabled.Value;
            int? syncIntervalSeconds = myConfig.SyncIntervalSeconds;
            // Create Base Directories
            string logsFolderPath = Path.Combine(myConfig.LogsDirectory, "DirSync");
            string generalLogsPath = Path.Combine(logsFolderPath, "Logs");
            string errorLogsPath = Path.Combine(logsFolderPath, "Error Logs");
            string archivePath = Path.Combine(logsFolderPath, "Archive");
            // Create Directories
            Directory.CreateDirectory(logsFolderPath);
            Directory.CreateDirectory(archivePath);
            Directory.CreateDirectory(generalLogsPath);
            Directory.CreateDirectory(errorLogsPath);

            Logger.Initialize(generalLogsPath, errorLogsPath);
            SyncService syncService = new (originPath, targetPath, useArchiving, archivePath);
            SyncScheduler syncScheduler = new (syncService, syncIntervalSeconds);
            syncScheduler.Start();
        }

        private static bool IsInteractiveModeRequested(string[] args)
        {
            return args.Contains("--i") || args.Contains("--interactive");
        }

        private static bool HelpRequested(string[] args)
        {
            return args.Contains("--h") || args.Contains("--help");
        }
        private static void ShowHelp()
        {
            Console.WriteLine("Usage: DirSync [options]");
            Console.WriteLine("Options:");
            Console.WriteLine("  --help or --h           Show this help message");
            Console.WriteLine("  --interactive or --i    Launch in interactive mode");
            Console.WriteLine("  --source <path>         Set the source directory");
            Console.WriteLine("  --replica <path>        Set the replica directory");
            Console.WriteLine("  --logs <path>           Set the location for logs directory");
            Console.WriteLine("  --interval <seconds>    (optional) 9000 by default. Set sync interval in seconds.");
            Console.WriteLine("  --archive               (optional) On by default. Enable archiving.");
            Console.WriteLine("Example:");
            Console.WriteLine("  DirSync --source=C:\\Source --replica=D:\\Replica --logs=C:\\logs --interval=9000");
        }
    }
}

