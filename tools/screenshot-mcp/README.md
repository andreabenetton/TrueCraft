# screenshot-mcp

Tiny MCP server that gives Claude Code a `take_screenshot` tool — useful for
verifying UI rendering visually without manually `Read`ing screenshot files.

## What it does

Three capture modes:

- `full` — the whole X display (`$DISPLAY`)
- `window` — the currently focused window (via `xdotool getactivewindow`)
- `rect` — a `(x, y, width, height)` crop of the root window

Returns the PNG bytes as base64 inside an MCP `ImageContent` reply.

## Requirements

Runs on Linux X11 only:

- `import` (ImageMagick) — `dnf install ImageMagick` on Fedora
- `xdotool` — `dnf install xdotool` on Fedora (only needed for `window` mode)
- Python 3.10+ with `venv` support

No Pillow or other Python image deps; everything goes through `import`.

## Setup

The MCP server runs in its own venv, bootstrapped on first launch by
`run.sh`. Nothing to do manually — Claude Code calls `run.sh` and the
script creates `.venv/` and `pip install -r requirements.txt` the first
time, then `exec`s the server.

If you want to pre-bootstrap:

```
./run.sh < /dev/null    # exits immediately after init since no MCP traffic
# or just
python3 -m venv tools/screenshot-mcp/.venv
tools/screenshot-mcp/.venv/bin/pip install -r tools/screenshot-mcp/requirements.txt
```

## Wiring into Claude Code

The repo's `.mcp.json` already declares this server. Claude Code picks up
that file automatically when started from this directory. To verify:

```
claude mcp list
```

should show `screenshot` as available. The tool's name in conversation is
`mcp__screenshot__take_screenshot`.

## Manual test

Drop into the server's directory and feed it a single MCP init message
to confirm it starts:

```
echo '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"manual","version":"0"}}}' | ./run.sh | head -c 200
```

Should print a JSON-RPC `result` envelope back.
