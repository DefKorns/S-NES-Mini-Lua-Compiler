using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace SNESMiniLuaCompiler.Utils
{
    public class AppUtils
    {
        #region Paths and Constants

        public static readonly string AppPath = Path.GetDirectoryName(Application.ExecutablePath) ?? string.Empty;
        public static readonly string LibPath = FileUtils.CombinePath(AppPath, "lib");
        public static readonly string DecodedPath = FileUtils.CombinePath(AppPath, "decoded");
        public static readonly string RecodedPath = FileUtils.CombinePath(AppPath, "recoded");
        public static readonly string OriginalPath = FileUtils.CombinePath("lib", "original");
        public static readonly string LuaJitPath = FileUtils.CombinePath(AppPath, "lib", "luajit");
        public static readonly string DecompilerPath = FileUtils.CombinePath("lib", "decompiler");
        public static readonly string DecompilerScript = FileUtils.CombinePath(DecompilerPath, "main.py");
        public static readonly string FirstRun = FileUtils.CombinePath("lib", "firstRun");

        #endregion

        #region Version

        /// <summary>
        /// Gets the application version as a string.
        /// </summary>
        public static string GetAppVersion() =>
            Assembly.GetExecutingAssembly()?.GetName().Version?.ToString() ?? "Unknown";

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

        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Extracts all required resources in a single call.
        /// </summary>
        public static void ExtractAllResources()
        {
            ExtractResourcesWithPrefix("SNESMiniLuaCompiler.lib.luajit.", LuaJitPath);
            ExtractResourcesWithPrefix("SNESMiniLuaCompiler.lib.decompiler.", DecompilerPath);
        }

        public static void ExtractResourceToFile(string resourceName, string outputPath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var resourceStream = assembly?.GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null)
                    throw new InvalidOperationException($"Resource '{resourceName}' not found.");
                FileUtils.CreatePath(Path.GetDirectoryName(outputPath) ?? string.Empty);
                using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    resourceStream.CopyTo(fileStream);
                }
            }
        }

        public static void ExtractResourcesWithPrefix(string resourcePrefix, string targetRoot)
        {
            ExceptionUtils.ThrowArgNull(resourcePrefix, nameof(resourcePrefix));
            ExceptionUtils.ThrowArgNull(targetRoot, nameof(targetRoot));

            var assembly = Assembly.GetExecutingAssembly();
            foreach (var resourceName in assembly?.GetManifestResourceNames() ?? Array.Empty<string>())
            {
                if (!resourceName.StartsWith(resourcePrefix, StringComparison.Ordinal))
                    continue;

                string relative = resourceName.Substring(resourcePrefix.Length);
                string[] parts = relative.Split('.');
                if (parts.Length < 2)
                    continue;

                string fileName = (parts[parts.Length - 2] ?? string.Empty) + "." + (parts[parts.Length - 1]?.Replace('_', '.') ?? string.Empty);
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

        /// <summary>
        /// Displays or hides a loading spinner.
        /// </summary>
        public static void LoadSpinner(bool displayLoader, PictureBox loaderImage, Form form)
        {
            ExceptionUtils.ThrowArgNull(loaderImage, nameof(loaderImage));
            ExceptionUtils.ThrowArgNull(form, nameof(form));

            form?.Invoke((MethodInvoker)delegate
            {
                if (loaderImage != null)
                    loaderImage.Visible = displayLoader;
                if (form != null)
                    form.Cursor = displayLoader ? Cursors.WaitCursor : Cursors.Default;
            });
        }

        /// <summary>
        /// Sets the enabled state, border color, border size, and mouse over color for a button.
        /// </summary>
        public static void SetButtonState(
            Button button,
            bool enabled,
            int borderSize = 0,
            Color? borderColor = null,
            Color? mouseOverBackColor = null)
        {
            if (button == null) return;
            button.Enabled = enabled;
            button.FlatAppearance.BorderSize = borderSize;
            if (borderColor.HasValue)
                button.FlatAppearance.BorderColor = borderColor.Value;
            if (mouseOverBackColor.HasValue)
                button.FlatAppearance.MouseOverBackColor = mouseOverBackColor.Value;
        }

        #endregion

        #region System Path and Directory Checks

        /// <summary>
        /// Gets the system path for a given button name.
        /// </summary>
        public static string GetSystemPath(string buttonName)
        {
            var systemPaths = new Dictionary<string, string>
            {
                { "famicomButton", FileUtils.CombinePath(OriginalPath, "hvc") },
                { "famicom50Button",FileUtils.CombinePath(OriginalPath, "hvcj") },
                { "sFamicomButton", FileUtils.CombinePath(OriginalPath, "shvc") },
                { "nesButton", FileUtils.CombinePath(OriginalPath, "nes") },
                { "snesPALButton", FileUtils.CombinePath(OriginalPath, "snes-eur") }
            };
            return systemPaths.TryGetValue(buttonName ?? string.Empty, out var path)
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
