using SNESMiniLuaCompiler.Models;

namespace SNESMiniLuaCompiler.Firmware
{
    /// <summary>One console an import tries to fill: where its files are collected, and what was found for it.</summary>
    internal sealed class ConsoleTarget(SystemModel system, ConsoleLayout layout)
    {
        public ConsoleTarget(SystemModel system)
            : this(system, ConsoleLayout.For(system))
        {
        }

        public SystemModel System { get; } = system;

        public ConsoleLayout Layout { get; } = layout;

        /// <summary>The folder the Lua files are collected in until the import is known to be good.</summary>
        public string Staging { get; } = OriginalsStore.NewStagingPath();

        public ImportResult Result { get; } = new();

        /// <summary>Did the source have files for this console?</summary>
        public bool Found { get; set; }

        /// <summary>Why not, when it matters (the console the user asked for).</summary>
        public string? NotFoundMessage { get; set; }
    }
}
