## 1. Pré-requisitos e contratos transversais

- [x] 1.1 Confirmar por testes que instituição ativa, identidade atual, permissões efetivas, MFA e sessão renovável de `stabilize-existing-platform` estão disponíveis antes de iniciar a integração do portal.
      **Evidência**: investigação direta confirmou que as 5 fundações já existem e têm
      teste dedicado — instituição ativa (`ICurrentUserContext`/`CurrentUserContext.cs`,
      também em `CurrentIdentityDTO.InstitutionId`), identidade atual (`GET
      /api/v1/auth/me`, `AuthController.cs:68`), permissões efetivas
      (`IAccessDecisionService`/`AccessDecisionService.cs`, também listadas em `/me`),
      MFA (fluxo `login`→`login/mfa`/`mfa/enroll`/`mfa/confirm` em `AuthController.cs`),
      sessão renovável (`ISessionService`/`SessionService.cs`, rotação via
      `Startup.cs` `OnValidatePrincipal`). Rodado
      `dotnet test --filter "FullyQualifiedName~AuthControllerMeTests|
      FullyQualifiedName~AccessDecisionServiceTests|
      FullyQualifiedName~AuthControllerMfaTests|FullyQualifiedName~SessionRotationTests"`
      — 21/21 aprovados, 0 falhas. Nenhum teste novo necessário (cobertura já forte).
- [x] 1.2 Documentar o contrato de rotas de mesma origem (`/`, `/api`, `/care`, `/stock` e `/admin`) e parametrizar nome público e domínio por configuração de implantação.
      **Evidência**: `docs/architecture/senior-portal-contracts.md` §1. Achado
      registrado explicitamente: a produção real hoje roteia por **subdomínio** via
      Caddy (`infra/deploy/ops/Caddyfile`), não por caminho — o contrato de mesma
      origem é o alvo (implementação em §8), não o estado atual. Domínio reaproveita
      `OPS_DOMAIN` já existente; nome público de exibição definido como config
      injetada em runtime (chave `publicName`, fallback `"SeniorCare"`), mecanismo de
      injeção fica para `4.1`.
- [x] 1.3 Definir o contrato versionado de contexto global, retorno seguro, perfil, logout e preferências que portal, assistência e estoque deverão cumprir.
      **Evidência**: `docs/architecture/senior-portal-contracts.md` §2-§5. Contexto
      global reusa `GET /api/v1/auth/me` sem endpoint novo; `returnTo` definido
      (caminho relativo, allowlist `/`, `/care`, `/stock`, fallback `/`, validador
      central fica para `4.5`); perfil/segurança hospedados no portal, logout via
      `POST /api/v1/auth/logout` já existente; preferências reusam as chaves de
      `localStorage` já usadas em `ThemeContext.tsx` (`theme`, `fontSize`) — por serem
      por-origem, ficam automaticamente compartilhadas assim que portal/care/stock
      estiverem sob a mesma origem, sem sincronização nova.
- [x] 1.4 Registrar a decisão de hospedagem inicial de `/admin` antes de implementar sua interface, preservando a rota e a permissão definidas no desenho.
      **Evidência**: `docs/architecture/senior-portal-contracts.md` §6 — `/admin`
      hospedado dentro do próprio Senior Portal (rotas do mesmo app), não um quarto
      artefato separado, resolvendo a "Open Question" do `design.md`. Racional: o
      caso de uso primário (administrar o catálogo de módulos de `3.3`) já é do
      próprio portal; um quarto app para um único caso de uso contradiria o racional
      de "limitar impacto de release" da decisão 1 do `design.md`.

## 2. Modelo e persistência do catálogo

- [x] 2.1 Criar entidades e configurações EF Core para `ModuleDefinition` e `InstitutionModule`, incluindo chaves, instituição, estados, ordenação, habilitação, versão de concorrência e campos de auditoria.
      **Evidência**: `Objects/Models/ModuleDefinition.cs` e `InstitutionModule.cs` +
      `Data/Builders/ModuleDefinitionBuilder.cs`/`InstitutionModuleBuilder.cs`.
      `ModuleDefinition` é catálogo sistêmico (chave única, ícone, caminho, permissão
      exigida); `InstitutionModule` é a configuração por instituição (par
      `{InstitutionId, ModuleDefinitionId}` único, `OperationalState`, `Order`,
      `IsEnabled`, `OperationalMessage`, `Version`/`xmin` como rowversion de
      concorrência otimista, `CreatedAtUtc`/`UpdatedAtUtc`). `InstitutionId` é `Guid`
      puro sem FK de banco, mesma convenção de `Role`/`ApplicationUser` (escopo
      institucional reforçado pela aplicação, não pelo schema).
- [x] 2.2 Criar migração de banco com restrições de unicidade, integridade referencial, limites de texto e enumeração dos estados operacionais.
      **Evidência**: `Data/Migrations/20260811144526_AddSeniorPortalCatalog.cs`.
      Índice único em `ModuleDefinition.Key` e no par `InstitutionModule`
      `{institution_id, module_definition_id}`; FK `InstitutionModule.ModuleDefinitionId
      → ModuleDefinition.Id` com `DeleteBehavior.Restrict`; FK
      `ModuleDefinition.RequiredPermissionId → Permission.Id`; `maxLength` em todos os
      campos de texto (`key` 50, `name` 100, `description` 300, `icon` 50, `path` 100,
      `operational_message` 280); `CHECK ck_institutionmodule_operational_state
      (operational_state BETWEEN 0 AND 3)` — primeiro CHECK constraint do repositório.
