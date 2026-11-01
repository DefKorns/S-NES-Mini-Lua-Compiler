# (S)NES Mini - Lua Compiler
Based on the work from [frostyfire03530](https://github.com/frostyfire03530).

### **What is it?**
This script allows you to decompile SNES Mini lua files.
This app is a GUI for my previous [script](https://github.com/DefKorns/classic-mini-lua-decompiler). The goal is to decrypt/encrypt (S)NES/Famicom's lua files for theme creation.

### **How to use it**
- Select your desired system
- Choose `Decode`
- Edit all the files you needed (`app location/decoded` folder) with your desired lua/text editor
- choose `Encode`
- copy the re-encrypted files to your theme

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
- Windows: `dotnet build` (Native AOT publish: `dotnet publish -c Release`).
- Linux: the native tools are built by `scripts/build-luajit-linux.sh` (static LuaJIT 2.0.x) and from the
  [Linux port of the decompiler](https://github.com/DefKorns/luajit-decompiler-v2/tree/linux-support) (CMake, static),
  copied to `Lib/luajit-linux` and `Lib/decompiler-linux`. Then
  `dotnet publish -c Release -r linux-x64 -p:TargetOs=linux`. The `build-linux` job of the CI does all of it
  (see `.github/workflows/build.yml`).

### **Development**
- Code follows the [.NET coding conventions](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions);
  `.editorconfig` holds the rules and the Roslyn analyzers run in every build.
- Files use LF line endings (`.sln` files use CRLF). Install the [pre-commit](https://pre-commit.com/) hooks with
  `pip install pre-commit && pre-commit install`. They check line endings, whitespace, YAML and JSON, and run
  `dotnet format whitespace`. The CI runs the same hooks. `Lib/` is excluded: it holds LuaJIT bytecode
  and binaries that no hook may change.

### Credits (alphabetically)
- [DefKorns](https://github.com/DefKorns)
- [frostyfire03530](https://github.com/frostyfire03530) - Initial development
- [marsinator358](https://github.com/marsinator358/luajit-decompiler-v2) - LuaJIT Decompiler v2 (MIT), the decompiler used by this app

### Thanks to (alphabetically)
- AluCarD (testing)
- [bobsayshilol](https://github.com/bobsayshilol/)
- [znixian](https://gitlab.com/znixian)
