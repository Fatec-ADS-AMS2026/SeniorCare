# Feature Specification: Estabilizar a plataforma existente

**Feature ID**: `001-stabilize-existing-platform`
**Created**: 2026-09-26 (data da migração)
**Status**: Concluída
**Input**: artefatos legados incorporados nesta feature durante a migração

> Este documento preserva requisitos e cenários verificáveis existentes. A migração
> não inventa prioridades P1/P2/P3 retroativas; os cenários associados a cada
> requisito continuam sendo os testes de aceitação autoritativos.

## User Scenarios & Testing

Os fluxos verificáveis estão expressos como cenários `WHEN`/`THEN` sob cada
requisito. Para trabalho novo, o fluxo Spec Kit deve acrescentar histórias
priorizadas e testes independentes sem enfraquecer estes contratos existentes.

## Contexto e escopo

### Why

A base implementada do SeniorCare ainda não constitui uma linha de entrega
confiável: o front-end assistencial não compila, os front-ends usam uma URL de API
fixa, o produto de estoque não possui backend, o login é apenas visual e não há
testes automatizados. Corrigir essas lacunas agora evita que os futuros módulos de
residente e prontuário sejam construídos sobre contratos inconsistentes e uma
plataforma incapaz de proteger dados pessoais sensíveis.

### What Changes

- Restaurar builds reproduzíveis dos três componentes e configurar URLs e opções
  de execução por ambiente, com falha explícita para configuração inválida.
- Padronizar os contratos dos CRUDs auxiliares, incluindo validação, paginação,
  respostas de erro, semântica de atualização e tratamento de exclusão.
- Completar o catálogo de produtos no backend e no banco, alinhando-o à interface
  de estoque já existente.
- Transformar o login visual em uma capacidade institucional de identidade e
  acesso, adaptada do Qualitas: contas individuais com ciclo de vida, política de
  senha, MFA, sessões compartilhadas entre os módulos, papéis técnicos, grupos de
  permissões, responsabilidades organizacionais, exceções individuais e decisões
  de acesso auditáveis.
- Criar APIs e telas administrativas para configurar usuários, papéis, grupos,
  permissões, vínculos organizacionais, políticas de segurança e sessões ativas,
  mantendo a autorização efetiva no backend.
- Consolidar uma baseline de acessibilidade para os componentes já existentes,
  com navegação por teclado, nomes acessíveis, foco visível, contraste e ajuste de
  fonte persistente.
- Introduzir testes automatizados de backend e front-end e torná-los gates do CI,
  junto aos builds, lint e verificações de segurança existentes.
- Documentar migração, compatibilidade e critérios de aceite da plataforma
  estabilizada.
- **BREAKING**: endpoints administrativos deixarão de aceitar acesso anônimo e
  passarão a retornar um envelope de erro uniforme; clientes deverão autenticar-se,
  respeitar as permissões efetivas configuradas e tratar os códigos HTTP definidos
  nas specs.
- Manter fora desta mudança residente, prontuário multidisciplinar, cuidado por
  turno, medicamentos, nutrição, financeiro, doações, dashboards e assinatura
  eletrônica.

### Capabilities

#### New Capabilities

- `runtime-configuration`: build reproduzível, configuração da API por ambiente,
  validação de startup e diagnóstico operacional dos componentes atuais.
- `support-catalogs`: contratos consistentes para os cadastros auxiliares e fluxo
  integrado do catálogo de produtos.
- `platform-authentication`: identidade vinculada à instituição, autenticação
  individual, política de senha, MFA, sessões revogáveis, RBAC enriquecido com
  escopo organizacional e exceções, configuração administrativa e auditoria de
  decisões de acesso.
- `accessibility-baseline`: comportamento acessível e preferências visuais nos
  componentes e fluxos já implementados.
- `automated-quality-gates`: testes automatizados e gates obrigatórios de
  qualidade para backend e front-ends.

#### Modified Capabilities

Nenhuma. O repositório ainda não possui specs principais publicadas; todas as
capacidades desta mudança inauguram contratos verificáveis para a base existente.

### Impact

- **Domínios afetados:** capacidades de apoio de profissionais (cargos), jornada
  do residente (planos de saúde e religião), estoque e operação (catálogos e
  produtos) e requisitos transversais de segurança e acessibilidade.
- **Atores afetados:** administradores institucionais e de segurança,
  trabalhadores autorizados, profissionais da equipe multidisciplinar,
  desenvolvedores, equipe de operação e, indiretamente, futuros usuários
  assistenciais. Cargo ou profissão não concederá acesso técnico implicitamente,
  e residentes não terão fluxo funcional criado nesta mudança.
- **Código:** API ASP.NET Core, Entity Framework/migrações, ambos os front-ends
  React, configuração Docker/nginx e workflows GitHub Actions.
- **APIs e dados:** novos endpoints e entidades para instituição, usuários,
  credenciais, MFA, papéis, permissões, grupos, vínculos organizacionais, exceções,
  políticas, sessões e auditoria, além do endpoint de produto e da revisão dos
  contratos CRUD; novas migrações; configuração de usuários iniciais sem
  credencial fixa no código ou no repositório.
- **Risco assistencial:** a mudança não autoriza uso com prontuários ou dados reais
  de saúde. Até existir autorização contextual, auditoria e o núcleo longitudinal,
  os ambientes de desenvolvimento, teste e demonstração devem usar dados
  sintéticos.