- [x] 2.3 Implementar provisionamento idempotente das definições aprovadas de assistência e estoque, inicialmente desabilitadas para cada instituição.
      **Evidência**: `Services/Entities/InstitutionModuleProvisioningService.cs`
      (`RunAsync`) cria só os pares `{instituição, ModuleDefinition ativa}` que
      faltarem, sempre como `OperationalState.DISABLED`/`IsEnabled=false`; registrado
      em `Startup.cs` (`IInstitutionModuleProvisioningService`) e disparado em
      `Program.cs` logo após o bootstrap do admin. Seed das 2 definições aprovadas
      (`care`, `stock`) via `HasData` em `ModuleDefinitionBuilder`. Idempotência
      coberta por `InstitutionModuleProvisioningServiceTests.RunAsync_CalledTwice_DoesNotDuplicateRows`.
- [x] 2.4 Implementar validação de chave, ícone, permissão e caminho relativo por allowlist, rejeitando origens externas, HTML e destinos desconhecidos.
      **Evidência**: `Infrastructure/Validation/ModuleDefinitionValidator.cs`. Allowlist
      (não denylist) — chave via regex `^[a-z][a-z0-9-]{1,49}$`, ícone contra o conjunto
      fixo já usado pelos front-ends (`HeartStraight`, `Package`), caminho contra
      prefixos fixos (`/care`, `/stock`) rejeitando esquema (`://`), origem
      protocol-relative (`//`) e qualquer coisa fora de `/`, e permissão contra o
      conjunto de `Permission.Id` existentes.
- [x] 2.5 Implementar validação e sanitização das mensagens operacionais para impedir conteúdo pessoal, clínico ou técnico sensível.
      **Evidência**: `Infrastructure/Validation/OperationalMessageSanitizer.cs` rejeita
      HTML (`<`/`>`), URL (`https?://`) e reusa o mesmo vocabulário denylist de
      `.github/scripts/check-clinical-scope.sh` para termos clínicos/pessoais
      (prontuário, assinatura, conselho profissional, etc.), além do limite de 280
      caracteres já reforçado pelo schema.
- [x] 2.6 Cobrir entidades, migração, seeds e validações com testes unitários e de integração em PostgreSQL.
      **Evidência**: unitários — `ModuleDefinitionValidatorTests.cs`,
      `OperationalMessageSanitizerTests.cs`. Integração (Testcontainers/PostgreSQL) —
      `IntegrationTests/Data/SeniorPortalCatalogPersistenceTests.cs` (unicidade de
      `Key`, unicidade do par instituição/módulo, FK inválida, CHECK de
      `operational_state` fora do range, concorrência otimista) e
      `IntegrationTests/Services/InstitutionModuleProvisioningServiceTests.cs`
      (provisionamento em instituição nova, idempotência). Rodado `dotnet test` na
      solução inteira — 52/52 testes unitários e 125/125 de integração aprovados, 0
      falhas.

## 3. APIs e autorização do catálogo

- [x] 3.1 Implementar o serviço que combina identidade, instituição, conta ativa, permissões efetivas e configuração para produzir o catálogo mínimo do usuário.
      **Evidência**: `Services/Entities/ModuleCatalogService.cs`
      (`GetForCurrentUserAsync`). Instituição via `ICurrentUserContext`, conta ativa via
      `IAccessDecisionService.EvaluateAsync` (nega `INVALID_CONTEXT` se
      `AccountState != ACTIVE` — não duplicado aqui), permissão efetiva avaliada por
      módulo a partir de `ModuleDefinition.RequiredPermission.{Resource,Action}` (mesma
      precedência de todo o resto do sistema, nunca reimplementada). Configuração via
      join `InstitutionModule`+`ModuleDefinition` da instituição atual.
- [x] 3.2 Implementar `GET /api/v1/me/modules` com ordenação determinística e omissão de módulos desabilitados ou não autorizados.
      **Evidência**: `Controllers/ModuleCatalogController.cs` (rota literal
      `api/v1/me/modules`, contrato exato de design.md decisão 5). Omite
      `IsEnabled=false`, `OperationalState=DISABLED`, `ModuleDefinition.IsActive=false` e
      qualquer módulo sem permissão efetiva concedida; ordena por `Order` e depois por
      `Key` (desempate determinístico). Resposta mínima: chave, apresentação, caminho,
      ordem, estado operacional e mensagem operacional.
- [x] 3.3 Implementar APIs administrativas versionadas para consultar e alterar habilitação, ordem, estado e mensagem operacional com permissão específica.
      **Evidência**: `Controllers/AdminInstitutionModuleController.cs`
      (`api/v1/AdminInstitutionModule`, GET/GET-by-id/PUT — sem POST/DELETE, já que
      `InstitutionModule` é só provisionado, nunca criado/excluído por API, §2.3/§3.3).
      Nova permissão `InstitutionModule` `read`/`write` em `PermissionBuilder.cs`
      (distinta de `Module/care`/`Module/stock`, que gateiam visibilidade do usuário
      final, não administração), seedada pela migração
      `20260812110359_AddInstitutionModuleAdminPermissions.cs`. `PUT` valida a mensagem
      operacional via `OperationalMessageSanitizer` (§2.5) antes de persistir.
- [x] 3.4 Aplicar concorrência otimista e respostas de conflito nas alterações administrativas sem sobrescrever silenciosamente uma versão mais nova.
      **Evidência**: `AdminInstitutionModuleController.Put` define
      `Entry(...).Property<uint>("Version").OriginalValue = request.RowVersion` (mesmo
      mecanismo de `GenericRepository.Update`) antes do `SaveChangesAsync`, garantindo
      que o `SaveChanges` compare contra a versão que o cliente leu, não uma releitura
      implícita. `DbUpdateConcurrencyException` já mapeada para 409 em
      `GlobalExceptionHandler.cs` (reaproveitada, não duplicada). Testado em
      `AdminInstitutionModuleControllerTests.Put_StaleRowVersion_ReturnsConflict`.
