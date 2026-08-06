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
- implementar operação completa multi-instituição nos módulos de negócio,
  residente ou dados assistenciais; o IAM, porém, já delimitará toda identidade e
  decisão pela instituição para evitar uma migração insegura posterior;
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

### 6. Delimitar identidade e sessão pela instituição

A API usará os componentes consolidados do ecossistema ASP.NET para identidade e
derivação de senha, estendidos com `InstitutionId`, estado da conta e
`IdentityOrigin`. A primeira entrega implementará apenas origem `LOCAL`, mas o
modelo admitirá `LDAP` e `OIDC` sem fingir que os provedores já existem. Mesmo que
o piloto tenha uma única ILPI, toda identidade, sessão, configuração e decisão de
acesso carregará o contexto institucional; a interface ocultará a escolha quando
ela for inequívoca.

**Racional:** uma fronteira institucional explícita evita acesso cruzado e prepara
evolução futura sem antecipar multitenancy nos domínios assistencial e financeiro.

**Alternativa considerada:** adicionar instituição somente quando surgir a segunda
ILPI. Rejeitada porque migrar identidades e auditoria sem fronteira depois seria
mais arriscado e poderia produzir registros ambíguos.

### 7. Separar profissão, responsabilidade organizacional e acesso técnico

O modelo de acesso, adaptado do Qualitas, terá `User`, `Role`, `Permission`,
`PermissionGroup`, `OrganizationalRole`, `OrganizationalRoleAssignment`,
`UserPermissionOverride` e `AccessPolicy`. A profissão continuará sendo dado da
pessoa; não concederá acesso. Papéis técnicos agregarão grupos de permissões por
módulo. Responsabilidades organizacionais terão escopo de instituição, unidade ou
setor e validade. Exceções individuais `ALLOW`/`DENY` exigirão escopo, autoria,
justificativa e validade.

Um `AccessDecisionService` central avaliará recurso, ação, funcionalidade e alvo.
A precedência será: conta/contexto inválido; bypass restrito de `SYSTEM_ADMIN`;
`DENY` individual; política condicional de negação; `ALLOW` individual; política
condicional de concessão; RBAC; e negação padrão. `SYSTEM_ADMIN` será reservado a
operações sistêmicas, não atribuível a usuários operacionais e sempre auditado.

**Racional:** papéis fixos `Administrator`/`Operator` não representam a equipe
multidisciplinar nem responsabilidades temporárias. A precedência determinística e
a negação padrão tornam conflitos explicáveis e testáveis.

**Alternativas consideradas:** autorizar diretamente pelo cargo profissional,
rejeitada por misturar credencial profissional com necessidade operacional; ou
manter permissões somente no front-end, rejeitada porque o cliente não é fronteira
de segurança.

### 8. Adotar política moderna de credencial, MFA e sessão revogável

Contas locais exigirão senha mínima de 15 caracteres quando usada sozinha ou 8
quando MFA for obrigatório, aceitarão ao menos 64 caracteres, espaços e Unicode e
bloquearão valores comuns ou comprometidos. Não haverá composição arbitrária nem
troca periódica sem evidência de comprometimento. Parâmetros institucionais poderão
fortalecer, nunca enfraquecer, esse piso. A derivação adaptativa e seus parâmetros
ficarão sob o provedor de identidade e poderão ser atualizados no próximo login.

MFA por TOTP e códigos de recuperação será obrigatório para administradores e
configurável para os demais. O login emitirá acesso curto mantido em memória e uma
sessão de renovação rotativa em cookie `HttpOnly`, `Secure` e `SameSite`, com
proteção contra requisições forjadas, detecção de reutilização e revogação. Os dois
front-ends compartilharão a sessão; nenhum token será salvo em `localStorage` ou
`sessionStorage`.

**Racional:** segue orientação atual de segurança, reduz segredos conhecidos pela
instituição e combina boa experiência entre módulos com resposta a roubo de
sessão.

**Alternativas consideradas:** token duradouro acessível a JavaScript, rejeitado
por exposição a XSS; rotação periódica obrigatória de senha, rejeitada por induzir
padrões previsíveis sem evidência de benefício; MFA opcional para administradores,
rejeitada pelo impacto dessas contas.

### 9. Usar ativação, recuperação e bootstrap sem senha distribuída

O bootstrap idempotente criará a instituição inicial e uma conta administrativa
`PROVISIONED`. Em vez de receber uma senha permanente conhecida pela operação, a
conta concluirá ativação por token aleatório, curto, de uso único e armazenado por
hash. O mesmo mecanismo fundamentará recuperação, com resposta uniforme para
identificadores existentes e inexistentes. Após redefinição, sessões anteriores e
o security stamp serão invalidados. O startup normal jamais redefinirá credenciais.

