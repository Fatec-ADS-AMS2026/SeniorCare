## Purpose

Fornece a entrada institucional unificada para que usuários internos descubram e
acessem os módulos autorizados do SeniorCare com uma única sessão, contexto
institucional consistente e navegação acessível, sem concentrar dados ou regras de
negócio dos módulos.

## ADDED Requirements

### Requirement: Senior Portal é a entrada institucional unificada
A plataforma SHALL oferecer o Senior Portal como ponto primário de entrada para
trabalhadores, profissionais e gestores autorizados. O portal SHALL distinguir sua
finalidade interna do futuro portal destinado a residentes, familiares,
responsáveis legais ou curadores.

#### Scenario: Usuário interno abre a plataforma
- **WHEN** uma pessoa acessa a raiz institucional do SeniorCare
- **THEN** a plataforma apresenta autenticação ou a página de módulos conforme o estado da sessão

#### Scenario: Familiar tenta usar o portal interno
- **WHEN** uma identidade sem vínculo interno válido tenta acessar o Senior Portal
- **THEN** a plataforma nega o acesso e não presume permissões por vínculo familiar ou representação legal

### Requirement: Portal reutiliza a identidade e a sessão institucional
O portal SHALL consumir a identidade, o contexto institucional, a autenticação
multifator e a sessão protegida definidas pela capacidade de autenticação da
plataforma. Uma sessão válida SHALL ser reutilizada pelos módulos autorizados sem
novo login, e credenciais de autenticação SHALL NOT ser persistidas em armazenamento
do navegador acessível a scripts.

#### Scenario: Navegação autenticada para outro módulo
- **WHEN** uma pessoa com sessão válida seleciona um módulo autorizado
- **THEN** o módulo reutiliza a mesma sessão institucional sem solicitar novamente as credenciais

#### Scenario: Sessão não renovável
- **WHEN** o portal não consegue validar ou renovar a sessão
- **THEN** ele encerra o contexto local, direciona ao login e não revela módulos ou dados da sessão anterior

#### Scenario: MFA pendente
- **WHEN** a autenticação primária foi aceita, mas a política exige segundo fator ainda não validado
- **THEN** o portal limita a navegação ao fluxo de MFA e não disponibiliza os módulos

### Requirement: Contexto institucional é explícito e consistente
O portal SHALL exibir a instituição ativa e SHALL propagar o mesmo contexto aos
módulos. Quando existir uma única instituição possível, a seleção MAY ser ocultada,
mas o contexto SHALL permanecer presente nas decisões do backend. Troca futura de
instituição SHALL exigir capacidade própria e reavaliação das permissões.

#### Scenario: Única instituição habilitada
- **WHEN** a identidade possui exatamente um contexto institucional válido
- **THEN** o portal apresenta essa instituição sem exigir seleção redundante

#### Scenario: Contexto institucional divergente
- **WHEN** um destino de módulo tenta operar com instituição diferente da sessão
- **THEN** o backend nega a operação, o portal informa que o contexto é inválido e nenhum dado cruzado é exibido

### Requirement: Catálogo de módulos possui contrato estável e configurável
Cada módulo SHALL possuir identificador estável, nome, descrição curta, ícone
aprovado, destino relativo ou autorizado, ordem, estado operacional e permissão
necessária. A configuração SHALL poder ser alterada por administrador autorizado
sem exigir recompilação do portal, respeitando validações e histórico.

#### Scenario: Administrador cadastra módulo válido
- **WHEN** um administrador autorizado salva um módulo com identificador único, destino permitido e permissão existente
- **THEN** a plataforma registra a configuração e sua autoria e passa a considerá-la nas consultas subsequentes

#### Scenario: Destino externo não autorizado
- **WHEN** uma configuração informa URL absoluta ou origem não permitida
- **THEN** a plataforma rejeita a configuração sem criar redirecionamento aberto

#### Scenario: Identificador duplicado
- **WHEN** um administrador tenta cadastrar outro módulo com identificador já existente na mesma instituição
- **THEN** a plataforma rejeita a alteração e preserva a configuração anterior

### Requirement: Descoberta de módulos usa permissões efetivas
O backend SHALL retornar somente módulos habilitados cuja permissão efetiva seja
concedida à identidade no contexto atual. A interface SHALL usar o resultado para
visibilidade e navegação, mas cada módulo e API SHALL revalidar a autorização da
operação solicitada.

