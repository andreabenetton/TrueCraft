#!/usr/bin/env python3
"""
Screen-capture MCP server for Claude Code.

Exposes one tool, `take_screenshot`, with three modes:

  full     capture the entire X display ($DISPLAY)
  window   capture the currently focused window (via xdotool getactivewindow)
  rect     capture a pixel rectangle of the root window (x, y, width, height)

Shells out to ImageMagick's `import` and `xdotool` — no Python image-processing
dependencies. Returns the PNG as base64-encoded MCP ImageContent.

Runs as a stdio MCP server; see ../README.md for Claude Code wiring.
"""
from __future__ import annotations

import asyncio
import base64
import os
import shutil
import subprocess
import sys
import tempfile
from typing import Any

from mcp.server import Server
from mcp.server.stdio import stdio_server
from mcp.types import ImageContent, TextContent, Tool

server = Server("screenshot")


def _require_binary(name: str) -> str:
    path = shutil.which(name)
    if not path:
        raise RuntimeError(
            f"required binary '{name}' not found on PATH; install ImageMagick / xdotool"
        )
    return path


def _capture(mode: str, x: int, y: int, width: int, height: int, delay_ms: int) -> bytes:
    """Run the right `import` invocation and return PNG bytes."""
    import_bin = _require_binary("import")

    if delay_ms > 0:
        # `import` itself has no delay flag in this version; sleep here.
        import time as _t
        _t.sleep(delay_ms / 1000.0)

    with tempfile.NamedTemporaryFile(suffix=".png", delete=False) as tf:
        out = tf.name
    try:
        if mode == "full":
            cmd = [import_bin, "-window", "root", out]
        elif mode == "window":
            xdotool = _require_binary("xdotool")
            wid = subprocess.check_output([xdotool, "getactivewindow"], text=True).strip()
            cmd = [import_bin, "-window", wid, out]
        elif mode == "rect":
            if width <= 0 or height <= 0:
                raise ValueError("rect mode requires positive width and height")
            crop = f"{width}x{height}+{x}+{y}"
            cmd = [import_bin, "-window", "root", "-crop", crop, "+repage", out]
        else:
            raise ValueError(f"unknown mode: {mode!r} (expected full / window / rect)")

        env = os.environ.copy()
        env.setdefault("DISPLAY", ":0")
        proc = subprocess.run(cmd, capture_output=True, env=env, check=False)
        if proc.returncode != 0:
            stderr = proc.stderr.decode(errors="replace").strip()
            raise RuntimeError(f"{' '.join(cmd)!r} exited {proc.returncode}: {stderr}")

        with open(out, "rb") as fh:
            return fh.read()
    finally:
        try:
            os.unlink(out)
        except FileNotFoundError:
            pass


@server.list_tools()
async def list_tools() -> list[Tool]:
    return [
        Tool(
            name="take_screenshot",
            description=(
                "Capture the screen, the active window, or a rectangle of the root "
                "X display, and return a PNG image. Useful for verifying UI "
                "rendering visually. Runs on X11 only (uses ImageMagick `import`)."
            ),
            inputSchema={
                "type": "object",
                "properties": {
                    "mode": {
                        "type": "string",
                        "enum": ["full", "window", "rect"],
                        "description": "Capture mode. 'full' grabs $DISPLAY; "
                                       "'window' grabs xdotool's active window; "
                                       "'rect' grabs the (x,y,width,height) crop of root.",
                        "default": "full",
                    },
                    "x": {"type": "integer", "default": 0, "description": "Rect X (rect mode only)."},
                    "y": {"type": "integer", "default": 0, "description": "Rect Y (rect mode only)."},
                    "width":  {"type": "integer", "default": 0, "description": "Rect width (rect mode only)."},
                    "height": {"type": "integer", "default": 0, "description": "Rect height (rect mode only)."},
                    "delay_ms": {
                        "type": "integer",
                        "default": 0,
                        "description": "Delay before capture, in milliseconds. "
                                       "Useful when the caller just told the user "
                                       "to focus a window.",
                    },
                },
            },
        ),
    ]


@server.call_tool()
async def call_tool(name: str, arguments: dict[str, Any]) -> list[ImageContent | TextContent]:
    if name != "take_screenshot":
        return [TextContent(type="text", text=f"unknown tool: {name}")]

    mode = arguments.get("mode", "full")
    try:
        png = _capture(
            mode=mode,
            x=int(arguments.get("x", 0)),
            y=int(arguments.get("y", 0)),
            width=int(arguments.get("width", 0)),
            height=int(arguments.get("height", 0)),
            delay_ms=int(arguments.get("delay_ms", 0)),
        )
    except Exception as e:
        return [TextContent(type="text", text=f"screenshot failed: {e}")]

    return [
        ImageContent(
            type="image",
            data=base64.b64encode(png).decode("ascii"),
            mimeType="image/png",
        ),
    ]


async def main() -> None:
    async with stdio_server() as (read, write):
        await server.run(read, write, server.create_initialization_options())


if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        sys.exit(0)
