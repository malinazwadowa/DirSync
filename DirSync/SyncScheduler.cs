namespace DirSync
{
    /// <summary>
    /// Handles periodic execution of directory synchronization using the provided <see cref="SyncService"/>.
    /// <br/><br/>
    /// Uses a time interval (in seconds) to determine how often synchronization should occur.
    /// </summary>
    internal class SyncScheduler
    {
        private readonly SyncService _syncService;
        private readonly int? _intervalInSeconds;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncScheduler"/> class.
        /// </summary>
        /// <param name="syncService">The synchronization service used to perform sync operations.</param>
        /// <param name="intervalInSeconds">Time interval between sync operations, in seconds.</param>
        public SyncScheduler(SyncService syncService, int? intervalInSeconds)
        {
            _syncService = syncService;
            _intervalInSeconds = intervalInSeconds;
        }

        public void Start()
        {
            if (_intervalInSeconds == null || _intervalInSeconds <=0)
            {
                Console.WriteLine("Interval is not set.");
                return;
            }

            for (int i = 5; i > 0; i--)
            {
                Console.Write($"\rSync session beginning in {i}...   ");
                Thread.Sleep(1000);
            }

            var interval = TimeSpan.FromSeconds(_intervalInSeconds.Value);

            while (true)
            {
                var start = DateTime.UtcNow;

                bool status = _syncService.PerformSync();

                if (!status)
                {
                    Logger.LogMessage("Errors during synchronization of directories, check error log for details", true);
                }

                var elapsed = DateTime.UtcNow - start;
                var delay = interval - elapsed;

                if (delay > TimeSpan.Zero)
                {
                    Thread.Sleep(delay);
                }
                else
                {
                    Logger.LogError($"Synchronization took longer than the specified interval. Starting the next session with a delay of {delay.Duration():hh\\:mm\\:ss\\.fff}. Adjust the settings or system environment for correct operation.");
                }
            }
        }
    }
}
