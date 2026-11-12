# Mermaid diagram rendering (Docker / minlag/mermaid-cli). See .mermaid/render.sh.
MERMAID_RENDER ?= .mermaid/render.sh
DOCS_DIR ?= docs
MERMAID_IMAGE ?= minlag/mermaid-cli:latest
MERMAID_THEME ?= redux-dark
# \# escapes the comment character; override e.g. make mermaid-render MERMAID_BG="#1e1e1e"
MERMAID_BG ?= \#222222
MERMAID_SCALE ?= 10
MERMAID_CSS ?= .mermaid/mermaid.css
MERMAID_CONFIG ?= .mermaid/mermaid.config.json

.PHONY: mermaid-render
mermaid-render:
	DOCS_DIR="$(DOCS_DIR)" MERMAID_IMAGE="$(MERMAID_IMAGE)" MERMAID_THEME="$(MERMAID_THEME)" MERMAID_BG="$(MERMAID_BG)" MERMAID_SCALE="$(MERMAID_SCALE)" MERMAID_CSS="$(MERMAID_CSS)" MERMAID_CONFIG="$(MERMAID_CONFIG)" "$(MERMAID_RENDER)"

.PHONY: mermaid-clean
mermaid-clean:
	find "$(DOCS_DIR)" -type f \( -name '*.generated.png' -o -name '*.generated.svg' \) -delete
