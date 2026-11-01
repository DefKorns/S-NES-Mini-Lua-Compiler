#!/usr/bin/env bash
# Builds a static LuaJIT 2.0.4 for Linux x86-64 from third_party/luajit (the source Nintendo publishes for the
# consoles, see third_party/luajit/NINTENDO.md) and copies luajit + jit/bcsave.lua to OUT_DIR
# (default: Lib/luajit-linux). The consoles use LuaJIT 2.0.x (bytecode version 1), so do NOT use the 2.1 branch.
# Needs: make, gcc.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
OUT_DIR="${1:-$ROOT/Lib/luajit-linux}"
WORK_DIR="$(mktemp -d)"
trap 'rm -rf "$WORK_DIR"' EXIT

# make builds inside the source tree, so build in a copy
cp -r "$ROOT/third_party/luajit/." "$WORK_DIR/"
cd "$WORK_DIR"
make -j"$(nproc)" CC="gcc -static" BUILDMODE=static >/dev/null

mkdir -p "$OUT_DIR/jit"
cp src/luajit "$OUT_DIR/luajit"
cp src/jit/bcsave.lua "$OUT_DIR/jit/"   # the only part of jit/ that "luajit -b" needs
echo "LuaJIT built in $OUT_DIR: $("$OUT_DIR/luajit" -v)"
