## Purpose

Substitui o login meramente visual por uma capacidade institucional de identidade,
autenticação, autorização e auditoria, compartilhada pelos módulos do SeniorCare e
preparada para a futura proteção contextual de informações assistenciais.

## ADDED Requirements

### Requirement: Identidade é individual e vinculada à instituição
A plataforma SHALL manter uma conta individual para cada pessoa usuária e SHALL
vincular toda identidade local a uma instituição. Contas compartilhadas e senhas
fixas no código, banco de demonstração ou repositório SHALL NOT ser permitidas.
Enquanto a instalação possuir uma única instituição habilitada, a interface MAY
ocultar a seleção de instituição sem remover esse limite da autorização.

#### Scenario: Credenciais válidas na única instituição
- **WHEN** uma pessoa com conta ativa apresenta credenciais válidas e existe uma única instituição habilitada
- **THEN** a plataforma autentica a identidade nesse contexto institucional sem exigir uma seleção redundante

#### Scenario: Identidade não pertence à instituição
- **WHEN** uma identidade tenta iniciar ou usar sessão em instituição à qual não está vinculada
- **THEN** a plataforma nega o acesso sem revelar dados de outra instituição e registra a decisão

#### Scenario: Conta compartilhada
- **WHEN** um administrador tenta cadastrar uma conta operacional sem identidade individual atribuível
- **THEN** a plataforma rejeita o cadastro e orienta a criação de uma conta individual

### Requirement: Conta possui ciclo de vida controlado
Cada conta SHALL possuir um dos estados `PROVISIONED`, `ACTIVE`, `INACTIVE`,
`BLOCKED` ou `EXPIRED`. Somente contas `ACTIVE` SHALL iniciar sessões; bloqueio,
inativação ou expiração SHALL impedir novas autenticações e revogar ou invalidar
as sessões existentes conforme a política institucional.

#### Scenario: Conta provisionada
- **WHEN** uma pessoa recebe convite válido para uma conta `PROVISIONED`
- **THEN** ela define a própria senha, conclui os fatores exigidos e a conta passa a `ACTIVE`

#### Scenario: Conta inativa, bloqueada ou expirada
- **WHEN** uma conta em estado diferente de `ACTIVE` tenta autenticar-se
- **THEN** a plataforma nega a sessão com mensagem genérica e registra o estado determinante sem expô-lo ao cliente anônimo

#### Scenario: Administrador inativa conta com sessão aberta
- **WHEN** um administrador autorizado inativa uma conta
- **THEN** as sessões dessa conta deixam de autorizar novas requisições e a alteração fica auditada

### Requirement: Origem da identidade é extensível
A plataforma SHALL registrar a origem de cada identidade como `LOCAL`, `LDAP` ou
`OIDC`. Esta mudança SHALL implementar autenticação `LOCAL`; as demais origens
SHALL permanecer pontos de extensão e SHALL NOT simular integração inexistente.

#### Scenario: Identidade local
- **WHEN** uma conta de origem `LOCAL` autentica-se
- **THEN** a credencial é validada pelo provedor local conforme a política de senha vigente

#### Scenario: Origem ainda não habilitada
- **WHEN** um administrador tenta habilitar `LDAP` ou `OIDC` sem provedor configurado
- **THEN** a plataforma recusa a ativação e informa que a integração não está disponível

### Requirement: Política de senha segue práticas atuais e possui piso seguro
Para contas locais, a plataforma SHALL exigir no mínimo 15 caracteres quando a
senha for o único fator, ou no mínimo 8 caracteres quando MFA for obrigatório para
a conta. A plataforma SHALL aceitar senhas com pelo menos 64 caracteres, espaços e
caracteres Unicode; SHALL comparar novas senhas com lista de valores comuns ou
comprometidos; SHALL NOT impor composição arbitrária por classes de caracteres; e
SHALL NOT exigir troca periódica sem evidência de comprometimento. Configuração
institucional MAY fortalecer, mas SHALL NOT enfraquecer, esses limites.

#### Scenario: Senha longa e válida
- **WHEN** uma pessoa define uma senha dentro do tamanho aceito que não consta na lista de bloqueio
- **THEN** a plataforma aceita espaços e caracteres Unicode sem truncamento silencioso

