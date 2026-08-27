## MODIFIED Requirements

### Requirement: Ativação e recuperação não distribuem senha conhecida
Administradores SHALL criar a conta e disparar ativação ou recuperação, mas SHALL
NOT visualizar nem definir uma senha permanente conhecida por eles. Ativação e
recuperação SHALL usar token aleatório, armazenado de forma não recuperável, de uso
único e com validade curta. As respostas públicas SHALL ser uniformes para impedir
enumeração de contas. Quando o ambiente tiver um canal de e-mail configurado, a
plataforma SHALL enviar o token automaticamente para o identificador da conta. O
fallback que apresenta o token bruto SHALL ser restrito à saída única do bootstrap;
contas administrativas posteriores SHALL usar reenvio autenticado após a correção
do canal, sem recuperar ou expor o token anterior.

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

#### Scenario: Envio automático com canal configurado
- **WHEN** um token de ativação ou recuperação é gerado e o ambiente tem canal de
  e-mail configurado
- **THEN** a plataforma envia o token para o identificador da conta automaticamente,
  sem incluir o token em nenhum registro de auditoria ou log

#### Scenario: Falha no envio automático
- **WHEN** o envio do e-mail de ativação ou recuperação falha após o token já ter
  sido criado
- **THEN** a operação que originou o token permanece bem-sucedida, a falha é
  auditada sem o conteúdo da mensagem, e a pessoa que disparou a operação é
  informada de que o envio automático não ocorreu

#### Scenario: Canal de e-mail não configurado
- **WHEN** um token de ativação ou recuperação é gerado e o ambiente não tem
  canal de e-mail configurado
- **THEN** a plataforma cria o token normalmente sem tentar envio; o bootstrap
  apresenta seu token uma única vez, enquanto a criação administrativa posterior
  informa `emailSent: false` e permite reenvio seguro após configurar o canal

#### Scenario: Reenvio administrativo de ativação
- **WHEN** um administrador autorizado solicita novo envio para uma conta local
  `PROVISIONED` da própria instituição
- **THEN** tokens de ativação anteriores são invalidados, um novo token é emitido
  e enviado sem que seu valor apareça na resposta, no log ou na auditoria

### Requirement: Credencial administrativa inicial é provisionada com segurança
A primeira identidade administrativa SHALL ser criada por procedimento explícito,
idempotente e limitado à instituição inicial, independentemente de a API ser
executada por uma IDE ou em container. O procedimento SHALL receber configuração
fora do repositório, SHALL criar uma conta `PROVISIONED` sem senha e SHALL iniciar
a ativação por token para que a própria pessoa defina sua credencial. A plataforma
e seus auxiliares de desenvolvimento SHALL NOT possuir senha administrativa inicial
conhecida, compartilhada ou versionada. Os front-ends SHALL apenas conduzir a
ativação, o login e o cadastro de MFA; a criação inicial pertence ao bootstrap da
API.

#### Scenario: Primeira execução da API pelo Rider
- **WHEN** a API é iniciada pelo Rider contra banco vazio com os três parâmetros
  de bootstrap válidos definidos na Run Configuration
- **THEN** uma única instituição e uma única conta administrativa `PROVISIONED`
  sem senha são criadas e um token de ativação de uso único é emitido

#### Scenario: Primeira execução em containers
- **WHEN** a stack é iniciada por Docker Compose contra banco vazio com os três
  parâmetros de bootstrap válidos fornecidos fora das imagens
- **THEN** uma única instituição e uma única conta administrativa `PROVISIONED`
  sem senha são criadas e um token de ativação de uso único é emitido

#### Scenario: Ativação pelo frontend em desenvolvimento
- **WHEN** a pessoa abre pelo frontend executado no WebStorm ou em container o
  link de ativação emitido pela API
- **THEN** ela define a própria senha conforme a política vigente e nenhum
  frontend cria, escolhe ou recupera uma senha inicial em seu lugar

#### Scenario: Automação local do primeiro acesso
- **WHEN** um auxiliar de desenvolvimento automatiza ativação, login e cadastro
  de MFA
- **THEN** ele exige uma senha fornecida fora do repositório ou gera uma senha
  efêmera de alta entropia para aquela execução, sem usar valor padrão conhecido

#### Scenario: Configuração de bootstrap parcial
- **WHEN** apenas parte dos três parâmetros obrigatórios de bootstrap é fornecida
- **THEN** a API falha antes do provisionamento, identifica as chaves ausentes sem
  revelar valores e não cria instituição ou conta parcial

#### Scenario: Reinício posterior pela IDE ou por container
- **WHEN** uma instalação já provisionada reinicia com os mesmos parâmetros
- **THEN** nenhuma conta ou instituição duplicada é criada e nenhum token, senha
  ou configuração de MFA é emitido ou redefinido silenciosamente

### Requirement: Autenticação multifator protege contas privilegiadas
MFA SHALL ser obrigatório para administradores e contas com privilégios de
configuração de acesso, e SHALL ser configurável para os demais usuários. A
primeira entrega SHALL suportar TOTP e códigos de recuperação de uso único,
armazenados de forma protegida. Uma conta sujeita a MFA SHALL NOT concluir a sessão
antes de validar o segundo fator. O cadastro de MFA SHALL apresentar a chave em
texto e SHALL também apresentar um código QR equivalente para reduzir erro de
digitação; a chave em texto SHALL permanecer disponível mesmo quando o QR code é
exibido.

#### Scenario: Administrador sem MFA cadastrado
- **WHEN** um administrador com credenciais primárias válidas ainda não cadastrou MFA
- **THEN** a plataforma restringe a sessão ao fluxo de cadastro e confirmação do segundo fator

#### Scenario: Segundo fator inválido
- **WHEN** uma conta sujeita a MFA apresenta código inválido ou reutilizado
- **THEN** a plataforma nega a conclusão da sessão e registra a falha sem registrar o código

#### Scenario: Código de recuperação
- **WHEN** a pessoa utiliza um código de recuperação válido
- **THEN** a plataforma conclui a verificação, invalida somente esse código e alerta para a quantidade restante

#### Scenario: Cadastro de MFA exibe QR code
- **WHEN** uma pessoa inicia o cadastro do segundo fator
- **THEN** a tela de cadastro apresenta tanto o código QR quanto a chave em texto
  correspondente ao mesmo segredo
