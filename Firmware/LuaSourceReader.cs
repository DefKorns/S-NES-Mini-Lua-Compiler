using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using DiscUtils.SquashFs;
using DiscUtils.Streams;
using SharpCompress.Compressors.Xz;
using SNESMiniLuaCompiler.Models;

namespace SNESMiniLuaCompiler.Firmware
{
    internal enum FirmwareSourceFormat
    {
        Unknown,
        SquashFs,
        Tar,
        TarGz,
    }

    /// <summary>
    /// Reads the consoles' Lua files (only those: no images, fonts or sounds) from a firmware file
    /// (squashfs / .hsqs, tar, tar.gz), a folder, or the tar stream of a console.
    /// </summary>
    internal static class LuaSourceReader
    {
        private const string ResourcesPrefix = "resources/";

        private static readonly SquashFileSystemReaderOptions SquashFsOptions = new()
        {
            GetDecompressor = (kind, _) => kind == SquashFileSystemCompressionKind.Xz
                ? compressed => new XZStream(compressed)
                : null!,
        };

        public static FirmwareSourceFormat Detect(string filePath)
        {
            using FileStream stream = File.OpenRead(filePath);
            byte[] header = new byte[262];
            int length = stream.ReadAtLeast(header, header.Length, throwOnEndOfStream: false);

            return header.AsSpan(0, length) switch
            {
                [0x68, 0x73, 0x71, 0x73, ..] => FirmwareSourceFormat.SquashFs,
                [0x1f, 0x8b, ..] => FirmwareSourceFormat.TarGz,
                var span when span.Length >= 262 && span[257..262].SequenceEqual("ustar"u8) => FirmwareSourceFormat.Tar,
                _ => FirmwareSourceFormat.Unknown,
            };
        }

        /// <summary>
        /// Reads the Lua files of every target's UI folder out of a firmware file, in one pass (a firmware can
        /// hold the UI of several consoles; a tar.gz is read once, not once per console).
        /// </summary>
        public static void ExtractFromFirmwareFile(string sourcePath, IReadOnlyList<ConsoleTarget> targets, CancellationToken cancellationToken)
        {
            foreach (ConsoleTarget target in targets)
                target.NotFoundMessage = $"The firmware has no '{target.Layout.UiPath}' folder: it is not the firmware of this console.";

            switch (Detect(sourcePath))
            {
                case FirmwareSourceFormat.SquashFs:
                    ExtractFromSquashFs(sourcePath, targets, cancellationToken);
                    break;
                case FirmwareSourceFormat.Tar:
                    using (FileStream tar = File.OpenRead(sourcePath))
                        ExtractFromTar(tar, targets, uiPathOverride: null, cancellationToken);
                    break;
                case FirmwareSourceFormat.TarGz:
                    using (FileStream gz = File.OpenRead(sourcePath))
                    using (var tar = new GZipStream(gz, CompressionMode.Decompress))
                        ExtractFromTar(tar, targets, uiPathOverride: null, cancellationToken);
                    break;
                default:
                    throw new InvalidDataException("This is not a firmware file the app can read (a squashfs / .hsqs image, a .tar or a .tar.gz).");
            }
        }

        /// <summary>Reads a tar stream whose entries are relative to the UI folder (what the SSH import receives).</summary>
        public static void ExtractFromTar(Stream tarStream, ConsoleTarget target, CancellationToken cancellationToken) =>
            ExtractFromTar(tarStream, [target], uiPathOverride: string.Empty, cancellationToken);

        /// <summary>
        /// The resources folders of a folder the user picked. The folder itself, or its own resources folder, is the
        /// only one when it has scripts in it; otherwise every resources folder with scripts below it (a "base UI"
        /// folder with the consoles side by side).
        /// </summary>
        public static IReadOnlyList<string> FindResourcesRoots(string folder)
        {
            if (Directory.Exists(Path.Combine(folder, "scripts")))
                return [folder];

            string ownResources = Path.Combine(folder, "resources");
            if (Directory.Exists(Path.Combine(ownResources, "scripts")))
                return [ownResources];

            return [.. Directory.EnumerateDirectories(folder, "resources", SearchOption.AllDirectories)
                .Where(directory => Directory.Exists(Path.Combine(directory, "scripts")))];
        }

        /// <summary>
        /// Reads the Lua files each target has in <paramref name="roots"/>. A single folder is the console the user
        /// asked for, whatever it is called; with several, each console takes the folder named like it.
        /// </summary>
        public static void CopyFromFolder(IReadOnlyList<string> roots, IReadOnlyList<ConsoleTarget> targets, SystemModel requested, CancellationToken cancellationToken)
        {
            string consoles = string.Join(", ", roots.Select(ConsoleFolderName).Order());
            foreach (ConsoleTarget target in targets)
            {
                if (roots.Count == 0)
                {
                    target.NotFoundMessage = "No folder with a 'scripts' folder inside was found there. Pick the console's UI folder (the one that has 'resources').";
                    continue;
                }

                if (roots.Count == 1)
                {
                    if (target.System == requested)
                        CopyFromRoot(roots[0], target, cancellationToken);

                    continue;
                }

                string[] named = [.. roots.Where(root => target.Layout.MatchesFolderName(ConsoleFolderName(root)))];
                if (named.Length == 1)
                    CopyFromRoot(named[0], target, cancellationToken);
                else
                    target.NotFoundMessage = named.Length == 0
                        ? $"That folder has several consoles ({consoles}) and none is named like this one. Pick the folder of your console."
                        : $"That folder has more than one folder named like this console ({consoles}). Pick the folder of your console.";
            }
        }

