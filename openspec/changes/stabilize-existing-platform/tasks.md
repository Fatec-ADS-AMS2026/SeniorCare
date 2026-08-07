## 1. Baseline e infraestrutura de testes

- [x] 1.1 Instalar/restaurar o SDK .NET 8 e registrar os comandos reproduzíveis de build dos três componentes.
- [x] 1.2 Corrigir a incompatibilidade de `ApiResponse` no front-end assistencial e comprovar lint e build limpos.
- [x] 1.3 Criar projetos de testes unitários e de integração do backend e adicioná-los à solution.
- [x] 1.4 Configurar PostgreSQL efêmero para testes de integração, com health check, migração desde banco vazio e descarte ao final.
- [x] 1.5 Configurar runner, biblioteca de componentes, DOM de teste, mock HTTP e coleta de cobertura em cada front-end.
- [x] 1.6 Adicionar testes de caracterização dos CRUDs atuais antes de alterar seus contratos.

## 2. Configuração e diagnóstico de execução

- [x] 2.1 Substituir a URL absoluta da API por caminho de mesma origem nos dois clientes HTTP.
- [x] 2.2 Configurar proxy de desenvolvimento nos dois projetos Vite com destino externo e validação de formato.
- [x] 2.3 Configurar o nginx de cada front-end para encaminhar `/api` à API na rede Docker.
- [x] 2.4 Adicionar modelos de configuração sem segredos para desenvolvimento, teste e produção e documentar todas as variáveis obrigatórias.
- [ ] 2.5 Adicionar validação de startup para conexão, chaves, instituição e bootstrap, garantindo mensagens sem valores secretos. **Parcial nesta mudança**: validação de `ConnectionStrings:DefaultConnection` implementada e testada (`Program.GetMissingConfiguration`); chaves/instituição/bootstrap dependem de entidades que ainda não existem (capability `platform-authentication`, §4 em diante) — reabrir esta tarefa quando esse trabalho chegar.
- [x] 2.6 Separar endpoints de vida e prontidão e testar o comportamento com banco disponível e indisponível.
- [x] 2.7 Verificar nos bundles de produção que não há destino operacional em `localhost` nem segredos incorporados.

## 3. Contratos HTTP e tratamento de erros

- [ ] 3.1 Definir DTOs de criação, atualização, resposta e listagem paginada sem expor entidades de persistência.
- [ ] 3.2 Implementar Problem Details centralizado com códigos estáveis, erros por campo e identificador de correlação.
- [ ] 3.3 Remover mensagens de exceção das respostas e adicionar testes que impeçam vazamento de detalhes internos.
- [ ] 3.4 Padronizar 400/401/403/404/409/422/500 em todos os controllers existentes.
- [ ] 3.5 Implementar paginação e filtro consistentes nos dez catálogos definidos pela spec.
- [ ] 3.6 Tornar o ID da rota canônico, rejeitar divergência com o corpo e remover o `PATCH` que executa substituição total.
- [ ] 3.7 Adicionar concorrência otimista aos catálogos e testes para edição com versão desatualizada.
- [ ] 3.8 Publicar e validar o contrato OpenAPI resultante contra os clientes dos dois front-ends.

## 4. Instituição, identidade e política de senha

