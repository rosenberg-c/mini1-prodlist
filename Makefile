include Makefile.mermaid.mk

DOTNET ?= dotnet
JB ?= jb
DOCKER ?= docker
SOFFICE ?= /Applications/LibreOffice.app/Contents/MacOS/soffice
PROJECT ?= App.csproj
SOLUTION ?= App.sln
ASSET_TESTS ?= tests/assettests/App.Tests.csproj
TODO_TESTS ?= tests/todotests/App.Tests.csproj
APP_TESTS ?= tests/apptests/App.Integration.Tests.csproj
ASSETS_HOST ?= tests/assetshost/AssetsHost.csproj
TODO_HOST ?= tests/todohost/TodoHost.csproj

-include .env
DATA_FILE ?= todo.json
DATA_EXAMPLE ?= todo.example.json
APP_DATA_PATH ?= $(CURDIR)/$(DATA_FILE)

.PHONY: deps
deps:
	git submodule update --init --recursive

.PHONY: update-storage-dep
update-storage-dep:
	git submodule update --init --remote dependencies/sharp-storage
	git add dependencies/sharp-storage

.PHONY: build
build: deps
	$(DOTNET) build $(PROJECT)

.PHONY: test
test: deps
	$(DOTNET) test $(ASSET_TESTS)
	$(DOTNET) test $(TODO_TESTS)
	$(DOTNET) test $(APP_TESTS)

.PHONY: run
run: deps
	APP_DATA_PATH="$(APP_DATA_PATH)" $(DOTNET) run --project $(PROJECT)

.PHONY: lint
lint: deps
	$(DOTNET) format $(PROJECT) --verify-no-changes
	$(DOTNET) format $(ASSETS_HOST) --verify-no-changes
	$(DOTNET) format $(TODO_HOST) --verify-no-changes
	$(DOTNET) format $(ASSET_TESTS) --verify-no-changes
	$(DOTNET) format $(TODO_TESTS) --verify-no-changes
	$(DOTNET) format $(APP_TESTS) --verify-no-changes

.PHONY: format
format: deps
	$(JB) cleanupcode $(SOLUTION) --profile="Built-in: Reformat Code" --include="src/**/*.cs;tests/**/*.cs"

.PHONY: clean
clean:
	$(DOTNET) clean $(PROJECT)

.PHONY: presentation
presentation:
	sh scripts/join-slides.sh presentation/slides.generated.md $$(printf '%s\n' presentation/slides/slide-*.md | sort -V)
	$(DOCKER) run --rm --init -v "$(CURDIR):/home/marp/app" marpteam/marp-cli \
		presentation/slides.generated.md --theme-set presentation/theme.css \
		--allow-local-files --pptx -o presentation/slides.generated.pptx
	$(SOFFICE) --headless --convert-to odp --outdir presentation presentation/slides.generated.pptx

.PHONY: reset
reset:
	cp "$(DATA_EXAMPLE)" "$(APP_DATA_PATH)"
	@for file in bin/Debug/net*/$(DATA_FILE); do \
		if [ -f "$$file" ]; then cp "$(DATA_EXAMPLE)" "$$file" || exit $$?; fi; \
	done
