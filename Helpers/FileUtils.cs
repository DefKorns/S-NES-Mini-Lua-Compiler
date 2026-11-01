using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace SNESMiniLuaCompiler.Helpers
{
    public static class FileUtils
    {
        #region Fields

        public static readonly string DecodedHashFile = CombinePath("lib", "hashes");

        #endregion

        #region Existence Checks
        public static bool SafeFileExists(string path) =>
            !string.IsNullOrWhiteSpace(path) && File.Exists(path);

        public static bool SafeDirectoryExists(string path) =>
            !string.IsNullOrWhiteSpace(path) && Directory.Exists(path);

        #endregion

        #region Path Utilities

        public static string CombinePath(params string[] parts) =>
            Path.Combine(parts ?? []);

        #endregion

        #region Directory and File Operations

        public static void CopyAssets(string sourceFolder, string destFolder)
        {
            ExceptionUtils.GlobalTryCatch(() =>
            {
                if (string.IsNullOrWhiteSpace(sourceFolder) || string.IsNullOrWhiteSpace(destFolder))
                    throw new ArgumentException("Source and destination folders must not be null or whitespace.");

                CreatePath(destFolder);

                foreach (string file in Directory.GetFiles(sourceFolder))
                {
                    string dest = CombinePath(destFolder, Path.GetFileName(file));
                    ExceptionUtils.GlobalTryCatch(
                        () => File.Copy(file, dest, true),
                        $"Error copying file '{file}' to '{dest}'.",
                        "FileUtils.CopyAssets"
                    );
                }

                foreach (string folder in Directory.GetDirectories(sourceFolder))
                {
                    string dest = CombinePath(destFolder, Path.GetFileName(folder));
                    ExceptionUtils.GlobalTryCatch(
                        () => CopyAssets(folder, dest),
                        $"Error copying directory '{folder}' to '{dest}'.",
                        "FileUtils.CopyAssets"
                    );
                }
            },
            "Error copying assets.",
            "FileUtils.CopyAssets");
        }

        public static void CreatePath(string path)
        {
            ExceptionUtils.GlobalTryCatch(() =>
            {
                if (string.IsNullOrWhiteSpace(path))
                    throw new ArgumentException("Path must not be null or whitespace.", nameof(path));

                if (SafeDirectoryExists(path))
                    return;

                Directory.CreateDirectory(path);
            },
            "Error creating directory.",
            "AppUtils.CreatePath");
        }

        public static void DeleteFile(string file)
        {
            if (!SafeFileExists(file))
                return;

            ExceptionUtils.GlobalTryCatch(() => File.Delete(file), "Error deleting file.", "AppUtils.DeleteFile");
        }

        public static void DeletePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                ExceptionUtils.HandleException(
                    new ArgumentException("Provided path is null or whitespace.", nameof(path)),
                    "DeletePath: Provided path is null or whitespace.",
                    "AppUtils.DeletePath");
                return;
            }

            if (!SafeDirectoryExists(path))
                return;

            ExceptionUtils.GlobalTryCatch(() => Directory.Delete(path, true), "Error deleting path.", "AppUtils.DeletePath");
        }

        public static void DeleteEmptyDirectories(string startLocation)
        {
            if (!SafeDirectoryExists(startLocation))
                return;

            ExceptionUtils.GlobalTryCatch(() =>
            {
                foreach (string directory in Directory.GetDirectories(startLocation))
                {
                    DeleteEmptyDirectories(directory);

                    if (!Directory.EnumerateFileSystemEntries(directory).Any())
                        Directory.Delete(directory, false);
                }
            }, "Error deleting directory.", "AppUtils.DeleteEmptyDirectories");
        }

        #endregion

        #region File Hashing

        public static void GenerateFileHash(string fileName)
        {
            if (!SafeFileExists(fileName))
                return;

            string fileHash = GetSHA256HashFromFile(fileName);
            if (string.IsNullOrEmpty(fileHash))
                return;

            ExceptionUtils.GlobalTryCatch(() =>
            {
                using StreamWriter writer = File.AppendText(DecodedHashFile);
                writer.WriteLine(fileHash);
            }, $"Error generating file hash for '{fileName}'.", "FileUtils.GenerateFileHash");
        }

        public static string GetSHA256HashFromFile(string fileName)
        {
            using FileStream stream = File.OpenRead(fileName);
            return Convert.ToHexString(SHA256.HashData(stream));
        }

        public static bool HasEditedFiles(string hash)
        {
            return !SafeReadAllLines(DecodedHashFile).Any(line => line.Contains(hash, StringComparison.Ordinal));
        }

        public static string[] SafeReadAllLines(string filePath)
        {
            return ExceptionUtils.SafeFileOperation(
                () => File.ReadAllLines(filePath),
             [],
           filePath,
             "FileUtils.SafeReadAllLines"
            );
        }
        #endregion

        public static int CountFilesRecursive(string sDir)
        {
            if (!SafeDirectoryExists(sDir))
                return 0;
            return Directory.GetFiles(sDir, "*", SearchOption.AllDirectories).Length;
        }
    }
}
