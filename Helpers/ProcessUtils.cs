using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace SNESMiniLuaCompiler.Helpers
{
    public partial class ProcessUtils
    {
        #region Fields and Properties

        private const ProcessWindowStyle Hidden = ProcessWindowStyle.Hidden;
        public static string LuaJitPath { get; set; } = Path.Combine(AppUtils.LibPath, "luajit");

        #endregion

        #region Executable Path Utilities

        public static string FindExePath(string exe)
        {
            exe = Environment.ExpandEnvironmentVariables(exe ?? string.Empty);

            if (FileUtils.SafeFileExists(exe)) return exe;

            if (string.IsNullOrEmpty(Path.GetDirectoryName(exe)))
            {
                foreach (var path in (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(';'))
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
                var exePath = FindExePath("pythonw.exe");
                var start = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = AppUtils.DecompilerScript + " --file " + file + " --output " + decFile + " --catch_asserts",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };

                using var process = Process.Start(start);
                using var reader = process?.StandardOutput;
                var result = reader?.ReadToEnd();
            },
            "Error running command.",
            "ProcessUtils.RunCmd");
        }

        public static void RunLuaJit(string decodeFile, string encodeFile)
        {
            ExceptionUtils.GlobalTryCatch(() =>
            {
                var exePath = Path.Combine(LuaJitPath, "luajit.exe");
                if (!FileUtils.SafeFileExists(exePath))
                    throw new FileNotFoundException("luajit.exe not found in LuaJitPath.", exePath);

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

                startInfo.EnvironmentVariables["LUA_CPATH"] = "?.dll;" + Path.Combine(LuaJitPath, "?.dll");
                startInfo.EnvironmentVariables["LUA_PATH"] =
                    "?.lua;" +
                    Path.Combine(LuaJitPath, "jit", "?.lua") + ";" +
                    Path.Combine(LuaJitPath, "?.lua");

                string currentPath = startInfo.EnvironmentVariables["PATH"] ?? Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
                startInfo.EnvironmentVariables["PATH"] = LuaJitPath + ";" + currentPath;

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
            if (FileUtils.SafeDirectoryExists(path))
                Process.Start("explorer.exe", $"\"{path}\"");
        }

        public static bool PythonVersion()
        {
            bool resultValue = false;
            ExceptionUtils.GlobalTryCatch(() =>
            {
                string result = "";

                ProcessStartInfo pycheck = new ()
                {
                    FileName = @"python.exe",
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(pycheck) ?? throw new InvalidOperationException("Failed to start python.exe process.");
                using var reader = process.StandardOutput;
                result = reader.ReadToEnd();

                if (string.IsNullOrWhiteSpace(result))
                {
                    using StreamReader errorReader = process.StandardError;
                    result = errorReader.ReadToEnd();
                }

                var match = PythonRegex().Match(result);
                if (match.Success && int.TryParse(match.Groups[1].Value, out int majorVersion))
                {
                    resultValue = majorVersion == 3;
                }
                else
                {
                    resultValue = false;
                }
            },
            "Unexpected error while checking Python version.",
            "MainForm.PythonVersion");
            return resultValue;
        }

        [GeneratedRegex(@"Python (\d+)\.")]
        private static partial Regex PythonRegex();

        #endregion
    }
}