- **Impacto regulatório:** a autenticação e os controles mínimos reduzem exposição,
  mas não demonstram conformidade integral com a LGPD, S-RES, NGS2/SBIS ou normas
  profissionais. Nenhuma operação sem papel ou assinatura eletrônica será
  declarada.

## Requirements

### Capacidade `runtime-configuration`

_Contrato de capacidade migrado sem alteração normativa._

## runtime-configuration Specification

### Purpose
Estabelece uma execução reproduzível e configurável para a API e os dois
front-ends, evitando dependências de endereços locais e falhas silenciosas entre
desenvolvimento, teste e produção.
### Requirements
#### Requirement: Todos os componentes produzem artefatos de entrega
Cada componente versionado SHALL concluir seu build de produção a partir de uma
instalação limpa das dependências e sem erros de compilação, tipagem ou resolução
de módulos.

##### Scenario: Build limpo da plataforma
- **WHEN** o pipeline executa os builds da API, do front-end assistencial e do front-end de estoque em um checkout limpo
- **THEN** os três builds terminam com sucesso e produzem os artefatos esperados

##### Scenario: Erro de tipagem bloqueia a entrega
- **WHEN** um front-end contém uma incompatibilidade TypeScript
- **THEN** o build falha e a versão não pode avançar para publicação

#### Requirement: Endereço da API é configurável por ambiente
Cada front-end SHALL obter o endereço público da API de configuração fornecida no
build ou na execução, sem depender de `localhost` em artefatos de produção.

##### Scenario: Execução local
- **WHEN** o front-end é iniciado no ambiente de desenvolvimento com a URL local configurada
- **THEN** suas requisições usam a API local informada

##### Scenario: Execução em produção
- **WHEN** o front-end é publicado com a URL de produção configurada
- **THEN** o navegador envia as requisições para essa URL e o bundle não contém a URL local como destino operacional

##### Scenario: Configuração obrigatória ausente
- **WHEN** um build ou startup de produção não recebe a URL obrigatória da API
- **THEN** o processo falha com uma mensagem de configuração clara e sem expor segredos

#### Requirement: Configurações e segredos são separados
A plataforma SHALL aceitar opções não secretas por configuração versionável e
segredos somente por mecanismos externos ao repositório, mantendo valores reais
fora de imagens, bundles, logs e arquivos rastreados.

##### Scenario: Credencial fornecida por ambiente
- **WHEN** a API inicia em produção
- **THEN** credenciais de banco, chaves de autenticação e usuário inicial são obtidos de fontes externas ao código e não são registrados em log

##### Scenario: Exemplo de configuração
- **WHEN** um desenvolvedor prepara um novo ambiente
- **THEN** encontra um modelo sem segredos que enumera as variáveis obrigatórias e seus formatos

#### Requirement: Diagnóstico distingue vida e prontidão
A API SHALL expor diagnóstico de vida do processo e prontidão das dependências
necessárias, sem divulgar dados pessoais, credenciais ou detalhes internos
sensíveis.

##### Scenario: Processo vivo com banco indisponível
- **WHEN** o processo da API está em execução, mas o banco não responde
- **THEN** a verificação de vida permanece disponível e a verificação de prontidão informa indisponibilidade

##### Scenario: Serviço pronto
- **WHEN** API e banco estão operacionais e a migração exigida está aplicada
- **THEN** a verificação de prontidão retorna sucesso
### Capacidade `support-catalogs`

_Contrato de capacidade migrado sem alteração normativa._

## support-catalogs Specification

### Purpose
Define contratos previsíveis e seguros para os cadastros auxiliares já presentes
e completa o catálogo de produtos necessário ao front-end de estoque.
### Requirements
#### Requirement: Contrato uniforme para cadastros auxiliares
Os cadastros de plano de saúde, cargo, religião, fornecedor, fabricante,
transportadora, grupo de produto, tipo de produto, unidade de medida e produto
SHALL usar a mesma convenção de sucesso, paginação e erro.

##### Scenario: Consulta paginada
- **WHEN** um usuário autorizado consulta um cadastro com página, tamanho e filtro válidos
- **THEN** a API retorna os itens, a paginação solicitada e o total encontrado em um contrato uniforme

##### Scenario: Recurso inexistente
- **WHEN** um usuário autorizado consulta, altera ou remove um identificador inexistente
- **THEN** a API retorna HTTP 404 no envelope de erro padronizado

##### Scenario: Requisição inválida
- **WHEN** os dados enviados violam uma regra de validação
- **THEN** a API retorna HTTP 400 ou 422 com erros por campo e não altera o banco

##### Scenario: Falha interna
- **WHEN** ocorre uma falha inesperada
- **THEN** a API retorna um identificador de correlação sem expor exceção, credencial, consulta ou estrutura interna

#### Requirement: Alterações preservam identidade e concorrência
Uma atualização SHALL usar o identificador da rota como identidade canônica,
rejeitar divergências e detectar edição concorrente para não sobrescrever uma
alteração posterior sem aviso.

##### Scenario: Identificador divergente
- **WHEN** o identificador do corpo diverge do identificador da rota
- **THEN** a API rejeita a requisição e nenhum registro é alterado

##### Scenario: Versão desatualizada
- **WHEN** um cliente tenta salvar uma versão anterior à versão persistida
- **THEN** a API retorna conflito e fornece informação suficiente para o cliente recarregar o registro

