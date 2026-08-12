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

- [ ] 4.1 Criar a aplicação React/TypeScript/Vite independente do Senior Portal com lint, testes, build reprodutível e configuração de runtime externa ao bundle.
- [ ] 4.2 Implementar cliente HTTP e estado de autenticação para restaurar acesso curto em memória pela sessão protegida, sem `localStorage`, `sessionStorage` ou cookie legível por script.
- [ ] 4.3 Implementar login, etapa de MFA pendente, expiração não renovável e limpeza completa do contexto local.
- [ ] 4.4 Implementar carregamento do contexto institucional explícito e impedir renderização do catálogo diante de divergência ou sessão inválida.
- [ ] 4.5 Implementar validação central de `return path` relativo contra rotas permitidas, com fallback para `/` e registro de rejeições.
- [ ] 4.6 Criar testes unitários dos estados de autenticação, restauração, MFA, instituição e redirecionamento seguro.

## 5. Experiência do catálogo e funções globais

- [ ] 5.1 Implementar catálogo responsivo consumindo apenas `GET /api/v1/me/modules`, sem consultas de residente, prontuário, finanças ou outros dados de negócio.
- [ ] 5.2 Implementar cards ordenados para `AVAILABLE`, `MAINTENANCE` e `UNAVAILABLE`, sem renderizar `DISABLED` e sem permitir abertura normal dos estados não disponíveis.
- [ ] 5.3 Implementar estados de carregamento, vazio, sessão expirada e falha recuperável com retorno seguro e identificador de correlação quando fornecido.
- [ ] 5.4 Implementar perfil, segurança da conta, preferências de contraste e fonte, e logout compartilhado conforme o contrato global.
- [ ] 5.5 Aplicar tokens visuais e nomenclatura global documentados, sem criar dependência de um design system ou runtime de microfrontend.
- [ ] 5.6 Validar teclado, foco, nomes acessíveis, contraste, mensagens independentes de cor e ordem assistiva em celular, tablet e desktop.
- [ ] 5.7 Criar testes de componentes e jornadas do catálogo, estados operacionais, preferências e funções globais.

## 6. Integração do módulo assistencial

- [ ] 6.1 Configurar build, assets e roteamento do front-end assistencial para o caminho-base `/care`, incluindo refresh e acesso direto a rotas profundas.
- [ ] 6.2 Substituir o uso de `auth_token` legível por JavaScript pela restauração de acesso curto em memória usando a sessão institucional.
- [ ] 6.3 Adicionar retorno consistente ao portal e links para perfil, segurança e logout sem duplicar regras de credencial.
- [ ] 6.4 Preservar deep links autorizados após login ou renovação e rejeitar destinos externos, desconhecidos ou sem permissão.
- [ ] 6.5 Cobrir restauração, 401, 403, revogação, logout, retorno e deep links com testes automatizados do módulo assistencial.

## 7. Integração do módulo de estoque

- [ ] 7.1 Configurar build, assets e roteamento do front-end de estoque para o caminho-base `/stock`, incluindo refresh e acesso direto a rotas profundas.
- [ ] 7.2 Substituir o uso de `auth_token` legível por JavaScript pela restauração de acesso curto em memória usando a sessão institucional.
- [ ] 7.3 Adicionar retorno consistente ao portal e links para perfil, segurança e logout sem duplicar regras de credencial.
- [ ] 7.4 Preservar deep links autorizados após login ou renovação e rejeitar destinos externos, desconhecidos ou sem permissão.
- [ ] 7.5 Cobrir restauração, 401, 403, revogação, logout, retorno e deep links com testes automatizados do módulo de estoque.

## 8. Implantação, segurança e observabilidade

- [ ] 8.1 Adicionar imagem e serviço do portal ao Docker Compose sem remover as entradas legadas durante a fase aditiva.
- [ ] 8.2 Configurar nginx para servir portal, API e módulos sob a mesma origem, com fallback de SPA restrito a cada caminho-base e cabeçalhos de segurança apropriados.
- [ ] 8.3 Validar sob TLS os atributos e o escopo do cookie de renovação, proteção CSRF, rotação, detecção de reutilização e logout iniciado em qualquer aplicação.
- [ ] 8.4 Adicionar pipelines de lint, testes, build e análise de dependências do portal e ajustar a CI dos três artefatos para os caminhos-base.
- [ ] 8.5 Adicionar métricas e logs correlacionáveis para restauração de sessão, latência/erro do catálogo, redirecionamentos rejeitados, 401/403 e estado indisponível, sem dados sensíveis.
- [ ] 8.6 Executar testes de segurança para open redirect, XSS armazenado na configuração, acesso cruzado entre instituições, enumeração de módulos e bypass por URL direta.
- [ ] 8.7 Executar smoke tests na imagem de produção para raiz, assets, refresh de deep link, navegação entre módulos, MFA, manutenção, logout e falha segura do IAM.

## 9. Migração, contingência e aceite

- [ ] 9.1 Habilitar assistência e estoque primeiro em homologação e comprovar que nenhum módulo apenas planejado aparece no catálogo operacional.
- [ ] 9.2 Implementar redirecionamentos dos logins e landing pages legados para o portal, preservando somente retornos internos validados e exigindo novo login para credenciais incompatíveis.
- [ ] 9.3 Documentar operação, configuração do catálogo, estados, auditoria, implantação, monitoramento, links diretos de contingência e rollback sem restaurar autenticação insegura.
- [ ] 9.4 Validar que a indisponibilidade do portal não invalida uma sessão existente nem impede acesso direto autorizado a módulos saudáveis.
- [ ] 9.5 Realizar aceite com representantes de administração, segurança, assistência e estoque cobrindo acessibilidade, clareza institucional e ausência de dados clínicos ou financeiros no portal.
- [ ] 9.6 Atualizar `docs/escopo-do-projeto.md` e a documentação arquitetural para distinguir Senior Portal interno, portal futuro de residentes/famílias e módulos futuros ainda não implementados.
- [ ] 9.7 Ativar o portal como raiz somente após os testes de homologação, registrar evidências de aceite e verificar os critérios de rollback durante a janela de observação.