#### Scenario: Senha comum ou comprometida
- **WHEN** uma pessoa tenta definir uma senha presente na lista de bloqueio
- **THEN** a plataforma rejeita a senha e fornece orientação para escolher outra sem revelar dados sensíveis da verificação

#### Scenario: Regra institucional enfraquece o piso
- **WHEN** um administrador tenta configurar um mínimo inferior ao aplicável ou limitar senhas a menos de 64 caracteres
- **THEN** a plataforma rejeita a configuração e preserva o piso seguro

#### Scenario: Senha antiga sem indício de comprometimento
- **WHEN** uma senha atinge uma idade arbitrária sem evento de risco ou regra legal específica aplicável
- **THEN** a plataforma não força sua troca apenas pelo tempo decorrido

### Requirement: Senhas são derivadas e nunca recuperáveis
Senhas locais SHALL ser armazenadas somente por derivação criptográfica adaptativa,
com parâmetros atualizáveis e salt individual. Senhas, códigos MFA, tokens de
ativação, recuperação e sessão SHALL NOT aparecer em logs, respostas
administrativas ou exportações. Uma autenticação válida MAY atualizar
transparentemente uma derivação obsoleta.

#### Scenario: Persistência de nova senha
- **WHEN** uma senha local é criada ou alterada
- **THEN** somente sua derivação protegida e os metadados necessários são persistidos

#### Scenario: Administrador consulta usuário
- **WHEN** um administrador autorizado consulta ou edita uma conta
- **THEN** nenhuma senha, derivação, segredo MFA ou token é retornado

### Requirement: Ativação e recuperação não distribuem senha conhecida
Administradores SHALL criar a conta e disparar ativação ou recuperação, mas SHALL
NOT visualizar nem definir uma senha permanente conhecida por eles. Ativação e
recuperação SHALL usar token aleatório, armazenado de forma não recuperável, de uso
único e com validade curta. As respostas públicas SHALL ser uniformes para impedir
enumeração de contas.

#### Scenario: Ativação inicial
- **WHEN** uma conta `PROVISIONED` recebe e utiliza um token de ativação válido
- **THEN** a pessoa define a própria senha e o token é invalidado após o uso

#### Scenario: Solicitação de recuperação
- **WHEN** alguém solicita recuperação para um identificador existente ou inexistente
- **THEN** a plataforma retorna a mesma resposta pública e somente envia instruções quando houver conta elegível

#### Scenario: Token expirado ou reutilizado
- **WHEN** um token de ativação ou recuperação expirado ou já utilizado é apresentado
- **THEN** a plataforma rejeita a operação sem alterar credenciais

#### Scenario: Senha redefinida
- **WHEN** uma recuperação válida conclui a definição de nova senha
- **THEN** as sessões anteriores da conta são revogadas e o evento é auditado

### Requirement: Mudança autenticada de senha exige confirmação de identidade
Uma pessoa autenticada SHALL informar a senha atual ou concluir reautenticação
recente antes de alterar a senha. Após a mudança, a plataforma SHALL revogar as
demais sessões e MAY preservar somente a sessão atual quando a política permitir.

#### Scenario: Senha atual incorreta
- **WHEN** uma pessoa tenta alterar a senha sem confirmar a senha atual ou reautenticação aceita
- **THEN** a plataforma nega a alteração e mantém as sessões e credencial existentes

#### Scenario: Mudança concluída
- **WHEN** a pessoa confirma a identidade e define uma nova senha válida
- **THEN** a credencial é atualizada, as sessões determinadas pela política são revogadas e o evento é auditado

### Requirement: Autenticação multifator protege contas privilegiadas
MFA SHALL ser obrigatório para administradores e contas com privilégios de
configuração de acesso, e SHALL ser configurável para os demais usuários. A
primeira entrega SHALL suportar TOTP e códigos de recuperação de uso único,
armazenados de forma protegida. Uma conta sujeita a MFA SHALL NOT concluir a sessão
antes de validar o segundo fator.

#### Scenario: Administrador sem MFA cadastrado
- **WHEN** um administrador com credenciais primárias válidas ainda não cadastrou MFA
- **THEN** a plataforma restringe a sessão ao fluxo de cadastro e confirmação do segundo fator

