DOTNET ?= dotnet
JB ?= jb
PROJECT ?= App.csproj
SOLUTION ?= App.sln
PRODUCT_TESTS ?= tests/producttests/App.Tests.csproj
APP_TESTS ?= tests/apptests/App.Integration.Tests.csproj
PRODUCT_HOST ?= tests/producthost/ProductHost.csproj

-include .env
APP_DATA_PATH ?= $(CURDIR)/products.json

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
	$(DOTNET) test $(PRODUCT_TESTS)
	$(DOTNET) test $(APP_TESTS)

.PHONY: run
run: deps
	APP_DATA_PATH="$(APP_DATA_PATH)" $(DOTNET) run --project $(PROJECT)

.PHONY: lint
lint: deps
	$(DOTNET) format $(PROJECT) --verify-no-changes
	$(DOTNET) format $(PRODUCT_HOST) --verify-no-changes
	$(DOTNET) format $(PRODUCT_TESTS) --verify-no-changes
	$(DOTNET) format $(APP_TESTS) --verify-no-changes

.PHONY: format
format: deps
	$(JB) cleanupcode $(SOLUTION) --profile="Built-in: Reformat Code" --include="src/**/*.cs;tests/**/*.cs"

.PHONY: clean
clean:
	$(DOTNET) clean $(PROJECT)

.PHONY: reset
reset:
	cp todo.example.json "$(APP_DATA_PATH)"
