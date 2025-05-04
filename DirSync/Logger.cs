namespace DirSync
{
    /// <summary>
    /// Logs messages to console and files. Handles session logs.
    /// </summary>
    static class Logger
    {
        private static string _generalLogsPath;
        private static string _errorLogsPath;
        private static string _logFilePath;
        private static string _errorLogFilePath;
        private static string _currentSessionLogFile;

        public static void Initialize(string generalLogsFolderPath, string errorLogsFolderPath)
        {
            _generalLogsPath = generalLogsFolderPath;
            _errorLogsPath = errorLogsFolderPath;

            SetupLogFiles();
        }

        public static void CreateNewSessionLog(string sessionId)
        {
            string fileName = sessionId + ".txt";
            _currentSessionLogFile = Path.Combine(_generalLogsPath, fileName);

            if (!File.Exists(_currentSessionLogFile))
            {
                File.Create(_currentSessionLogFile).Dispose();
            }
        }

        public static void DeleteCurrentSessionLog(string sessionId)
        {
            File.Delete(_currentSessionLogFile);
        }

        private static void SetupLogFiles()
        {
            _logFilePath = Path.Combine(_generalLogsPath, "Log.txt");

            if (!File.Exists(_logFilePath))
            {
                File.Create(_logFilePath).Dispose();
            }

            _errorLogFilePath = Path.Combine(_errorLogsPath, "Error_Log.txt");

            if (!File.Exists(_errorLogFilePath))
            {
                File.Create(_errorLogFilePath).Dispose();
            }
        }

        /// <summary>
        /// Logs message to console and file.
        /// </summary>
        /// <param name="message">Content of logged message.</param>
        /// <param name="useSessionLog">False by default, if true will log to dedicated session file, otherwise general log is used.</param>
        public static void LogMessage(string message, bool? useSessionLog = null)
        {
            Console.WriteLine(message);

            if(useSessionLog == true)
            {
                LogToFile(message, _currentSessionLogFile);
            }
            else
            {
                LogToFile(message, _logFilePath);
            }
        }

        /// <summary>
        /// Logs error message to dedicated Error Log.
        /// </summary>
        /// <param name="message">Content of the logged message.</param>
        public static void LogError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
            LogToFile(message, _errorLogFilePath);
        }

        private static void LogToFile(string message, string filePath)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(filePath, true))
                {
                    sw.WriteLine($"{DateTime.Now}: {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log message to file: {ex.Message}");
            }
        }
    }
}
