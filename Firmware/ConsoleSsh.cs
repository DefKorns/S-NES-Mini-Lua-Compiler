using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SNESMiniLuaCompiler.Firmware
{
    /// <summary>Reads the Lua files of a console over SSH (hakchi), with the ssh client of the system and tar on the console.</summary>
    internal static class ConsoleSsh
    {
        /// <summary>The address of a console connected by USB (hakchi).</summary>
        public const string UsbHost = "169.254.13.37";

        public static bool IsValidHost(string host) =>
            host.Length is > 0 and <= 253 && !host.StartsWith('-') && host.All(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '-' or ':' or '_');

        public static async Task DownloadLuaAsync(
            string host,
            ConsoleLayout layout,
            string destination,
            ImportResult result,
            CancellationToken cancellationToken)
        {
            if (!IsValidHost(host))
                throw new ArgumentException("That is not a valid address.", nameof(host));

            string? folder = null;
            foreach (string candidate in layout.ConsolePaths)
            {
                if (await RunAsync(host, $"test -d {Quote(candidate)}", cancellationToken) == 0)
                {
                    folder = candidate;
                    break;
                }
            }

            if (folder is null)
                throw new InvalidDataException($"The console at {host} has no {layout.UiPath} folder. Is it the console you picked?");

            // only the Lua files travel: the UI folder also holds the images, fonts and sounds
            using Process process = Start(host, $"cd {Quote(folder)} && tar -cf - $(find resources -type f -name '*.lua')");
            Task<string> error = process.StandardError.ReadToEndAsync(cancellationToken);
            await Task.Run(() => LuaSourceReader.ExtractFromTar(process.StandardOutput.BaseStream, string.Empty, destination, result, cancellationToken), cancellationToken);

            // tar pads the archive after its end marker: read it, or ssh blocks writing it and never exits
            await process.StandardOutput.BaseStream.CopyToAsync(Stream.Null, cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
                throw new IOException($"The console did not send the files (ssh exit code {process.ExitCode}): {(await error).Trim()}");
        }

        private static async Task<int> RunAsync(string host, string command, CancellationToken cancellationToken)
        {
            using Process process = Start(host, command);
            Task<string> output = process.StandardOutput.ReadToEndAsync(cancellationToken);
            Task<string> error = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            await Task.WhenAll(output, error);

            // 255 is ssh itself failing: no route to the host, a refused login...
            if (process.ExitCode == 255)
                throw new IOException($"Could not connect to {host}: {error.Result.Trim()}");

            return process.ExitCode;
        }

        private static Process Start(string host, string command)
        {
            var startInfo = new ProcessStartInfo("ssh")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            foreach (string argument in new[] { "-o", "BatchMode=yes", "-o", "ConnectTimeout=5", "-o", "StrictHostKeyChecking=accept-new", "-p", "22", $"root@{host}", command })
                startInfo.ArgumentList.Add(argument);

            try
            {
                return Process.Start(startInfo) ?? throw new InvalidOperationException("Could not start the ssh client.");
            }
            catch (Win32Exception exception)
            {
                throw new InvalidOperationException(
                    "The OpenSSH client (ssh) was not found. On Windows it is an optional feature (Settings > Apps > Optional features > OpenSSH Client); on Linux install openssh-client.",
                    exception);
            }
        }

        private static string Quote(string value) => $"'{value.Replace("'", "'\"'\"'", StringComparison.Ordinal)}'";
    }
}
