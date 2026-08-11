## Context

Consulte `proposal.md` para a motivação e
`specs/senior-portal/spec.md` para o comportamento verificável. O repositório
possui uma API ASP.NET Core compartilhada e duas aplicações React/Vite separadas:
uma assistencial e outra de estoque. Ambas repetem landing page, login, layouts,
rotas e navegação, e atualmente ainda usam um cookie `auth_token` acessível ao
JavaScript. A mudança `stabilize-existing-platform` especifica a substituição desse
estado por identidade institucional, permissões efetivas e sessão curta renovável.

O portal depende dessa fundação. Uma credencial de acesso mantida apenas em memória
não atravessa processos JavaScript distintos; por isso, cada aplicação deverá
restaurar seu próprio acesso curto por meio da mesma sessão de renovação protegida.
O desenho também deve preservar `build once, deploy many`, os dois artefatos web
existentes e a operação de baixo custo das ILPIs-piloto.

## Goals / Non-Goals

**Goals:**

- adicionar uma aplicação de entrada sem fundir os módulos existentes;
- compartilhar sessão, contexto institucional e autorização sob a mesma origem;
- tornar descoberta e estado dos módulos configuráveis e auditáveis;
- permitir deep links seguros, retorno global e contingência por link direto;
- manter configuração de ambiente fora dos bundles imutáveis;
- oferecer integração incremental que possa ser revertida por etapa.

**Non-Goals:**

- adotar module federation, iframes ou outro runtime de microfrontends;
- criar um design system completo ou um monorepo de pacotes nesta mudança;
- mover fluxos de negócio para o portal;
- agregar dados clínicos, assistenciais, financeiros ou analíticos na página inicial;
- implementar troca operacional entre múltiplas instituições;
- substituir os endpoints e serviços de IAM definidos na estabilização.

## Decisions

### 1. Criar uma terceira aplicação web leve e independente

O Senior Portal será um novo artefato React/TypeScript/Vite. Ele conterá login,
catálogo de módulos, perfil e segurança da conta, preferências transversais e
administração do catálogo. Assistência e estoque permanecerão aplicações e builds
independentes.

**Racional:** preserva os investimentos atuais, limita o impacto de cada release e
permite que módulos futuros tenham ciclos próprios sem transformar o portal em um
monólito de interface.

**Alternativas consideradas:** fundir as aplicações em uma SPA, rejeitada pelo
alto custo de migração e acoplamento de releases; microfrontends em runtime,
rejeitados porque a escala atual não justifica complexidade de carregamento,
compatibilidade e observabilidade; iframes, rejeitados por acessibilidade,
navegação, CSP e compartilhamento de contexto.

### 2. Publicar todas as aplicações sob uma única origem

A borda nginx publicará o portal em `/`, a API em `/api`, assistência em `/care`,
estoque em `/stock` e administração transversal em `/admin`. Cada bundle será
construído para seu caminho-base relativo. Destinos do catálogo serão caminhos
relativos registrados em allowlist; URLs externas não serão aceitas.

```text
https://seniorcare.ilpi.org/
├── /                 Senior Portal
├── /care             Assistência
├── /stock            Estoque
├── /admin            Administração transversal
└── /api              API compartilhada
```

**Racional:** mesma origem simplifica cookies protegidos, CORS, CSRF, TLS e
navegação, sem exigir que os três front-ends sejam compilados juntos.

**Alternativas consideradas:** subdomínios por módulo, adiados por ampliar escopo de
cookies, CORS e DNS; URLs absolutas injetadas no build, rejeitadas por violar
`build once, deploy many`.

### 3. Compartilhar renovação de sessão, não token JavaScript

O login do portal criará a sessão definida em `platform-authentication`. O cookie
de renovação terá escopo necessário para os endpoints de autenticação da mesma
origem, será `HttpOnly`, `Secure` e `SameSite` apropriado. Ao abrir portal ou módulo,
a aplicação chamará o endpoint de restauração usando o cookie e guardará o acesso
curto somente em sua própria memória. Rotação, CSRF, reutilização, revogação e
logout continuarão centralizados na API.

**Racional:** aplicações separadas não compartilham memória com segurança. Usar a
sessão protegida para emitir acessos curtos preserva SSO sem reintroduzir token em
`localStorage`, `sessionStorage` ou cookie legível por script.

**Alternativas consideradas:** propagar token por query string ou fragmento,
rejeitada por vazamento em histórico, logs e referer; `localStorage` compartilhado,
rejeitado por XSS; cookie bearer diretamente aceito por todas as APIs, rejeitado
por aumentar superfície de CSRF e reduzir separação entre sessão e acesso.

