## 1. Baseline e infraestrutura de testes

- [ ] 1.1 Instalar/restaurar o SDK .NET 8 e registrar os comandos reproduzíveis de build dos três componentes.
- [ ] 1.2 Corrigir a incompatibilidade de `ApiResponse` no front-end assistencial e comprovar lint e build limpos.
- [ ] 1.3 Criar projetos de testes unitários e de integração do backend e adicioná-los à solution.
- [ ] 1.4 Configurar PostgreSQL efêmero para testes de integração, com health check, migração desde banco vazio e descarte ao final.
- [ ] 1.5 Configurar runner, biblioteca de componentes, DOM de teste, mock HTTP e coleta de cobertura em cada front-end.
- [ ] 1.6 Adicionar testes de caracterização dos CRUDs atuais antes de alterar seus contratos.

## 2. Configuração e diagnóstico de execução

- [ ] 2.1 Substituir a URL absoluta da API por caminho de mesma origem nos dois clientes HTTP.
- [ ] 2.2 Configurar proxy de desenvolvimento nos dois projetos Vite com destino externo e validação de formato.
- [ ] 2.3 Configurar o nginx de cada front-end para encaminhar `/api` à API na rede Docker.
- [ ] 2.4 Adicionar modelos de configuração sem segredos para desenvolvimento, teste e produção e documentar todas as variáveis obrigatórias.
- [ ] 2.5 Adicionar validação de startup para conexão, chaves e bootstrap, garantindo mensagens sem valores secretos.
- [ ] 2.6 Separar endpoints de vida e prontidão e testar o comportamento com banco disponível e indisponível.
- [ ] 2.7 Verificar nos bundles de produção que não há destino operacional em `localhost` nem segredos incorporados.

## 3. Contratos HTTP e tratamento de erros

- [ ] 3.1 Definir DTOs de criação, atualização, resposta e listagem paginada sem expor entidades de persistência.
- [ ] 3.2 Implementar Problem Details centralizado com códigos estáveis, erros por campo e identificador de correlação.
- [ ] 3.3 Remover mensagens de exceção das respostas e adicionar testes que impeçam vazamento de detalhes internos.
- [ ] 3.4 Padronizar 400/401/403/404/409/422/500 em todos os controllers existentes.
- [ ] 3.5 Implementar paginação e filtro consistentes nos dez catálogos definidos pela spec.
- [ ] 3.6 Tornar o ID da rota canônico, rejeitar divergência com o corpo e remover o `PATCH` que executa substituição total.
- [ ] 3.7 Adicionar concorrência otimista aos catálogos e testes para edição com versão desatualizada.
- [ ] 3.8 Publicar e validar o contrato OpenAPI resultante contra os clientes dos dois front-ends.

## 4. Identidade, autenticação e autorização

- [ ] 4.1 Adicionar persistência de usuários, papéis e sessões de renovação com migração aditiva.
- [ ] 4.2 Configurar derivação segura de senha, política de conta e papéis `Administrator` e `Operator`.
- [ ] 4.3 Implementar bootstrap administrativo explícito, idempotente e alimentado por secret externo.
- [ ] 4.4 Implementar login com resposta genérica para falha e acesso de curta duração.
- [ ] 4.5 Implementar renovação rotativa protegida, revogação, logout e rejeição de sessão reutilizada.
- [ ] 4.6 Implementar limitação configurável de tentativas e bloqueio temporário sem enumeração de contas.
- [ ] 4.7 Proteger todos os endpoints de catálogo: leitura autenticada e escrita exclusiva de administrador.
- [ ] 4.8 Adicionar testes HTTP para login, expiração, logout, 401, 403, limitação e bootstrap idempotente.
- [ ] 4.9 Documentar que os papéis administrativos não concedem autorização clínica nem permitem dados reais de prontuário.

## 5. Auditoria administrativa

- [ ] 5.1 Criar modelo e migração append-only para eventos de autenticação e alterações de catálogo.
- [ ] 5.2 Capturar ator, ação, recurso, instante UTC, correlação e resultado sem payload sensível, senha ou token.
- [ ] 5.3 Impedir atualização e exclusão da auditoria pela API e pela camada normal de repositórios.
- [ ] 5.4 Registrar login, logout, bloqueio, falha relevante, criação, alteração e inativação.
- [ ] 5.5 Adicionar testes de atribuição, imutabilidade e ausência de segredos na auditoria.

## 6. Catálogos auxiliares e produto

