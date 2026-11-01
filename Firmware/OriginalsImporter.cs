using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SNESMiniLuaCompiler.Models;

namespace SNESMiniLuaCompiler.Firmware
{
    /// <summary>A console whose original files were imported, and how many Lua files that was.</summary>
    internal sealed record ImportedConsole(SystemModel System, ImportResult Result);

    /// <summary>
    /// Imports the original Lua files of the consoles: from a console over SSH, from a firmware file
    /// (squashfs / .hsqs, tar, tar.gz) or from a folder. Only the Lua files are read.
    /// A firmware file or a folder can hold several consoles: every one the source has is imported and kept,
    /// so the app does not ask for it again; the console the user asked for has to be one of them.
    /// </summary>
    internal static class OriginalsImporter
    {
        public static Task<IReadOnlyList<ImportedConsole>> FromFolderAsync(SystemModel requested, string folder, CancellationToken cancellationToken = default) =>
            ImportAsync(
                requested,
                ConsoleLayout.All,
                targets => Task.Run(
                    () => LuaSourceReader.CopyFromFolder(LuaSourceReader.FindResourcesRoots(folder), targets, requested, cancellationToken),
                    cancellationToken));

        public static Task<IReadOnlyList<ImportedConsole>> FromFirmwareFileAsync(SystemModel requested, string file, CancellationToken cancellationToken = default) =>
            ImportAsync(
                requested,
                ConsoleLayout.All,
                targets => Task.Run(() => LuaSourceReader.ExtractFromFirmwareFile(file, targets, cancellationToken), cancellationToken));

        public static Task<IReadOnlyList<ImportedConsole>> FromConsoleAsync(SystemModel requested, string host, CancellationToken cancellationToken = default) =>
            ImportAsync(
                requested,
                [requested],
                targets => ConsoleSsh.DownloadLuaAsync(host, targets[0], cancellationToken));

        // everything is read into staging folders first: a failed import never touches the originals already there
        private static async Task<IReadOnlyList<ImportedConsole>> ImportAsync(
            SystemModel requested,
            IReadOnlyList<SystemModel> consoles,
            Func<IReadOnlyList<ConsoleTarget>, Task> read)
        {
            List<ConsoleTarget> targets = [.. consoles.Select(console => new ConsoleTarget(console))];
            try
            {
                foreach (ConsoleTarget target in targets)
                    Directory.CreateDirectory(target.Staging);

                await read(targets).ConfigureAwait(false);

                var good = new List<ConsoleTarget>();
                foreach (ConsoleTarget target in targets)
                {
                    try
                    {
                        Validate(target);
                        good.Add(target);
                    }
                    catch (InvalidDataException) when (target.System != requested)
                    {
                        // the source has nothing (valid) for this console: only the one the user asked for has to be there
                    }
                }

                foreach (ConsoleTarget target in good)
                    OriginalsStore.Replace(target.System, target.Staging);

                return [.. good.Select(target => new ImportedConsole(target.System, target.Result))];
            }
            finally
            {
                foreach (ConsoleTarget target in targets)
                    OriginalsStore.CleanUp(target.Staging);
            }
        }

        private static void Validate(ConsoleTarget target)
        {
            if (!target.Found)
                throw new InvalidDataException(target.NotFoundMessage ?? "No Lua files of a console were found there.");

            if (!File.Exists(Path.Combine(target.Staging, "scripts", "main.lua")))
                throw new InvalidDataException(Describe(target.Result));

            CheckIsThisConsole(target.Layout, target.Staging);
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
