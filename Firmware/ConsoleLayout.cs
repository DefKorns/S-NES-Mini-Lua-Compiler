using System.Collections.Generic;
using SNESMiniLuaCompiler.Models;

namespace SNESMiniLuaCompiler.Firmware
{
    /// <summary>
    /// Where a console keeps its Lua files inside its firmware, and the folder the app uses for that console.
    /// The paths are the ones of the original firmware images (the same registry the MiniThemeStudio uses).
    /// </summary>
    internal sealed record ConsoleLayout(string FolderName, string UiPath)
    {
        public static ConsoleLayout For(SystemModel system) => system switch
        {
            SystemModel.Nes => new("nes", "usr/share/clover-ui"),
            SystemModel.Famicom => new("hvc", "usr/share/clover-ui"),
            SystemModel.Shonen => new("hvcj", "usr/share/ui/hvc"),
            SystemModel.SuperFamicom => new("shvc", "usr/share/ui/shvc"),
            SystemModel.SnesPal => new("snes-eur", "usr/share/ui/snes-eur"),
            _ => new("snes-usa", "usr/share/ui/snes-usa"),
        };

        /// <summary>
        /// Folders of a console where the UI can be read: the untouched original first, then the live one
        /// (with hakchi, an installed theme is mounted over the live folder).
        /// </summary>
        public IReadOnlyList<string> ConsolePaths => [$"/var/squashfs/{UiPath}", $"/{UiPath}"];
    }
}
