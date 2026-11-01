namespace SNESMiniLuaCompiler.Firmware
{
    /// <summary>What an import found: the Lua files it kept and the ones it skipped because they are not LuaJIT bytecode.</summary>
    internal sealed class ImportResult
    {
        public int Files { get; set; }

        public int NotBytecode { get; set; }
    }
}
