namespace DirSync
{
    /// <summary>
    /// Performs synchronization of state for specified source directory and replica directory.
    /// </summary>
    internal class SyncService
    {
        private string _originPath;
        private string _targetPath;
        private string _archivePath;
        private bool _useArchiving;
        private bool _operationsWerePerformed;

        private string _sessionArchivePath = null!;
        private string _currentSessionId;

        /// <summary>
        /// Tetstst
        /// </summary>
        /// <param name="sourcePath">Directory used as blueprint.</param>
        /// <param name="replicaPath">Directory in which copy of blueprint will be created </param>
        /// <param name="useArchiving">If True, overhead or modified files from replica path will be stored in archive instead of being deleted every sync.</param>
        /// <param name="archivePath">Location of the archive directory.</param>
        public SyncService(string sourcePath, string replicaPath, bool useArchiving, string archivePath)
        {
            _originPath = sourcePath;
            _targetPath = replicaPath;
            _archivePath = archivePath;
            _useArchiving = useArchiving;
        }

        public bool PerformSync()
        {
            _operationsWerePerformed = false;
            SetupSyncFiles();
            bool syncSuccessful = true;

            Logger.LogMessage("=======================", true);
            Logger.LogMessage("Starting sync session:");
            if (!DeleteOverheadDirectoriesFromTarget()) syncSuccessful = false;
            if (!RecreateMissingDirectories()) syncSuccessful = false;
            if (!RemoveOverheadFiles()) syncSuccessful = false;
            if (!ReplaceEditedFiles()) syncSuccessful = false;
            if (!ImportMissingFiles()) syncSuccessful = false;

            Logger.LogMessage("------------------------------------------------", true);
            Logger.LogMessage("Was synchronization successful? >> " + syncSuccessful + " <<", true);
            Logger.LogMessage("================================================", true);

            //
            ClenupFiles();

            return syncSuccessful;
        }

        private void SetupSyncFiles()
        {
            _currentSessionId = $"Sync_Session_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
            _sessionArchivePath = Path.Combine(_archivePath, _currentSessionId);
            Directory.CreateDirectory(_sessionArchivePath);
            Logger.CreateNewSessionLog(_currentSessionId);
        }

        private bool DeleteOverheadDirectoriesFromTarget()
        {
            string[] targetDirs = Directory.GetDirectories(_targetPath, "*", SearchOption.AllDirectories);
            OperationStatus status = new OperationStatus();

            Logger.LogMessage("------------------------------------------------", true);
            Logger.LogMessage(_useArchiving ? "Moving overhead directories to archive..." : "Deleting overhead directories...", true);

            foreach (string targetDir in targetDirs)
            {
                if (!Directory.Exists(targetDir))
                {
                    continue;
                }

                string relativePath = Path.GetRelativePath(_targetPath, targetDir);
                string dirToCheck = Path.Combine(_originPath, relativePath);

                if (!Directory.Exists(dirToCheck))
                {
                    if (_useArchiving)
                    {
                        string archiveDir = Path.Combine(_sessionArchivePath, relativePath);
                        status.AssertTrue(FileSystemUtils.MoveDirectory(targetDir, archiveDir, true));
                        _operationsWerePerformed = true;
                    }
                    else
                    {
                        status.AssertTrue(FileSystemUtils.DeleteDirectory(targetDir, true));
                        _operationsWerePerformed = true;
                    }
                }
            }
            return status.Success;
        }

        private bool RecreateMissingDirectories()
        {
            string[] originDirs = Directory.GetDirectories(_originPath, "*", SearchOption.AllDirectories);
            OperationStatus status = new OperationStatus();

            Logger.LogMessage("------------------------------------------------", true);
            Logger.LogMessage("Recreating missing directories structure...", true);
            
            foreach (string originDir in originDirs)
            {
                string targetDirToCheck = Path.Combine(_targetPath, Path.GetRelativePath(_originPath, originDir));
                if (!Directory.Exists(targetDirToCheck))
                {
                    status.AssertTrue(FileSystemUtils.CreateDirectory(targetDirToCheck, true));
                    _operationsWerePerformed = true;
                }
            }
            return status.Success;
        }

