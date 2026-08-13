# Operação do Senior Portal

> **Documento operacional**, produzido pela §9.3 (Migração, contingência e
> aceite) da mudança OpenSpec `introduce-senior-portal`. Voltado a quem
> administra uma instituição em produção (configurar catálogo, investigar um
> incidente, ou decidir um rollback) — não repete decisões de arquitetura já
> registradas em `docs/architecture/senior-portal-contracts.md` (rotas,
> contrato de contexto) nem passos de implantação já cobertos por
> `infra/deploy/README.md`/`BOOTSTRAP.md`. Este documento distingue o Senior
> Portal (interno) do portal futuro de residentes/famílias — ver
> `docs/escopo-do-projeto.md` seção 12.3.

## 1. Visão geral

O Senior Portal é o ponto de entrada único da equipe já autenticada: um
catálogo de módulos (hoje `care` e `stock`) que substitui logins e landing
pages separados de cada front-end. Ele não é um sistema de dados próprio —
não persiste nem exibe prontuário, financeiro ou qualquer dado assistencial;
só o catálogo (nome, descrição, estado operacional) e a navegação para os
módulos que a conta já tem permissão de acessar (spec.md, seção "Senior
Portal").

## 2. Configuração do catálogo de módulos

Um `InstitutionModule` é criado automaticamente (provisionamento idempotente,
`InstitutionModuleProvisioningService`, §2.3) para cada `ModuleDefinition`
ativa do sistema, **sempre desabilitado por padrão** (`IsEnabled = false`,
`OperationalState = DISABLED`). Nenhum módulo aparece no catálogo de um
usuário até um administrador habilitá-lo explicitamente — isso é o que
impede um módulo "só planejado" (existente no sistema, mas ainda não
oferecido pela instituição) de vazar pro catálogo operacional (§9.1).

Para configurar:

- **Endpoint**: `GET/PUT /api/v1/AdminInstitutionModule` (lista) e
  `GET/PUT /api/v1/AdminInstitutionModule/{id}` (item), implementados em
  `AdminInstitutionModuleController.cs`.
- **Permissão exigida**: `InstitutionModule` / `read` para consultar,
  `InstitutionModule` / `write` para alterar (`RequirePermissionAttribute`,
  mesmo mecanismo de RBAC usado no resto do sistema — não há UI própria de
  administração ainda; a chamada precisa ser feita via cliente HTTP
  autenticado com a sessão institucional).
- **Campos editáveis por módulo**: `isEnabled`, `order` (posição no
  catálogo), `operationalState`, `operationalMessage`.
- **Concorrência otimista**: todo `PUT` exige o `rowVersion` mais recente
  (lido no `GET` anterior) — uma versão desatualizada retorna `409 Conflict`
  em vez de sobrescrever silenciosamente (`Put_StaleRowVersion_ReturnsConflict`).
- Não existe `POST`/`DELETE`: linhas nunca são criadas ou excluídas por este
  controller, só provisionadas automaticamente (design.md decisão 4-5).

`AdminInstitutionModuleControllerTests.cs` é a especificação executável mais
precisa desse comportamento — inclui isolamento entre instituições
(`Put_ModuleFromOtherInstitution_ReturnsNotFound`) e rejeição de mensagem
operacional com HTML ou termo clínico (§3, §8.6).

## 3. Estados operacionais

`OperationalState` (`InstitutionModule.OperationalState`, valores fixos por
constraint no banco — mudar exige migração nova, não só recompilar):

| Valor | Efeito no catálogo | Quando usar |
|---|---:|---|
| `AVAILABLE` (0) | módulo aparece como link clicável | operação normal |
| `MAINTENANCE` (1) | módulo aparece como card não clicável, com `operationalMessage` | manutenção programada, sabida com antecedência |
| `UNAVAILABLE` (2) | módulo aparece como card não clicável, com `operationalMessage` | indisponibilidade não programada (incidente, dependência externa fora do ar) |
| `DISABLED` (3) | módulo **não aparece** no catálogo | módulo nunca oferecido pela instituição, ou temporariamente retirado de oferta |

`operationalMessage` (texto livre, até 280 caracteres) é validado no servidor
por `OperationalMessageSanitizer` antes de persistir — rejeita HTML/caracteres
`< >` (defesa primária contra XSS armazenado, §8.6) e termos de escopo
clínico (`clinical-scope`, o mesmo denylist que a CI aplica ao código-fonte).
Uma mensagem rejeitada nunca chega a ser salva; o `PUT` retorna
`422 Unprocessable Entity` com o motivo.

## 4. Auditoria

Toda alteração de catálogo e toda decisão de acesso ficam registradas na
tabela `AuditEvents` (categorias `CATALOG` e `ACCESS_DECISION`,
`AuditEventCategory.cs`), cada evento com `CorrelationId` (mesmo
`TraceIdentifier` da requisição HTTP, permitindo cruzar com os logs de
aplicação da seção 6), ator, instituição, valores antes/depois e resultado.

Não existe ainda uma tela de administração para consultar auditoria — a
consulta hoje é direta no banco (mesma via de acesso administrativo descrita
em `infra/deploy/README.md`, túnel SSH + `POSTGRES_PUBLISH` ou pgAdmin via
`--profile tools`). Uma UI de auditoria é trabalho futuro, fora do escopo
desta mudança.

## 5. Implantação

Coberto em detalhe por `infra/deploy/README.md` e §8 desta mudança
(`openspec/changes/introduce-senior-portal/tasks.md`). Resumo operacional:

- Serviço `senior-portal` no `docker-compose.yml` (produção) e
  `docker-test/docker-compose.yml` (build local), porta padrão `3002`.
- `PUBLIC_NAME` (variável de ambiente do container) define o nome exibido no
  cabeçalho do portal — injetado em runtime via
  `docker-entrypoint.d/10-public-config.sh`, nunca embutido no build.
- **Roteamento hoje (produção real): por subdomínio** — `portal.$OPS_DOMAIN`
  aponta pro container `senior-portal`, exatamente como `care.$OPS_DOMAIN` e
  `estoque.$OPS_DOMAIN` já apontam pros outros dois. `SessionCookieDomain`
  (`CONFIGURATION.md`) precisa estar configurado pro domínio pai
  (`.exemplo.com.br`) para a sessão ser compartilhada entre os três
  subdomínios — sem isso, cada um recebe um cookie *host-only* independente.
- **Roteamento alvo (ainda não ativado): por caminho** — `infra/deploy/ops/Caddyfile`
  já está reescrito para `/`, `/care`, `/stock`, `/api` sob uma única origem
  (§8.2), mas a ativação real (build das imagens com `VITE_BASE_PATH` +
  deploy coordenado do novo Caddyfile) é §9.7, que só acontece depois da
  homologação (§9.1/§9.4/§9.5) — **não fazer esse corte manualmente fora
  desse processo**.

## 6. Monitoramento

Sem stack de métricas nova (decisão explícita, §8.5) — logs estruturados via
`ILogger`, todos correlacionáveis pelo mesmo `CorrelationId`/`TraceIdentifier`
que já aparece em `ProblemDetails.correlationId` (respostas de erro da API) e
em `AuditEvents.CorrelationId`:

| Sinal | Onde | Nível |
|---|---|---|
| Restauração de sessão (rotação bem-sucedida) | `Startup.ValidateSessionPrincipalAsync` | Information |
| Restauração de sessão rejeitada (reuso de chave rotacionada, sessão revogada/expirada) | idem | Warning — possível sinal de roubo de cookie |
| 401 sem cookie de sessão válido | `Startup`, `OnRedirectToLogin` | Information |
| 403 por política do `[Authorize]` | `Startup`, `OnRedirectToAccessDenied` | Warning |
| 403 por permissão específica (`RequirePermissionAttribute`) | audit `ACCESS_DECISION` (seção 4), não log solto | — |
| Latência e contagem de módulos em estado não disponível | `ModuleCatalogController.Get` | Information (Warning em falha) |
| Redirecionamento de `returnTo` rejeitado | `console.warn` no navegador, cada front-end (`returnPath.ts`) | — (client-side, não chega ao backend) |
| Erro 500 não tratado | `GlobalExceptionHandler` (já existia antes desta mudança) | Error |

Nenhum desses logs inclui a chave de sessão (só o id, que não é segredo) nem
qualquer dado assistencial/financeiro.

## 7. Links diretos de contingência

Se o Senior Portal ficar indisponível (deploy quebrado, container fora do
ar), a equipe **não perde acesso aos módulos**: `care-web` e `stock-web`
continuam acessíveis diretamente pela própria URL (hoje,
`https://care.$OPS_DOMAIN` e `https://estoque.$OPS_DOMAIN`), com a mesma
sessão institucional — o portal é uma camada de navegação sobre a sessão
compartilhada, não uma dependência dela (§9.4 valida isso formalmente contra
homologação real antes do corte de produção, §9.7).

Cada front-end de módulo mantém seu próprio `/login` como via de contingência
— sob roteamento por subdomínio (estado atual), ele funciona normalmente,
sem qualquer redirecionamento pro portal (o redirecionamento de logins
legados, §9.2, só se ativa quando `VITE_BASE_PATH` está configurado para
roteamento por caminho, ou seja, depois de §9.7).

## 8. Rollback sem restaurar autenticação insegura

Reverter uma imagem do `senior-portal` (`docker compose up
-d --no-deps senior-portal` com uma tag anterior, ou remover o serviço da
release manifest do deploy) não afeta a sessão nem a autenticação dos outros
dois front-ends — eles não dependem do portal para autenticar; todos os três
sempre autenticaram diretamente contra a API via o mesmo cookie `HttpOnly`
(nunca existiu um mecanismo de token em `localStorage`/`sessionStorage` para
"restaurar" por engano). Um rollback do Senior Portal é, na pior hipótese,
perder a navegação centralizada — nunca uma regressão de segurança de sessão.

Reverter o roteamento (voltar de caminho pra subdomínio, ou nunca ativar
§9.7) também é seguro pelo mesmo motivo: o cookie de sessão e o RBAC do
backend não mudam com a topologia de borda. O único ponto de atenção é
`SessionCookieDomain` — se o rollback envolver voltar de uma única origem
(caminho) pra três subdomínios sem reconfigurar essa variável, a sessão deixa
de ser compartilhada entre os três front-ends (host-only por padrão, seção 5)
até o valor ser restaurado — comportamento degradado (mais logins), não
inseguro.