- [ ] 4.1 Modelar instituição e estender a identidade ASP.NET com `InstitutionId`, `IdentityOrigin` e estados `PROVISIONED`, `ACTIVE`, `INACTIVE`, `BLOCKED` e `EXPIRED`.
- [ ] 4.2 Criar migração aditiva para instituição, usuários e credenciais locais, com índices e restrições de isolamento institucional.
- [ ] 4.3 Implementar origem `LOCAL` e pontos de extensão desabilitados para `LDAP` e `OIDC`, rejeitando ativação sem provedor real.
- [ ] 4.4 Configurar derivação adaptativa de senha com salt individual, atualização transparente de parâmetros e ausência de segredos em respostas e logs.
- [ ] 4.5 Implementar piso de senha: 15 caracteres sem MFA ou 8 com MFA obrigatório, aceitação de pelo menos 64 caracteres, espaços e Unicode.
- [ ] 4.6 Integrar bloqueio de senhas comuns ou comprometidas sem regra arbitrária de composição nem expiração periódica automática.
- [ ] 4.7 Implementar configuração institucional que somente fortaleça os pisos de senha e adicionar validações de limites.
- [ ] 4.8 Implementar bootstrap idempotente da instituição e do administrador `PROVISIONED`, sem senha fixa ou redefinição silenciosa.
- [ ] 4.9 Implementar ativação por token aleatório, curto, de uso único e armazenado por hash, com definição da senha pela própria pessoa.
- [ ] 4.10 Implementar recuperação com resposta antienumeração, token protegido e revogação das sessões após redefinição.
- [ ] 4.11 Implementar mudança autenticada de senha com senha atual ou reautenticação recente e revogação das demais sessões.
- [ ] 4.12 Adicionar testes de isolamento institucional, ciclo de vida, política de senha, bootstrap, ativação, recuperação e ausência de segredos.

## 5. Modelo e decisão de acesso

- [ ] 5.1 Modelar `Role`, `Permission(resource, action, feature)`, `PermissionGroup` e as associações de composição, todos delimitados pela instituição quando aplicável.
- [ ] 5.2 Modelar responsabilidade organizacional e atribuições por instituição, unidade ou setor, com início e término de validade.
- [ ] 5.3 Modelar exceções individuais `ALLOW`/`DENY` com permissão, escopo, justificativa, autoria e validade.
- [ ] 5.4 Modelar políticas condicionais de concessão e negação com versão, estado e condições estritamente validadas.
- [ ] 5.5 Criar migração aditiva das entidades de autorização e seeds somente de permissões sistêmicas, sem usuários ou senhas padrão.
- [ ] 5.6 Implementar `AccessDecisionService` central por instituição, recurso, ação, funcionalidade e escopo-alvo.
- [ ] 5.7 Implementar precedência: contexto/estado inválido, `SYSTEM_ADMIN`, `DENY` individual, política `DENY`, `ALLOW` individual, política `ALLOW`, RBAC e `DENY` padrão.
- [ ] 5.8 Restringir `SYSTEM_ADMIN` a operações sistêmicas, impedir sua atribuição a usuários operacionais e destacar todo uso na auditoria.
- [ ] 5.9 Garantir que profissão/cargo não conceda acesso e que responsabilidade organizacional conceda somente capacidades configuradas e vigentes.
- [ ] 5.10 Proteger todos os endpoints existentes pelo serviço de decisão, com HTTP 401 para anonimato e HTTP 403 para decisão negativa.
- [ ] 5.11 Implementar invalidação/versionamento do contexto efetivo quando papel, grupo, vínculo, exceção ou política mudar.
- [ ] 5.12 Criar testes matriciais de precedência, negação padrão, escopo, validade, conflito e manipulação do cliente.
- [ ] 5.13 Documentar que acesso administrativo não concede autorização clínica futura.

## 6. Configuração administrativa de acesso

- [ ] 6.1 Criar APIs protegidas para listar, criar, ativar, inativar, bloquear e expirar contas sem expor credenciais.
- [ ] 6.2 Criar APIs protegidas para papéis, permissões, grupos e suas associações, com validação e histórico.
- [ ] 6.3 Criar APIs protegidas para responsabilidades organizacionais, atribuições e validade por escopo.
- [ ] 6.4 Criar APIs protegidas para exceções individuais e políticas condicionais, exigindo justificativa nos casos definidos.
- [ ] 6.5 Criar API de política institucional para bloqueio, duração de sessão, MFA e fortalecimento de senha com limites seguros.
- [ ] 6.6 Criar API de sessões ativas com revogação individual ou de todas as sessões de uma conta.
- [ ] 6.7 Impedir inativação do último administrador institucional ativo e exigir reautenticação para alterações críticas.
- [ ] 6.8 Criar endpoint da identidade atual com instituição, módulos, papéis, responsabilidades e permissões efetivas, sem detalhes internos sensíveis.
- [ ] 6.9 Criar endpoint administrativo de explicação de decisão com acesso restrito e dados suficientes para suporte e auditoria.
- [ ] 6.10 Adicionar testes de autorização administrativa, invariantes, concorrência, histórico e invalidação de contexto.

