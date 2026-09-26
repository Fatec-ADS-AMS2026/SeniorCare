# Comandos reproduzíveis de build dos três componentes do projeto SeniorCare
# Requer: .NET 8 SDK, Node.js 20+ e npm

BACKEND_DIR  := SeniorCareManager-Backend/SeniorCareManager.WebAPI
CARE_DIR     := SeniorCareManager-Frontend/SeniorCareManagerFrontend
STOCK_DIR    := SeniorStockManager-Frontend/SeniorStockManagerFrontend
PORTAL_DIR   := SeniorPortal-Frontend/SeniorPortalFrontend

# ──────────────────────────────────────────────
# Backend (.NET 8)
# ──────────────────────────────────────────────
.PHONY: backend-restore backend-build backend-test backend-coverage

backend-restore:
	dotnet restore $(BACKEND_DIR)/SeniorCareManager.WebAPI.csproj

backend-build: backend-restore
	dotnet build $(BACKEND_DIR)/SeniorCareManager.WebAPI.csproj \
	  --configuration Release --no-restore

backend-test: backend-build
	dotnet test $(BACKEND_DIR)/SeniorCareManager.WebAPI.sln \
	  --configuration Release \
	  --logger "trx;LogFileName=test-results.trx" \
	  --results-directory $(BACKEND_DIR)/TestResults

backend-coverage: backend-restore
	rm -rf SeniorCareManager-Backend/TestResults/coverage
	dotnet test $(BACKEND_DIR)/SeniorCareManager.WebAPI.sln \
	  --configuration Release \
	  --collect:"XPlat Code Coverage" \
	  --results-directory SeniorCareManager-Backend/TestResults/coverage

# ──────────────────────────────────────────────
# Front-end assistencial (SeniorCareManagerFrontend)
# ──────────────────────────────────────────────
.PHONY: care-install care-lint care-build care-test care-coverage

care-install:
	npm ci --prefix $(CARE_DIR)

care-lint: care-install
	npm run lint --prefix $(CARE_DIR)

care-build: care-install
	npm run build --prefix $(CARE_DIR)

care-test: care-install
	npm test --prefix $(CARE_DIR)

care-coverage: care-install
	npm run test:coverage --prefix $(CARE_DIR)

# ──────────────────────────────────────────────
# Front-end de estoque (SeniorStockManagerFrontend)
# ──────────────────────────────────────────────
.PHONY: stock-install stock-lint stock-build stock-test stock-coverage

stock-install:
	npm ci --prefix $(STOCK_DIR)

stock-lint: stock-install
	npm run lint --prefix $(STOCK_DIR)

stock-build: stock-install
	npm run build --prefix $(STOCK_DIR)

stock-test: stock-install
	npm test --prefix $(STOCK_DIR)

stock-coverage: stock-install
	npm run test:coverage --prefix $(STOCK_DIR)

# ──────────────────────────────────────────────
# Portal do residente
# ──────────────────────────────────────────────
.PHONY: portal-install portal-coverage

portal-install:
	npm ci --prefix $(PORTAL_DIR)

portal-coverage: portal-install
	npm run test:coverage --prefix $(PORTAL_DIR)

# ──────────────────────────────────────────────
# Atalhos agregados
# ──────────────────────────────────────────────
.PHONY: install build lint test coverage

install: backend-restore care-install stock-install

build: backend-build care-build stock-build

lint: care-lint stock-lint


coverage: backend-coverage care-coverage stock-coverage portal-coverage
	python3 scripts/generate_coverage_report.py
test: backend-test care-test stock-test
