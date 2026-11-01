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
    /// Reads the console's Lua files (only those: no images, fonts or sounds) from a firmware file
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

        /// <summary>Reads the Lua files of <paramref name="uiPath"/> (a folder of the firmware) into <paramref name="destination"/>.</summary>
        public static void ExtractFromFirmwareFile(
            string sourcePath,
            string uiPath,
            string destination,
            ImportResult result,
            CancellationToken cancellationToken)
        {
            string prefix = uiPath.Trim('/') + "/";
            switch (Detect(sourcePath))
            {
                case FirmwareSourceFormat.SquashFs:
                    ExtractFromSquashFs(sourcePath, prefix, destination, result, cancellationToken);
                    break;
                case FirmwareSourceFormat.Tar:
                    using (FileStream tar = File.OpenRead(sourcePath))
                        ExtractFromTar(tar, prefix, destination, result, cancellationToken);
                    break;
                case FirmwareSourceFormat.TarGz:
                    using (FileStream gz = File.OpenRead(sourcePath))
                    using (var tar = new GZipStream(gz, CompressionMode.Decompress))
                        ExtractFromTar(tar, prefix, destination, result, cancellationToken);
                    break;
                default:
                    throw new InvalidDataException("This is not a firmware file the app can read (a squashfs / .hsqs image, a .tar or a .tar.gz).");
            }
        }

        /// <summary>Reads a tar stream; <paramref name="pathPrefix"/> is the UI folder in it ("" when entries are relative to it).</summary>
        public static void ExtractFromTar(
            Stream tarStream,
            string pathPrefix,
            string destination,
            ImportResult result,
            CancellationToken cancellationToken)
        {
            using var reader = new TarReader(tarStream, leaveOpen: true);
            while (reader.GetNextEntry() is { } entry)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (entry.EntryType is not (TarEntryType.RegularFile or TarEntryType.V7RegularFile) || entry.DataStream is null)
                    continue;

                string name = entry.Name.Replace('\\', '/').TrimStart('.', '/');
                if (!name.StartsWith(pathPrefix, StringComparison.Ordinal))
                    continue;

                WriteLua(destination, name[pathPrefix.Length..], entry.DataStream, result);
            }
        }

        /// <summary>
        /// Reads the Lua files of a folder the user picked: the console's resources folder, the folder that
        /// contains it, or anything above that (a folder with several consoles: the one named like this console).
        /// </summary>
        public static void CopyFromFolder(
            string folder,
            ConsoleLayout layout,
            string destination,
            ImportResult result,
            CancellationToken cancellationToken)
        {
            string root = FindResourcesRoot(folder, layout);

            foreach (string file in Directory.EnumerateFiles(root, "*.lua", SearchOption.AllDirectories))
            {
                cancellationToken.ThrowIfCancellationRequested();
                using FileStream input = File.OpenRead(file);
                WriteLuaRelativeToResources(destination, Path.GetRelativePath(root, file).Replace('\\', '/'), input, result);
            }
        }

        private static string FindResourcesRoot(string folder, ConsoleLayout layout)
        {
            // the folder itself, or its own resources folder, is the answer: whatever else is inside is not looked at
            if (Directory.Exists(Path.Combine(folder, "scripts")))
                return folder;

            string ownResources = Path.Combine(folder, "resources");
            if (Directory.Exists(Path.Combine(ownResources, "scripts")))
                return ownResources;

            // otherwise every resources folder with scripts below it
            var roots = new List<string>();
            roots.AddRange(Directory.EnumerateDirectories(folder, "resources", SearchOption.AllDirectories)
                    .Where(directory => Directory.Exists(Path.Combine(directory, "scripts"))));

            if (roots.Count == 0)
                throw new InvalidDataException("No folder with a 'scripts' folder inside was found there. Pick the console's UI folder (the one that has 'resources').");

            if (roots.Count == 1)
                return roots[0];

            // several consoles in one folder (a "base UI" folder): the one that is named like this console
            string[] named = [.. roots.Where(root => layout.MatchesFolderName(ConsoleFolderName(root)))];
            if (named.Length == 1)
                return named[0];

            string consoles = string.Join(", ", roots.Select(ConsoleFolderName).Order());
            throw new InvalidDataException(named.Length == 0
                ? $"That folder has several consoles ({consoles}) and none is named like this one. Pick the folder of your console."
                : $"That folder has more than one folder named like this console ({consoles}). Pick the folder of your console.");
        }

        // .../snesusa/resources -> snesusa;  .../snesusa (the resources are its scripts) -> snesusa
        private static string ConsoleFolderName(string root)
        {
            string name = Path.GetFileName(Path.TrimEndingDirectorySeparator(root));
            return name.Equals("resources", StringComparison.OrdinalIgnoreCase)
                ? Path.GetFileName(Path.GetDirectoryName(Path.TrimEndingDirectorySeparator(root)) ?? string.Empty)
                : name;
        }

        private static void ExtractFromSquashFs(
            string sourcePath,
            string prefix,
            string destination,
            ImportResult result,
            CancellationToken cancellationToken)
        {
            using FileStream stream = File.OpenRead(sourcePath);
            using var fileSystem = new SquashFileSystemReader(stream, SquashFsOptions);
            string root = prefix.TrimEnd('/').Replace('/', Path.DirectorySeparatorChar);
            if (!fileSystem.DirectoryExists(root))
                throw new InvalidDataException($"The firmware has no '{prefix.TrimEnd('/')}' folder: it is not the firmware of this console.");

            foreach (string file in fileSystem.GetFiles(root, "*.*", SearchOption.AllDirectories))
            {
                cancellationToken.ThrowIfCancellationRequested();
                string relative = file.Replace('\\', '/').TrimStart('/');
                if (!relative.StartsWith(prefix, StringComparison.Ordinal))
                    continue;

                using SparseStream input = fileSystem.OpenFile(file, FileMode.Open);
                WriteLua(destination, relative[prefix.Length..], input, result);
            }
        }

        // pathInUi is relative to the UI folder: resources/scripts/main.lua
        private static void WriteLua(string destination, string pathInUi, Stream content, ImportResult result)
        {
            if (!pathInUi.StartsWith(ResourcesPrefix, StringComparison.Ordinal))
                return;

            WriteLuaRelativeToResources(destination, pathInUi[ResourcesPrefix.Length..], content, result);
        }

        // pathInResources: scripts/main.lua
        private static void WriteLuaRelativeToResources(string destination, string pathInResources, Stream content, ImportResult result)
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
                result.NotBytecode++;
                return;
            }

            string fullRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(destination)) + Path.DirectorySeparatorChar;
            string target = Path.GetFullPath(Path.Combine(fullRoot, pathInResources));
            if (!target.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Unsafe path in the source: {pathInResources}");

            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.WriteAllBytes(target, bytes);
            result.Files++;
        }
    }
}
