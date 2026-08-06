## Context

Consulte `proposal.md` para a motivação. A API atual é um monólito ASP.NET Core
com Entity Framework e PostgreSQL, organizado em controllers, serviços e
repositórios genéricos. Ela persiste nove catálogos e não possui identidade,
autenticação ou auditoria. Os dois front-ends React repetem parte da infraestrutura
de API e componentes, usam URL fixa em localhost e esperam contratos de resposta
que não são uniformes. O front-end assistencial não compila; o de estoque possui
uma tela de produto sem backend correspondente.

A implantação segue `build once, deploy many`, com imagens imutáveis, Docker
Compose, health checks e releases coordenadas por manifest. O desenho deve manter
essa propriedade, permitir uma atualização atômica dos três componentes e evitar
uma reestruturação ampla antes de estabilizar a base.

## Goals / Non-Goals

**Goals:**

- preservar a arquitetura implantável atual e estabelecer contratos consistentes;
- tornar configuração, autenticação, autorização e auditoria transversais;
- completar o fluxo de produto sem criar ainda movimentos de estoque;
- tornar migrações e builds reproduzíveis e verificáveis no CI;
- permitir que as próximas mudanças partam de uma baseline testada.

**Non-Goals:**

- dividir a API em microsserviços ou unificar os dois front-ends;
- introduzir instituição multi-tenant, residente ou dados assistenciais;
- criar controle de estoque por lote, validade ou movimento;
- implementar autorização clínica contextual, prontuário ou assinatura;
- certificar conformidade integral com LGPD, S-RES, NGS2/SBIS ou WCAG.

## Decisions

### 1. Manter o monólito modular e os dois front-ends

A mudança evoluirá a API atual e preservará os dois artefatos web. Autenticação,
contratos HTTP, auditoria e persistência serão capacidades transversais do monólito;
catálogos permanecerão módulos de apoio delimitados.

**Racional:** a instabilidade atual é contratual e de qualidade, não de escala. Uma
decomposição aumentaria deploys, observabilidade e consistência distribuída sem
entregar valor ao piloto.

**Alternativa considerada:** microsserviços por domínio. Rejeitada nesta fase por
custo operacional e ausência dos domínios centrais.

### 2. Usar API de mesma origem em produção

Os bundles chamarão um caminho relativo, por exemplo `/api/v1`. O nginx de cada
front-end encaminhará `/api` para a API na rede Docker. No desenvolvimento, o Vite
usará proxy cujo destino poderá ser configurado por variável de ambiente.

**Racional:** elimina `localhost` do bundle, reduz CORS em produção e preserva a
mesma imagem entre instalações. Configurações de host e TLS ficam na borda, sem
recompilar a aplicação.

**Alternativas consideradas:** injetar uma URL absoluta no build, incompatível com
`build once, deploy many`; ou gerar `config.js` no startup, flexível mas
desnecessário enquanto API e web puderem compartilhar origem.

### 3. Padronizar erros com Problem Details e sucesso por recurso

Falhas seguirão Problem Details, com código estável, erros por campo e identificador
de correlação. Listagens retornarão um envelope paginado explícito. Recursos
individuais retornarão representações tipadas, sem envelope genérico ambíguo.
Exceções serão convertidas centralmente e não serão concatenadas às respostas.

**Racional:** usa semântica HTTP reconhecida e elimina as diferenças atuais entre
controllers e os campos `error`/`errors` dos clientes.

**Alternativa considerada:** manter um envelope `ApiResponse<T>` para todos os
resultados. Rejeitada porque duplica status HTTP e já causou divergência de tipos.

### 4. Separar modelos de persistência dos contratos da API

Requests e responses próprios validarão campos, referências e versões. O ID da
rota será canônico. Entidades receberão `IsActive` e token de concorrência onde
aplicável. O repositório genérico deixará de decidir semântica de negócio; serviços
de cada catálogo aplicarão validação, inativação e referência.

**Racional:** impede overposting, exclusão em cascata indevida e atualização total
disfarçada de `PATCH`.

**Alternativa considerada:** expor entidades diretamente e corrigir apenas os
controllers. Rejeitada por manter acoplamento entre schema, API e validação.

### 5. Implementar produto como catálogo, não como saldo transacional

Será criada entidade `Product` com relações para tipo e unidade de medida, campos
do formulário existente, estado ativo e concorrência. Campos quantitativos e de
custo já apresentados serão preservados como dados iniciais/administrativos, mas
o desenho não criará tabelas de movimento, lote ou inventário. Uma futura mudança
de estoque deverá migrar o saldo para projeção derivada de movimentos.

**Racional:** fecha a quebra objetiva entre tela e API sem antecipar o domínio de
estoque rastreável.

**Alternativa considerada:** remover a tela de produto. Rejeitada porque produto é
pré-requisito legítimo e o esforço já existente pode ser consolidado.

### 6. Usar identidade consolidada e sessão curta revogável

A API adotará o mecanismo de identidade suportado pelo ecossistema ASP.NET para
usuários, derivação de senha e papéis. O login emitirá acesso de curta duração e
uma sessão de renovação rotativa armazenada de forma protegida e revogável. O
front-end manterá credencial de acesso somente pelo tempo necessário e não usará
`localStorage` para tokens. Renovação e logout usarão cookie `HttpOnly`, `Secure` e
política `SameSite`, com defesa contra requisições forjadas.