        private bool RemoveOverheadFiles()
        {
            string[] targetFiles = Directory.GetFiles(_targetPath, "*", SearchOption.AllDirectories);
            OperationStatus status = new OperationStatus();

            Logger.LogMessage("------------------------------------------------", true);
            Logger.LogMessage(_useArchiving ? "Moving overhead files to archive..." : "Deleting overhead files...", true);

            foreach (string targetFile in targetFiles)
            {
                string relativePath = Path.GetRelativePath(_targetPath, targetFile);
                string fileToCheck = Path.Combine(_originPath, relativePath);
                if (!File.Exists(fileToCheck))
                {
                    if (_useArchiving)
                    {
                        string fileArchivePath = Path.Combine(_sessionArchivePath, relativePath);
                        status.AssertTrue(FileSystemUtils.MoveFile(targetFile, fileArchivePath, true));
                        _operationsWerePerformed = true;
                    }
                    else
                    {
                        status.AssertTrue(FileSystemUtils.DeleteFile(targetFile, true));
                        _operationsWerePerformed = true;
                    }
                }
            }
            return status.Success;
        }

        private bool ReplaceEditedFiles()
        {
            string[] targetFiles = Directory.GetFiles(_targetPath, "*", SearchOption.AllDirectories);
            OperationStatus status = new OperationStatus();

            Logger.LogMessage("------------------------------------------------", true);
            Logger.LogMessage(_useArchiving ? "Moving changed files to archive..." : "Deleting changed files...", true);

            foreach (string targetFile in targetFiles)
            {
                string relativePath = Path.GetRelativePath(_targetPath, targetFile);
                string fileToCheck = Path.Combine(_originPath, relativePath);

                string targetFileHash = FileSystemUtils.GetFileHash(targetFile);
                string fileToCheckHash = FileSystemUtils.GetFileHash(fileToCheck);

                if (targetFileHash != fileToCheckHash)
                {
                    if (_useArchiving)
                    {
                        string fileArchivePath = Path.Combine(_sessionArchivePath, relativePath);
                        if (!FileSystemUtils.MoveFile(targetFile, fileArchivePath, true))
                        {
                            status.Fail();
                            continue;
                        }
                        status.AssertTrue(FileSystemUtils.CopyFile(fileToCheck, targetFile, true));
                        _operationsWerePerformed = true;
                    }
                    else
                    {
                        status.AssertTrue(FileSystemUtils.DeleteFile(targetFile, true));
                        _operationsWerePerformed = true;
                    }
                }
            }
            return status.Success;
        }


        private bool ImportMissingFiles()
        {
            string[] originFiles = Directory.GetFiles(_originPath, "*", SearchOption.AllDirectories);
            OperationStatus status = new OperationStatus();

            Logger.LogMessage("------------------------------------------------", true);
            Logger.LogMessage("Importing missing files...", true);

            foreach (string originFile in originFiles)
            {
                string relativePath = Path.GetRelativePath(_originPath, originFile);
                string fileToCheck = Path.Combine(_targetPath, relativePath);

                if (!File.Exists(fileToCheck))
                {
                    status.AssertTrue(FileSystemUtils.CopyFile(originFile, fileToCheck, true));
                    _operationsWerePerformed = true;
                }
            }
            return status.Success;
        }

        private void ClenupFiles()
        {
            if (!_operationsWerePerformed)
            {
                Logger.DeleteCurrentSessionLog(_currentSessionId);
            }

            if (Directory.GetDirectories(_sessionArchivePath).Length == 0 && Directory.GetFiles(_sessionArchivePath).Length == 0)
            {
                Directory.Delete(_sessionArchivePath, true);
            }
        }
    }
}
