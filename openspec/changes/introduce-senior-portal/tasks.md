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

- [ ] 2.1 Criar entidades e configurações EF Core para `ModuleDefinition` e `InstitutionModule`, incluindo chaves, instituição, estados, ordenação, habilitação, versão de concorrência e campos de auditoria.
- [ ] 2.2 Criar migração de banco com restrições de unicidade, integridade referencial, limites de texto e enumeração dos estados operacionais.
- [ ] 2.3 Implementar provisionamento idempotente das definições aprovadas de assistência e estoque, inicialmente desabilitadas para cada instituição.
- [ ] 2.4 Implementar validação de chave, ícone, permissão e caminho relativo por allowlist, rejeitando origens externas, HTML e destinos desconhecidos.
- [ ] 2.5 Implementar validação e sanitização das mensagens operacionais para impedir conteúdo pessoal, clínico ou técnico sensível.
- [ ] 2.6 Cobrir entidades, migração, seeds e validações com testes unitários e de integração em PostgreSQL.

## 3. APIs e autorização do catálogo

- [ ] 3.1 Implementar o serviço que combina identidade, instituição, conta ativa, permissões efetivas e configuração para produzir o catálogo mínimo do usuário.
- [ ] 3.2 Implementar `GET /api/v1/me/modules` com ordenação determinística e omissão de módulos desabilitados ou não autorizados.
- [ ] 3.3 Implementar APIs administrativas versionadas para consultar e alterar habilitação, ordem, estado e mensagem operacional com permissão específica.
- [ ] 3.4 Aplicar concorrência otimista e respostas de conflito nas alterações administrativas sem sobrescrever silenciosamente uma versão mais nova.
- [ ] 3.5 Auditar alterações do catálogo, mudanças de estado, acessos negados e redirecionamentos rejeitados com ator, instituição, módulo, resultado e correlação, sem tokens ou dados clínicos.
- [ ] 3.6 Garantir que os endpoints dos módulos e da API continuem revalidando autorização, independentemente da visibilidade fornecida pelo catálogo.
- [ ] 3.7 Criar testes de integração para isolamento institucional, conta bloqueada, permissão concedida/revogada, estados operacionais, validação, concorrência e auditoria.
- [ ] 3.8 Atualizar a especificação OpenAPI e os exemplos de resposta sem expor regras internas de autorização ou dados dos domínios.

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
