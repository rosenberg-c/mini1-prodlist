#!/usr/bin/env bash
# Render every .mmd under DOCS_DIR to a sibling <name>.generated.png via Docker.
# Contract (env vars, all optional):
#   DOCS_DIR      input directory (default: docs)
#   MERMAID_IMAGE docker image (default: minlag/mermaid-cli:latest)
#   MERMAID_THEME theme name (default: dark)
#   MERMAID_BG    renderer background color (default: #222222)
#   MERMAID_SCALE png scale factor (default: 4)
#   MERMAID_CSS   css file passed to mmdc -C (default: .mermaid/mermaid.css)
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
DOCS_DIR="${DOCS_DIR:-docs}"
IMAGE="${MERMAID_IMAGE:-minlag/mermaid-cli:latest}"
THEME="${MERMAID_THEME:-dark}"
BG="${MERMAID_BG:-#222222}"
SCALE="${MERMAID_SCALE:-4}"
CSS_FILE="${MERMAID_CSS:-.mermaid/mermaid.css}"

cd "$ROOT"

[ -d "$DOCS_DIR" ] || { echo "render: input dir not found: $DOCS_DIR" >&2; exit 1; }

status=0
count=0
while IFS= read -r -d '' f; do
  rel="${f#./}"
  out="${rel%.mmd}.generated.png"
  count=$((count + 1))
  echo "render: $rel -> $out"
  if ! docker run --rm \
    -v "$ROOT:/work" -w /work \
    "$IMAGE" \
      -i "$rel" -o "$out" \
      -t "$THEME" -b "$BG" \
      -C "$CSS_FILE" \
      --scale "$SCALE"; then
    echo "render: FAILED $rel" >&2
    status=1
  fi
done < <(find "$DOCS_DIR" -type f -name '*.mmd' -print0 | LC_ALL=C sort -z)

if [ "$count" -eq 0 ]; then
  echo "render: no .mmd files under $DOCS_DIR (nothing to do)" >&2
fi
exit "$status"