#### Requirement: Exclusão respeita referências e histórico administrativo
Um cadastro referenciado SHALL NOT ser removido fisicamente. A plataforma SHALL
permitir inativação quando o item não puder mais ser usado, preservando registros
que já o referenciam.

##### Scenario: Inativação de item em uso
- **WHEN** um administrador inativa um item já referenciado
- **THEN** o item deixa de aparecer como opção para novos registros e permanece legível nos registros existentes

##### Scenario: Exclusão incompatível
- **WHEN** um cliente solicita exclusão física de item referenciado
- **THEN** a API retorna conflito e não produz exclusão em cascata indevida

#### Requirement: Catálogo de produtos é integrado de ponta a ponta
A plataforma SHALL persistir e disponibilizar produtos com descrição, nome
genérico, grupo/tipo, unidade de medida e os atributos de controle já apresentados
na interface de estoque.

##### Scenario: Cadastro válido de produto
- **WHEN** um administrador informa os campos obrigatórios e referências ativas válidas
- **THEN** o produto é persistido e pode ser consultado pelos dois lados da integração

##### Scenario: Referência inválida
- **WHEN** o produto referencia tipo ou unidade inexistente ou inativa
- **THEN** a API rejeita o cadastro com erro no campo correspondente

##### Scenario: Consulta e filtro de produtos
- **WHEN** um usuário autorizado pesquisa por descrição ou nome genérico
- **THEN** a API retorna os produtos correspondentes de forma paginada

##### Scenario: Limite do catálogo
- **WHEN** um produto é cadastrado ou atualizado nesta capacidade
- **THEN** a operação não cria movimento, lote, recebimento, dispensação ou inventário, que permanecem fora desta mudança

#### Requirement: Operações administrativas são atribuíveis
Criação, alteração e inativação de cadastros SHALL registrar usuário autenticado,
data/hora, tipo de ação e identificador do recurso, sem armazenar senha ou token.

##### Scenario: Alteração autenticada
- **WHEN** um administrador altera um cadastro
- **THEN** a plataforma preserva uma entrada de auditoria atribuível à sessão responsável
### Capacidade `platform-authentication`

_Contrato de capacidade migrado sem alteração normativa._

## platform-authentication Specification

### Purpose
Substitui o login meramente visual por uma capacidade institucional de identidade,
autenticação, autorização e auditoria, compartilhada pelos módulos do SeniorCare e
preparada para a futura proteção contextual de informações assistenciais.
### Requirements
#### Requirement: Identidade é individual e vinculada à instituição
A plataforma SHALL manter uma conta individual para cada pessoa usuária e SHALL
vincular toda identidade local a uma instituição. Contas compartilhadas e senhas
fixas no código, banco de demonstração ou repositório SHALL NOT ser permitidas.
Enquanto a instalação possuir uma única instituição habilitada, a interface MAY
ocultar a seleção de instituição sem remover esse limite da autorização.

##### Scenario: Credenciais válidas na única instituição
- **WHEN** uma pessoa com conta ativa apresenta credenciais válidas e existe uma única instituição habilitada
- **THEN** a plataforma autentica a identidade nesse contexto institucional sem exigir uma seleção redundante

##### Scenario: Identidade não pertence à instituição
- **WHEN** uma identidade tenta iniciar ou usar sessão em instituição à qual não está vinculada
- **THEN** a plataforma nega o acesso sem revelar dados de outra instituição e registra a decisão

##### Scenario: Conta compartilhada
- **WHEN** um administrador tenta cadastrar uma conta operacional sem identidade individual atribuível
- **THEN** a plataforma rejeita o cadastro e orienta a criação de uma conta individual

#### Requirement: Conta possui ciclo de vida controlado
Cada conta SHALL possuir um dos estados `PROVISIONED`, `ACTIVE`, `INACTIVE`,
`BLOCKED` ou `EXPIRED`. Somente contas `ACTIVE` SHALL iniciar sessões; bloqueio,
inativação ou expiração SHALL impedir novas autenticações e revogar ou invalidar
as sessões existentes conforme a política institucional.

##### Scenario: Conta provisionada
- **WHEN** uma pessoa recebe convite válido para uma conta `PROVISIONED`
- **THEN** ela define a própria senha, conclui os fatores exigidos e a conta passa a `ACTIVE`

##### Scenario: Conta inativa, bloqueada ou expirada
- **WHEN** uma conta em estado diferente de `ACTIVE` tenta autenticar-se
- **THEN** a plataforma nega a sessão com mensagem genérica e registra o estado determinante sem expô-lo ao cliente anônimo

##### Scenario: Administrador inativa conta com sessão aberta
- **WHEN** um administrador autorizado inativa uma conta
- **THEN** as sessões dessa conta deixam de autorizar novas requisições e a alteração fica auditada

#### Requirement: Origem da identidade é extensível
A plataforma SHALL registrar a origem de cada identidade como `LOCAL`, `LDAP` ou
`OIDC`. Esta mudança SHALL implementar autenticação `LOCAL`; as demais origens
SHALL permanecer pontos de extensão e SHALL NOT simular integração inexistente.

##### Scenario: Identidade local
- **WHEN** uma conta de origem `LOCAL` autentica-se
- **THEN** a credencial é validada pelo provedor local conforme a política de senha vigente

