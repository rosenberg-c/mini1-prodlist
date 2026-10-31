DOTNET ?= dotnet
JB ?= jb
PROJECT ?= App.csproj
TESTS ?= tests/App.Tests.csproj

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
	$(DOTNET) test $(TESTS)

.PHONY: run
run: deps
	$(DOTNET) run --project $(PROJECT)

.PHONY: lint
lint: deps
	$(DOTNET) format $(PROJECT) --verify-no-changes
	$(DOTNET) format $(TESTS) --verify-no-changes

.PHONY: format
format: deps
	$(JB) cleanupcode $(PROJECT) --profile="Built-in: Reformat Code"
	$(JB) cleanupcode $(TESTS) --profile="Built-in: Reformat Code"

.PHONY: clean
clean:
	$(DOTNET) clean $(PROJECT)

.PHONY: reset
reset:
	cp products.example.json products.json