- [ ] 6.1 Adicionar estado ativo e token de concorrência aos nove catálogos existentes com migração compatível.
- [ ] 6.2 Substituir exclusão física por inativação e proteger referências contra cascata indevida.
- [ ] 6.3 Implementar validações de campos, unicidade e referências ativas por catálogo.
- [ ] 6.4 Criar entidade, configuração, DTOs, repositório, serviço e controller de produto.
- [ ] 6.5 Criar migração de produto com relações a tipo e unidade e pré-validação dos dados existentes.
- [ ] 6.6 Implementar pesquisa paginada de produto por descrição e nome genérico.
- [ ] 6.7 Adaptar o formulário de produto para validação, concorrência, inativação e erros Problem Details.
- [ ] 6.8 Adicionar testes de integração do ciclo de produto, referências inválidas e limite sem movimentos/lotes.
- [ ] 6.9 Executar testes de regressão em plano de saúde, cargo, religião, fornecedor, fabricante, transportadora, grupo, tipo e unidade.

## 7. Sessão e contratos nos front-ends

- [ ] 7.1 Criar contexto de autenticação tipado e fluxo de login funcional nos dois front-ends.
- [ ] 7.2 Manter acesso somente em memória e renovação em cookie protegido, sem token em `localStorage` ou cookie acessível a script.
- [ ] 7.3 Implementar proteção de rotas, restauração de sessão e redirecionamento sem loop após HTTP 401.
- [ ] 7.4 Ocultar ou desabilitar ações de escrita para operador, mantendo a API como autoridade final.
- [ ] 7.5 Substituir o `ApiResponse<T>` divergente por clientes tipados compatíveis com recursos, paginação e Problem Details.
- [ ] 7.6 Padronizar estados de carregamento, vazio, validação, conflito, indisponibilidade e repetição segura nos CRUDs.
- [ ] 7.7 Adicionar testes de login, logout, sessão expirada, rota protegida, papel insuficiente e falhas de CRUD em cada front-end.

## 8. Baseline de acessibilidade

- [ ] 8.1 Corrigir semântica, nome acessível e foco visível em botões, campos, busca, tabela, cabeçalho e navegação dos dois front-ends.
- [ ] 8.2 Implementar gerenciamento de foco, fechamento por teclado e retorno ao acionador em todos os modais.
- [ ] 8.3 Associar mensagens de validação aos campos e anunciar erros e resultados sem depender apenas de cor.
- [ ] 8.4 Validar e normalizar preferências persistidas de contraste e fonte, incluindo restauração do padrão.
- [ ] 8.5 Substituir o placeholder da página de acessibilidade por instruções e controles reais.
- [ ] 8.6 Adicionar verificações automatizadas de acessibilidade para login, modal, tabela e formulário representativo.
- [ ] 8.7 Executar e registrar teste manual somente por teclado no login e em um CRUD de cada front-end.

## 9. CI, migração e entrega

- [ ] 9.1 Adicionar `dotnet test` com resultados e cobertura ao job de backend.
- [ ] 9.2 Adicionar testes e cobertura aos jobs dos dois front-ends, preservando os filtros por caminho.
- [ ] 9.3 Fazer o check agregado falhar quando qualquer build, teste, migração, lint ou gate de segurança aplicável falhar.
- [ ] 9.4 Adicionar verificação automatizada de fixtures sintéticas e ausência de credenciais nos artefatos.
- [ ] 9.5 Testar migrações em banco vazio e em snapshot sintético da versão imediatamente anterior.
- [ ] 9.6 Criar script de pré-validação dos dados e garantir falha antes de constraints incompatíveis.
- [ ] 9.7 Atualizar Dockerfiles, Compose, nginx, release e health checks para a nova configuração e autenticação.
- [ ] 9.8 Executar smoke test coordenado de prontidão, bootstrap, login, permissões e produto no ambiente Docker de teste.
- [ ] 9.9 Documentar implantação, bootstrap, backup pré-deploy, rollback e incompatibilidade com clientes antigos.
- [ ] 9.10 Atualizar README, documentação de API e relatório de avaliação com as evidências finais de aderência.

## 10. Aceite da mudança

- [ ] 10.1 Executar todos os cenários das cinco specs e registrar evidências automatizadas ou manuais conforme definido.
- [ ] 10.2 Confirmar que os três componentes compilam em checkout limpo e que o Graphify está atualizado.
- [ ] 10.3 Confirmar que nenhum endpoint administrativo aceita acesso anônimo e que operadores não possuem escrita.
- [ ] 10.4 Confirmar que nenhum dado clínico, prontuário, dashboard ou assinatura foi introduzido por esta mudança.
- [ ] 10.5 Validar a mudança OpenSpec em modo estrito e preparar o handoff para revisão e implementação.
