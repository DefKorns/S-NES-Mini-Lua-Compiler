using SNESMiniLuaCompiler.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SNESMiniLuaCompiler.Helpers
{
    public class AppUtils
    {
        #region Paths and Constants

        public static readonly string AppPath = AppContext.BaseDirectory;
        public static readonly string LibPath = FileUtils.CombinePath(AppPath, "lib");
        public static readonly string DecodedPath = FileUtils.CombinePath(AppPath, "decoded");
        public static readonly string RecodedPath = FileUtils.CombinePath(AppPath, "recoded");
        public static readonly string LuaJitPath = FileUtils.CombinePath(AppPath, "lib", "luajit");
        public static readonly string DecompilerPath = FileUtils.CombinePath("lib", "decompiler");
        public static readonly string DecompilerScript = FileUtils.CombinePath(DecompilerPath, "main.py");
        public static readonly string ResourcesPath = FileUtils.CombinePath("lib", "resources");
        public static readonly string SystemResourcePrefix = "SNESMiniLuaCompiler.Lib";
        public static readonly string ConfigFile = Path.Combine(LibPath, "settings.config");

        #endregion

        #region Version

        /// <summary>
        /// Gets the application version as a string.
        /// </summary>
        public static string GetAppVersion() =>
            Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";

        #endregion

        #region Directory/Resource Management

        /// <summary>
        /// Ensures all required module directories exist.
        /// </summary>
        public static void EnsureAllModuleDirectories()
        {
            EnsureDirectoryExists(LuaJitPath);
            EnsureDirectoryExists(DecompilerPath);
        }

        public static void EnsureDirectoryExists(string path)
        {
            if (!FileUtils.SafeDirectoryExists(path))
                Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Extracts all required resources in a single call.
        /// </summary>
        public static void ExtractAllResources()
        {
            ExceptionUtils.GlobalTryCatch(() =>
            {
                // Existing extraction logic here
                var resources = new (string Suffix, string Path)[]
                {
                    ("luajit", LuaJitPath),
                    ("decompiler", DecompilerPath)
                };

                foreach (var (suffix, path) in resources)
                    ExtractResourcesWithPrefix($"{SystemResourcePrefix}.{suffix}.", path);  
            }, "Failed to extract resources.", "AppUtils.ExtractAllResources");
            //var resources = new (string Suffix, string Path)[]
            //{
            //    ("luajit", LuaJitPath),
            //    ("decompiler", DecompilerPath)
            //};

            //foreach (var (suffix, path) in resources)
            //    ExtractResourcesWithPrefix($"{SystemResourcePrefix}.{suffix}.", path);
        }

        public static void ExtractSelectedResources(SystemModel systemModel)
        {
            string systemName = Path.GetFileName(GetSystemPath(systemModel));
            string resourcePrefix = $"{SystemResourcePrefix}.original.{systemName}.resources.";
            string outputDir = FileUtils.CombinePath(ResourcesPath, systemName);
            ExtractResourcesWithPrefix(resourcePrefix, outputDir);
        }


        public static void ExtractResourceToFile(string resourceName, string outputPath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var resourceStream = (assembly?.GetManifestResourceStream(resourceName)) ?? throw new InvalidOperationException($"Resource '{resourceName}' not found.");

            FileUtils.CreatePath(Path.GetDirectoryName(outputPath) ?? string.Empty);

            using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            resourceStream.CopyTo(fileStream);
        }

        public static void ExtractResourcesWithPrefix(string resourcePrefix, string targetRoot)
        {
            ExceptionUtils.ThrowArgNull(resourcePrefix, nameof(resourcePrefix));
            ExceptionUtils.ThrowArgNull(targetRoot, nameof(targetRoot));

            // Replace '-' with '_' in the resourcePrefix for matching
            string normalizedPrefix = resourcePrefix.Replace('-', '_');
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly?.GetManifestResourceNames();

            if (resourceNames is null || resourceNames.Length == 0)
                return;

            foreach (var resourceName in resourceNames.AsSpan())
            {
                if (!resourceName.StartsWith(normalizedPrefix, StringComparison.Ordinal))
                    continue;

                string relative = resourceName.AsSpan(normalizedPrefix.Length).ToString();
                string[] parts = relative.Split('.');
                if (parts.Length < 2)
                    continue;

                string fileName = parts[^2] + "." + (parts[^1]?.Replace('_', '.') ?? string.Empty);
                string dirPath = parts.Length > 2
                    ? string.Join(Path.DirectorySeparatorChar.ToString(), parts, 0, parts.Length - 2)
                    : string.Empty;

                string outputPath = FileUtils.CombinePath(targetRoot, dirPath, fileName);

                ExceptionUtils.ThrowArgNull(resourceName, nameof(resourceName));
                ExceptionUtils.ThrowArgNull(outputPath, nameof(outputPath));
                ExtractResourceToFile(resourceName, outputPath);
            }
        }

        #endregion

        #region System Path and Directory Checks

        /// <summary>
        /// Gets the system path for a given button name.
        /// </summary>
        public static string GetSystemPath(SystemModel systemModel)
        {
            var systemPaths = new Dictionary<SystemModel, string>
            {
                { SystemModel.Famicom, FileUtils.CombinePath(ResourcesPath, "hvc") },
                { SystemModel.Shonen,FileUtils.CombinePath(ResourcesPath, "hvcj") },
                { SystemModel.SuperFamicom, FileUtils.CombinePath(ResourcesPath, "shvc") },
                { SystemModel.Nes, FileUtils.CombinePath(ResourcesPath, "nes") },
                { SystemModel.SnesPal, FileUtils.CombinePath(ResourcesPath, "snes-eur") }
            };
            return systemPaths.TryGetValue(systemModel, out var path)
                ? path
                : FileUtils.CombinePath(ResourcesPath, "snes-usa");
        }

        /// <summary>
        /// Checks if both decoded and recoded directories exist.
        /// </summary>
        public static bool AreDecodedAndRecodedDirsPresent() =>
            FileUtils.SafeDirectoryExists(DecodedPath) && FileUtils.SafeDirectoryExists(RecodedPath);

        #endregion

        

        public static void SaveConfig(string key, string value)
        {
            // Read all lines if file exists, else create new list
            var lines = FileUtils.SafeFileExists(ConfigFile) ? [.. File.ReadAllLines(ConfigFile)] : new List<string>();
            bool found = false;

            // Update the key if it exists
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].StartsWith(key + "=", StringComparison.OrdinalIgnoreCase))
                {
                    lines[i] = $"{key}={value}";
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                lines.Add($"{key}={value}");
            }

            File.WriteAllLines(ConfigFile, lines);
        }

        // Optionally, add a method to read config values
        public static string? LoadConfig(string key)
        {
            if (!File.Exists(ConfigFile))
                return null;

            foreach (var line in File.ReadAllLines(ConfigFile))
            {
                if (line.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase))
                return line[(key.Length + 1)..];
            }
            return null;
        }
    }
}
