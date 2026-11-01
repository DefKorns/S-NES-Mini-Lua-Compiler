# LuaJIT 2.0.4 as published by Nintendo

Nintendo publishes its open source notices at <https://support.nintendo.com/jp/oss/index.html>:

> The Nintendo Classic Mini Family Computer console uses some open-source software (OSS) under an OSS license.
> Customers may obtain, copy, modify, and redistribute the source code of this OSS in accordance with the OSS
> license. Due to its nature, this OSS is not covered by any warranty or support. Please understand this.

Source: the open source packages of the NES Classic Edition (2016) and the Super NES Classic Edition (2017),
folder `ext-luajit-83dda3b07286174de43ee5adb25e7cc5fbe308e3` (identical in both). This is the LuaJIT the
consoles' Lua scripts are built with.

It is the official **LuaJIT 2.0.4** (release commit `69e5342e`, 2015-05-14) plus Nintendo's changes, which are only
about building it for their targets (ARM compiler `__rev`, the Nintendo 3DS "CTR" target, `HASH_BIAS` written as
`0xfb3ee249`, which is the same value as `-0x04c11db7`). Bytecode produced with it is byte for byte the same as
the official 2.0.4 (checked on the 613 files of the consoles).

Kept here: `src`, `dynasm`, `Makefile`, `COPYRIGHT`, `README` (what is needed to build `luajit` on Windows and
Linux). Left out: the documentation, the Visual Studio projects, and Nintendo's CMake files (they need
their private build system). LuaJIT is MIT licensed, see `COPYRIGHT`.
