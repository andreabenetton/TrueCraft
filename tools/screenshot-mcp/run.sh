#!/usr/bin/env bash
# Wrapper that launches the screenshot MCP server in its own venv.
# Called by Claude Code via the repo's .mcp.json — keeps the config
# absolute-path-free.
set -euo pipefail
here="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
venv="$here/.venv"

if [[ ! -x "$venv/bin/python" ]]; then
    python3 -m venv "$venv"
    "$venv/bin/pip" install --quiet --upgrade pip
    "$venv/bin/pip" install --quiet -r "$here/requirements.txt"
fi

exec "$venv/bin/python" "$here/server.py" "$@"
