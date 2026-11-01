using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace SNESMiniLuaCompiler.Helpers
{
    public static class ProcessUtils
    {
        #region Fields and Properties

        public static string LuaJitPath => AppUtils.LuaJitPath;

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
                string outDir = Path.Combine(Path.GetTempPath(), "SNESMiniLuaCompiler", "dec", Guid.NewGuid().ToString("N")[..8]);
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
                    Task<string>? stdOutTask = process?.StandardOutput.ReadToEndAsync();
                    string? stdErr = process?.StandardError.ReadToEnd();
                    process?.WaitForExit();
                    string details = $"exit code {process?.ExitCode}, stdout: {stdOutTask?.Result}, stderr: {stdErr}";
                    Debug.WriteLine("Decompiler: " + details);

                    // With -s a file that fails to decompile is skipped and the exit code is still 0,
                    // so an empty output folder is the only failure signal
                    string[] produced = Directory.GetFiles(outDir, "*", SearchOption.AllDirectories);
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
                string exePath = AppUtils.LuaJitExe;
                if (!FileUtils.SafeFileExists(exePath))
                    throw new FileNotFoundException("luajit not found in LuaJitPath.", exePath);

                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = $"-b \"{decodeFile}\" \"{encodeFile}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                // "-b" loads jit/bcsave.lua with require, so lib/luajit has to be on the module path
                startInfo.EnvironmentVariables["LUA_PATH"] = Path.Combine(LuaJitPath, "?.lua");

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
    }
}
