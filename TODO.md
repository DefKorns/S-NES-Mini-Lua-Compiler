# TODO

## Replace the Python decompiler with LuaJIT Decompiler v2

The app decompiles the console's Lua files with the old Python LJD (`Lib/decompiler`, run as
`pythonw main.py --file <in> --output <out> --catch_asserts`). That project is dead, its own README warns
the output may not match the original, and it makes users install Python.

Replace it with [LuaJIT Decompiler v2](https://github.com/marsinator358/luajit-decompiler-v2)
(marsinator358, MIT license). It is a single Windows executable from the releases page
(`luajit-decompiler-v2.exe`, latest `Mar_24_2024`) that, according to its README, fixes the bugs and quirks
of the Python decompiler and fully supports gotos and stripped bytecode, locals and upvalues included.

Steps:

- [x] Add `luajit-decompiler-v2.exe` under `Lib/decompiler` (or `Lib/decompiler-v2`) and remove the Python
      sources (`ljd/`, `main.py`), keeping its `LICENSE` next to the exe.
- [x] `Helpers/ProcessUtils.cs`: run the exe in `RunDecompiler` instead of `pythonw.exe`. Check the
      arguments with `luajit-decompiler-v2.exe -?`: by default it writes to an `output` folder next to
      the exe, so pass the output folder explicitly if it has an option for it, or move the files from
      there.
- [x] `Helpers/AppUtils.cs`: point `DecompilerPath` at the exe and drop `DecompilerScript`.
- [x] Remove the Python check (`ProcessUtils.PythonVersion` and the UI that asks for Python).
- [ ] Check that the build workflow (`.github/workflows/build.yml`) and the published package copy the exe.
- [x] README: drop the Python requirement and credit LuaJIT Decompiler v2.
- [ ] Keep the encoder as it is: `Lib/luajit/luajit.exe` must stay the LuaJIT version the consoles use
      (2.0.x), or the recompiled files will not load.

Test before releasing:

- [ ] Decode the Lua files of each console (NES, Famicom, SNES, Super Famicom, Shonen Jump) and compare
      a few of them with what the Python decompiler gave.
- [ ] Encode them back without changes and check the consoles still boot to the menu with them.
- [ ] Do the same with an edited file (for example a colour in `scripts/env/colors_*.lua`).
