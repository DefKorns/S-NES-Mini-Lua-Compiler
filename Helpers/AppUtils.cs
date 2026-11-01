using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SNESMiniLuaCompiler.Models;

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
        public static readonly string DecompilerPath = FileUtils.CombinePath(AppPath, "lib", "decompiler");
        public static readonly string ExeSuffix = OperatingSystem.IsWindows() ? ".exe" : string.Empty;
        public static readonly string DecompilerExe = FileUtils.CombinePath(DecompilerPath, "luajit-decompiler-v2" + ExeSuffix);
        public static readonly string LuaJitExe = FileUtils.CombinePath(LuaJitPath, "luajit" + ExeSuffix);
        public static readonly string ResourcesPath = FileUtils.CombinePath(Path.GetTempPath(), "SNESMiniLuaCompiler", "resources");
        // Embedded resources are named Lib/<path> (see LogicalName in the .csproj)
        public static readonly string SystemResourcePrefix = "Lib/";
        public static readonly string ConfigFile = Path.Combine(LibPath, "settings.config");

        #endregion

        #region Version

        public static string GetAppVersion()
            => Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? "Unknown";
        public static void SaveGitVersionToConfig()
        {
            string version = GetAppVersion();
            SaveConfig("gitversion", version);
        }

        #endregion

        #region Directory/Resource Management

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

        public static void ExtractAllResources()
        {
            ExceptionUtils.GlobalTryCatch(() =>
            {
                var resources = new (string Suffix, string Path)[]
                {
                    ("luajit", LuaJitPath),
                    ("decompiler", DecompilerPath)
                };

                foreach ((string suffix, string path) in resources)
                    ExtractResourcesWithPrefix($"{SystemResourcePrefix}{suffix}/", path);

                // Extracted files lose the executable bit on Linux
                if (!OperatingSystem.IsWindows())
                {
                    foreach (string? exe in new[] { LuaJitExe, DecompilerExe })
                    {
                        if (FileUtils.SafeFileExists(exe))
                            File.SetUnixFileMode(exe, File.GetUnixFileMode(exe) | UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute);
                    }
                }
            }, "Failed to extract resources.", "AppUtils.ExtractAllResources");
        }

        public static void ExtractSelectedResources(SystemModel systemModel)
        {
            string systemName = Path.GetFileName(GetSystemPath(systemModel));
            string resourcePrefix = $"{SystemResourcePrefix}original/{systemName}/resources/";
            string tempDir = Path.Combine(Path.GetTempPath(), "SNESMiniLuaCompiler", "resources", systemName);
            ExtractResourcesWithPrefix(resourcePrefix, tempDir);
        }

        public static void ExtractResourceToFile(string resourceName, string outputPath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using Stream resourceStream = (assembly?.GetManifestResourceStream(resourceName)) ?? throw new InvalidOperationException($"Resource '{resourceName}' not found.");

            FileUtils.CreatePath(Path.GetDirectoryName(outputPath) ?? string.Empty);

            using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            resourceStream.CopyTo(fileStream);
        }

        public static void ExtractResourcesWithPrefix(string resourcePrefix, string targetRoot)
        {
            ExceptionUtils.ThrowArgNull(resourcePrefix, nameof(resourcePrefix));
            ExceptionUtils.ThrowArgNull(targetRoot, nameof(targetRoot));

            var assembly = Assembly.GetExecutingAssembly();
            string[]? resourceNames = assembly?.GetManifestResourceNames();

            if (resourceNames is null || resourceNames.Length == 0)
                return;

            foreach (string? resourceName in resourceNames.AsSpan())
            {
                // MSBuild writes the directory separator of the build machine into the name
                string normalizedName = resourceName.Replace('\\', '/');
                if (!normalizedName.StartsWith(resourcePrefix, StringComparison.Ordinal))
                    continue;

                string relative = normalizedName[resourcePrefix.Length..];
                if (relative.Length == 0)
                    continue;

                string outputPath = FileUtils.CombinePath(targetRoot, relative.Replace('/', Path.DirectorySeparatorChar));

                ExceptionUtils.ThrowArgNull(resourceName, nameof(resourceName));
                ExceptionUtils.ThrowArgNull(outputPath, nameof(outputPath));
                ExtractResourceToFile(resourceName, outputPath);
            }
        }

        #endregion

        #region System Path and Directory Checks

        public static string GetSystemPath(SystemModel systemModel) =>
    systemModel switch
    {
        SystemModel.Famicom => FileUtils.CombinePath(ResourcesPath, "hvc"),
        SystemModel.Shonen => FileUtils.CombinePath(ResourcesPath, "hvcj"),
        SystemModel.SuperFamicom => FileUtils.CombinePath(ResourcesPath, "shvc"),
        SystemModel.Nes => FileUtils.CombinePath(ResourcesPath, "nes"),
        SystemModel.SnesPal => FileUtils.CombinePath(ResourcesPath, "snes-eur"),
        _ => FileUtils.CombinePath(ResourcesPath, "snes-usa")
    };
        //public static string GetSystemPath(SystemModel systemModel)
        //{
        //    var systemPaths = new Dictionary<SystemModel, string>
        //    {
        //        { SystemModel.Famicom, FileUtils.CombinePath(ResourcesPath, "hvc") },
        //        { SystemModel.Shonen,FileUtils.CombinePath(ResourcesPath, "hvcj") },
        //        { SystemModel.SuperFamicom, FileUtils.CombinePath(ResourcesPath, "shvc") },
        //        { SystemModel.Nes, FileUtils.CombinePath(ResourcesPath, "nes") },
        //        { SystemModel.SnesPal, FileUtils.CombinePath(ResourcesPath, "snes-eur") }
        //    };
        //    return systemPaths.TryGetValue(systemModel, out var path)
        //        ? path
        //        : FileUtils.CombinePath(ResourcesPath, "snes-usa");
        //}

        public static bool AreDecodedAndRecodedDirsPresent() =>
            FileUtils.SafeDirectoryExists(DecodedPath) && FileUtils.SafeDirectoryExists(RecodedPath);

        #endregion

        public static void SaveConfig(string key, string value)
        {
            List<string> lines = FileUtils.SafeFileExists(ConfigFile)
                ? File.ReadAllLines(ConfigFile).ToList()
                : [];

            int idx = lines.FindIndex(l => l.StartsWith(key + "=", StringComparison.Ordinal));
            string entry = $"{key}={value}";
            if (idx >= 0)
                lines[idx] = entry;
            else
                lines.Add(entry);

            File.WriteAllLines(ConfigFile, lines);
        }

        //public static void SaveConfig(string key, string value)
        //{
        //    List<string> lines = FileUtils.SafeFileExists(ConfigFile) ? [.. File.ReadAllLines(ConfigFile)] : new List<string>();
        //    bool found = false;

        //    for (int i = 0; i < lines.Count; i++)
        //    {
        //        if (lines[i].StartsWith(key + "=", StringComparison.OrdinalIgnoreCase))
        //        {
        //            lines[i] = $"{key}={value}";
        //            found = true;
        //            break;
        //        }
        //    }
        //    if (!found)
        //    {
        //        lines.Add($"{key}={value}");
        //    }

        //    File.WriteAllLines(ConfigFile, lines);
        //}

        public static string? LoadConfig(string key)
        {
            if (!File.Exists(ConfigFile))
                return null;

            foreach (string line in File.ReadAllLines(ConfigFile))
            {
                if (line.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase))
                    return line[(key.Length + 1)..];
            }
            return null;
        }
    }
}