- [x] 3.5 Auditar alterações do catálogo, mudanças de estado, acessos negados e redirecionamentos rejeitados com ator, instituição, módulo, resultado e correlação, sem tokens ou dados clínicos.
      **Evidência**: toda alteração admin grava `AuditEventCategory.CATALOG`
      (`AdminInstitutionModuleController.Put`, before/after com
      IsEnabled/Order/OperationalState/OperationalMessage, `TargetScopeKey` = chave do
      módulo). Acesso negado já é automático via `RequirePermissionAttribute`
      (`AuditEventCategory.ACCESS_DECISION` em toda checagem `[RequirePermission]`,
      inclusive nos novos endpoints) — não duplicado. "Redirecionamentos rejeitados" é
      responsabilidade do validador central de `return path` no front-end (tarefa 4.5,
      fora do escopo desta seção de backend) — decisão registrada aqui para não ficar
      implícita. Correlação: `TraceIdentifier`, já automático em `AuditService`
      (nenhuma mudança necessária). Testado em
      `AdminInstitutionModuleControllerTests.Put_ValidUpdate_ChangesStateAndRecordsAudit`.
- [x] 3.6 Garantir que os endpoints dos módulos e da API continuem revalidando autorização, independentemente da visibilidade fornecida pelo catálogo.
      **Evidência**: `Module/care` e `Module/stock` (visibilidade no catálogo) e as
      permissões de domínio reais (ex.: `Product/read`) são checadas
      independentemente — `ModuleCatalogService` nunca concede nada, só lê decisões já
      calculadas por `IAccessDecisionService` para os endpoints de fato protegidos por
      `[RequirePermission]`. Provado por
      `ModulePermissionIsolationTests`: conceder `Module/care` sem `Product/read`
      mantém `GET /api/v1/Product` em 403; conceder `Product/read` sem `Module/care`
      mantém `GET /api/v1/me/modules` vazio.
- [x] 3.7 Criar testes de integração para isolamento institucional, conta bloqueada, permissão concedida/revogada, estados operacionais, validação, concorrência e auditoria.
      **Evidência**: `ModuleCatalogControllerTests.cs` (permissão concedida/revogada,
      módulo nunca habilitado, `MAINTENANCE` com mensagem, isolamento entre duas
      instituições, ordenação determinística, conta `BLOCKED`) e
      `AdminInstitutionModuleControllerTests.cs` (permissão de leitura concedida/negada,
      atualização válida + auditoria, `RowVersion` obsoleto → 409, mensagem com termo
      clínico → 422, módulo de outra instituição → 404). 15 testes novos, todos
      passando junto com a suíte completa (52/52 unitários, 141/141 de integração).
- [x] 3.8 Atualizar a especificação OpenAPI e os exemplos de resposta sem expor regras internas de autorização ou dados dos domínios.
      **Evidência**: OpenAPI é gerado automaticamente por Swashbuckle a partir dos
      `ActionResult<T>` dos controllers (sem YAML/JSON mantido à mão neste repositório;
      `Startup.cs` publica `/swagger/v1/swagger.json` em todo ambiente) — nenhuma
      mudança de configuração necessária. Contrato verificado por um novo teste,
      `OpenApiContractTests.SeniorPortalCatalog_ContractExposesExpectedRoutesAndVerbs`,
      confirmando as rotas/verbos esperados (`GET /api/v1/me/modules` só leitura;
      `AdminInstitutionModule` sem POST/DELETE).

## 4. Base da aplicação Senior Portal

- [x] 4.1 Criar a aplicação React/TypeScript/Vite independente do Senior Portal com lint, testes, build reprodutível e configuração de runtime externa ao bundle.
      **Evidência**: `SeniorPortal-Frontend/SeniorPortalFrontend/` — mesmo scaffold
      (package.json/vite.config.ts/tsconfig/eslint/tailwind) de care-web/stock-web,
      só o `name` muda, mais `VITE_BASE_PATH` (build-time, preparando o roteamento
      por caminho de §8). Config de runtime fora do bundle: `public/public-config.json`
      (chave `publicName`, fallback `"SeniorCare"`) lido por
      `RuntimeConfigContext.tsx` via `fetch('/public-config.json', {cache:
      'no-store'})` — arquivo estático copiado verbatim para `dist/` pelo Vite
      (confirmado no build), nunca inlinado no JS; o entrypoint do container que
      o sobrescreve a partir de env var é implementação de §8. `npm run
      lint`/`test`/`build` rodados localmente: lint 0 erros (só 4 avisos
      `react-refresh` de uma versão mais nova do plugin, mesmo padrão de
      arquivo de care-web), build reproduz `dist/` completo,
      `check-frontend-bundle.sh` e `check-clinical-scope.sh` (path do portal
      adicionado ao scanner) passam contra o bundle/código gerados.
- [x] 4.2 Implementar cliente HTTP e estado de autenticação para restaurar acesso curto em memória pela sessão protegida, sem `localStorage`, `sessionStorage` ou cookie legível por script.
      **Evidência**: investigação direta (antes de implementar) encontrou que o
      modelo de "acesso curto emitido separadamente e guardado em memória"
      descrito em `design.md` decisão 3 nunca foi construído no backend — o que
      existe (`AuthController.cs`, `Startup.cs`) é um único cookie de sessão
      `HttpOnly`/`Secure`/`SameSite=Strict` com rotação silenciosa no servidor,
      já usado por care-web/stock-web (`withCredentials: true`, sem
      Authorization header, sem token em memória). Esse modelo satisfaz o
      objetivo de segurança da tarefa com margem maior (zero token passa pelo
      JavaScript, não só "por pouco tempo") — decisão confirmada com o usuário:
      reusar o padrão real em vez de construir o endpoint de emissão de token
      que o design.md descrevia. `features/api/api.ts` (idêntico ao padrão
      existente) + `contexts/AuthContext.tsx` (`refresh` chama `GET /auth/me`).