        private static void CopyFromRoot(string root, ConsoleTarget target, CancellationToken cancellationToken)
        {
            target.Found = true;
            foreach (string file in Directory.EnumerateFiles(root, "*.lua", SearchOption.AllDirectories))
            {
                cancellationToken.ThrowIfCancellationRequested();
                using FileStream input = File.OpenRead(file);
                WriteLuaRelativeToResources(target, Path.GetRelativePath(root, file).Replace('\\', '/'), input);
            }
        }

        // .../snesusa/resources -> snesusa;  .../snesusa (the resources are its scripts) -> snesusa
        private static string ConsoleFolderName(string root)
        {
            string name = Path.GetFileName(Path.TrimEndingDirectorySeparator(root));
            return name.Equals("resources", StringComparison.OrdinalIgnoreCase)
                ? Path.GetFileName(Path.GetDirectoryName(Path.TrimEndingDirectorySeparator(root)) ?? string.Empty)
                : name;
        }

        private static void ExtractFromSquashFs(string sourcePath, IReadOnlyList<ConsoleTarget> targets, CancellationToken cancellationToken)
        {
            using FileStream stream = File.OpenRead(sourcePath);
            using var fileSystem = new SquashFileSystemReader(stream, SquashFsOptions);

            foreach (ConsoleTarget target in targets)
            {
                string prefix = UiPrefix(target, null);
                string root = prefix.TrimEnd('/').Replace('/', Path.DirectorySeparatorChar);
                if (!fileSystem.DirectoryExists(root))
                    continue;

                target.Found = true;
                foreach (string file in fileSystem.GetFiles(root, "*.*", SearchOption.AllDirectories))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string relative = file.Replace('\\', '/').TrimStart('/');
                    if (!relative.StartsWith(prefix, StringComparison.Ordinal))
                        continue;

                    using SparseStream input = fileSystem.OpenFile(file, FileMode.Open);
                    WriteLua(target, relative[prefix.Length..], input);
                }
            }
        }

        // uiPathOverride: the UI folder of every target when it is the same (the SSH import sends entries relative to it)
        private static void ExtractFromTar(Stream tarStream, IReadOnlyList<ConsoleTarget> targets, string? uiPathOverride, CancellationToken cancellationToken)
        {
            using var reader = new TarReader(tarStream, leaveOpen: true);
            while (reader.GetNextEntry() is { } entry)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (entry.EntryType is not (TarEntryType.RegularFile or TarEntryType.V7RegularFile) || entry.DataStream is null)
                    continue;

                string name = entry.Name.Replace('\\', '/').TrimStart('.', '/');
                ConsoleTarget[] matching = [.. targets.Where(target => name.StartsWith(UiPrefix(target, uiPathOverride), StringComparison.Ordinal))];
                if (matching.Length == 0)
                    continue;

                foreach (ConsoleTarget target in matching)
                    target.Found = true;

                if (matching.Length == 1)
                {
                    WriteLua(matching[0], name[UiPrefix(matching[0], uiPathOverride).Length..], entry.DataStream);
                    continue;
                }

                // two consoles can share a UI folder: the entry is read once and given to both
                using var copy = new MemoryStream();
                entry.DataStream.CopyTo(copy);
                foreach (ConsoleTarget target in matching)
                {
                    copy.Position = 0;
                    WriteLua(target, name[UiPrefix(target, uiPathOverride).Length..], copy);
                }
            }
        }

        private static string UiPrefix(ConsoleTarget target, string? uiPathOverride) =>
            uiPathOverride is null ? target.Layout.UiPath.Trim('/') + "/" : uiPathOverride;

        // pathInUi is relative to the UI folder: resources/scripts/main.lua
        private static void WriteLua(ConsoleTarget target, string pathInUi, Stream content)
        {
            if (!pathInUi.StartsWith(ResourcesPrefix, StringComparison.Ordinal))
                return;

            WriteLuaRelativeToResources(target, pathInUi[ResourcesPrefix.Length..], content);
        }

        // pathInResources: scripts/main.lua
        private static void WriteLuaRelativeToResources(ConsoleTarget target, string pathInResources, Stream content)
        {
            if (!pathInResources.EndsWith(".lua", StringComparison.OrdinalIgnoreCase)
                || pathInResources.Split('/').Any(segment => segment.StartsWith('.')))
                return;

            // the console's files are LuaJIT bytecode; a text file here is something else (a decoded theme, a script)
            using var buffer = new MemoryStream();
            content.CopyTo(buffer);
            byte[] bytes = buffer.ToArray();
            if (bytes.Length < 3 || bytes[0] != 0x1B || bytes[1] != (byte)'L' || bytes[2] != (byte)'J')
            {
                target.Result.NotBytecode++;
                return;
            }

            string fullRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(target.Staging)) + Path.DirectorySeparatorChar;
            string path = Path.GetFullPath(Path.Combine(fullRoot, pathInResources));
            if (!path.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Unsafe path in the source: {pathInResources}");

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, bytes);
            target.Result.Files++;
        }
    }
}