Papéis iniciais: `Administrator`, com escrita nos catálogos, e `Operator`, com
leitura. Políticas serão aplicadas na API; proteção de rota no front-end melhora a
experiência, mas não será considerada barreira de segurança.

**Racional:** evita autenticação artesanal e cria revogação sem conceder, por
inferência, acesso clínico futuro.

**Alternativas consideradas:** token duradouro em cookie acessível a JavaScript,
rejeitado por exposição a XSS; sessão puramente em memória no servidor, rejeitada
por dificultar evolução horizontal e operação em mais de uma instância.

### 7. Provisionar administrador de forma explícita e idempotente

Um comando ou modo de bootstrap receberá email e senha por secret externo, criará
a primeira conta apenas quando não houver administrador e registrará somente o
resultado não sensível. O startup normal não redefinirá credenciais.

**Racional:** permite instalação de baixo custo sem credenciais padrão conhecidas.

**Alternativa considerada:** usuário seed fixo na migração. Rejeitada por vazar
segredo e replicar a mesma credencial entre ILPIs.

### 8. Introduzir auditoria administrativa append-only

Um registro de auditoria persistirá ator, ação, tipo/ID do recurso, instante UTC,
correlação e resultado. Não armazenará senha, token nem payload integral. A
aplicação não oferecerá update/delete dessa tabela. Eventos de login, logout,
bloqueio e alterações de catálogo serão cobertos.

**Racional:** fornece atribuição mínima para a base atual sem fingir que esta
auditoria atende os requisitos futuros de prontuário.

**Alternativa considerada:** depender apenas de logs de aplicação. Rejeitada
porque logs podem ter retenção e formato inadequados e não compõem histórico
consultável por recurso.

### 9. Testar com a mesma classe de banco usada em produção

O backend terá testes unitários e integração com PostgreSQL efêmero, aplicação de
migrações e chamadas HTTP pela aplicação hospedada em teste. Os front-ends usarão
runner TypeScript, biblioteca de testes por comportamento, mocks HTTP e verificação
automatizada de acessibilidade. O CI publicará resultados e cobertura.

**Racional:** banco em memória esconderia diferenças de constraints, concorrência
e SQL; testes de componentes são mais estáveis que snapshots extensos.

**Alternativas consideradas:** SQLite/in-memory para toda integração, rejeitados
como única evidência; testes end-to-end completos em navegador para tudo,
reservados a poucos smoke tests por custo e fragilidade.

### 10. Evoluir acessibilidade nos componentes compartilhados atuais

As correções serão aplicadas primeiro a Button, FormControls, Table, Modal,
SearchBar, cabeçalho e layouts em ambos os front-ends. Login e um CRUD por
front-end servirão como jornadas de referência. A página de acessibilidade
explicará os controles disponíveis, sem duplicar preferências.

**Racional:** corrigir primitivas propaga ganhos e reduz divergência, mesmo antes
de extrair um pacote comum.

**Alternativa considerada:** criar imediatamente uma biblioteca compartilhada.
Adiada para não combinar migração estrutural com estabilização funcional.

## Risks / Trade-offs

- **[Quebra simultânea de API e clientes]** → publicar backend e front-ends no
  mesmo release manifest e manter testes de contrato antes do deploy.
- **[Bloqueio de instalação sem administrador]** → validar secret e executar
  bootstrap antes de ativar a obrigatoriedade de autenticação; fornecer diagnóstico
  operacional claro.
- **[Migração falha por dados atuais inválidos]** → executar pré-validação,
  produzir relatório e abortar antes de constraints destrutivas.
- **[Saldo de produto interpretado como estoque rastreável]** → rotular a
  capacidade como catálogo e documentar que movimentos/lotes estão fora do escopo.
- **[Papéis administrativos reutilizados indevidamente no futuro]** → nomes e
  políticas explícitos; toda capacidade clínica deverá definir autorização
  contextual própria.
- **[Auditoria cresce sem política de retenção]** → índices, monitoramento de volume
  e retenção configurável futura; não apagar enquanto a política institucional não
  estiver aprovada.
- **[Dependência de testes em contêiner no CI]** → usar imagem PostgreSQL fixada,
  health check e diagnóstico do container; manter testes unitários independentes.
- **[Correções duplicadas nos dois front-ends]** → checklist comum e testes
  equivalentes; extração de design system permanece evolução posterior.

## Migration Plan

1. Corrigir o build assistencial e introduzir testes sem alterar contratos.
2. Adicionar configuração de mesma origem e proxies, mantendo temporariamente uma
   opção local explícita para desenvolvimento.
3. Criar migrações aditivas para identidade, sessões, auditoria, produto, estado
   ativo e concorrência; validar upgrade sobre cópia sintética da versão anterior.
4. Implementar autenticação, bootstrap e políticas; provisionar administrador de
   homologação por secret externo.
5. Publicar o novo contrato HTTP e adaptar os dois front-ends no mesmo release.
6. Executar testes, smoke test de login/CRUD/produto e verificação manual de
   teclado em homologação.
7. Fazer backup pré-deploy, aplicar a release coordenada e monitorar prontidão,
   erros 401/403/409/422 e falhas de renovação.
8. Remover qualquer compatibilidade temporária somente após confirmar que não há
   cliente antigo em uso.

**Rollback:** interromper o tráfego, restaurar o release manifest anterior e o
backup pré-deploy se a migração tiver alterado dados incompatíveis. Migrações serão
preferencialmente aditivas para permitir rollback do binário; remoções de colunas
ou tabelas ficam fora desta mudança.