### 4. Separar definição sistêmica de habilitação institucional

O backend terá uma definição estável de módulo e uma configuração institucional:

- `ModuleDefinition`: `Id`, `Key`, nome padrão, descrição padrão, ícone aprovado,
  caminho permitido, permissão exigida e capacidade associada;
- `InstitutionModule`: instituição, definição, estado operacional, ordem,
  habilitação, mensagem operacional sanitizada, versão e auditoria.

As definições iniciais serão provisionadas de forma idempotente. Habilitação,
ordem e estado serão administráveis. Nome, ícone e caminho não serão texto ou HTML
arbitrário; alterações estruturais exigirão valores aprovados ou validação forte.

**Racional:** a separação impede que cada ILPI crie destinos inseguros, mas permite
controlar implantação, manutenção e prioridade local sem recompilar o portal.

**Alternativas consideradas:** catálogo inteiramente hardcoded, rejeitado por
exigir release para cada estado ou ativação; registro livre no banco, rejeitado por
redirecionamento aberto, XSS armazenado e módulos sem contrato.

### 5. Derivar o catálogo efetivo no backend

`GET /api/v1/me/modules` combinará sessão, instituição, estado da conta,
`ModuleDefinition`, `InstitutionModule` e permissões efetivas. A resposta mínima
conterá chave, apresentação aprovada, caminho relativo, ordem e estado operacional.
O endpoint não retornará regras internas de autorização nem dados dos domínios.

APIs administrativas versionadas permitirão consultar e alterar habilitação,
ordem, estado e mensagem operacional. Toda alteração usará concorrência otimista,
permissão específica e auditoria. O `AccessDecisionService` continuará protegendo
o destino e suas operações; o catálogo não será credencial de acesso.

**Racional:** evita que o cliente reconstrua precedência de permissões e garante
que módulos novos entrem no portal por contrato, não por alteração espalhada em
menus.

**Alternativas consideradas:** enviar todos os módulos e filtrar no navegador,
rejeitada por exposição desnecessária e divergência; consultar cada módulo
separadamente, rejeitada por latência e comportamento inconsistente.

### 6. Tratar estado operacional como configuração, não health check síncrono

O catálogo armazenará `AVAILABLE`, `MAINTENANCE`, `UNAVAILABLE` ou `DISABLED`.
Mudanças manuais e automações operacionais futuras poderão atualizar o estado. O
portal não fará uma chamada direta a cada módulo antes de renderizar; readiness
continua nos endpoints de operação e pode alimentar atualização assíncrona com
cache no futuro.

**Racional:** fan-out de health checks em cada carregamento tornaria o portal lento
e faria indisponibilidade transitória bloquear toda a navegação.

**Alternativas consideradas:** inferir estado somente no navegador, rejeitada por
expor topologia e produzir resultados diferentes por cliente; considerar módulo
sempre disponível, rejeitada por experiência ruim durante manutenção.

### 7. Preservar deep links com return path validado

O portal aceitará apenas destinos relativos conhecidos pelo catálogo e rotas-base
dos módulos. O retorno solicitado será normalizado, validado contra allowlist e
reavaliado após login. Se falhar, o destino será `/`. Cada módulo oferecerá ação
global de retorno ao portal e continuará protegendo suas rotas.

**Racional:** mantém links de trabalho úteis sem criar redirecionamento aberto ou
permitir que a presença de uma URL contorne autorização.

**Alternativas consideradas:** sempre descartar deep links, rejeitada por reduzir
produtividade; aceitar `returnUrl` arbitrária, rejeitada por phishing e exfiltração.

### 8. Centralizar experiência global por contrato antes de extrair pacote

Esta mudança definirá tokens visuais, nomenclatura, posição da navegação global,
contrato de perfil/logout, preferências e critérios de acessibilidade. Portal,
assistência e estoque receberão pequenos adaptadores próprios. A extração de um
pacote compartilhado será avaliada depois que o comportamento estabilizar.

**Racional:** evita introduzir simultaneamente workspaces npm, publicação de pacote
e migração completa dos componentes, mas impede que cada módulo invente uma
experiência incompatível.

**Alternativas consideradas:** copiar livremente layouts, rejeitada por manter a
divergência; criar design system completo agora, adiado por ser uma mudança maior
que a integração necessária.

### 9. Manter o portal livre de dados de negócio

O portal consumirá apenas contexto de identidade, catálogo, preferências e estado
operacional. Não haverá contagens de residentes, alertas clínicos, saldos,
indicadores ou notificações de negócio nesta mudança. Notificações globais futuras
exigirão contrato próprio de minimização, autorização e origem.

