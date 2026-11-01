using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SNESMiniLuaCompiler.Models;

namespace SNESMiniLuaCompiler.Firmware
{
    /// <summary>
    /// Imports the original Lua files of a console: from the console over SSH, from a firmware file
    /// (squashfs / .hsqs, tar, tar.gz) or from a folder. Only the Lua files are read.
    /// </summary>
    internal static class OriginalsImporter
    {
        public static Task<ImportResult> FromFolderAsync(SystemModel system, string folder, CancellationToken cancellationToken = default) =>
            ImportAsync(
                system,
                (staging, result) => Task.Run(() => LuaSourceReader.CopyFromFolder(folder, ConsoleLayout.For(system), staging, result, cancellationToken), cancellationToken));

        public static Task<ImportResult> FromFirmwareFileAsync(SystemModel system, string file, CancellationToken cancellationToken = default) =>
            ImportAsync(
                system,
                (staging, result) => Task.Run(
                    () => LuaSourceReader.ExtractFromFirmwareFile(file, ConsoleLayout.For(system).UiPath, staging, result, cancellationToken),
                    cancellationToken));

        public static Task<ImportResult> FromConsoleAsync(SystemModel system, string host, CancellationToken cancellationToken = default) =>
            ImportAsync(
                system,
                (staging, result) => ConsoleSsh.DownloadLuaAsync(host, ConsoleLayout.For(system), staging, result, cancellationToken));

        // everything is read into a staging folder first: a failed import never touches the originals already there
        private static async Task<ImportResult> ImportAsync(SystemModel system, Func<string, ImportResult, Task> read)
        {
            string staging = OriginalsStore.NewStagingPath();
            var result = new ImportResult();
            try
            {
                Directory.CreateDirectory(staging);
                await read(staging, result).ConfigureAwait(false);

                if (!File.Exists(Path.Combine(staging, "scripts", "main.lua")))
                    throw new InvalidDataException(Describe(result));

                CheckIsThisConsole(ConsoleLayout.For(system), staging);

                OriginalsStore.Replace(system, staging);
                return result;
            }
            finally
            {
                OriginalsStore.CleanUp(staging);
            }
        }

        // the files of a NES are not those of a SNES: a file only one family has tells them apart
        private static void CheckIsThisConsole(ConsoleLayout layout, string staging)
        {
            if (!File.Exists(Path.Combine(staging, layout.RequiredFile)))
                throw new InvalidDataException($"These are not the files of this console ({layout.RequiredFile} is missing): they are from another console.");

            if (layout.ForbiddenFile is not null && File.Exists(Path.Combine(staging, layout.ForbiddenFile)))
                throw new InvalidDataException($"These are not the files of this console ({layout.ForbiddenFile} belongs to the Shonen Jump edition).");
        }

        private static string Describe(ImportResult result) => result.NotBytecode > 0
            ? $"Those Lua files are plain text ({result.NotBytecode} of them), not the console's compiled files. Pick the original files, not a decoded theme."
            : "No Lua files of a console were found there (scripts/main.lua is missing).";
    }
}
