# (S)NES Mini - Lua Compiler
Based on the work from [frostyfire03530](https://github.com/frostyfire03530).

### **What is it?**
This app decompiles and recompiles the Lua files (LuaJIT bytecode) of the SNES Mini, NES Mini and Famicom Mini, for theme creation.

### **How to use it**
- Click the icon of your console: the app decodes its Lua files; use **Open folder** to reach the `decoded` folder
- Edit the files you need with your favourite Lua/text editor
- Choose `Encode`: only the files you changed are encoded, and **Open folder** takes you to the output folder
- Copy the re-encoded files to your theme

Enjoy

### **Requirements**
- **Windows**: nothing to install.
- **Linux (x86-64)**: a distribution with glibc 2.34 or newer (Ubuntu 22.04, Debian 12 or newer) and a desktop session
  (X11 or Wayland through XWayland). The Avalonia UI needs `libfontconfig1`, `libx11-6`, `libice6`, `libsm6`,
  `libxext6`, `libxcursor1`, `libxrandr2`, `libxi6`, `libgl1` and `libicu` (on Ubuntu 22.04: `libicu70`).
  Download the `snes-mini-lua-compiler-linux-x64` artifact from the CI run, then `chmod +x SNESMiniLuaCompiler`.
- macOS is not supported (nobody has tested it, and LuaJIT 2.0 has no ARM64 support).

The app extracts LuaJIT 2.0 (the version the consoles use, bytecode version 1) and the decompiler to the
`lib` folder next to it on first run.

### **Building**
- Windows: `dotnet build` (Native AOT publish: `dotnet publish -c Release`, needs Visual Studio with the C++ tools).
- The LuaJIT used for encoding is built from [`third_party/luajit`](third_party/luajit/NINTENDO.md):
  `scriptsuild-luajit-windows.bat` (Visual Studio, writes `Lib\luajit`) and `scripts/build-luajit-linux.sh`
  (static, writes `Lib/luajit-linux`). The Windows binary is committed; the Linux one is built by the CI.
- Linux: the decompiler comes from the
  [Linux port](https://github.com/DefKorns/luajit-decompiler-v2/tree/linux-support) (CMake, static), copied to
  `Lib/decompiler-linux`. Then `dotnet publish -c Release -r linux-x64 -p:TargetOs=linux`. The `build-linux` job of
  the CI does all of it (see `.github/workflows/build.yml`).

### **Development**
- Code follows the [.NET coding conventions](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions);
  `.editorconfig` holds the rules and the Roslyn analyzers run in every build.
- Files use LF line endings (`.sln` files use CRLF). Install the [pre-commit](https://pre-commit.com/) hooks with
  `pip install pre-commit && pre-commit install`. They check line endings, whitespace, YAML and JSON, and run
  `dotnet format whitespace`. The CI runs the same hooks. `Lib/` is excluded: it holds LuaJIT bytecode
  and binaries that no hook may change.

### **Open source used**
- **LuaJIT 2.0.4** (by Mike Pall, MIT license) encodes the Lua files. The app builds it from the source that
  Nintendo publishes for the consoles: the `ext-luajit` folder of the NES Classic Edition and Super NES Classic
  Edition open source packages, kept in [`third_party/luajit`](third_party/luajit/NINTENDO.md) with its license. It is the
  same LuaJIT the consoles' scripts are built with, and it produces the same bytecode as the official 2.0.4.
  Nintendo's open source notices are at <https://support.nintendo.com/jp/oss/index.html>:

  > The Nintendo Classic Mini Family Computer console uses some open-source software (OSS) under an OSS license.
  > Customers may obtain, copy, modify, and redistribute the source code of this OSS in accordance with the OSS
  > license. Due to its nature, this OSS is not covered by any warranty or support. Please understand this.

- **[LuaJIT Decompiler v2](https://github.com/marsinator358/luajit-decompiler-v2)** by marsinator358 (MIT) decompiles
  the files; the Linux build comes from a [port](https://github.com/DefKorns/luajit-decompiler-v2/tree/linux-support).
- **[Avalonia](https://avaloniaui.net/)**, [Semi.Avalonia](https://github.com/irihitech/Semi.Avalonia),
  [Lucide](https://lucide.dev/) icons (through Lucide.Avalonia), CommunityToolkit.Mvvm and Serilog.

### Credits (alphabetically)
- [DefKorns](https://github.com/DefKorns)
- [frostyfire03530](https://github.com/frostyfire03530) - Initial development
- [marsinator358](https://github.com/marsinator358/luajit-decompiler-v2) - LuaJIT Decompiler v2 (MIT), the decompiler used by this app

### Thanks to (alphabetically)
- AluCarD (testing)
- [bobsayshilol](https://github.com/bobsayshilol/)
- [znixian](https://gitlab.com/znixian)