##### Scenario: Origem ainda não habilitada
- **WHEN** um administrador tenta habilitar `LDAP` ou `OIDC` sem provedor configurado
- **THEN** a plataforma recusa a ativação e informa que a integração não está disponível

#### Requirement: Política de senha segue práticas atuais e possui piso seguro
Para contas locais, a plataforma SHALL exigir no mínimo 15 caracteres quando a
senha for o único fator, ou no mínimo 8 caracteres quando MFA for obrigatório para
a conta. A plataforma SHALL aceitar senhas com pelo menos 64 caracteres, espaços e
caracteres Unicode; SHALL comparar novas senhas com lista de valores comuns ou
comprometidos; SHALL NOT impor composição arbitrária por classes de caracteres; e
SHALL NOT exigir troca periódica sem evidência de comprometimento. Configuração
institucional MAY fortalecer, mas SHALL NOT enfraquecer, esses limites.

##### Scenario: Senha longa e válida
- **WHEN** uma pessoa define uma senha dentro do tamanho aceito que não consta na lista de bloqueio
- **THEN** a plataforma aceita espaços e caracteres Unicode sem truncamento silencioso

##### Scenario: Senha comum ou comprometida
- **WHEN** uma pessoa tenta definir uma senha presente na lista de bloqueio
- **THEN** a plataforma rejeita a senha e fornece orientação para escolher outra sem revelar dados sensíveis da verificação

##### Scenario: Regra institucional enfraquece o piso
- **WHEN** um administrador tenta configurar um mínimo inferior ao aplicável ou limitar senhas a menos de 64 caracteres
- **THEN** a plataforma rejeita a configuração e preserva o piso seguro

##### Scenario: Senha antiga sem indício de comprometimento
- **WHEN** uma senha atinge uma idade arbitrária sem evento de risco ou regra legal específica aplicável
- **THEN** a plataforma não força sua troca apenas pelo tempo decorrido

#### Requirement: Senhas são derivadas e nunca recuperáveis
Senhas locais SHALL ser armazenadas somente por derivação criptográfica adaptativa,
com parâmetros atualizáveis e salt individual. Senhas, códigos MFA, tokens de
ativação, recuperação e sessão SHALL NOT aparecer em logs, respostas
administrativas ou exportações. Uma autenticação válida MAY atualizar
transparentemente uma derivação obsoleta.

##### Scenario: Persistência de nova senha
- **WHEN** uma senha local é criada ou alterada
- **THEN** somente sua derivação protegida e os metadados necessários são persistidos

##### Scenario: Administrador consulta usuário
- **WHEN** um administrador autorizado consulta ou edita uma conta
- **THEN** nenhuma senha, derivação, segredo MFA ou token é retornado

#### Requirement: Ativação e recuperação não distribuem senha conhecida
Administradores SHALL criar a conta e disparar ativação ou recuperação, mas SHALL
NOT visualizar nem definir uma senha permanente conhecida por eles. Ativação e
recuperação SHALL usar token aleatório, armazenado de forma não recuperável, de uso
único e com validade curta. As respostas públicas SHALL ser uniformes para impedir
enumeração de contas.

##### Scenario: Ativação inicial
- **WHEN** uma conta `PROVISIONED` recebe e utiliza um token de ativação válido
- **THEN** a pessoa define a própria senha e o token é invalidado após o uso

##### Scenario: Solicitação de recuperação
- **WHEN** alguém solicita recuperação para um identificador existente ou inexistente
- **THEN** a plataforma retorna a mesma resposta pública e somente envia instruções quando houver conta elegível

##### Scenario: Token expirado ou reutilizado
- **WHEN** um token de ativação ou recuperação expirado ou já utilizado é apresentado
- **THEN** a plataforma rejeita a operação sem alterar credenciais

##### Scenario: Senha redefinida
- **WHEN** uma recuperação válida conclui a definição de nova senha
- **THEN** as sessões anteriores da conta são revogadas e o evento é auditado

#### Requirement: Mudança autenticada de senha exige confirmação de identidade
Uma pessoa autenticada SHALL informar a senha atual ou concluir reautenticação
recente antes de alterar a senha. Após a mudança, a plataforma SHALL revogar as
demais sessões e MAY preservar somente a sessão atual quando a política permitir.

##### Scenario: Senha atual incorreta
- **WHEN** uma pessoa tenta alterar a senha sem confirmar a senha atual ou reautenticação aceita
- **THEN** a plataforma nega a alteração e mantém as sessões e credencial existentes

##### Scenario: Mudança concluída
- **WHEN** a pessoa confirma a identidade e define uma nova senha válida
- **THEN** a credencial é atualizada, as sessões determinadas pela política são revogadas e o evento é auditado

#### Requirement: Autenticação multifator protege contas privilegiadas
MFA SHALL ser obrigatório para administradores e contas com privilégios de
configuração de acesso, e SHALL ser configurável para os demais usuários. A
primeira entrega SHALL suportar TOTP e códigos de recuperação de uso único,
armazenados de forma protegida. Uma conta sujeita a MFA SHALL NOT concluir a sessão
antes de validar o segundo fator.

##### Scenario: Administrador sem MFA cadastrado
- **WHEN** um administrador com credenciais primárias válidas ainda não cadastrou MFA
- **THEN** a plataforma restringe a sessão ao fluxo de cadastro e confirmação do segundo fator

