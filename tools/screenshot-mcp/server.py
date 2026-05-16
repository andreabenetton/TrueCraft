#!/usr/bin/env python3
"""
Screen-capture MCP server for Claude Code.

Exposes two tools:

  list_windows     enumerate visible X windows so the caller can pick one
                   by name / class before grabbing it
  take_screenshot  capture the entire display, a specific window, or a
                   pixel rectangle of the root, and return PNG bytes

`take_screenshot` modes:

  full     capture the entire X display ($DISPLAY)
  active   capture the currently focused window (xdotool getactivewindow)
  window   capture a window selected by id / title regex / WM_CLASS
  rect     capture an (x, y, width, height) crop of the root window

Shells out to ImageMagick's `import` and `xdotool` — no Python
image-processing dependencies. Returns PNGs as base64-encoded MCP
ImageContent.

Runs as a stdio MCP server; see README.md for Claude Code wiring.
"""
from __future__ import annotations

import asyncio
import base64
import json
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


def _xdotool() -> str:
    return _require_binary("xdotool")


def _x_env() -> dict[str, str]:
    env = os.environ.copy()
    env.setdefault("DISPLAY", ":0")
    return env


def _get_window_info(wid: str) -> dict[str, Any]:
    """Best-effort metadata for an X window id. Missing fields stay None."""
    xd = _xdotool()
    env = _x_env()

    def _run(args: list[str]) -> str | None:
        try:
            return subprocess.check_output(args, text=True, env=env, stderr=subprocess.DEVNULL).strip()
        except subprocess.CalledProcessError:
            return None

    geom_raw = _run([xd, "getwindowgeometry", "--shell", wid])
    geom: dict[str, int] | None = None
    if geom_raw:
        kv = {}
        for line in geom_raw.splitlines():
            if "=" in line:
                k, v = line.split("=", 1)
                kv[k] = v
        try:
            geom = {
                "x": int(kv["X"]),
                "y": int(kv["Y"]),
                "width": int(kv["WIDTH"]),
                "height": int(kv["HEIGHT"]),
            }
        except (KeyError, ValueError):
            geom = None

    return {
        "id": wid,
        "name": _run([xd, "getwindowname", wid]) or "",
        "class": _run([xd, "getwindowclassname", wid]) or "",
        "pid": _run([xd, "getwindowpid", wid]) or "",
        "geometry": geom,
    }


def _resolve_window_id(
    title: str | None,
    wm_class: str | None,
    window_id: str | None,
) -> str:
    """Look up an X window id from the caller's selector. Raises if no match."""
    if window_id:
        return window_id.strip()

    xd = _xdotool()
    env = _x_env()
    args = [xd, "search", "--onlyvisible", "--limit", "1"]
    if wm_class:
        args += ["--class", wm_class]
    if title:
        args += ["--name", title]
    if not (title or wm_class):
        raise ValueError("window mode needs one of: window_id, title, wm_class")

    try:
        out = subprocess.check_output(args, text=True, env=env, stderr=subprocess.DEVNULL).strip()
    except subprocess.CalledProcessError:
        out = ""
    if not out:
        raise RuntimeError(
            f"no visible window matched (title={title!r}, class={wm_class!r}). "
            "Try list_windows to see what is open."
        )
    # `xdotool search` returns one id per line; --limit 1 ensures at most one.
    return out.splitlines()[0].strip()


def _capture(
    mode: str,
    x: int,
    y: int,
    width: int,
    height: int,
    delay_ms: int,
    title: str | None,
    wm_class: str | None,
    window_id: str | None,
) -> bytes:
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
        elif mode == "active":
            wid = subprocess.check_output([_xdotool(), "getactivewindow"], text=True, env=_x_env()).strip()
            cmd = [import_bin, "-window", wid, out]
        elif mode == "window":
            wid = _resolve_window_id(title, wm_class, window_id)
            cmd = [import_bin, "-window", wid, out]
        elif mode == "rect":
            if width <= 0 or height <= 0:
                raise ValueError("rect mode requires positive width and height")
            crop = f"{width}x{height}+{x}+{y}"
            cmd = [import_bin, "-window", "root", "-crop", crop, "+repage", out]
        else:
            raise ValueError(f"unknown mode: {mode!r} (expected full / active / window / rect)")

        proc = subprocess.run(cmd, capture_output=True, env=_x_env(), check=False)
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
            name="list_windows",
            description=(
                "List all visible X windows with their id, name, WM_CLASS, "
                "owning PID, and geometry. Use this to find a window to pass "
                "to take_screenshot's window mode."
            ),
            inputSchema={"type": "object", "properties": {}},
        ),
        Tool(
            name="take_screenshot",
            description=(
                "Capture the screen, a specific window, or a rectangle of the "
                "root X display, and return a PNG image. Modes:\n"
                "  full   - the entire $DISPLAY\n"
                "  active - whichever window currently has focus\n"
                "  window - a specific window by id, title regex, or WM_CLASS "
                "(call list_windows first to discover candidates)\n"
                "  rect   - an (x, y, width, height) crop of the root window\n"
                "Runs on X11 only (uses ImageMagick `import` + xdotool)."
            ),
            inputSchema={
                "type": "object",
                "properties": {
                    "mode": {
                        "type": "string",
                        "enum": ["full", "active", "window", "rect"],
                        "description": "Capture mode (see tool description).",
                        "default": "full",
                    },
                    "window_id": {
                        "type": "string",
                        "description": "X window id (decimal). window mode only.",
                    },
                    "title": {
                        "type": "string",
                        "description": "Regex match against window name. "
                                       "window mode only.",
                    },
                    "wm_class": {
                        "type": "string",
                        "description": "Match against WM_CLASS. window mode only.",
                    },
                    "x":      {"type": "integer", "default": 0, "description": "Rect X (rect mode only)."},
                    "y":      {"type": "integer", "default": 0, "description": "Rect Y (rect mode only)."},
                    "width":  {"type": "integer", "default": 0, "description": "Rect width (rect mode only)."},
                    "height": {"type": "integer", "default": 0, "description": "Rect height (rect mode only)."},
                    "delay_ms": {
                        "type": "integer",
                        "default": 0,
                        "description": "Delay before capture, in milliseconds.",
                    },
                },
            },
        ),
    ]


@server.call_tool()
async def call_tool(name: str, arguments: dict[str, Any]) -> list[ImageContent | TextContent]:
    if name == "list_windows":
        try:
            xd = _xdotool()
            out = subprocess.check_output(
                [xd, "search", "--onlyvisible", "--name", ""],
                text=True, env=_x_env(), stderr=subprocess.DEVNULL,
            ).strip()
            ids = [line.strip() for line in out.splitlines() if line.strip()]
            windows = [_get_window_info(wid) for wid in ids]
            # Filter out windows with no name AND no class — usually invisible WM scaffolding.
            windows = [w for w in windows if w["name"] or w["class"]]
            return [TextContent(type="text", text=json.dumps(windows, indent=2))]
        except Exception as e:
            return [TextContent(type="text", text=f"list_windows failed: {e}")]

    if name != "take_screenshot":
        return [TextContent(type="text", text=f"unknown tool: {name}")]

    try:
        png = _capture(
            mode=arguments.get("mode", "full"),
            x=int(arguments.get("x", 0)),
            y=int(arguments.get("y", 0)),
            width=int(arguments.get("width", 0)),
            height=int(arguments.get("height", 0)),
            delay_ms=int(arguments.get("delay_ms", 0)),
            title=arguments.get("title"),
            wm_class=arguments.get("wm_class"),
            window_id=arguments.get("window_id"),
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