## 7. Sessão, MFA e proteção contra abuso

- [ ] 7.1 Implementar login com resposta genérica, acesso de curta duração e contexto institucional explícito.
- [ ] 7.2 Persistir famílias de renovação por hash e implementar cookie `HttpOnly`, `Secure` e `SameSite` com proteção CSRF adequada.
- [ ] 7.3 Implementar rotação de renovação, detecção de reutilização, revogação individual, revogação por conta e logout.
- [ ] 7.4 Compartilhar a sessão de mesma origem entre os módulos assistencial e estoque sem persistir credenciais em `localStorage` ou `sessionStorage`.
- [ ] 7.5 Implementar MFA TOTP e códigos de recuperação de uso único com armazenamento protegido.
- [ ] 7.6 Exigir MFA para administradores e contas de configuração de acesso e permitir obrigatoriedade institucional para os demais.
- [ ] 7.7 Restringir contas privilegiadas sem MFA ao fluxo de cadastro e recuperação do segundo fator.
- [ ] 7.8 Implementar limitação por conta e origem, atraso progressivo ou bloqueio temporário, com valores padrão e configuração segura.
- [ ] 7.9 Adicionar testes HTTP de login, MFA, expiração, rotação, reutilização, logout, 401, limitação e revogação após mudança de conta ou senha.

## 8. Auditoria de identidade e acesso

- [ ] 8.1 Criar modelo e migração append-only para eventos de autenticação, sessão, configuração, catálogo e decisão de acesso.
- [ ] 8.2 Capturar ator, instituição, recurso, ação, funcionalidade, escopo-alvo, instante UTC, correlação, resultado e camada determinante.
- [ ] 8.3 Registrar login, logout, bloqueio, MFA, ativação, recuperação, mudança de senha, estados de conta e revogação de sessão.
- [ ] 8.4 Registrar versões anterior e posterior permitidas de papel, grupo, vínculo, exceção, política e parâmetro de segurança.
- [ ] 8.5 Registrar decisões protegidas e uso de `SYSTEM_ADMIN` sem payload integral, senha, token, segredo ou código MFA.
- [ ] 8.6 Impedir atualização e exclusão da auditoria pela API e pela camada normal de repositórios.
- [ ] 8.7 Adicionar testes de atribuição, correlação, imutabilidade, camada determinante e ausência de segredos.

## 9. Catálogos auxiliares e produto

- [ ] 9.1 Adicionar estado ativo e token de concorrência aos nove catálogos existentes com migração compatível.
- [ ] 9.2 Substituir exclusão física por inativação e proteger referências contra cascata indevida.
- [ ] 9.3 Implementar validações de campos, unicidade e referências ativas por catálogo.
- [ ] 9.4 Criar entidade, configuração, DTOs, repositório, serviço e controller de produto.
- [ ] 9.5 Criar migração de produto com relações a tipo e unidade e pré-validação dos dados existentes.
- [ ] 9.6 Implementar pesquisa paginada de produto por descrição e nome genérico.
- [ ] 9.7 Adaptar o formulário de produto para validação, concorrência, inativação e erros Problem Details.
- [ ] 9.8 Adicionar testes de integração do ciclo de produto, referências inválidas e limite sem movimentos/lotes.
- [ ] 9.9 Executar testes de regressão em plano de saúde, cargo, religião, fornecedor, fabricante, transportadora, grupo, tipo e unidade.

## 10. Identidade, acesso e contratos nos front-ends

