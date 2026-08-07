# Comandos reproduzíveis de build dos três componentes do projeto SeniorCare
# Requer: .NET 8 SDK, Node.js 20+ e npm

BACKEND_DIR  := SeniorCareManager-Backend/SeniorCareManager.WebAPI
CARE_DIR     := SeniorCareManager-Frontend/SeniorCareManagerFrontend
STOCK_DIR    := SeniorStockManager-Frontend/SeniorStockManagerFrontend

# ──────────────────────────────────────────────
# Backend (.NET 8)
# ──────────────────────────────────────────────
.PHONY: backend-restore backend-build backend-test

backend-restore:
	dotnet restore $(BACKEND_DIR)/SeniorCareManager.WebAPI.csproj

backend-build: backend-restore
	dotnet build $(BACKEND_DIR)/SeniorCareManager.WebAPI.csproj \
	  --configuration Release --no-restore

backend-test: backend-build
	dotnet test $(BACKEND_DIR)/../SeniorCareManager.WebAPI.sln \
	  --configuration Release \
	  --logger "trx;LogFileName=test-results.trx" \
	  --results-directory $(BACKEND_DIR)/TestResults

# ──────────────────────────────────────────────
# Front-end assistencial (SeniorCareManagerFrontend)
# ──────────────────────────────────────────────
.PHONY: care-install care-lint care-build care-test

care-install:
	npm ci --prefix $(CARE_DIR)

care-lint: care-install
	npm run lint --prefix $(CARE_DIR)

care-build: care-install
	npm run build --prefix $(CARE_DIR)

care-test: care-install
	npm test --prefix $(CARE_DIR)

# ──────────────────────────────────────────────
# Front-end de estoque (SeniorStockManagerFrontend)
# ──────────────────────────────────────────────
.PHONY: stock-install stock-lint stock-build stock-test

stock-install:
	npm ci --prefix $(STOCK_DIR)

stock-lint: stock-install
	npm run lint --prefix $(STOCK_DIR)

stock-build: stock-install
	npm run build --prefix $(STOCK_DIR)

stock-test: stock-install
	npm test --prefix $(STOCK_DIR)

# ──────────────────────────────────────────────
# Atalhos agregados
# ──────────────────────────────────────────────
.PHONY: install build lint test

install: backend-restore care-install stock-install

build: backend-build care-build stock-build

lint: care-lint stock-lint

test: backend-test care-test stock-test