#### Scenario: Módulo autorizado
- **WHEN** a identidade possui a permissão exigida por um módulo habilitado
- **THEN** o portal apresenta o módulo com seu estado operacional atual

#### Scenario: Módulo sem permissão
- **WHEN** a identidade não possui a permissão exigida
- **THEN** o módulo não aparece no catálogo efetivo e seu acesso direto continua protegido pelo backend

#### Scenario: Permissão revogada durante a sessão
- **WHEN** a permissão de um módulo é revogada para uma identidade autenticada
- **THEN** consultas posteriores removem o módulo do catálogo e o acesso direto passa a ser negado

### Requirement: Portal representa estados operacionais sem conceder acesso indevido
Um módulo SHALL possuir estado `AVAILABLE`, `MAINTENANCE`, `UNAVAILABLE` ou
`DISABLED`. Somente módulos `AVAILABLE` SHALL iniciar navegação normal. Módulos em
manutenção ou indisponíveis MAY permanecer visíveis para usuários autorizados com
mensagem segura; módulos desabilitados SHALL NOT aparecer no catálogo efetivo.

#### Scenario: Módulo disponível
- **WHEN** um usuário autorizado seleciona módulo `AVAILABLE`
- **THEN** o portal navega para o destino configurado

#### Scenario: Módulo em manutenção
- **WHEN** um usuário autorizado visualiza módulo `MAINTENANCE`
- **THEN** o portal impede a abertura normal e apresenta orientação institucional sem detalhes internos

#### Scenario: Módulo indisponível inesperadamente
- **WHEN** o destino de um módulo `AVAILABLE` não responde
- **THEN** o portal ou o módulo apresenta falha recuperável, opção de retorno e identificador de correlação quando disponível

### Requirement: Navegação entre aplicações preserva destino seguro
Portal e módulos SHALL ser publicados sob a mesma origem e SHALL fornecer retorno
consistente ao portal. Após autenticação ou renovação, um destino interno solicitado
MAY ser restaurado somente se estiver no conjunto permitido e se a identidade
possuir acesso efetivo.

#### Scenario: Retorno ao portal
- **WHEN** uma pessoa seleciona a navegação global dentro de assistência ou estoque
- **THEN** ela retorna ao catálogo de módulos sem encerrar a sessão

#### Scenario: Deep link autorizado
- **WHEN** uma pessoa autenticada abre uma rota interna válida de módulo permitido
- **THEN** a aplicação preserva a rota solicitada após validar sessão e autorização

#### Scenario: Redirecionamento malicioso
- **WHEN** login ou retorno recebe destino externo, desconhecido ou não autorizado
- **THEN** a plataforma ignora o destino e navega para a página segura padrão

### Requirement: URLs antigas migram de forma coordenada
Entradas de login e landing pages legadas dos módulos SHALL redirecionar para o
portal após a migração, preservando somente caminhos internos autorizados. Durante
a transição, acesso direto aos módulos MAY permanecer disponível como contingência,
mas SHALL utilizar a mesma sessão e autorização.

#### Scenario: Login legado do estoque
- **WHEN** uma pessoa abre a antiga URL de login do estoque após a ativação do portal
- **THEN** ela é direcionada ao login do portal com destino de retorno interno validado

#### Scenario: Cliente antigo sem sessão compatível
- **WHEN** um cliente legado tenta reutilizar credencial incompatível ou persistida de forma insegura
- **THEN** a plataforma exige nova autenticação e não converte silenciosamente a credencial

### Requirement: Funções globais permanecem disponíveis em todos os módulos
O portal SHALL oferecer acesso a perfil, segurança da conta, preferências de
acessibilidade e logout. Os módulos SHALL fornecer navegação consistente para essas
funções ou para o portal, sem duplicar regras de credencial e sessão.

#### Scenario: Usuário altera preferência visual
- **WHEN** uma pessoa autenticada altera preferência de contraste ou fonte suportada
- **THEN** a preferência é aplicada no portal e restaurada pelos módulos compatíveis