- [ ] 10.1 Criar contexto tipado de autenticação e autorização compartilhável conceitualmente pelos dois front-ends.
- [ ] 10.2 Implementar login, ativação, recuperação, troca de senha, cadastro de MFA e uso de código de recuperação.
- [ ] 10.3 Manter acesso somente em memória e renovação em cookie protegido, sem token em armazenamento persistente acessível a script.
- [ ] 10.4 Implementar restauração e compartilhamento da sessão, redirecionamento sem loop após HTTP 401 e tratamento explícito de HTTP 403.
- [ ] 10.5 Consumir o contexto atual para menus e ações, mantendo claro que a API é a autoridade final.
- [ ] 10.6 Criar telas administrativas de usuários, estados de conta e disparo de ativação/recuperação sem campo de senha administrativa.
- [ ] 10.7 Criar telas de papéis, grupos, permissões, responsabilidades, atribuições, exceções e políticas com escopo e validade visíveis.
- [ ] 10.8 Criar telas de parâmetros de segurança e sessões ativas com confirmação e reautenticação nas ações críticas.
- [ ] 10.9 Substituir o `ApiResponse<T>` divergente por clientes tipados compatíveis com recursos, paginação e Problem Details.
- [ ] 10.10 Padronizar estados de carregamento, vazio, validação, conflito, indisponibilidade e repetição segura nos fluxos.
- [ ] 10.11 Adicionar testes de login, MFA, logout, expiração, rota protegida, visibilidade por permissão, administração de acesso e falhas de CRUD.

## 11. Baseline de acessibilidade

- [ ] 11.1 Corrigir semântica, nome acessível e foco visível em botões, campos, busca, tabela, cabeçalho e navegação dos dois front-ends.
- [ ] 11.2 Implementar gerenciamento de foco, fechamento por teclado e retorno ao acionador em todos os modais.
- [ ] 11.3 Associar mensagens de validação aos campos e anunciar erros e resultados sem depender apenas de cor.
- [ ] 11.4 Validar e normalizar preferências persistidas de contraste e fonte, incluindo restauração do padrão.
- [ ] 11.5 Substituir o placeholder da página de acessibilidade por instruções e controles reais.
- [ ] 11.6 Adicionar verificações automatizadas de acessibilidade para login, MFA, modal, tabela e formulário representativo.
- [ ] 11.7 Executar e registrar teste manual somente por teclado no login e em um CRUD de cada front-end.

## 12. CI, migração e entrega

- [ ] 12.1 Adicionar `dotnet test` com resultados e cobertura ao job de backend.
- [ ] 12.2 Adicionar testes e cobertura aos jobs dos dois front-ends, preservando os filtros por caminho.
- [ ] 12.3 Fazer o check agregado falhar quando qualquer build, teste, migração, lint ou gate de segurança aplicável falhar.
- [ ] 12.4 Adicionar verificação automatizada de fixtures sintéticas e ausência de credenciais nos artefatos.
- [ ] 12.5 Testar migrações em banco vazio e em snapshot sintético da versão imediatamente anterior.
- [ ] 12.6 Criar script de pré-validação dos dados e garantir falha antes de constraints incompatíveis.
- [ ] 12.7 Atualizar Dockerfiles, Compose, nginx, release e health checks para a nova configuração, instituição e autenticação.
- [ ] 12.8 Executar smoke test coordenado de prontidão, bootstrap, ativação, login, MFA, permissões e produto no ambiente Docker de teste.
- [ ] 12.9 Documentar implantação, bootstrap, canal de ativação, backup pré-deploy, rollback e incompatibilidade com clientes antigos.
- [ ] 12.10 Atualizar README, documentação de API e relatório de avaliação com as evidências finais de aderência.

## 13. Aceite da mudança

- [ ] 13.1 Executar todos os cenários das cinco specs e registrar evidências automatizadas ou manuais conforme definido.
- [ ] 13.2 Confirmar que os três componentes compilam em checkout limpo e que o Graphify está atualizado.
- [ ] 13.3 Confirmar que nenhum endpoint administrativo aceita acesso anônimo e que toda escrita exige permissão efetiva.
- [ ] 13.4 Confirmar isolamento institucional, negação padrão, precedência, MFA administrativo e revogação de sessão por testes dedicados.
- [ ] 13.5 Confirmar que profissão não concede acesso e que nenhum dado clínico, prontuário, dashboard ou assinatura foi introduzido por esta mudança.
- [ ] 13.6 Validar a mudança OpenSpec em modo estrito e preparar o handoff para revisão e implementação.
