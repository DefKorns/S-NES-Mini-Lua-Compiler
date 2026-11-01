using System;
using System.Collections.Generic;
using System.Linq;
using SNESMiniLuaCompiler.Models;

namespace SNESMiniLuaCompiler.Firmware
{
    /// <summary>
    /// Where a console keeps its Lua files inside its firmware, the folder the app uses for that console, and how
    /// to recognise its files. The firmware paths are the ones of the original firmware images (the same registry
    /// the MiniThemeStudio uses).
    /// </summary>
    /// <param name="DisplayName">The name of the console.</param>
    /// <param name="FolderName">The folder of the console in the app.</param>
    /// <param name="UiPath">The UI folder in the firmware.</param>
    /// <param name="FolderAliases">Names a folder of this console is known by (normalised: lower case, no - _ or space).</param>
    /// <param name="RequiredFile">A file only this family of consoles has, relative to the resources folder.</param>
    /// <param name="ForbiddenFile">A file that shows the files are of a sister console, relative to the resources folder.</param>
    internal sealed record ConsoleLayout(
        string DisplayName,
        string FolderName,
        string UiPath,
        IReadOnlyList<string> FolderAliases,
        string RequiredFile,
        string? ForbiddenFile = null)
    {
        private const string SnesPalette = "scripts/env/colors_usa.lua";

        public static IReadOnlyList<SystemModel> All { get; } = Enum.GetValues<SystemModel>();

        public static ConsoleLayout For(SystemModel system) => system switch
        {
            SystemModel.Nes => new("NES Classic Edition", "nes", "usr/share/clover-ui", ["nes"], "scripts/colors_nes.lua"),
            SystemModel.Famicom => new("Famicom Mini", "hvc", "usr/share/clover-ui", ["hvc", "famicom"], "scripts/colors_hvc.lua", "scripts/constants.lua"),
            SystemModel.Shonen => new("Famicom Shonen Jump 50th", "hvcj", "usr/share/ui/hvc", ["hvcj", "hvcjshonen", "shonen"], "scripts/constants.lua"),
            SystemModel.SuperFamicom => new("Super Famicom", "shvc", "usr/share/ui/shvc", ["shvc", "superfamicom"], SnesPalette),
            SystemModel.SnesPal => new("SNES Mini (Europe)", "snes-eur", "usr/share/ui/snes-eur", ["sneseur"], SnesPalette),
            _ => new("SNES Mini (USA)", "snes-usa", "usr/share/ui/snes-usa", ["snesusa", "snes"], SnesPalette),
        };

        /// <summary>
        /// Folders of a console where the UI can be read: the untouched original first, then the live one
        /// (with hakchi, an installed theme is mounted over the live folder).
        /// </summary>
        public IReadOnlyList<string> ConsolePaths => [$"/var/squashfs/{UiPath}", $"/{UiPath}"];

        /// <summary>Is <paramref name="folderName"/> the name of a folder of this console (hvc, snesusa, snes-usa...)?</summary>
        public bool MatchesFolderName(string folderName) =>
            FolderAliases.Contains(Normalize(folderName));

        private static string Normalize(string name) =>
            new string(name.Where(c => c is not ('-' or '_' or ' ')).ToArray()).ToLowerInvariant();
    }
}