##### Scenario: Segundo fator inválido
- **WHEN** uma conta sujeita a MFA apresenta código inválido ou reutilizado
- **THEN** a plataforma nega a conclusão da sessão e registra a falha sem registrar o código

##### Scenario: Código de recuperação
- **WHEN** a pessoa utiliza um código de recuperação válido
- **THEN** a plataforma conclui a verificação, invalida somente esse código e alerta para a quantidade restante

#### Requirement: Sessão é compartilhada, curta, rotativa e revogável
Uma autenticação SHALL produzir uma sessão institucional válida para os módulos
assistencial e de estoque, sem novo login entre eles. O acesso SHALL usar credencial
de curta duração mantida em memória e renovação protegida por cookie `HttpOnly`,
`Secure` e política `SameSite` adequada. Credenciais de autenticação SHALL NOT ser
persistidas em `localStorage` ou `sessionStorage`. Tokens de renovação SHALL ser
rotacionados, detectados quando reutilizados e revogáveis individualmente ou por
conta.

##### Scenario: Navegação entre módulos
- **WHEN** uma pessoa com sessão válida e permissões efetivas abre outro módulo do SeniorCare
- **THEN** o módulo reutiliza a sessão e solicita novo login apenas se ela não puder ser renovada

##### Scenario: Sessão expirada
- **WHEN** a credencial de acesso expira e a renovação não é válida
- **THEN** a API retorna HTTP 401 e a interface solicita nova autenticação

##### Scenario: Rotação de renovação
- **WHEN** uma renovação válida é utilizada
- **THEN** a plataforma invalida o token anterior e emite uma nova credencial de renovação para a mesma sessão

##### Scenario: Reutilização de token rotacionado
- **WHEN** um token de renovação já rotacionado é reapresentado
- **THEN** a plataforma revoga a família de sessão afetada e registra o evento sem registrar o token

##### Scenario: Logout
- **WHEN** a pessoa encerra a sessão
- **THEN** a sessão é revogada, o cookie protegido é removido e novas requisições são recusadas

#### Requirement: Tentativas de autenticação são protegidas contra abuso
O serviço SHALL combinar limitação por conta e origem, atraso progressivo ou
bloqueio temporário configurável e resposta uniforme. O bloqueio SHALL evitar
negação permanente provocada por terceiros, e seus limites SHALL possuir valores
seguros mesmo quando nenhuma configuração institucional for informada.

##### Scenario: Repetição de falhas
- **WHEN** uma origem ou conta excede o limite de falhas dentro da janela configurada
- **THEN** novas tentativas são temporariamente limitadas sem permitir enumeração de usuários

##### Scenario: Autenticação posterior ao bloqueio temporário
- **WHEN** o intervalo de bloqueio termina e credenciais válidas são apresentadas
- **THEN** a autenticação pode prosseguir e os contadores são atualizados conforme a política

#### Requirement: Profissão, papel técnico e responsabilidade organizacional são distintos
A plataforma SHALL representar separadamente: cargo ou profissão da pessoa; papel
técnico que agrega permissões; e responsabilidade organizacional exercida em uma
instituição, unidade ou setor por período de validade. Cargo ou profissão SHALL NOT
conceder permissão técnica implicitamente. Uma responsabilidade organizacional MAY
conceder somente as capacidades explicitamente configuradas para ela.

##### Scenario: Profissional sem papel técnico
- **WHEN** uma pessoa possui profissão cadastrada, mas nenhum papel ou vínculo com capacidade de acesso
- **THEN** ela não recebe permissões técnicas por causa da profissão

##### Scenario: Responsabilidade vencida
- **WHEN** o período de validade de uma atribuição organizacional termina
- **THEN** as capacidades derivadas dessa atribuição deixam de compor o acesso efetivo

##### Scenario: Responsabilidade limitada ao setor
- **WHEN** uma atribuição organizacional é válida somente para determinado setor
- **THEN** as capacidades dela não autorizam ação equivalente fora desse escopo

#### Requirement: Permissões são compostas por recurso, ação e funcionalidade
Cada permissão SHALL identificar recurso, ação e, quando aplicável,
funcionalidade. Permissões SHALL poder ser agrupadas por módulo, e papéis técnicos
SHALL ser compostos por um ou mais grupos. Alterar a composição SHALL afetar novas
decisões sem exigir alteração do código cliente.

##### Scenario: Papel recebe grupo de módulo
- **WHEN** um administrador autorizado associa um grupo de permissões a um papel
- **THEN** usuários com esse papel passam a receber as permissões do grupo dentro dos escopos válidos

##### Scenario: Permissão removida do grupo
- **WHEN** uma permissão é removida de um grupo
- **THEN** ela deixa de ser concedida pelo grupo nas decisões subsequentes e a alteração é auditada

#### Requirement: Exceções individuais são explícitas, limitadas e justificadas
Um administrador autorizado MAY criar exceção individual `ALLOW` ou `DENY` para
recurso, ação, funcionalidade e escopo determinados. Toda exceção SHALL possuir
justificativa, autoria, início e término de validade; exceções permanentes SHALL
exigir justificativa destacada. `DENY` individual válido SHALL prevalecer sobre
concessões individuais, condicionais ou baseadas em papel.

##### Scenario: Exceção temporária de concessão
- **WHEN** uma exceção `ALLOW` válida corresponde à ação solicitada e nenhuma negação prioritária se aplica
- **THEN** a ação é autorizada até o fim da validade definida

