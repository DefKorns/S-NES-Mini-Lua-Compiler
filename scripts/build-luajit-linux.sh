#!/usr/bin/env bash
# Builds a static LuaJIT 2.0.x for Linux x86-64 and copies luajit + jit/bcsave.lua to OUT_DIR
# (default: Lib/luajit-linux). The consoles use LuaJIT 2.0.x (bytecode version 1), so do NOT use the 2.1 branch.
# Needs: git, make, gcc.
set -euo pipefail

LUAJIT_BRANCH=v2.0
LUAJIT_COMMIT=592f9e3f549c7f176a9a7a0d70189559305f2eda   # tip of the v2.0 branch used when validating against the consoles' 2.0.2 luajit.exe
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
OUT_DIR="${1:-$ROOT/Lib/luajit-linux}"
WORK_DIR="$(mktemp -d)"
trap 'rm -rf "$WORK_DIR"' EXIT

git clone -q -b "$LUAJIT_BRANCH" https://github.com/LuaJIT/LuaJIT "$WORK_DIR/luajit"
cd "$WORK_DIR/luajit"
git checkout -q "$LUAJIT_COMMIT"
make -j"$(nproc)" CC="gcc -static" BUILDMODE=static >/dev/null

mkdir -p "$OUT_DIR/jit"
cp src/luajit "$OUT_DIR/luajit"
cp src/jit/bcsave.lua "$OUT_DIR/jit/"   # the only part of jit/ that "luajit -b" needs
echo "LuaJIT built in $OUT_DIR"