**Racional:** permite instalação de baixo custo sem credencial padrão nem senha
transmitida por administrador.

**Alternativa considerada:** usuário e senha seed em migração ou variável de
ambiente permanente. Rejeitada por replicar e expor um segredo reutilizável.

### 10. Tornar configuração e decisões de acesso administráveis e auditáveis

APIs e telas específicas administrarão usuários, papéis, grupos, permissões,
responsabilidades organizacionais, exceções, parâmetros de segurança e sessões.
Um endpoint de contexto atual fornecerá somente as permissões efetivas necessárias
para menus e ações; a API continuará validando cada operação. Mudanças serão
versionadas ou historizadas e invalidarão o contexto efetivo quando necessário.

Auditoria append-only registrará autenticação, MFA, ativação, recuperação, sessão,
configuração e decisões protegidas com ator, instituição, recurso, ação,
funcionalidade, escopo, resultado, camada determinante, instante UTC e correlação.
Não armazenará senha, token, código MFA nem payload integral. O domínio não
oferecerá update/delete desses registros.

**Racional:** a equipe da ILPI precisa ajustar acesso sem mudança de código, e a
universidade precisa explicar posteriormente por que uma ação foi permitida ou
negada, sem sugerir que essa auditoria já satisfaz requisitos clínicos futuros.

**Alternativa considerada:** configuração por seed e logs de aplicação. Rejeitada
por não atender mudanças operacionais, histórico de autoria e consulta por decisão.

### 11. Testar com a mesma classe de banco usada em produção

O backend terá testes unitários e integração com PostgreSQL efêmero, aplicação de
migrações e chamadas HTTP pela aplicação hospedada em teste. Os front-ends usarão
runner TypeScript, biblioteca de testes por comportamento, mocks HTTP e verificação
automatizada de acessibilidade. O CI publicará resultados e cobertura.

**Racional:** banco em memória esconderia diferenças de constraints, concorrência
e SQL; testes de componentes são mais estáveis que snapshots extensos.

**Alternativas consideradas:** SQLite/in-memory para toda integração, rejeitados
como única evidência; testes end-to-end completos em navegador para tudo,
reservados a poucos smoke tests por custo e fragilidade.

### 12. Evoluir acessibilidade nos componentes compartilhados atuais

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
- **[Bloqueio de instalação sem administrador]** → validar parâmetros, executar o
  bootstrap e testar o canal de ativação antes de exigir autenticação; fornecer
  diagnóstico operacional sem expor o token.
- **[Complexidade do IAM maior que a base atual]** → entregar por camadas: conta e
  instituição, sessão/MFA, RBAC, escopo organizacional e exceções; manter negação
  padrão entre etapas e cobrir a precedência com tabela de testes.
- **[Administrador remove o próprio acesso ou o último acesso privilegiado]** →
  validar invariantes, exigir reautenticação para mudanças críticas e impedir a
  inativação do último administrador institucional ativo.
- **[Mudança de permissão não alcança sessão aberta]** → versionar o contexto de
  acesso e invalidá-lo nas alterações, mantendo tokens de acesso curtos.
- **[Canal de ativação indisponível em ILPI de baixo orçamento]** → definir um modo
  operacional de entrega do link de uso único sem imprimir ou registrar o token e
  exigir confirmação da identidade pelo procedimento institucional.
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
3. Criar migrações aditivas para instituição, identidade, credenciais, MFA,
   sessões, autorização, auditoria, produto, estado ativo e concorrência; validar
   upgrade sobre cópia sintética da versão anterior.
4. Implementar identidade local, ativação, recuperação, bootstrap e sessão;
   provisionar instituição e administrador `PROVISIONED` de homologação.
5. Implementar o serviço de decisão, RBAC, configuração administrativa e auditoria
   antes de proteger os endpoints com negação padrão.
6. Publicar o novo contrato HTTP e adaptar os dois front-ends no mesmo release,
   removendo qualquer token persistido no navegador.
7. Executar testes de precedência, MFA, sessão, login, CRUD e produto, além da
   verificação manual de teclado em homologação.
8. Fazer backup pré-deploy, aplicar a release coordenada e monitorar prontidão,
   ativações, erros 401/403/409/422, falhas de renovação e negações anormais.
9. Remover qualquer compatibilidade temporária somente após confirmar que não há
   cliente antigo em uso.

**Rollback:** interromper o tráfego, restaurar o release manifest anterior e o
backup pré-deploy se a migração tiver alterado dados incompatíveis. Migrações serão
preferencialmente aditivas para permitir rollback do binário; remoções de colunas
ou tabelas ficam fora desta mudança.