#### Scenario: Segundo fator inválido
- **WHEN** uma conta sujeita a MFA apresenta código inválido ou reutilizado
- **THEN** a plataforma nega a conclusão da sessão e registra a falha sem registrar o código

#### Scenario: Código de recuperação
- **WHEN** a pessoa utiliza um código de recuperação válido
- **THEN** a plataforma conclui a verificação, invalida somente esse código e alerta para a quantidade restante

### Requirement: Sessão é compartilhada, curta, rotativa e revogável
Uma autenticação SHALL produzir uma sessão institucional válida para os módulos
assistencial e de estoque, sem novo login entre eles. O acesso SHALL usar credencial
de curta duração mantida em memória e renovação protegida por cookie `HttpOnly`,
`Secure` e política `SameSite` adequada. Credenciais de autenticação SHALL NOT ser
persistidas em `localStorage` ou `sessionStorage`. Tokens de renovação SHALL ser
rotacionados, detectados quando reutilizados e revogáveis individualmente ou por
conta.

#### Scenario: Navegação entre módulos
- **WHEN** uma pessoa com sessão válida e permissões efetivas abre outro módulo do SeniorCare
- **THEN** o módulo reutiliza a sessão e solicita novo login apenas se ela não puder ser renovada

#### Scenario: Sessão expirada
- **WHEN** a credencial de acesso expira e a renovação não é válida
- **THEN** a API retorna HTTP 401 e a interface solicita nova autenticação

#### Scenario: Rotação de renovação
- **WHEN** uma renovação válida é utilizada
- **THEN** a plataforma invalida o token anterior e emite uma nova credencial de renovação para a mesma sessão

#### Scenario: Reutilização de token rotacionado
- **WHEN** um token de renovação já rotacionado é reapresentado
- **THEN** a plataforma revoga a família de sessão afetada e registra o evento sem registrar o token

#### Scenario: Logout
- **WHEN** a pessoa encerra a sessão
- **THEN** a sessão é revogada, o cookie protegido é removido e novas requisições são recusadas

### Requirement: Tentativas de autenticação são protegidas contra abuso
O serviço SHALL combinar limitação por conta e origem, atraso progressivo ou
bloqueio temporário configurável e resposta uniforme. O bloqueio SHALL evitar
negação permanente provocada por terceiros, e seus limites SHALL possuir valores
seguros mesmo quando nenhuma configuração institucional for informada.

#### Scenario: Repetição de falhas
- **WHEN** uma origem ou conta excede o limite de falhas dentro da janela configurada
- **THEN** novas tentativas são temporariamente limitadas sem permitir enumeração de usuários

#### Scenario: Autenticação posterior ao bloqueio temporário
- **WHEN** o intervalo de bloqueio termina e credenciais válidas são apresentadas
- **THEN** a autenticação pode prosseguir e os contadores são atualizados conforme a política

### Requirement: Profissão, papel técnico e responsabilidade organizacional são distintos
A plataforma SHALL representar separadamente: cargo ou profissão da pessoa; papel
técnico que agrega permissões; e responsabilidade organizacional exercida em uma
instituição, unidade ou setor por período de validade. Cargo ou profissão SHALL NOT
conceder permissão técnica implicitamente. Uma responsabilidade organizacional MAY
conceder somente as capacidades explicitamente configuradas para ela.

#### Scenario: Profissional sem papel técnico
- **WHEN** uma pessoa possui profissão cadastrada, mas nenhum papel ou vínculo com capacidade de acesso
- **THEN** ela não recebe permissões técnicas por causa da profissão

#### Scenario: Responsabilidade vencida
- **WHEN** o período de validade de uma atribuição organizacional termina
- **THEN** as capacidades derivadas dessa atribuição deixam de compor o acesso efetivo

#### Scenario: Responsabilidade limitada ao setor
- **WHEN** uma atribuição organizacional é válida somente para determinado setor
- **THEN** as capacidades dela não autorizam ação equivalente fora desse escopo

### Requirement: Permissões são compostas por recurso, ação e funcionalidade
Cada permissão SHALL identificar recurso, ação e, quando aplicável,
funcionalidade. Permissões SHALL poder ser agrupadas por módulo, e papéis técnicos
SHALL ser compostos por um ou mais grupos. Alterar a composição SHALL afetar novas
decisões sem exigir alteração do código cliente.