- [x] 4.3 Implementar login, etapa de MFA pendente, expiração não renovável e limpeza completa do contexto local.
      **Evidência**: `features/auth/pages/LoginPage.tsx`+`LoginForm`,
      `MfaChallengePage.tsx` (`mfa_required`), `MfaEnrollPage.tsx`
      (`mfa_enrollment_required` — cadastro completo, já que sem isso um
      usuário sem MFA configurado ficaria num beco sem saída). "Expiração não
      renovável": `AuthContext.tsx` nunca tenta renovar a sessão por conta
      própria — qualquer 401 fora do bootstrap de login/restauração
      (`registerUnauthorizedHandler`) limpa `identity` inteiro e manda pro
      `/login`, mesmo mecanismo do logout explícito.
- [x] 4.4 Implementar carregamento do contexto institucional explícito e impedir renderização do catálogo diante de divergência ou sessão inválida.
      **Evidência**: `features/auth/components/RequireAuth.tsx` — sessão em
      restauração aguarda; anônima redireciona pro `/login` (preservando
      destino via `returnTo`); autenticada mas sem `identity.institutionId`
      (spec.md "Contexto institucional divergente") bloqueia a renderização com
      mensagem segura, sem detalhes internos, em vez de arriscar renderizar
      algo institucionalmente incoerente.