#### Scenario: Logout iniciado em um módulo
- **WHEN** uma pessoa encerra a sessão a partir de qualquer módulo
- **THEN** a sessão compartilhada é revogada e portal e demais módulos passam a exigir autenticação

### Requirement: Portal minimiza dados e não apresenta conteúdo clínico
O catálogo SHALL usar somente dados mínimos de identidade, instituição, módulos,
permissões e estado operacional. O portal SHALL NOT carregar ou apresentar listas
de residentes, diagnósticos, medicamentos, evoluções, documentos clínicos,
informações financeiras individuais ou indicadores que permitam reidentificação.

#### Scenario: Carregamento da página inicial
- **WHEN** o portal monta o catálogo de módulos
- **THEN** nenhuma consulta de prontuário, residente ou transação financeira é necessária

#### Scenario: Configuração contém informação sensível
- **WHEN** um administrador tenta inserir dado pessoal ou clínico em nome, descrição ou mensagem operacional de módulo
- **THEN** a plataforma rejeita ou sinaliza a configuração conforme validação definida e não a publica

### Requirement: Configuração e navegação crítica são auditáveis
A plataforma SHALL auditar criação e alteração do catálogo, mudanças de estado,
negações de acesso, redirecionamentos rejeitados e eventos de sessão já exigidos
pelo IAM. Os registros SHALL incluir ator, instituição, módulo, ação, resultado,
instante e correlação sem armazenar senha, token ou conteúdo clínico.

#### Scenario: Estado de módulo alterado
- **WHEN** um administrador coloca um módulo em manutenção
- **THEN** a auditoria registra autoria, instituição, estado anterior, novo estado e instante

#### Scenario: Acesso direto negado
- **WHEN** uma identidade tenta abrir diretamente um módulo sem permissão
- **THEN** a decisão negativa é auditada sem revelar ao cliente regras internas ou dados do módulo

### Requirement: Portal possui experiência acessível e responsiva
Os fluxos de login, catálogo, navegação, perfil e falha SHALL ser operáveis por
teclado, possuir nomes e foco perceptíveis, contraste adequado, mensagens não
dependentes apenas de cor e adaptação a celular, tablet e desktop. A ordem visual
dos módulos SHALL corresponder à ordem de navegação assistiva.

#### Scenario: Seleção somente por teclado
- **WHEN** uma pessoa navega pelo catálogo sem dispositivo apontador
- **THEN** todos os módulos disponíveis, funções globais e mensagens podem ser alcançados e acionados em ordem previsível

#### Scenario: Estado de manutenção
- **WHEN** um módulo é exibido como indisponível ou em manutenção
- **THEN** seu estado possui texto acessível e não é comunicado somente por cor ou ícone

### Requirement: Falha do portal admite contingência controlada
Uma falha do portal SHALL NOT invalidar automaticamente sessões válidas nem impedir
o uso de links diretos previamente conhecidos para módulos críticos, desde que o
módulo e o backend possam validar sessão e autorização. A contingência SHALL ser
documentada, monitorada e não SHALL contornar MFA, estado da conta ou permissão.

#### Scenario: Portal indisponível e módulo saudável
- **WHEN** o portal está indisponível, mas um módulo crítico e o IAM estão operacionais
- **THEN** uma pessoa com sessão e permissão válidas pode usar o link direto documentado do módulo

#### Scenario: IAM indisponível
- **WHEN** um módulo não consegue validar sessão ou autorização durante a contingência
- **THEN** ele falha de modo seguro, não concede acesso e apresenta orientação operacional sem dados sensíveis

### Requirement: Módulos futuros não são anunciados antes de estarem prontos
Financeiro, doações, prontuário, nutrição, dashboards e outros módulos futuros
SHALL aparecer no portal somente quando sua capacidade estiver implementada,
validada, habilitada e autorizada. O catálogo SHALL NOT usar cards inativos como
promessa de funcionalidade indisponível no ambiente operacional.

#### Scenario: Módulo futuro apenas planejado
- **WHEN** uma capacidade existe somente no escopo ou roadmap
- **THEN** ela não aparece no catálogo operacional

#### Scenario: Novo módulo entra em produção
- **WHEN** uma capacidade validada é implantada, configurada como `AVAILABLE` e concedida à identidade
- **THEN** o portal passa a exibi-la sem exigir mudança nos contratos dos demais módulos