**Racional:** reduz impacto de uma falha do portal e evita transformá-lo em um
dashboard não governado com dados sensíveis.

**Alternativas consideradas:** cards com resumos de todos os domínios, rejeitada
por ampliar acoplamento, consultas, risco de reidentificação e requisitos de
atualização estatística.

### 10. Implantar de forma aditiva e manter contingência controlada

Portal e rotas-base serão introduzidos antes de remover entradas antigas. Durante
a transição, os hosts atuais permanecerão funcionais e passarão a consumir a mesma
sessão. Depois dos smoke tests, a raiz apontará para o portal e logins legados
redirecionarão para ele. Links diretos documentados continuarão válidos para
contingência, sempre sujeitos ao IAM.

**Racional:** evita um corte único envolvendo três aplicações, proxy, sessão e
catálogo e permite rollback sem restaurar credenciais inseguras.

**Alternativas consideradas:** substituição simultânea e definitiva das landing
pages, rejeitada pelo risco operacional e pela dificuldade de isolar falhas.

## Risks / Trade-offs

- **[A estabilização do IAM atrasa]** → manter esta mudança bloqueada para
  implementação até sessão, permissões efetivas e endpoint de identidade estarem
  validados; não criar autenticação provisória no portal.
- **[Portal vira ponto único de falha]** → preservar deep links protegidos,
  readiness independente e procedimento de contingência; não acoplar APIs de
  negócio à disponibilidade do portal.
- **[Cookie não funciona sob caminhos distintos]** → testar atributos, rotação,
  CSRF e logout nos três artefatos sob TLS e mesma origem antes da migração.
- **[Base path quebra assets ou refresh de rota]** → configurar Vite e nginx por
  aplicação e testar acesso direto a rotas profundas em imagem de produção.
- **[Catálogo divergente do deploy]** → provisionamento idempotente, validação de
  destinos, smoke test de cada módulo e estado `UNAVAILABLE` durante inconsistência.
- **[Descrição operacional vaza informação]** → texto limitado e sanitizado,
  orientação sem stack trace, host interno, residente ou dado de saúde.
- **[Permissão removida permanece no cliente]** → acessos curtos, versionamento de
  contexto e revalidação em cada módulo e API.
- **[Três interfaces continuam visualmente divergentes]** → contrato global e
  testes de jornada agora; pacote compartilhado após estabilização do padrão.
- **[Confusão com portal familiar]** → nomes, rotas, atores e documentação
  separados; nenhuma identidade externa é provisionada por esta mudança.

## Migration Plan

1. Concluir e validar instituição, identidade, sessão compartilhada, permissões
   efetivas e auditoria da mudança `stabilize-existing-platform`.
2. Adicionar migração e seeds idempotentes para definições e configurações de
   módulos, mantendo todos desabilitados até seus destinos estarem implantados.
3. Implementar APIs de catálogo efetivo e administração e cobri-las com testes de
   instituição, autorização, concorrência, validação e auditoria.
4. Criar o Senior Portal com login, restauração de sessão, catálogo, perfil,
   acessibilidade e estados de falha.
5. Preparar assistência e estoque para `/care` e `/stock`, restauração da sessão,
   retorno ao portal e acesso direto a rotas profundas.
6. Atualizar nginx, Docker Compose e CI de forma aditiva, preservando temporariamente
   as URLs de entrada anteriores.
7. Habilitar assistência e estoque no catálogo de homologação; executar smoke tests
   de login, MFA, navegação, deep link, 401, 403, manutenção, logout e contingência.
8. Tornar o portal a raiz de produção e ativar redirecionamentos seguros dos logins
   legados.
9. Monitorar falhas de restauração, redirecionamentos rejeitados, latência do
   catálogo, 401/403 e indisponibilidade por módulo antes de retirar compatibilidade.

**Rollback:** desabilitar o portal na borda e restaurar a entrada anterior, mantendo
as migrações aditivas e a sessão nova. Se um módulo falhar no caminho-base, marcá-lo
`UNAVAILABLE` e usar seu host de contingência previamente validado. Não restaurar
cookies legíveis por script nem credenciais antigas durante o rollback.

## Open Questions

- Qual nome público e domínio serão usados na primeira ILPI? A resposta altera
  configuração de implantação e identidade visual, não os contratos desta mudança.
- A administração transversal em `/admin` ficará inicialmente no portal ou em um
  artefato separado? O contrato de rota e permissão permanece o mesmo; a decisão
  pode ser tomada no detalhamento da interface.
