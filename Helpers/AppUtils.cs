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

        //public static readonly string AppPath = Path.GetDirectoryName(Application.ExecutablePath) ?? string.Empty;
        public static readonly string AppPath = AppContext.BaseDirectory;
        public static readonly string LibPath = FileUtils.CombinePath(AppPath, "lib");
        public static readonly string DecodedPath = FileUtils.CombinePath(AppPath, "decoded");
        public static readonly string RecodedPath = FileUtils.CombinePath(AppPath, "recoded");
        public static readonly string OriginalPath = FileUtils.CombinePath("lib", "original");
        public static readonly string LuaJitPath = FileUtils.CombinePath(AppPath, "lib", "luajit");
        public static readonly string DecompilerPath = FileUtils.CombinePath("lib", "decompiler");
        public static readonly string DecompilerScript = FileUtils.CombinePath(DecompilerPath, "main.py");
        public static readonly string FirstRun = FileUtils.CombinePath("lib", "firstRun");
        public static readonly string ResourcesPath = FileUtils.CombinePath("lib", "resources");
        public static readonly string SystemResourcePrefix = "SNESMiniLuaCompiler.Lib";

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
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Extracts all required resources in a single call.
        /// </summary>
        public static void ExtractAllResources()
        {
            var resources = new (string Suffix, string Path)[]
            {
                ("luajit", LuaJitPath),
                ("decompiler", DecompilerPath)
            };

            foreach (var (suffix, path) in resources)
                ExtractResourcesWithPrefix($"{SystemResourcePrefix}.{suffix}.", path);
        }

        //public static void ExtractSelectedResources()
        //{
        //    ExtractResourcesWithPrefix("SNESMiniLuaCompiler.Lib.original.hvcj.", ResourcesPath);
        //    //ExtractResourcesWithPrefix("SNESMiniLuaCompiler.Lib.decompiler.", DecompilerPath);
        //}

        //public static void ExtractSelectedResources(SystemModel systemModel)
        //{
        //    // Use GetSystemPath to determine the correct resource subdirectory
        //    string systemPath = GetSystemPath(systemModel);
        //    // Compose the resource prefix for embedded resources
        //    string resourcePrefix = $"{SystemResourcePrefix}.original.{Path.GetFileName(systemPath)}.resources.";
        //    // Compose the output directory for extraction
        //    string outputDir = FileUtils.CombinePath(ResourcesPath, Path.GetFileName(systemPath));
        //    ExtractResourcesWithPrefix(resourcePrefix, ResourcesPath);
        //}

        public static void ExtractSelectedResources(SystemModel systemModel)
        {
            string systemPath = GetSystemPath(systemModel);
            string systemName = Path.GetFileName(GetSystemPath(systemModel));
            string resourcePrefix = $"{SystemResourcePrefix}.original.{systemName}.resources.";
            string outputDir = FileUtils.CombinePath(ResourcesPath, systemName);
            ExtractResourcesWithPrefix(resourcePrefix, outputDir);
        }


        public static void ExtractResourceToFile(string resourceName, string outputPath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var resourceStream = assembly?.GetManifestResourceStream(resourceName);
            if (resourceStream == null)
                throw new InvalidOperationException($"Resource '{resourceName}' not found.");
            FileUtils.CreatePath(Path.GetDirectoryName(outputPath) ?? string.Empty);
            using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            resourceStream.CopyTo(fileStream);
        }

        public static void ExtractResourcesWithPrefix(string resourcePrefix, string targetRoot)
        {
            ExceptionUtils.ThrowArgNull(resourcePrefix, nameof(resourcePrefix));
            ExceptionUtils.ThrowArgNull(targetRoot, nameof(targetRoot));

            var assembly = Assembly.GetExecutingAssembly();
            foreach (var resourceName in assembly?.GetManifestResourceNames() ?? [])
            {
                if (!resourceName.StartsWith(resourcePrefix, StringComparison.Ordinal))
                    continue;

                string relative = resourceName.AsSpan(resourcePrefix.Length).ToString();
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

        #region UI Helpers

        ///// <summary>
        ///// Displays or hides a loading spinner.
        ///// </summary>
        //public static void LoadSpinner(bool displayLoader, PictureBox loaderImage, Form form)
        //{
        //    ExceptionUtils.ThrowArgNull(loaderImage, nameof(loaderImage));
        //    ExceptionUtils.ThrowArgNull(form, nameof(form));

        //    form?.Invoke((MethodInvoker)delegate
        //    {
        //        if (loaderImage != null)
        //            loaderImage.Visible = displayLoader;
        //        if (form != null)
        //            form.Cursor = displayLoader ? Cursors.WaitCursor : Cursors.Default;
        //    });
        //}

        /// <summary>
        /// Sets the enabled state, border color, border size, and mouse over color for a button.
        /// </summary>
        //public static void SetButtonState(
        //    Button button,
        //    bool enabled,
        //    int borderSize = 0,
        //    Color? borderColor = null,
        //    Color? mouseOverBackColor = null)
        //{
        //    if (button == null) return;
        //    button.Enabled = enabled;
        //    button.FlatAppearance.BorderSize = borderSize;
        //    if (borderColor.HasValue)
        //        button.FlatAppearance.BorderColor = borderColor.Value;
        //    if (mouseOverBackColor.HasValue)
        //        button.FlatAppearance.MouseOverBackColor = mouseOverBackColor.Value;
        //}

        #endregion

        #region System Path and Directory Checks

        /// <summary>
        /// Gets the system path for a given button name.
        /// </summary>
        public static string GetSystemPath(SystemModel systemModel)
        {
            var systemPaths = new Dictionary<SystemModel, string>
            {
                { SystemModel.Famicom, FileUtils.CombinePath(OriginalPath, "hvc") },
                { SystemModel.Shonen,FileUtils.CombinePath(OriginalPath, "hvcj") },
                { SystemModel.SuperFamicom, FileUtils.CombinePath(OriginalPath, "shvc") },
                { SystemModel.Nes, FileUtils.CombinePath(OriginalPath, "nes") },
                { SystemModel.SnesPal, FileUtils.CombinePath(OriginalPath, "snes-eur") }
            };
            return systemPaths.TryGetValue(systemModel, out var path)
                ? path
                : FileUtils.CombinePath(OriginalPath, "snes-usa");
        }

        /// <summary>
        /// Checks if both decoded and recoded directories exist.
        /// </summary>
        public static bool AreDecodedAndRecodedDirsPresent() =>
            FileUtils.SafeDirectoryExists(DecodedPath) && FileUtils.SafeDirectoryExists(RecodedPath);

        #endregion 
    }
}