##### Scenario: Exceção de negação
- **WHEN** uma exceção `DENY` válida corresponde à ação solicitada
- **THEN** a ação é negada mesmo que um papel conceda a permissão

##### Scenario: Exceção expirada
- **WHEN** a validade de uma exceção termina
- **THEN** ela deixa de influenciar decisões sem precisar ser excluída do histórico

#### Requirement: Decisão de acesso segue precedência determinística e negação padrão
O backend SHALL ser a autoridade final de acesso e SHALL avaliar, nesta ordem:
estado e contexto institucional; bypass restrito de `SYSTEM_ADMIN`; `DENY`
individual; política condicional de negação; `ALLOW` individual; política
condicional de concessão; RBAC por papéis, grupos e permissões; e, na ausência de
concessão, `DENY` padrão. `SYSTEM_ADMIN` SHALL ser reservado a operações do sistema,
não SHALL ser atribuível a usuários operacionais da ILPI e todo uso SHALL ser
auditado.

##### Scenario: Interface exibe ação não autorizada
- **WHEN** um cliente manipulado solicita uma ação ausente das permissões efetivas
- **THEN** o backend retorna HTTP 403 e nenhum dado é alterado

##### Scenario: Regras conflitantes
- **WHEN** mais de uma camada produz resultados conflitantes
- **THEN** a decisão segue a precedência definida e registra a camada determinante

##### Scenario: Nenhuma regra concede acesso
- **WHEN** nenhuma concessão válida corresponde ao recurso, ação, funcionalidade e escopo
- **THEN** o backend nega o acesso por padrão

##### Scenario: Operação sistêmica privilegiada
- **WHEN** uma identidade técnica `SYSTEM_ADMIN` executa operação sistêmica autorizada
- **THEN** o bypass é limitado à operação prevista e gera registro de auditoria destacado

#### Requirement: Cliente obtém contexto e permissões efetivas sem decidir autorização
A plataforma SHALL fornecer endpoint autenticado da identidade atual contendo
instituição, módulos, papéis, responsabilidades válidas e permissões efetivas
necessárias à interface. O front-end SHALL usar esses dados para navegação e
visibilidade, mas SHALL NOT substituir a validação de cada requisição pelo backend.
Detalhes internos de políticas, segredos e regras não aplicáveis SHALL NOT ser
expostos ao usuário comum.

##### Scenario: Carregamento da aplicação
- **WHEN** uma sessão válida carrega um módulo
- **THEN** o cliente obtém o contexto atual e oculta ou desabilita funções sem permissão efetiva

##### Scenario: Permissão alterada durante a sessão
- **WHEN** uma configuração de acesso muda para a identidade autenticada
- **THEN** decisões posteriores usam a nova configuração e o contexto do cliente é atualizado ou invalidado

#### Requirement: Administração de acesso possui configuração dedicada
A plataforma SHALL oferecer APIs e telas protegidas para administrar usuários,
papéis, grupos, permissões, vínculos organizacionais, exceções individuais,
políticas de segurança e sessões ativas. Somente identidades com permissão
específica SHALL alterar essas configurações. Mudanças SHALL ser validadas,
versionadas ou historizadas e auditadas.

##### Scenario: Administrador configura papel
- **WHEN** um administrador de acesso autorizado altera grupos ou permissões de um papel
- **THEN** a plataforma valida referências e escopos, aplica a nova versão e registra antes, depois, autoria e justificativa quando exigida

##### Scenario: Operador consulta configuração protegida
- **WHEN** uma identidade sem permissão administrativa tenta consultar ou alterar configuração de acesso
- **THEN** a API retorna HTTP 403 e não expõe a configuração

##### Scenario: Administrador revoga sessão
- **WHEN** um administrador autorizado revoga uma sessão ativa de uma conta
- **THEN** essa sessão deixa de autorizar requisições sem afetar sessões não selecionadas, salvo decisão explícita de revogar todas

#### Requirement: Parâmetros de segurança são configuráveis dentro de limites seguros
Administradores autorizados SHALL poder configurar duração de bloqueio, limites de
tentativas, duração de acesso e renovação, exigência de MFA e fortalecimento da
política de senha. A plataforma SHALL validar limites mínimos e máximos seguros,
SHALL impedir configurações incompatíveis e SHALL auditar toda alteração.

##### Scenario: Configuração válida
- **WHEN** um administrador salva parâmetros dentro dos limites permitidos
- **THEN** a plataforma aplica a configuração à instituição, preserva sua versão anterior no histórico e informa quando ela passa a valer

##### Scenario: Sessão excessivamente longa
- **WHEN** um administrador tenta configurar duração superior ao limite de segurança
- **THEN** a plataforma rejeita a alteração e mantém a configuração anterior

#### Requirement: Credencial administrativa inicial é provisionada com segurança
A primeira identidade administrativa SHALL ser criada por procedimento explícito,
idempotente e limitado à instituição inicial. O procedimento SHALL receber dados
sensíveis fora do repositório e SHALL preferir convite de ativação à distribuição
de senha pronta.

##### Scenario: Primeiro provisionamento
- **WHEN** uma instalação vazia recebe parâmetros válidos de instituição e administrador
- **THEN** uma única instituição e conta `PROVISIONED` são criadas e a ativação segura é iniciada

