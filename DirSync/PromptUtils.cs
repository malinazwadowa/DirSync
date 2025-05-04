namespace DirSync
{   
    /// <summary>
    /// Prompt helper class. 
    /// </summary>
    internal static class PromptUtils
    {
        public static Config PromptForConfig()
        {
            Config config = Config.GetEmptyConfig();

            Console.WriteLine("Please provide data needed for the app to operate.");
            config.SourceDirectoryPath = PromptUtils.PromptForPath("Source Directory");
            config.ReplicaDirectoryPath = PromptUtils.PromptForPath("Replica Directory");
            config.LogsDirectory = PromptUtils.PromptForPath("Logs Directory");
            config.SyncIntervalSeconds = PromptUtils.PromptForPositiveInt("synchronization interval in seconds");
            config.ArchiveEnabled = PromptUtils.PromptForConfirmation("Do you want to use archiving?");

            return config;
        }

        public static string PromptForPath(string label)
        {
            while (true)
            {
                Console.WriteLine($"Enter path to {label}:");
                string? path = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                {
                    Console.WriteLine("----------------------------------------------");
                    return path;
                }
                else
                {
                    Console.WriteLine("----------------------------------------------");
                    Console.WriteLine("Invalid path or directory does not exist. Please enter correct path.");
                }
            }
        }

        public static int PromptForPositiveInt(string label)
        {
            while (true)
            {
                Console.WriteLine($"Enter value for {label}:");
                string? input = Console.ReadLine();

                if (int.TryParse(input, out int result) && result >= 0)
                {
                    Console.WriteLine("----------------------------------------------");
                    return result;
                }
                else
                {
                    Console.WriteLine("----------------------------------------------");
                    Console.WriteLine("Invalid number. Please enter a valid non-negative integer.");
                }
            }
        }

        /// <summary>
        /// Will ask again until correct input is provided.
        /// </summary>
        /// <param name="message">Content of the message >" (y/n):" will be added after provided message.</param>
        /// <returns>True or false based on input.</returns>
        public static bool PromptForConfirmation(string message)
        {
            while (true)
            {
                Console.Write($"{message} (y/n): ");
                string? input = Console.ReadLine()?.ToLower();

                if (input == "y" || input == "yes")
                {
                    return true;
                }
                else if (input == "n" || input == "no")
                {
                    return false;
                }
                else
                {
                    Console.WriteLine("Please enter 'y' (yes) or 'n' (no).");
                }
            }
        }
    }
}