using System;
using System.Diagnostics;
using System.IO;

namespace SNESMiniLuaCompiler.Helpers
{
    public class ProcessUtils
    {
        #region Fields and Properties

        private const ProcessWindowStyle Hidden = ProcessWindowStyle.Hidden;
        //public static string LuaJitPath { get; set; } = Path.Combine(AppUtils.LibPath, "luajit");
        public static string LuaJitPath => AppUtils.LuaJitPath;

        #endregion

        #region Executable Path Utilities

        public static string FindExePath(string exe)
        {
            exe = Environment.ExpandEnvironmentVariables(exe ?? string.Empty);

            if (FileUtils.SafeFileExists(exe)) return exe;

            if (string.IsNullOrEmpty(Path.GetDirectoryName(exe)))
            {
                //foreach (var path in (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(';'))
                foreach (var path in (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
                     .Split(Path.PathSeparator))
                {
                    var fullPath = Path.Combine(path.Trim(), exe ?? string.Empty);
                    if (FileUtils.SafeFileExists(fullPath)) return Path.GetFullPath(fullPath);
                }
            }

            return string.Empty;
        }

        #endregion

        #region Process Execution

        public static void RunDecompiler(string file, string decFile)
        {
            ExceptionUtils.GlobalTryCatch(() =>
            {
                if (!FileUtils.SafeFileExists(AppUtils.DecompilerExe))
                    throw new FileNotFoundException("luajit-decompiler-v2 not found.", AppUtils.DecompilerExe);

                // The decompiler writes <input name>.lua into the output folder, so use a private one per file.
                // Keep it short: the Windows build uses ANSI file APIs and fails above MAX_PATH (260 characters),
                // and it skips such files silently (-s) while still exiting with 0.
                var outDir = Path.Combine(Path.GetTempPath(), "SNESMiniLuaCompiler", "dec", Guid.NewGuid().ToString("N")[..8]);
                Directory.CreateDirectory(outDir);
                try
                {
                    var start = new ProcessStartInfo
                    {
                        FileName = AppUtils.DecompilerExe,
                        Arguments = $"\"{file}\" -o \"{outDir}\" -s -f",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    using var process = Process.Start(start);
                    var stdOutTask = process?.StandardOutput.ReadToEndAsync();
                    var stdErr = process?.StandardError.ReadToEnd();
                    process?.WaitForExit();
                    var details = $"exit code {process?.ExitCode}, stdout: {stdOutTask?.Result}, stderr: {stdErr}";
                    Debug.WriteLine("Decompiler: " + details);

                    // With -s a file that fails to decompile is skipped and the exit code is still 0,
                    // so an empty output folder is the only failure signal
                    var produced = Directory.GetFiles(outDir, "*", SearchOption.AllDirectories);
                    if (produced.Length == 0)
                        throw new InvalidOperationException($"Failed to decompile '{file}' ({details}).");
                    File.Move(produced[0], decFile, true);
                }
                finally
                {
                    Directory.Delete(outDir, true);
                }
            },
            "Error running decompiler.",
            "ProcessUtils.RunDecompiler");
        }

        public static void RunLuaJit(string decodeFile, string encodeFile)
        {
            ExceptionUtils.GlobalTryCatch(() =>
            {
                var exePath = AppUtils.LuaJitExe;
                if (!FileUtils.SafeFileExists(exePath))
                    throw new FileNotFoundException("luajit not found in LuaJitPath.", exePath);

                var startInfo = new ProcessStartInfo
                {
                    WorkingDirectory = LuaJitPath,
                    FileName = exePath,
                    Arguments = $"-b \"{decodeFile}\" \"{encodeFile}\"",
                    WindowStyle = Hidden,
                    CreateNoWindow = false,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                var libExtension = OperatingSystem.IsWindows() ? "dll" : "so";
                startInfo.EnvironmentVariables["LUA_CPATH"] = $"?.{libExtension};" + Path.Combine(LuaJitPath, $"?.{libExtension}");
                startInfo.EnvironmentVariables["LUA_PATH"] =
                    "?.lua;" +
                    Path.Combine(LuaJitPath, "jit", "?.lua") + ";" +
                    Path.Combine(LuaJitPath, "?.lua");

                string currentPath = startInfo.EnvironmentVariables["PATH"] ?? Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
                startInfo.EnvironmentVariables["PATH"] = LuaJitPath + Path.PathSeparator + currentPath;

                using var process = Process.Start(startInfo);
                string stdOut = process?.StandardOutput?.ReadToEnd() ?? string.Empty;
                string stdErr = process?.StandardError?.ReadToEnd() ?? string.Empty;
                process?.WaitForExit();

                Debug.WriteLine("LuaJIT stdout: " + (stdOut ?? string.Empty));
                Debug.WriteLine("LuaJIT stderr: " + (stdErr ?? string.Empty));
            },
            "Error running LuaJIT.",
            "ProcessUtils.RunLuaJit");
        }

        #endregion

        #region Utility Methods

        public static void OpenDirectory(string path)
        {
            if (!FileUtils.SafeDirectoryExists(path)) return;

            Process.Start(new ProcessStartInfo
            {
                FileName = OperatingSystem.IsWindows() ? "explorer.exe" : "xdg-open",
                Arguments = $"\"{path}\"",
                UseShellExecute = false
            });
        }


        #endregion
    }
}