##### Scenario: Reinício posterior
- **WHEN** a instalação já provisionada reinicia com os mesmos parâmetros
- **THEN** nenhuma conta duplicada é criada e nenhuma credencial é redefinida silenciosamente

#### Requirement: Eventos de identidade, configuração e acesso são auditáveis
A plataforma SHALL auditar autenticações, falhas relevantes, MFA, ativações,
recuperações, mudanças de credencial, estados de conta, sessões, configuração de
acesso e decisões protegidas. Cada decisão SHALL registrar ator, instituição,
recurso, ação, funcionalidade, escopo-alvo, resultado, camada ou regra determinante,
data, correlação e metadados necessários à investigação, sem credenciais ou dados
secretos.

##### Scenario: Acesso negado
- **WHEN** uma requisição autenticada é negada por autorização
- **THEN** o registro identifica a decisão e sua camada determinante sem registrar token ou senha

##### Scenario: Configuração de acesso alterada
- **WHEN** uma regra, papel, vínculo, exceção ou parâmetro de segurança é alterado
- **THEN** o registro associa autoria, instituição, instante e valores anterior e posterior permitidos para auditoria

##### Scenario: Correlação de evento
- **WHEN** uma investigação consulta uma requisição protegida
- **THEN** os eventos relacionados podem ser correlacionados sem expor segredos da sessão

#### Requirement: Autenticação não implica autorização clínica
As permissões desta capacidade SHALL cobrir apenas funções existentes e a
administração da plataforma. Elas SHALL NOT conceder acesso futuro a prontuário,
prescrição, evolução multidisciplinar ou outros dados clínicos por inferência.

##### Scenario: Introdução futura de dado assistencial
- **WHEN** uma capacidade clínica for adicionada
- **THEN** ela exige especificação própria de autorização contextual, consentimento quando aplicável e auditoria antes de reutilizar identidades ou papéis
### Capacidade `accessibility-baseline`

_Contrato de capacidade migrado sem alteração normativa._

## accessibility-baseline Specification

### Purpose
Define uma baseline acessível para login, navegação, tabelas, formulários e modais
já existentes, reduzindo barreiras para trabalhadores com diferentes condições e
níveis de familiaridade digital.
### Requirements
#### Requirement: Fluxos existentes são operáveis por teclado
Todos os controles interativos dos fluxos existentes SHALL ser alcançáveis e
acionáveis por teclado, em ordem coerente, com foco visível e sem armadilha de
foco.

##### Scenario: Navegação pelo login
- **WHEN** uma pessoa usa somente teclado no formulário de login
- **THEN** ela percorre campos, visualização de senha e envio em ordem lógica e identifica visualmente o foco

##### Scenario: Modal aberto
- **WHEN** um modal é aberto por teclado
- **THEN** o foco entra no modal, permanece nele enquanto aberto, fecha por mecanismo documentado e retorna ao controle de origem

#### Requirement: Controles possuem nome, estado e instrução acessíveis
Campos, botões, links, ícones e mensagens SHALL expor nomes e estados compreensíveis
a tecnologias assistivas; significado não SHALL depender apenas de cor ou forma.

##### Scenario: Botão somente com ícone
- **WHEN** um leitor de tela encontra ação de editar, excluir, aumentar fonte ou alterar contraste
- **THEN** anuncia o propósito e o estado pertinente da ação

##### Scenario: Erro de formulário
- **WHEN** a submissão contém campos inválidos
- **THEN** o resumo e cada campo indicam o erro em texto, associam-no ao controle e movem ou orientam o foco de forma previsível

#### Requirement: Preferências visuais são persistentes e limitadas
A plataforma SHALL oferecer contraste elevado e redimensionamento de fonte dentro
de limites seguros, persistindo a escolha no mesmo navegador sem impedir a
reinicialização para o padrão.

##### Scenario: Preferência persistida
- **WHEN** o usuário ajusta contraste ou fonte e recarrega a aplicação
- **THEN** a preferência válida é reaplicada antes ou durante a renderização sem tornar o conteúdo inacessível

##### Scenario: Valor persistido inválido
- **WHEN** o navegador contém preferência fora dos limites aceitos
- **THEN** a aplicação usa o padrão seguro e permite novo ajuste

#### Requirement: Estrutura e contraste possuem critérios verificáveis
As páginas existentes SHALL usar títulos, regiões, tabelas e rótulos semânticos e
SHALL atingir, no mínimo, os critérios WCAG 2.2 nível AA aplicáveis a contraste,
teclado, foco e identificação de erros.

##### Scenario: Verificação automatizada
- **WHEN** os fluxos existentes são submetidos à verificação automatizada de acessibilidade
- **THEN** nenhuma violação de impacto crítico ou sério permanece sem exceção documentada e aprovada

##### Scenario: Verificação manual de teclado
- **WHEN** login e um CRUD representativo são percorridos manualmente sem mouse
- **THEN** todas as ações essenciais podem ser concluídas e o resultado é registrado como evidência de aceite
### Capacidade `automated-quality-gates`

_Contrato de capacidade migrado sem alteração normativa._

## automated-quality-gates Specification

### Purpose
Cria proteção automatizada contra regressões na API e nos dois front-ends,
transformando build, contratos críticos, segurança e migrações em critérios
obrigatórios de entrega.
### Requirements
#### Requirement: Backend possui testes automatizados representativos
A API SHALL ter testes unitários e de integração para autenticação, autorização,
validação, contratos CRUD, produto, concorrência, auditoria e respostas de erro.