#### Scenario: Papel recebe grupo de módulo
- **WHEN** um administrador autorizado associa um grupo de permissões a um papel
- **THEN** usuários com esse papel passam a receber as permissões do grupo dentro dos escopos válidos

#### Scenario: Permissão removida do grupo
- **WHEN** uma permissão é removida de um grupo
- **THEN** ela deixa de ser concedida pelo grupo nas decisões subsequentes e a alteração é auditada

### Requirement: Exceções individuais são explícitas, limitadas e justificadas
Um administrador autorizado MAY criar exceção individual `ALLOW` ou `DENY` para
recurso, ação, funcionalidade e escopo determinados. Toda exceção SHALL possuir
justificativa, autoria, início e término de validade; exceções permanentes SHALL
exigir justificativa destacada. `DENY` individual válido SHALL prevalecer sobre
concessões individuais, condicionais ou baseadas em papel.

#### Scenario: Exceção temporária de concessão
- **WHEN** uma exceção `ALLOW` válida corresponde à ação solicitada e nenhuma negação prioritária se aplica
- **THEN** a ação é autorizada até o fim da validade definida

#### Scenario: Exceção de negação
- **WHEN** uma exceção `DENY` válida corresponde à ação solicitada
- **THEN** a ação é negada mesmo que um papel conceda a permissão

#### Scenario: Exceção expirada
- **WHEN** a validade de uma exceção termina
- **THEN** ela deixa de influenciar decisões sem precisar ser excluída do histórico

### Requirement: Decisão de acesso segue precedência determinística e negação padrão
O backend SHALL ser a autoridade final de acesso e SHALL avaliar, nesta ordem:
estado e contexto institucional; bypass restrito de `SYSTEM_ADMIN`; `DENY`
individual; política condicional de negação; `ALLOW` individual; política
condicional de concessão; RBAC por papéis, grupos e permissões; e, na ausência de
concessão, `DENY` padrão. `SYSTEM_ADMIN` SHALL ser reservado a operações do sistema,
não SHALL ser atribuível a usuários operacionais da ILPI e todo uso SHALL ser
auditado.

#### Scenario: Interface exibe ação não autorizada
- **WHEN** um cliente manipulado solicita uma ação ausente das permissões efetivas
- **THEN** o backend retorna HTTP 403 e nenhum dado é alterado

#### Scenario: Regras conflitantes
- **WHEN** mais de uma camada produz resultados conflitantes
- **THEN** a decisão segue a precedência definida e registra a camada determinante

#### Scenario: Nenhuma regra concede acesso
- **WHEN** nenhuma concessão válida corresponde ao recurso, ação, funcionalidade e escopo
- **THEN** o backend nega o acesso por padrão

#### Scenario: Operação sistêmica privilegiada
- **WHEN** uma identidade técnica `SYSTEM_ADMIN` executa operação sistêmica autorizada
- **THEN** o bypass é limitado à operação prevista e gera registro de auditoria destacado

### Requirement: Cliente obtém contexto e permissões efetivas sem decidir autorização
A plataforma SHALL fornecer endpoint autenticado da identidade atual contendo
instituição, módulos, papéis, responsabilidades válidas e permissões efetivas
necessárias à interface. O front-end SHALL usar esses dados para navegação e
visibilidade, mas SHALL NOT substituir a validação de cada requisição pelo backend.
Detalhes internos de políticas, segredos e regras não aplicáveis SHALL NOT ser
expostos ao usuário comum.

#### Scenario: Carregamento da aplicação
- **WHEN** uma sessão válida carrega um módulo
- **THEN** o cliente obtém o contexto atual e oculta ou desabilita funções sem permissão efetiva

#### Scenario: Permissão alterada durante a sessão
- **WHEN** uma configuração de acesso muda para a identidade autenticada
- **THEN** decisões posteriores usam a nova configuração e o contexto do cliente é atualizado ou invalidado

