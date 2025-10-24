using System;
using System.IO;
using System.Security.Cryptography;

namespace SNESMiniLuaCompiler.Helpers
{
    public class FileUtils
    {
        #region Fields

        public static readonly string DecodedHashFile = CombinePath("lib", "hashes");

        #endregion

        #region Existence Checks
        /// <summary>
        /// Checks if a file exists and the path is not null or whitespace.
        /// </summary>
        public static bool SafeFileExists(string path) =>
            !string.IsNullOrWhiteSpace(path) && File.Exists(path);

        /// <summary>
        /// Checks if a directory exists and the path is not null or whitespace.
        /// </summary>
        public static bool SafeDirectoryExists(string path) =>
            !string.IsNullOrWhiteSpace(path) && Directory.Exists(path);

        #endregion

        #region Path Utilities

        public static string CombinePath(params string[] parts) => 
            Path.Combine(parts ?? Array.Empty<string>());

        #endregion

        #region Directory and File Operations

        /// <summary>
        /// Copies assets from the source folder to the destination folder.
        /// </summary>
        public static void CopyAssets(string sourceFolder, string destFolder)
        {
            ExceptionUtils.GlobalTryCatch(() =>
            {
                if (string.IsNullOrWhiteSpace(sourceFolder) || string.IsNullOrWhiteSpace(destFolder))
                    throw new ArgumentException("Source and destination folders must not be null or whitespace.");

                CreatePath(destFolder);

                foreach (var file in Directory.GetFiles(sourceFolder) ?? Array.Empty<string>())
                {
                    var dest = CombinePath(destFolder, Path.GetFileName(file) ?? string.Empty);
                    ExceptionUtils.GlobalTryCatch(
                        () => File.Copy(file, dest, true),
                        $"Error copying file '{file}' to '{dest}'.",
                        "FileUtils.CopyAssets"
                    );
                }

                foreach (var folder in Directory.GetDirectories(sourceFolder) ?? Array.Empty<string>())
                {
                    var dest = CombinePath(destFolder, Path.GetFileName(folder) ?? string.Empty);
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

        /// <summary>
        /// Creates a directory if it does not exist.
        /// </summary>
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

        /// <summary>
        /// Deletes a file if it exists.
        /// </summary>
        public static void DeleteFile(string file)
        {
            //if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
            //    return;
            if (!SafeFileExists(file))
                return;

            ExceptionUtils.GlobalTryCatch(() => File.Delete(file), "Error deleting file.", "AppUtils.DeleteFile");

            //try
            //{
            //    File.Delete(file);
            //}
            //catch (Exception ex) when (ex is ArgumentException || ex is UnauthorizedAccessException || ex is IOException)
            //{
            //    HandleException(ex, "Error deleting file.", "AppUtils.DeleteFile");
            //    throw;
            //}
        }

        /// <summary>
        /// Deletes a directory and its contents.
        /// </summary>
        public static void DeletePath(string path)
        {
            //if (string.IsNullOrWhiteSpace(path))
            //{
            //    Debug.WriteLine("DeletePath: Provided path is null or whitespace.");
            //    return;
            //}
            if (string.IsNullOrWhiteSpace(path))
            {
                ExceptionUtils.HandleException(
                    new ArgumentException("Provided path is null or whitespace.", nameof(path)),
                    "DeletePath: Provided path is null or whitespace.",
                    "AppUtils.DeletePath");
                return;
            }

            if (!Directory.Exists(path))
                return;

            ExceptionUtils.GlobalTryCatch(() => Directory.Delete(path, true), "Error deleting path.", "AppUtils.DeletePath");

            //try
            //{
            //    Directory.Delete(path, true);
            //}
            //catch (Exception ex) when (ex is ArgumentException || ex is UnauthorizedAccessException || ex is IOException)
            //{
            //    HandleException(ex, "Error deleting path.", "AppUtils.DeletePath");
            //    throw;
            //}
        }

        /// <summary>
        /// Deletes empty directories recursively.
        /// </summary>
        public static void DeleteEmptyDirectories(string startLocation)
        {
            //if (string.IsNullOrWhiteSpace(startLocation) || !Directory.Exists(startLocation))
            //    return;
            if (!SafeDirectoryExists(startLocation))
                return;

            ExceptionUtils.GlobalTryCatch(() =>
            {
                foreach (var directory in Directory.GetDirectories(startLocation) ?? Array.Empty<string>())
                {
                    DeleteEmptyDirectories(directory);

                    //bool isEmpty = Directory.GetFiles(directory).Length == 0 &&
                    //               Directory.GetDirectories(directory).Length == 0;
                    bool isEmpty = (Directory.GetFiles(directory)?.Length ?? 0) == 0 &&
                                   (Directory.GetDirectories(directory)?.Length ?? 0) == 0;

                    if (isEmpty)
                    {
                        Directory.Delete(directory, false);
                    }
                }
            }, "Error deleting directory.", "AppUtils.DeleteEmptyDirectories");

            //try
            //{
            //    //foreach (var directory in Directory.GetDirectories(startLocation))
            //    foreach (var directory in Directory.GetDirectories(startLocation) ?? Array.Empty<string>())
            //    {
            //        DeleteEmptyDirectories(directory);

            //        //bool isEmpty = Directory.GetFiles(directory).Length == 0 &&
            //        //               Directory.GetDirectories(directory).Length == 0;
            //        bool isEmpty = (Directory.GetFiles(directory)?.Length ?? 0) == 0 &&
            //                       (Directory.GetDirectories(directory)?.Length ?? 0) == 0;

            //        if (isEmpty)
            //        {
            //            Directory.Delete(directory, false);
            //        }
            //    }
            //}
            //catch (Exception ex) when (
            //    ex is IOException ||
            //    ex is UnauthorizedAccessException ||
            //    ex is ArgumentException)
            //{
            //    HandleException(ex, "Error deleting directory.", "AppUtils.DeleteEmptyDirectories");
            //    throw;
            //}
        }

        #endregion

        #region File Hashing

        /// <summary>
        /// Generates a file hash and appends it to the hash file.
        /// </summary>
        /// 
        public static void GenerateFileHash(string fileName)
        {
            if (!SafeFileExists(fileName))
                return;

            string fileHash = string.Empty;
            ExceptionUtils.GlobalTryCatch(() =>
            {
                fileHash = GetSHA256HashFromFile(fileName);
                if (!string.IsNullOrEmpty(fileHash))
                {
                    using (var writer = File.AppendText(DecodedHashFile))
                    {
                        writer.WriteLine(fileHash);
                    }
                }
            }, $"Error generating file hash for '{fileName}'.", "FileUtils.GenerateFileHash");
        }
        //public static void GenerateFileHash(string fileName)
        //{
        //    var fileHash = GetSHA256HashFromFile(fileName);

        //    ExceptionUtils.GlobalTryCatch(() =>
        //    {
        //        using (var writer = File.AppendText(DecodedHashFile))
        //        {
        //            writer.WriteLine(fileHash);
        //        }
        //    }, "Error generating file hash.", "AppUtils.GenerateFileHash");

        //    //try
        //    //{
        //    //    using (var writer = File.AppendText(DecodedHashFile))
        //    //    {
        //    //        writer.WriteLine(fileHash);
        //    //    }
        //    //}
        //    //catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        //    //{
        //    //    HandleException(ex, "Error generating file hash.", "AppUtils.GenerateFileHash");
        //    //    throw;
        //    //}
        //}

        /// <summary>
        /// Computes the SHA256 hash of a file.
        /// </summary>
        public static string GetSHA256HashFromFile(string fileName)
        {
            using (var sha256 = SHA256.Create())
            using (var stream = File.OpenRead(fileName))
            {
                var hash = sha256.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
            }
        }

        /// <summary>
        /// Checks if a file has been edited based on its hash.
        /// </summary>
        //public static bool HasEditedFiles(string hash)
        //{
        //    foreach (var line in File.ReadAllLines(DecodedHashFile) ?? Array.Empty<string>())
        //    {
        //        if (line?.Contains(hash ?? string.Empty) == true)
        //        {
        //            return false;
        //        }
        //    }
        //    return true;
        //}
        public static bool HasEditedFiles(string hash)
        {
            foreach (var line in SafeReadAllLines(DecodedHashFile))
            {
                if (line?.Contains(hash ?? string.Empty) == true)
                {
                    return false;
                }
            }
            return true;
        }

        public static string SafeReadAllText(string filePath)
        {
            if (!SafeFileExists(filePath))
                return string.Empty;

            string result = string.Empty;
            ExceptionUtils.GlobalTryCatch(() =>
            {
                result = File.ReadAllText(filePath);
            }, "Error reading file.", "FileUtils.SafeReadAllText");
            return result;
        }

        public static string[] SafeReadAllLines(string filePath)
        {
            if (!SafeFileExists(filePath))
                return Array.Empty<string>();

            string[] result = Array.Empty<string>();
            ExceptionUtils.GlobalTryCatch(() =>
            {
                result = File.ReadAllLines(filePath);
            }, "Error reading lines from file.", "FileUtils.SafeReadAllLines");
            return result;
        }

        public static byte[] SafeReadBytes(string filePath)
        {
            if (!SafeFileExists(filePath))
                return Array.Empty<byte>();

            byte[] result = Array.Empty<byte>();
            ExceptionUtils.GlobalTryCatch(() =>
            {
                result = File.ReadAllBytes(filePath);
            }, "Error reading bytes from file.", "FileUtils.SafeReadBytes");
            return result;
        }

        public static void SafeWriteBytes(string filePath, byte[] bytes)
        {
            if (string.IsNullOrWhiteSpace(filePath) || bytes == null)
                return;

            ExceptionUtils.GlobalTryCatch(() =>
            {
                File.WriteAllBytes(filePath, bytes);
            }, "Error writing bytes to file.", "FileUtils.SafeWriteBytes");
        }

        public static void SafeAppendAllLines(string filePath, string[] lines)
        {
            if (string.IsNullOrWhiteSpace(filePath) || lines == null)
                return;

            ExceptionUtils.GlobalTryCatch(() =>
            {
                File.AppendAllLines(filePath, lines);
            }, "Error appending lines to file.", "FileUtils.SafeAppendAllLines");
        }

        public static void SafeAppendAllText(string filePath, string content)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            ExceptionUtils.GlobalTryCatch(() =>
            {
                File.AppendAllText(filePath, content ?? string.Empty);
            }, "Error appending text to file.", "FileUtils.SafeAppendAllText");
        }
        #endregion
    }
}