- [x] 4.5 Implementar validação central de `return path` relativo contra rotas permitidas, com fallback para `/` e registro de rejeições.
      **Evidência**: `utils/returnPath.ts` (`isSafeReturnPath`/`resolveReturnPath`)
      — implementa o contrato de `docs/architecture/senior-portal-contracts.md`
      §3 ao pé da letra: exatamente `/` ou sob `/care`/`/stock`; rejeita `//`,
      esquema (`http(s)://`, `javascript:`, etc.) e `\`; fallback `/`; loga a
      rejeição. Função pura, pensada para ser reusada por care/stock quando
      §6.4/§7.4 migrarem seus próprios deep links — ainda só consumida pelo
      portal (`LoginForm`, `MfaChallengePage`) nesta seção.
- [x] 4.6 Criar testes unitários dos estados de autenticação, restauração, MFA, instituição e redirecionamento seguro.
      **Evidência**: 42 testes novos (`vitest`) — `AuthContext.test.tsx`
      (restauração via `/me`, estado anônimo, expiração não renovável, guarda
      contra loop de redirecionamento), `RequireAuth.test.tsx` (loading,
      anônimo, contexto institucional divergente), `LoginForm.test.tsx` (login
      ok, credenciais inválidas, MFA obrigatório, cadastro de MFA obrigatório,
      `returnTo` válido e rejeitado), `MfaChallengePage.test.tsx`,
      `MfaEnrollPage.test.tsx`, `RuntimeConfigContext.test.tsx` (fallback em
      arquivo ausente/inválido/rede indisponível), `returnPath.test.ts`
      (allowlist e rejeições, tabela exaustiva). `npm run test` — 42/42
      aprovados; `npm run test:coverage` roda sem erro.

## 5. Experiência do catálogo e funções globais

- [x] 5.1 Implementar catálogo responsivo consumindo apenas `GET /api/v1/me/modules`, sem consultas de residente, prontuário, finanças ou outros dados de negócio.
      **Evidência**: `features/catalog/services/moduleCatalogService.ts` +
      `features/catalog/pages/CatalogPage.tsx` — única chamada de rede é `GET
      me/modules`; grid responsivo via Tailwind
      (`grid-cols-1 sm:grid-cols-2 lg:grid-cols-3`). Nenhum outro serviço/rota
      de domínio é importado por este módulo.
- [x] 5.2 Implementar cards ordenados para `AVAILABLE`, `MAINTENANCE` e `UNAVAILABLE`, sem renderizar `DISABLED` e sem permitir abertura normal dos estados não disponíveis.
      **Evidência**: `features/catalog/components/ModuleCard.tsx` — só
      `AVAILABLE` renderiza um `<a href={module.path}>` (navegação normal,
      mesma origem); `MAINTENANCE`/`UNAVAILABLE` renderizam um `<div>` não
      interativo com a mensagem operacional, sem link. `CatalogPage.tsx`
      ordena por `Order` e filtra `DISABLED` como defesa em profundidade
      própria do front-end (o backend já omite esse estado de `/me/modules`,
      §3.2) — testado em
      `CatalogPage.test.tsx.never renders a DISABLED module...`.
- [x] 5.3 Implementar estados de carregamento, vazio, sessão expirada e falha recuperável com retorno seguro e identificador de correlação quando fornecido.
      **Evidência**: `CatalogPage.tsx` — carregamento (`role="status"`),
      vazio (nenhum módulo, mensagem própria, não é erro), falha recuperável
      (`role="alert"`, mensagem do `ProblemDetails.detail`, `correlationId`
      quando o backend fornece — `ServiceResult`/`handleServiceError`
      estendidos em `serviceUtils.ts` pra propagá-lo — e botão "Tentar
      novamente"). Sessão expirada já é tratada por `RequireAuth`/
      `AuthContext` (§4.3/§4.4): qualquer 401 fora do bootstrap limpa o
      contexto e redireciona pro `/login` com retorno seguro
      (`resolveReturnPath`, §4.5) — não duplicado aqui.
- [x] 5.4 Implementar perfil, segurança da conta, preferências de contraste e fonte, e logout compartilhado conforme o contrato global.
      **Evidência**: `features/auth/pages/ProfilePage.tsx` (`/profile`) e
      `SecurityPage.tsx` (`/security`, regeneração de códigos de recuperação
      MFA — único contrato de "segurança da conta" já pronto no backend;
      troca de senha continua em care-web/stock-web, mesma decisão de §4.2
      registrada para `authService`). `features/preferences/components/
      PreferencesControls.tsx` liga contraste/fonte ao `ThemeContext` já
      existente (§4), sem chave nova. Logout compartilhado via
      `GlobalHeader.tsx` (`useAuth().logout()`, mesma chamada
      `POST /auth/logout` de §4.3), presente em toda rota autenticada.
- [x] 5.5 Aplicar tokens visuais e nomenclatura global documentados, sem criar dependência de um design system ou runtime de microfrontend.
      **Evidência**: `features/layout/GlobalHeader.tsx` +
      `AuthenticatedLayout.tsx` — navegação global (Catálogo/Perfil/
      Segurança/preferências/logout) presente em toda rota autenticada,
      reusando os tokens de cor/fonte já trazidos de care-web em §4 (nenhuma
      paleta nova). Sem pacote de design system nem runtime de
      microfrontend — só componentes React locais, conforme design.md
      decisão 8.
- [x] 5.6 Validar teclado, foco, nomes acessíveis, contraste, mensagens independentes de cor e ordem assistiva em celular, tablet e desktop.
      **Evidência**: módulos disponíveis são `<a>` nativos (foco/ativação por
      teclado de graça, `focus:outline` explícito); estados não disponíveis
      usam ícone **e** texto (nunca só cor — `Wrench`/`WarningCircle` +
      rótulo, spec.md "Estado de manutenção"); toda entrada de formulário tem
      `<label>` associado; grid responsivo sem reordenar visualmente por
      CSS (ordem assistiva = ordem do DOM = ordem visual). `jest-axe`
      (`toHaveNoViolations`) cobre `CatalogPage` (pronto), `GlobalHeader`,
      `PreferencesControls` e `ProfilePage` — 0 violação em todos.
- [x] 5.7 Criar testes de componentes e jornadas do catálogo, estados operacionais, preferências e funções globais.
      **Evidência**: 17 testes novos — `CatalogPage.test.tsx` (carregamento,
      vazio, `AVAILABLE`/`MAINTENANCE` lado a lado com ordenação, filtro de
      `DISABLED`, falha com `correlationId` e retry, a11y),
      `GlobalHeader.test.tsx` (navegação + logout, a11y),
      `PreferencesControls.test.tsx` (contraste, tamanho de fonte, limites, a11y),
      `ProfilePage.test.tsx` (identidade restaurada, estado vazio, a11y),
      `SecurityPage.test.tsx` (senha incorreta, sucesso com códigos novos, a11y).
      Total do app: 61/61 testes aprovados, lint/build/gates de higiene
      (`check-frontend-bundle.sh`, `check-clinical-scope.sh`) revalidados.
      Achado durante a implementação: Node 22+ expõe um `localStorage`
      experimental que quebra `window.localStorage` sob jsdom sem
      `--localstorage-file` — poliflhado uma vez em `src/test/setup.ts`
      (não é bug do componente; `ThemeContext.tsx` nunca tinha sido
      exercitado por um teste antes desta seção).

## 6. Integração do módulo assistencial

- [x] 6.1 Configurar build, assets e roteamento do front-end assistencial para o caminho-base `/care`, incluindo refresh e acesso direto a rotas profundas.
      **Evidência**: `vite.config.ts` ganhou `base` (mesma variável
      `VITE_BASE_PATH` do portal, §4.1; default `/` preserva o deploy atual
      por subdomínio — migrar a borda é §8.2). `routes/AppRoutes.tsx` ganhou
      `basename` no `createBrowserRouter`, derivado da mesma variável, pra
      casar as rotas internas (`/religion` etc.) contra a URL real depois que
      a borda migrar. Testado localmente: `VITE_BASE_PATH=/care/ npm run
      build` produz `index.html` com todas as referências de asset
      corretamente prefixadas (`/care/assets/...`, `/care/vite.svg`); build
      com a variável ausente (default) reproduz o `dist/` de sempre, sem
      diferença. SPA fallback (`nginx.conf`, `try_files ... /index.html`) já
      cobre "acesso direto a rotas profundas" e refresh — nenhuma mudança
      necessária aí (a borda ainda roteia por subdomínio; migrar
      `nginx.conf`/Caddy pra `/care` é §8.2, não duplicado aqui).
- [x] 6.2 Substituir o uso de `auth_token` legível por JavaScript pela restauração de acesso curto em memória usando a sessão institucional.
      **Evidência**: investigação direta (grep de `auth_token`,
      `localStorage.*token`, `sessionStorage.*token`, `Authorization` em todo
      `src/`) confirmou que este front-end **já não usa** `auth_token` — já
      migrado para o único cookie de sessão `HttpOnly` (mesmo modelo
      confirmado em §4.2), com o interceptor de `Authorization: Bearer`
      removido e documentado em `features/api/api.ts:6-10`:
      *"nunca houve (nem haverá) token/cookie 'auth_token'; o backend nem lê
      esse header"*. Nenhuma mudança de código necessária — tarefa já
      satisfeita antes de `introduce-senior-portal` começar.
- [x] 6.3 Adicionar retorno consistente ao portal e links para perfil, segurança e logout sem duplicar regras de credencial.
      **Evidência**: `features/layouts/components/Header/index.tsx` — links
      "Portal" (`/`), "Perfil" (`/profile`) e "Segurança" (`/security`, rotas
      do próprio Senior Portal, §5.4) adicionados ao lado do botão "Sair" já
      existente, mesma navegação de mesma origem — nenhuma regra de sessão
      duplicada (login/logout continuam só no `AuthContext`/`authService`
      já existentes).
- [x] 6.4 Preservar deep links autorizados após login ou renovação e rejeitar destinos externos, desconhecidos ou sem permissão.
      **Evidência**: o mecanismo interno (`location.state.from`, sintetizado
      só por `RequireAuth.tsx`, nunca alcançável por URL) já preservava deep
      links internos — mantido sem alteração. Novo: `utils/returnPath.ts`
      (mesmo contrato de `docs/architecture/senior-portal-contracts.md` §3 e
      do validador do portal, §4.5 — duplicado deliberadamente, sem pacote
      compartilhado ainda, design.md decisão 8) valida um `?returnTo=`
      cruzado (portal → care) antes de `LoginForm` navegar, com prioridade:
      `location.state.from` (mais específico) > `returnTo` validado > destino
      padrão do app. Destino externo/desconhecido em `returnTo` é
      descartado sem navegar (fallback pro destino padrão, nunca um 404 ou
      redirecionamento aberto).
- [x] 6.5 Cobrir restauração, 401, 403, revogação, logout, retorno e deep links com testes automatizados do módulo assistencial.
      **Evidência**: restauração/401/403/revogação/logout já cobertos pela
      suíte existente (`AuthContext.test.tsx`, `RequireAuth.test.tsx`,
      `Header.test.tsx`), revalidada sem regressão. Testes novos: 3 casos em
      `Header.test.tsx` (links Portal/Perfil/Segurança), 6 em
      `returnPath.test.ts` (allowlist e rejeições) e `LoginForm.test.tsx`
      (3 novos: `returnTo` válido, `returnTo` rejeitado cai no padrão,
      `location.state.from` tem prioridade sobre `returnTo`). 62/62 testes
      do app aprovados (19 novos), lint/build/`npm audit`/gates de higiene
      revalidados.

## 7. Integração do módulo de estoque

- [x] 7.1 Configurar build, assets e roteamento do front-end de estoque para o caminho-base `/stock`, incluindo refresh e acesso direto a rotas profundas.
      **Evidência**: mesmo padrão de §6.1 — `vite.config.ts` ganha `base`
      (`VITE_BASE_PATH`) e `routes/AppRoutes.tsx` ganha `basename` derivado
      da mesma variável no `createBrowserRouter`; default `/` preserva o
      deploy atual por subdomínio. Testado: `VITE_BASE_PATH=/stock/ npm run
      build` produz `index.html` com assets corretamente prefixados
      (`/stock/assets/...`, `/stock/vite.svg`); build default reproduz o
      `dist/` de sempre. SPA fallback (`nginx.conf`) já cobre refresh/acesso
      direto — migrar a borda pra `/stock` é §8.2, não duplicado aqui.
- [x] 7.2 Substituir o uso de `auth_token` legível por JavaScript pela restauração de acesso curto em memória usando a sessão institucional.
      **Evidência**: investigação direta (grep de `auth_token`,
      `localStorage.*token`, `sessionStorage.*token`, `Authorization`)
      confirmou que este front-end já usa só o cookie de sessão `HttpOnly`
      compartilhado — `features/api/api.ts:7-11` documenta o mesmo modelo
      (é, aliás, o comentário ORIGINAL que o de care-web depois referenciou).
      Nenhuma mudança de código necessária.
- [x] 7.3 Adicionar retorno consistente ao portal e links para perfil, segurança e logout sem duplicar regras de credencial.
      **Evidência**: `features/layout/components/Header/index.tsx` — mesmos
      links "Portal"/"Perfil"/"Segurança" de §6.3, mesma navegação de mesma
      origem, nenhuma regra de sessão duplicada.
- [x] 7.4 Preservar deep links autorizados após login ou renovação e rejeitar destinos externos, desconhecidos ou sem permissão.
      **Evidência**: `utils/returnPath.ts` — mesmo contrato de §4.5/§6.4,
      já incorporando a correção encontrada pelo review de §6: destino
      cruzado (fora do próprio router deste app) usa
      `window.location.assign()` (navegação de página inteira), nunca
      `navigate()` do React Router (que não alcança rota fora do próprio
      bundle/router — portal/care/stock são apps React separados, design.md
      decisão 1). Aplicado em `LoginForm.tsx`, `MfaChallengePage.tsx` e
      `MfaEnrollPage.tsx` (returnTo sobrevive à etapa de MFA via query
      string). `location.state.from` (interno, nunca alcançável por URL)
      mantém prioridade.
- [x] 7.5 Cobrir restauração, 401, 403, revogação, logout, retorno e deep links com testes automatizados do módulo de estoque.
      **Evidência**: restauração/401/403/revogação/logout já cobertos pela
      suíte existente, revalidada sem regressão. Testes novos: 1 em
      `Header.test.tsx` (links Portal/Perfil/Segurança), 6 em
      `returnPath.test.ts` (allowlist e rejeições), 3 em `LoginForm.test.tsx`
      (returnTo cruzado via `window.location.assign` — espionado
      diretamente, sem rota fake que mascare o bug corrigido em §6 —,
      returnTo rejeitado cai no padrão, `location.state.from` tem
      prioridade). Achado do review da PR #114 (não bloqueante): faltava
      cobertura do mesmo `returnTo` cruzado sobrevivendo à etapa de MFA —
      2 testes novos em `MfaChallengePage.test.tsx` e
      `MfaEnrollPage.test.tsx` criado do zero (não existia antes deste
      change) com 3 testes. 65/65 testes do app aprovados (15 novos no
      total), lint/build/`npm audit`/gates de higiene revalidados.

## 8. Implantação, segurança e observabilidade

- [x] 8.1 Adicionar imagem e serviço do portal ao Docker Compose sem remover as entradas legadas durante a fase aditiva.
      **Evidência**: `SeniorPortal-Frontend/SeniorPortalFrontend/{Dockerfile,
      nginx.conf, docker-entrypoint.d/10-public-config.sh}` novos, mesmo
      padrão multi-stage de care-web/stock-web + injeção de `public-config.json`
      em runtime (`PUBLIC_NAME`, contrato §4.1). `package-lock.json` do portal
      tinha entradas de plataforma opcional do esbuild corrompidas
      (`"extraneous": true` em vez de `"optional": true`) — regenerado dentro de
      um container `node:22-alpine` (mesma plataforma do build real), restaurando
      as 49 entradas esperadas (igual a care-web). Serviço `senior-portal`
      adicionado a `infra/deploy/docker-compose.yml` (prod, imagem pinada por
      `SENIOR_PORTAL_IMAGE`) e `infra/docker-test/docker-compose.yml`
      (build-from-source) — nenhuma entrada legada removida. `release.yml` ganhou
      a 4ª entrada de matriz. Testado ponta a ponta: `npm ci`/`npm test`/`npm run
      build` locais OK (61/61), `docker build` da imagem sobe limpo, container
      rodando isoladamente serve `/` (200), `/public-config.json` reflete
      `PUBLIC_NAME` de ambiente com `Cache-Control: no-store`, healthcheck fica
      `healthy`.
- [x] 8.2 Configurar nginx para servir portal, API e módulos sob a mesma origem, com fallback de SPA restrito a cada caminho-base e cabeçalhos de segurança apropriados.
      **Evidência**: `infra/deploy/ops/Caddyfile` reescrito de roteamento por
      subdomínio para roteamento por caminho (`handle_path /care/*` e
      `/stock/*` com stripping de prefixo, `handle /api/*` sem stripping —
      mesmo contrato dos proxies internos de cada front-end —, `handle`
      catch-all pro portal na raiz), com cabeçalhos de segurança centralizados
      na borda. Validado com `caddy validate`/`caddy fmt` (config válida).
      Capacidade de build por caminho-base adicionada aos Dockerfiles de
      care-web/stock-web (`ARG VITE_BASE_PATH=/`, default preserva o
      comportamento atual) — testado com `docker build --build-arg
      VITE_BASE_PATH=/care/` (e `/stock/`) produzindo `index.html` com assets
      corretamente prefixados, e build default seguindo com caminhos
      root-relative (sem regressão). **Ativação real fica para §9.7**: o corte
      coordenado (build das imagens com o novo `VITE_BASE_PATH` + deploy deste
      Caddyfile, ao mesmo tempo) é ação de implantação em produção real, não
      código — documentado explicitamente no próprio Caddyfile e nos
      Dockerfiles.
- [x] 8.3 Validar sob TLS os atributos e o escopo do cookie de renovação, proteção CSRF, rotação, detecção de reutilização e logout iniciado em qualquer aplicação.
      **Evidência**: `CookieSecurityAttributesTests.cs` novo (8 testes) —
      Secure/HttpOnly/SameSite=Strict presentes no login, na rotação e no
      logout (cookie limpo com data no passado); logout revoga a sessão
      compartilhada independente da `Origin` (care/stock/portal simulados via
      header); preflight CORS aceito pra origem permitida e sem cabeçalho
      `Access-Control-Allow-Origin` pra origem não listada (a ausência, não um
      erro, é o que o navegador usa pra bloquear). `CORS_ALLOWED_ORIGINS`
      padrão de dev ganhou `localhost:3002` (porta do portal no docker-test).
      `CONFIGURATION.md` atualizado para citar os três front-ends. Decisão
      explícita registrada: nenhum middleware de CSRF novo — `SameSite=Strict`
      + allowlist de CORS já é a defesa (mesma decisão de antes do portal,
      apenas confirmada, não alterada). Rotação/reuso/logout/revogação por
      troca de senha já cobertos por `SessionRotationTests.cs`, revalidados
      sem regressão. 149/149 testes de integração + 52/52 unitários.
- [x] 8.4 Adicionar pipelines de lint, testes, build e análise de dependências do portal e ajustar a CI dos três artefatos para os caminhos-base.
      **Evidência**: `.github/workflows/ci.yml` — `senior_portal` no
      `paths-filter`, job `senior-portal` clonado do de `stock-web` (lint,
      `npm audit` prod/dev, `test:coverage`, build, `check-frontend-bundle.sh`),
      `ci-required` com `senior-portal` no `needs:` e no loop de checagem de
      resultado. Validado localmente reproduzindo cada passo do job
      (`npm ci`/`lint`/`npm audit` x2/`test:coverage`/`build`/
      `check-frontend-bundle.sh dist`) — todos OK, sem erro bloqueante (só
      vulnerabilidades moderadas pré-existentes em react-router/esbuild,
      compartilhadas com care-web/stock-web).
- [x] 8.5 Adicionar métricas e logs correlacionáveis para restauração de sessão, latência/erro do catálogo, redirecionamentos rejeitados, 401/403 e estado indisponível, sem dados sensíveis.
      **Evidência**: decisão confirmada de não introduzir stack de métricas
      nova (Prometheus/OpenTelemetry) — `ILogger` estruturado estendendo o
      `TraceIdentifier`/`ProblemDetails.correlationId` já existente.
      `Startup.ValidateSessionPrincipalAsync` loga restauração com rotação
      (Info) e rejeição/reuso (Warning), sempre com `CorrelationId`; sessionId
      logado (não sensível), chave de sessão nunca. `OnRedirectToLogin`/
      `OnRedirectToAccessDenied` logam 401/403 com Method/Path sanitizados
      (mesma proteção contra CWE-117 do `GlobalExceptionHandler`) — 403 por
      permissão já era coberto pelo audit `ACCESS_DECISION`
      (`RequirePermissionAttribute`, que já cita "§8.5" no próprio comentário
      — decisão pré-existente confirmada, não nova). `ModuleCatalogController`
      ganhou latência (Stopwatch) e contagem de módulos em estado não
      disponível no log de sucesso, e log de falha antes de repropagar a
      exceção pro `GlobalExceptionHandler`. Redirecionamentos rejeitados: já
      logados via `console.warn` nos três `returnPath.ts` desde §4.5/§6.4/§7.4
      (o comentário do portal já citava "§8.5" como o lugar formal desta
      decisão). 201/201 testes de backend revalidados sem regressão.
- [x] 8.6 Executar testes de segurança para open redirect, XSS armazenado na configuração, acesso cruzado entre instituições, enumeração de módulos e bypass por URL direta.
      **Evidência**: open redirect — já coberto exaustivamente em
      `returnPath.test.ts` dos três apps (URL absoluta, protocolo-relativo,
      `javascript:`, truques de backslash, prefixo parecido, string vazia).
      Acesso cruzado entre instituições — `Get_TwoInstitutions_
      DoesNotLeakAcrossInstitutions` e `Put_ModuleFromOtherInstitution_
      ReturnsNotFound` já existentes. Enumeração de módulos —
      `Get_ModuleWithoutPermission_IsOmitted`/`Get_ModuleNeverEnabled_IsOmitted`
      já existentes (módulo ausente, não 403, impede diferenciar "não existe"
      de "sem permissão" pela forma da resposta). Bypass por URL direta —
      `RequireAuth.test.tsx` já cobre acesso não autenticado à raiz do portal;
      dado que cada rota de domínio busca seus próprios dados via API
      permission-gated no backend, alcançar uma rota do SPA sem sessão nunca
      expõe dado (defesa real é backend, já testada nos controllers de
      domínio). XSS armazenado — novo: `Put_OperationalMessageWithScriptPayload_
      ReturnsUnprocessableEntity` (nível HTTP real, não só a unidade do
      validador já coberta em `OperationalMessageSanitizerTests`) prova que o
      payload nunca é persistido; novo teste em `CatalogPage.test.tsx`
      (`renders a malicious operationalMessage as inert text...`) prova que,
      mesmo que um payload escapasse do backend, o JSX de `ModuleCard`
      interpola como texto — nunca cria `<script>`/`<img onerror>` nem executa.
      150/150 testes de integração + 62/62 do portal, sem regressão.
- [x] 8.7 Executar smoke tests na imagem de produção para raiz, assets, refresh de deep link, navegação entre módulos, MFA, manutenção, logout e falha segura do IAM.
      **Evidência**: `infra/docker-test/smoke-test.sh` novo — script do zero
      (não existia nenhum antes), 20 checagens via `curl`/TOTP calculado
      localmente contra a stack `docker-test` real (Postgres + API + os 3
      front-ends, todos buildados das imagens de produção, não `npm run dev`).
      Rodado de ponta a ponta contra um stack limpo (`docker compose down -v
      && up -d --build`): raiz (200 nos 3 apps), assets hasheados (200),
      refresh de deep link em rota profunda de cada app (cai no shell da SPA,
      não 404 cru), primeiro acesso completo (ativação → login → enroll MFA →
      TOTP → confirm → `/Auth/me` 200), catálogo autenticado após habilitar um
      módulo, transição pra MAINTENANCE refletida no catálogo (revertida ao
      final), logout (200) seguido de `/Auth/me` 401, e falha segura do IAM
      (container da API parado: shell estático do portal continua servindo
      200, chamada a `/api/*` falha com 502 — não trava nem finge sucesso —,
      API restaurada e healthcheck confirmado ao final). 20/20 OK na execução
      limpa.

## 9. Migração, contingência e aceite

- [ ] 9.1 Habilitar assistência e estoque primeiro em homologação e comprovar que nenhum módulo apenas planejado aparece no catálogo operacional.
- [ ] 9.2 Implementar redirecionamentos dos logins e landing pages legados para o portal, preservando somente retornos internos validados e exigindo novo login para credenciais incompatíveis.
- [ ] 9.3 Documentar operação, configuração do catálogo, estados, auditoria, implantação, monitoramento, links diretos de contingência e rollback sem restaurar autenticação insegura.
- [ ] 9.4 Validar que a indisponibilidade do portal não invalida uma sessão existente nem impede acesso direto autorizado a módulos saudáveis.
- [ ] 9.5 Realizar aceite com representantes de administração, segurança, assistência e estoque cobrindo acessibilidade, clareza institucional e ausência de dados clínicos ou financeiros no portal.
- [ ] 9.6 Atualizar `docs/escopo-do-projeto.md` e a documentação arquitetural para distinguir Senior Portal interno, portal futuro de residentes/famílias e módulos futuros ainda não implementados.
- [ ] 9.7 Ativar o portal como raiz somente após os testes de homologação, registrar evidências de aceite e verificar os critérios de rollback durante a janela de observação.
