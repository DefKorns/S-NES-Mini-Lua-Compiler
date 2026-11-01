using System;
using System.IO;
using System.Linq;
using SNESMiniLuaCompiler.Models;

namespace SNESMiniLuaCompiler.Firmware
{
    /// <summary>
    /// The original Lua files of each console, imported by the user from their own console or firmware backup.
    /// One folder per console, laid out like the console's resources folder (scripts/, animation/).
    /// </summary>
    internal static class OriginalsStore
    {
        public static string Root { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SNESMiniLuaCompiler",
            "Originals");

        public static string PathOf(SystemModel system) => Path.Combine(Root, ConsoleLayout.For(system).FolderName);

        public static bool IsImported(SystemModel system)
        {
            string path = PathOf(system);
            return Directory.Exists(path) && Directory.EnumerateFiles(path, "*.lua", SearchOption.AllDirectories).Any();
        }

        public static string NewStagingPath() => Path.Combine(Root, ".staging-" + Guid.NewGuid().ToString("N")[..8]);

        /// <summary>Removes the staging folder of an import that did not finish, and the store folder if nothing is left in it.</summary>
        public static void CleanUp(string stagingPath)
        {
            if (Directory.Exists(stagingPath))
                Directory.Delete(stagingPath, true);

            if (Directory.Exists(Root) && !Directory.EnumerateFileSystemEntries(Root).Any())
                Directory.Delete(Root);
        }

        /// <summary>Makes <paramref name="stagingPath"/> the originals of the console, replacing what was there.</summary>
        public static void Replace(SystemModel system, string stagingPath)
        {
            string target = PathOf(system);
            if (Directory.Exists(target))
                Directory.Delete(target, true);

            Directory.CreateDirectory(Root);
            Directory.Move(stagingPath, target);
        }
    }
}