##### Scenario: Testes com PostgreSQL compatível
- **WHEN** a suíte de integração é executada no CI
- **THEN** usa uma instância isolada e compatível com produção, aplica migrações desde banco vazio e descarta os dados ao final

##### Scenario: Regressão de autorização
- **WHEN** uma alteração permite que operador modifique cadastro administrativo
- **THEN** um teste falha e bloqueia a integração

#### Requirement: Front-ends possuem testes de comportamento
Cada front-end SHALL testar componentes compartilhados e fluxos críticos, incluindo
login, proteção de rotas, tratamento de erro, CRUD representativo e acessibilidade.

##### Scenario: Falha de API no CRUD
- **WHEN** a API retorna validação, não encontrado, conflito ou indisponibilidade
- **THEN** o teste confirma que a interface apresenta mensagem apropriada e preserva o estado necessário do usuário

##### Scenario: Sessão expirada
- **WHEN** uma requisição protegida retorna HTTP 401
- **THEN** o teste confirma que a sessão local é encerrada e a autenticação é solicitada sem loop de navegação

#### Requirement: CI bloqueia mudanças não verificadas
Pull requests e alterações na branch protegida SHALL executar os gates aplicáveis
de build, lint, testes, migração, análise de dependências, segredos e análise
estática antes de serem consideradas aptas à entrega.

##### Scenario: Módulo alterado
- **WHEN** uma mudança afeta backend ou um dos front-ends
- **THEN** o CI executa build e testes desse módulo e o check agregado falha se qualquer gate falhar

##### Scenario: Apenas documentação alterada
- **WHEN** uma mudança não afeta módulos executáveis
- **THEN** os jobs caros podem ser ignorados, mas o check agregado e as verificações de higiene permanecem conclusivos

#### Requirement: Cobertura é publicada e caminhos críticos não ficam sem teste
O CI SHALL publicar métricas de cobertura por componente e SHALL exigir testes
explícitos para todos os cenários críticos listados nesta mudança, sem aceitar
percentual global como substituto desses cenários.

##### Scenario: Novo caminho crítico sem teste
- **WHEN** uma mudança altera autenticação, autorização, migração ou contrato de cadastro sem teste correspondente
- **THEN** a revisão ou o gate de qualidade identifica a lacuna antes da entrega

#### Requirement: Dados de teste não contêm dados pessoais reais
Fixtures, seeds e evidências de teste SHALL usar dados sintéticos e SHALL NOT
conter dados de residentes, familiares, trabalhadores ou doadores reais.

##### Scenario: Verificação de fixture
- **WHEN** dados de teste são adicionados ao repositório
- **THEN** a revisão e as verificações automatizadas confirmam sua natureza sintética e ausência de segredos

#### Requirement: Migrações são verificadas em instalação e atualização
Cada migração SHALL ser testada tanto em banco vazio quanto sobre a versão
imediatamente anterior suportada, com falha segura e instrução de recuperação.

##### Scenario: Instalação limpa
- **WHEN** a versão é instalada em banco vazio
- **THEN** todas as migrações são aplicadas e a API alcança prontidão

##### Scenario: Atualização de versão
- **WHEN** um banco da versão anterior é atualizado
- **THEN** os dados de catálogo permanecem íntegros e as novas restrições são satisfeitas ou a atualização falha antes de publicar a aplicação

#### Requirement: Revisão semântica é versionada e independente de provedor
O repositório SHALL manter critérios de revisão semântica versionados e
independentes de um modelo específico. Esses critérios SHALL cobrir aderência ao
Spec Kit, autorização, proteção de dados pessoais, segurança e qualidade dos
achados. Os checks determinísticos obrigatórios SHALL permanecer separados da
revisão semântica e SHALL NOT depender de credencial de assinatura de modelo no
GitHub Actions.

##### Scenario: Revisor autorizado analisa um pull request
- **WHEN** um mecanismo autorizado realiza revisão semântica do pull request
- **THEN** ele encontra no repositório as regras de entrega, Spec Kit,
  autorização, LGPD, segurança e qualidade que devem orientar o parecer

##### Scenario: Pull request executa checks obrigatórios
- **WHEN** um pull request é aberto contra `main` ou `dev`
- **THEN** build, testes, SAST, SCA e detecção de segredos continuam conclusivos
  sem depender da execução de um modelo de linguagem

##### Scenario: Credencial do provedor removido
- **WHEN** não existe mais workflow versionado que use a integração da Anthropic
- **THEN** o repositório não referencia nem mantém o secret de assinatura desse
  provedor no GitHub Actions

## Success Criteria

A feature é aceita quando todos os cenários normativos acima são satisfeitos, as
tarefas marcadas como concluídas possuem implementação e evidência correspondentes
e os gates definidos na constituição do projeto passam. Tarefas ainda abertas em
`tasks.md` permanecem bloqueantes para a conclusão operacional da feature.

## Assumptions

- O escopo canônico e as decisões em aberto permanecem em
  [`docs/escopo-do-projeto.md`](../../docs/escopo-do-projeto.md).
- A API continua sendo a autoridade final de autorização.
- Nenhuma migração de formato altera o estado real das tarefas ou declara
  funcionalidades adicionais como entregues.
