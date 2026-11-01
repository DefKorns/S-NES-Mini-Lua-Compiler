# TODO

Done: the Python decompiler was replaced by LuaJIT Decompiler v2, with a Linux x86-64 build (native tools built
in the CI) and the pre-commit hooks.

## Test before releasing

- [ ] Decode the Lua files of each console (NES, Famicom, SNES, Super Famicom, Shonen Jump) in the app and
      compare a few of them with what the Python decompiler gave. Linux: only Famicom Mini was run through
      the UI (virtual display); the other consoles were only decompiled/recompiled by a test program.
- [ ] Encode them back without changes and check the consoles still boot to the menu with them.
- [ ] Do the same with an edited file (for example a colour in `scripts/env/colors_*.lua`).
- [ ] If the consoles do not boot, suspect first: the files whose table constants are serialized in a different
      hash order by the Linux LuaJIT build (`pixel_character.lua`, `clover_const.lua`) and the UTF-8 BOM the
      decompiler writes at the start of every file.

## Open

- [ ] Run the Linux package on a real desktop (GNOME/KDE, Wayland) and check "open folder" (`xdg-open`).
- [ ] Publish the Linux package as a release asset.
- [ ] Propose the Linux port to the upstream decompiler (pull request to marsinator358/luajit-decompiler-v2).
- [ ] The Windows decompiler is the upstream executable (the build skips files with paths over 260 characters
      silently); build the Windows exe from the fork in the CI if the limit ever matters.
- [ ] macOS: not done. LuaJIT 2.0 has no ARM64 support (x86-64 under Rosetta 2 would work), signing and
      notarization are needed, and nobody can test it.
