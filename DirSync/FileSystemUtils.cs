using System.IO;
using System.Security.Cryptography;

namespace DirSync
{
    /// <summary>
    /// Helper class for file and directory operations, like Moving, Deleting etc.
    /// </summary>
    internal static class FileSystemUtils
    {
        public static string GetFileHash(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = sha256.ComputeHash(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }
            }
        }
        /// <summary>
        /// Checks if parent directories exist for specified directory, if not creates them.
        /// </summary>
        /// <param name="dirPath">Full or relative path.</param>
        /// <returns>True if directories exist or were successfully created.</returns>
        private static bool EnsureParentDirectoriesExist(string dirPath)
        {
            try
            {
                string? parent = Path.GetDirectoryName(dirPath);

                if (parent != null && !Directory.Exists(parent))
                {
                    Directory.CreateDirectory(parent);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"> Failed to ensure parent directory for: {dirPath}.");
                Logger.LogError("Error: " + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Moves file from A to B.
        /// </summary>
        /// <param name="sourceFileName">Full or relative path.</param>
        /// <param name="destFileName">Full or relative path.</param>
        /// <param name="useSessionLog">Optional, false by default. If true will log message to current session log.</param>
        /// <returns>True upon successful move.</returns>
        public static bool MoveFile(string sourceFileName, string destFileName, bool? useSessionLog = null)
        {
            try
            {
                EnsureParentDirectoriesExist(destFileName);

                File.Move(sourceFileName, destFileName);
                Logger.LogMessage($"> Moved file from {sourceFileName} to {destFileName}.", useSessionLog);
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError($"> Failed to move file from {sourceFileName} to {destFileName}.");
                Logger.LogError("Error: " + e.ToString());
                return false;
            }
        }
        /// <summary>
        /// Copies specified file to specified location.
        /// </summary>
        /// <param name="sourceFileName">Full or relative path.</param>
        /// <param name="destFileName">Full or relative path.</param>
        /// <param name="useSessionLog">Optional, false by default. If true will log message to current session log.</param>
        /// <returns>True upon successful copying.</returns>
        public static bool CopyFile(string sourceFileName, string destFileName, bool? useSessionLog = null)
        {
            try
            {
                File.Copy(sourceFileName, destFileName);
                Logger.LogMessage($"> Copied file from {sourceFileName} to {destFileName}.", useSessionLog);
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError($"> Failed to move copy file from {sourceFileName} to {destFileName}.");
                Logger.LogError("Error: " + e.ToString());
                return false;
            }
        }
        /// <summary>
        /// Deletes specified file.
        /// </summary>
        /// <param name="fileName">Full or relative path.</param>
        /// <param name="useSessionLog">Optional, false by default. If true will log message to current session log.</param>
        /// <returns>True upon successful move.</returns>
        public static bool DeleteFile(string fileName, bool? useSessionLog = null)
        {
            try
            {
                File.Delete(fileName);
                Logger.LogMessage($"> Deleted file {fileName}.", useSessionLog);
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError($"> Failed to delete file {fileName}.");
                Logger.LogError("Error: " + e.ToString());
                return false;
            }
        }

        /// <summary>
        /// Moves directory with its content from A to B.
        /// </summary>
        /// <param name="sourceDirName">Full or relative path.</param>
        /// <param name="destDirName">Full or relative path.</param>
        /// <param name="useSessionLog">Optional, false by default. If true will log message to current session log.</param>
        /// <returns>True upon successful move.</returns>
        public static bool MoveDirectory(string sourceDirName, string destDirName, bool? useSessionLog = null)
        {
            try
            {
                EnsureParentDirectoriesExist(destDirName);

                Directory.Move(sourceDirName, destDirName);
                Logger.LogMessage($"> Moved directory from {sourceDirName} to {destDirName}", useSessionLog);
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError($"> Failed to move directory from {sourceDirName} to {destDirName}");
                Logger.LogError("Error: " + e.ToString());
                return false;
            }
        }

        /// <summary>
        /// Creates specified directory
        /// </summary>
        /// <param name="dirName">Full or relative path.</param>
        /// <param name="useSessionLog">Optional, false by default. If true will log message to current session log.</param>
        /// <returns>True upon successful creation.</returns>
        public static bool CreateDirectory(string dirName, bool? useSessionLog = null)
        {
            try
            {
                Directory.CreateDirectory(dirName);
                Logger.LogMessage($"> Created directory {dirName}.", useSessionLog);
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError($"> Failed to create directory {dirName}.");
                Logger.LogError("Error: " + e.ToString());
                return false;
            }
        }

        /// <summary>Deletes specified directory</summary>
        /// <param name="dirName">Full or relative path.</param>
        /// <param name="useSessionLog">Optional, false by default. If true will log message to current session log.</param>
        /// <returns>True upon successful deletion.</returns>
        public static bool DeleteDirectory(string dirName, bool? useSessionLog = null)
        {
            try
            {
                Directory.Delete(dirName, true);
                Logger.LogMessage($"> Deleted directory {dirName}.", useSessionLog);
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError($"> Failed to delete directory {dirName}.");
                Logger.LogError("Error: " + e.ToString());
                return false;
            }
        }
    }
}
