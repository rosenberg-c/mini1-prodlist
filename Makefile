DOTNET ?= dotnet
TESTS ?= tests/ProdList.Tests.csproj

.PHONY: test
test:
	$(DOTNET) test $(TESTS)

.PHONY: run
run:
	$(DOTNET) run

.PHONY: lint
lint:
	$(DOTNET) format ProdList.csproj --verify-no-changes
	$(DOTNET) format $(TESTS) --verify-no-changes

.PHONY: format
format:
	$(DOTNET) format ProdList.csproj
	$(DOTNET) format $(TESTS)