### Requirement: Administração de acesso possui configuração dedicada
A plataforma SHALL oferecer APIs e telas protegidas para administrar usuários,
papéis, grupos, permissões, vínculos organizacionais, exceções individuais,
políticas de segurança e sessões ativas. Somente identidades com permissão
específica SHALL alterar essas configurações. Mudanças SHALL ser validadas,
versionadas ou historizadas e auditadas.

#### Scenario: Administrador configura papel
- **WHEN** um administrador de acesso autorizado altera grupos ou permissões de um papel
- **THEN** a plataforma valida referências e escopos, aplica a nova versão e registra antes, depois, autoria e justificativa quando exigida

#### Scenario: Operador consulta configuração protegida
- **WHEN** uma identidade sem permissão administrativa tenta consultar ou alterar configuração de acesso
- **THEN** a API retorna HTTP 403 e não expõe a configuração

#### Scenario: Administrador revoga sessão
- **WHEN** um administrador autorizado revoga uma sessão ativa de uma conta
- **THEN** essa sessão deixa de autorizar requisições sem afetar sessões não selecionadas, salvo decisão explícita de revogar todas

### Requirement: Parâmetros de segurança são configuráveis dentro de limites seguros
Administradores autorizados SHALL poder configurar duração de bloqueio, limites de
tentativas, duração de acesso e renovação, exigência de MFA e fortalecimento da
política de senha. A plataforma SHALL validar limites mínimos e máximos seguros,
SHALL impedir configurações incompatíveis e SHALL auditar toda alteração.

#### Scenario: Configuração válida
- **WHEN** um administrador salva parâmetros dentro dos limites permitidos
- **THEN** a plataforma aplica a configuração à instituição, preserva sua versão anterior no histórico e informa quando ela passa a valer

#### Scenario: Sessão excessivamente longa
- **WHEN** um administrador tenta configurar duração superior ao limite de segurança
- **THEN** a plataforma rejeita a alteração e mantém a configuração anterior

### Requirement: Credencial administrativa inicial é provisionada com segurança
A primeira identidade administrativa SHALL ser criada por procedimento explícito,
idempotente e limitado à instituição inicial. O procedimento SHALL receber dados
sensíveis fora do repositório e SHALL preferir convite de ativação à distribuição
de senha pronta.

#### Scenario: Primeiro provisionamento
- **WHEN** uma instalação vazia recebe parâmetros válidos de instituição e administrador
- **THEN** uma única instituição e conta `PROVISIONED` são criadas e a ativação segura é iniciada

#### Scenario: Reinício posterior
- **WHEN** a instalação já provisionada reinicia com os mesmos parâmetros
- **THEN** nenhuma conta duplicada é criada e nenhuma credencial é redefinida silenciosamente

### Requirement: Eventos de identidade, configuração e acesso são auditáveis
A plataforma SHALL auditar autenticações, falhas relevantes, MFA, ativações,
recuperações, mudanças de credencial, estados de conta, sessões, configuração de
acesso e decisões protegidas. Cada decisão SHALL registrar ator, instituição,
recurso, ação, funcionalidade, escopo-alvo, resultado, camada ou regra determinante,
data, correlação e metadados necessários à investigação, sem credenciais ou dados
secretos.

#### Scenario: Acesso negado
- **WHEN** uma requisição autenticada é negada por autorização
- **THEN** o registro identifica a decisão e sua camada determinante sem registrar token ou senha

#### Scenario: Configuração de acesso alterada
- **WHEN** uma regra, papel, vínculo, exceção ou parâmetro de segurança é alterado
- **THEN** o registro associa autoria, instituição, instante e valores anterior e posterior permitidos para auditoria

#### Scenario: Correlação de evento
- **WHEN** uma investigação consulta uma requisição protegida
- **THEN** os eventos relacionados podem ser correlacionados sem expor segredos da sessão

### Requirement: Autenticação não implica autorização clínica
As permissões desta capacidade SHALL cobrir apenas funções existentes e a
administração da plataforma. Elas SHALL NOT conceder acesso futuro a prontuário,
prescrição, evolução multidisciplinar ou outros dados clínicos por inferência.

#### Scenario: Introdução futura de dado assistencial
- **WHEN** uma capacidade clínica for adicionada
- **THEN** ela exige especificação própria de autorização contextual, consentimento quando aplicável e auditoria antes de reutilizar identidades ou papéis
