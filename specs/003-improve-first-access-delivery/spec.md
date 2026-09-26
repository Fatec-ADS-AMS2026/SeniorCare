# Feature Specification: Melhorar a entrega do primeiro acesso

**Feature ID**: `003-improve-first-access-delivery`
**Created**: 2026-09-26 (data da migração)
**Status**: Implementada; aguardando convergência final
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

`stabilize-existing-platform` entregou identidade, MFA e controle de acesso
funcionais, mas dois pontos de fricção operacional ficaram deliberadamente
sem solução técnica, documentados como gap aceito (`tasks.md` do change
arquivado, tarefas 10.6 e 11.2; `design.md`, risco "Canal de ativação
indisponível em ILPI de baixo orçamento"; `infra/deploy/BOOTSTRAP.md`, seção
"Pendências conhecidas"):

1. **Nenhum token de ativação ou recuperação é entregue automaticamente.**
   A plataforma cria o token e o mostra só pra quem tem acesso ao
   log do processo (bootstrap); nas contas administrativas seguintes, o
   valor bruto não é recuperável do banco — a entrega até a pessoa nova depende de um
   procedimento manual fora do sistema. Isso não escala pra uma ILPI com
   rotatividade de equipe, e o próprio requisito já promovido
   (`platform-authentication`, "Ativação e recuperação não distribuem senha
   conhecida", cenário "Solicitação de recuperação") já assume que a
   plataforma "envia instruções" — hoje ela não envia nada, só o token
   existe internamente.
2. **Cadastro de MFA não tem QR code.** `MfaEnrollPage` mostra só a chave em
   texto (`authenticatorKey`) — todo usuário precisa digitar manualmente
   num app autenticador, mais sujeito a erro de digitação do que escanear.

Esta mudança fecha as duas lacunas com implementação real, substituindo o
procedimento manual documentado por um canal técnico — sem inventar um
sistema de notificação genérico nem preparar terreno pra alertas clínicos
futuros (fora de escopo).

### What Changes

- Adicionar uma capacidade de envio de e-mail transacional (SMTP,
  configurável por variável de ambiente, opcional — implantação sem SMTP
  configurado continua no fluxo manual já documentado, sem regressão).
- Disparar e-mail automaticamente nos três pontos que hoje só geram token
  internamente: ativação de conta nova (bootstrap e criação administrativa
  via `AdminUserOverview`) e recuperação de senha (`POST /auth/recover`).
- Adicionar renderização de QR code (biblioteca só no front-end, sem
  dependência nova no backend — o `otpauth://` já é gerado hoje) na tela de
  cadastro de MFA, mantendo a chave manual como alternativa.
- Atualizar a resposta de criação de usuário administrativo pra indicar se o
  e-mail foi enviado com sucesso (sem nunca incluir o token em si — a spec
  já proíbe isso).
- Permitir que um administrador autorizado reenvie a ativação de uma conta
  `PROVISIONED` depois de corrigir/configurar o SMTP; o reenvio emite um novo
  token, invalida ativações anteriores e retorna somente `emailSent`, nunca o
  token bruto.
- Auditar o envio (sucesso/falha) sem registrar o conteúdo da mensagem nem o
  token.
- Tornar explícito e verificável o contrato da primeira execução tanto com a
  API iniciada pelo Rider e os front-ends pelo WebStorm quanto com toda a stack
  em containers: o bootstrap cria, somente em banco vazio, uma conta
  administrativa `PROVISIONED` sem senha e entrega um token para que a própria
  pessoa defina a credencial. A plataforma não possui senha inicial padrão.
- Remover do helper local qualquer senha administrativa conhecida e versionada;
  a automação de desenvolvimento deverá receber a senha fora do repositório ou
  gerar uma credencial efêmera de alta entropia para aquela execução.

### Capabilities

#### New Capabilities

- `notification-delivery`: envio de e-mail transacional para eventos de
  identidade (ativação, recuperação), configurável, com falha segura
  (não bloqueia o fluxo que originou o envio) e sem dado sensível em log.

#### Modified Capabilities

- `platform-authentication`: os requisitos "Ativação e recuperação não
  distribuem senha conhecida", "Autenticação multifator protege contas
  privilegiadas" e "Credencial administrativa inicial é provisionada com
  segurança" passam a refletir a entrega real (e-mail automático quando
  configurado; QR code no cadastro de MFA; paridade entre IDE e containers;
  ausência de senha inicial padrão) em vez de só descrever o token existindo
  internamente.

### Impact

- **Domínios afetados:** identidade e acesso (ativação, recuperação, MFA);
  operação (configuração de SMTP por ambiente de implantação).
- **Atores afetados:** administradores institucionais (deixam de precisar
  entregar token manualmente), trabalhadores novos/recuperando senha
  (recebem instrução por e-mail em vez de canal informal), equipe de
  operação (nova variável de ambiente opcional a configurar por implantação).
- **Código:** API ASP.NET Core (novo serviço de envio + wiring nos
  controllers de Auth/AdminUser) e os três front-ends com cadastro de MFA
  (Senior Portal, care e stock), todos com QR code equivalente ao `otpAuthUri`.
- **Configuração:** novas variáveis de ambiente opcionais (`Smtp__*`) — ver
  `design.md`; ausência delas preserva o comportamento atual (token só
  interno), não é um requisito obrigatório de deploy.
- **Desenvolvimento local:** Run Configuration do Rider, execução dos
  front-ends pelo WebStorm, Compose e `bootstrap-dev-admin.sh`; o frontend não
  cria a conta, apenas conduz ativação, login e cadastro de MFA contra a API.
- **Risco de regressão:** nenhum fluxo existente deixa de concluir quando SMTP
  não está configurado. O bootstrap conserva seu fallback manual de saída única;
  para contas administrativas posteriores, a interface informa a falha e permite
  reenviar a ativação depois que o canal for corrigido, sem expor token.

## Requirements

### Capacidade `platform-authentication`

_Contrato de capacidade migrado sem alteração normativa._

### Updated Functional Requirements

#### Requirement: Ativação e recuperação não distribuem senha conhecida
Administradores SHALL criar a conta e disparar ativação ou recuperação, mas SHALL
NOT visualizar nem definir uma senha permanente conhecida por eles. Ativação e
recuperação SHALL usar token aleatório, armazenado de forma não recuperável, de uso
único e com validade curta. As respostas públicas SHALL ser uniformes para impedir
enumeração de contas. Quando o ambiente tiver um canal de e-mail configurado, a
plataforma SHALL enviar o token automaticamente para o identificador da conta. O
fallback que apresenta o token bruto SHALL ser restrito à saída única do bootstrap;
contas administrativas posteriores SHALL usar reenvio autenticado após a correção
do canal, sem recuperar ou expor o token anterior.

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

##### Scenario: Envio automático com canal configurado
- **WHEN** um token de ativação ou recuperação é gerado e o ambiente tem canal de
  e-mail configurado
- **THEN** a plataforma envia o token para o identificador da conta automaticamente,
  sem incluir o token em nenhum registro de auditoria ou log

##### Scenario: Falha no envio automático
- **WHEN** o envio do e-mail de ativação ou recuperação falha após o token já ter
  sido criado
- **THEN** a operação que originou o token permanece bem-sucedida, a falha é
  auditada sem o conteúdo da mensagem, e a pessoa que disparou a operação é
  informada de que o envio automático não ocorreu

##### Scenario: Canal de e-mail não configurado
- **WHEN** um token de ativação ou recuperação é gerado e o ambiente não tem
  canal de e-mail configurado
- **THEN** a plataforma cria o token normalmente sem tentar envio; o bootstrap
  apresenta seu token uma única vez, enquanto a criação administrativa posterior
  informa `emailSent: false` e permite reenvio seguro após configurar o canal

##### Scenario: Reenvio administrativo de ativação
- **WHEN** um administrador autorizado solicita novo envio para uma conta local
  `PROVISIONED` da própria instituição
- **THEN** tokens de ativação anteriores são invalidados, um novo token é emitido
  e enviado sem que seu valor apareça na resposta, no log ou na auditoria

#### Requirement: Credencial administrativa inicial é provisionada com segurança
A primeira identidade administrativa SHALL ser criada por procedimento explícito,
idempotente e limitado à instituição inicial, independentemente de a API ser
executada por uma IDE ou em container. O procedimento SHALL receber configuração
fora do repositório, SHALL criar uma conta `PROVISIONED` sem senha e SHALL iniciar
a ativação por token para que a própria pessoa defina sua credencial. A plataforma
e seus auxiliares de desenvolvimento SHALL NOT possuir senha administrativa inicial
conhecida, compartilhada ou versionada. Os front-ends SHALL apenas conduzir a
ativação, o login e o cadastro de MFA; a criação inicial pertence ao bootstrap da
API.

##### Scenario: Primeira execução da API pelo Rider
- **WHEN** a API é iniciada pelo Rider contra banco vazio com os três parâmetros
  de bootstrap válidos definidos na Run Configuration
- **THEN** uma única instituição e uma única conta administrativa `PROVISIONED`
  sem senha são criadas e um token de ativação de uso único é emitido

##### Scenario: Primeira execução em containers
- **WHEN** a stack é iniciada por Docker Compose contra banco vazio com os três
  parâmetros de bootstrap válidos fornecidos fora das imagens
- **THEN** uma única instituição e uma única conta administrativa `PROVISIONED`
  sem senha são criadas e um token de ativação de uso único é emitido

##### Scenario: Ativação pelo frontend em desenvolvimento
- **WHEN** a pessoa abre pelo frontend executado no WebStorm ou em container o
  link de ativação emitido pela API
- **THEN** ela define a própria senha conforme a política vigente e nenhum
  frontend cria, escolhe ou recupera uma senha inicial em seu lugar

##### Scenario: Automação local do primeiro acesso
- **WHEN** um auxiliar de desenvolvimento automatiza ativação, login e cadastro
  de MFA
- **THEN** ele exige uma senha fornecida fora do repositório ou gera uma senha
  efêmera de alta entropia para aquela execução, sem usar valor padrão conhecido

##### Scenario: Configuração de bootstrap parcial
- **WHEN** apenas parte dos três parâmetros obrigatórios de bootstrap é fornecida
- **THEN** a API falha antes do provisionamento, identifica as chaves ausentes sem
  revelar valores e não cria instituição ou conta parcial

##### Scenario: Reinício posterior pela IDE ou por container
- **WHEN** uma instalação já provisionada reinicia com os mesmos parâmetros
- **THEN** nenhuma conta ou instituição duplicada é criada e nenhum token, senha
  ou configuração de MFA é emitido ou redefinido silenciosamente

#### Requirement: Autenticação multifator protege contas privilegiadas
MFA SHALL ser obrigatório para administradores e contas com privilégios de
configuração de acesso, e SHALL ser configurável para os demais usuários. A
primeira entrega SHALL suportar TOTP e códigos de recuperação de uso único,
armazenados de forma protegida. Uma conta sujeita a MFA SHALL NOT concluir a sessão
antes de validar o segundo fator. O cadastro de MFA SHALL apresentar a chave em
texto e SHALL também apresentar um código QR equivalente para reduzir erro de
digitação; a chave em texto SHALL permanecer disponível mesmo quando o QR code é
exibido.

##### Scenario: Administrador sem MFA cadastrado
- **WHEN** um administrador com credenciais primárias válidas ainda não cadastrou MFA
- **THEN** a plataforma restringe a sessão ao fluxo de cadastro e confirmação do segundo fator

##### Scenario: Segundo fator inválido
- **WHEN** uma conta sujeita a MFA apresenta código inválido ou reutilizado
- **THEN** a plataforma nega a conclusão da sessão e registra a falha sem registrar o código

##### Scenario: Código de recuperação
- **WHEN** a pessoa utiliza um código de recuperação válido
- **THEN** a plataforma conclui a verificação, invalida somente esse código e alerta para a quantidade restante

##### Scenario: Cadastro de MFA exibe QR code
- **WHEN** uma pessoa inicia o cadastro do segundo fator
- **THEN** a tela de cadastro apresenta tanto o código QR quanto a chave em texto
  correspondente ao mesmo segredo
### Capacidade `notification-delivery`

_Contrato de capacidade migrado sem alteração normativa._

### Functional Requirements

#### Requirement: Envio de e-mail transacional é opcional e configurável por ambiente
A plataforma SHALL suportar envio de e-mail transacional via SMTP configurado por
variáveis de ambiente para a implantação inteira. Esta entrega SHALL NOT persistir
credenciais SMTP por instituição. A ausência de configuração SHALL ser tratada como estado
válido (canal desabilitado), NOT como erro de inicialização, e SHALL preservar o
comportamento de qualquer fluxo que dependa do canal como se o envio nunca fosse
tentado.

##### Scenario: SMTP configurado
- **WHEN** todas as variáveis de configuração de SMTP obrigatórias estão presentes
- **THEN** a plataforma habilita o envio de e-mail para os eventos suportados

##### Scenario: SMTP ausente
- **WHEN** nenhuma variável de configuração de SMTP está presente
- **THEN** a plataforma inicia normalmente com o canal de e-mail desabilitado, sem
  impedir nenhum fluxo que apenas deixa de enviar a notificação

##### Scenario: SMTP parcialmente configurado
- **WHEN** algumas, mas não todas, as variáveis de configuração de SMTP obrigatórias
  estão presentes
- **THEN** a plataforma falha na inicialização com uma mensagem indicando quais
  variáveis estão faltando, no mesmo padrão usado para outras configurações
  obrigatórias condicionais da plataforma

#### Requirement: Falha de envio não compromete a operação de origem
Uma falha ao enviar um e-mail transacional SHALL NOT reverter nem impedir a
conclusão da operação que originou o envio. A falha SHALL ser auditada sem incluir
o conteúdo da mensagem, token, senha ou segredo de MFA.

##### Scenario: Envio bem-sucedido
- **WHEN** um e-mail transacional é enviado com sucesso
- **THEN** o evento é auditado com o destinatário e o tipo de notificação, sem o
  conteúdo da mensagem

##### Scenario: Envio malsucedido
- **WHEN** a tentativa de envio de um e-mail transacional falha
- **THEN** a operação que originou o envio permanece concluída, a falha é auditada
  sem o conteúdo da mensagem ou detalhe de erro sensível, e a resposta da operação
  de origem indica que o envio automático não ocorreu

#### Requirement: Ativação administrativa pode ser reenviada sem expor token
A plataforma SHALL permitir que um administrador com permissão de escrita em
contas solicite novo envio de ativação para uma conta local `PROVISIONED` da mesma
instituição. O reenvio SHALL invalidar tokens de ativação anteriores, emitir um
novo token de uso único e retornar somente o resultado da entrega (`emailSent`),
nunca o token ou o link bruto.

##### Scenario: Reenvio após corrigir SMTP
- **WHEN** um administrador autorizado solicita o reenvio para uma conta local
  `PROVISIONED` da própria instituição depois de corrigir o canal SMTP
- **THEN** a plataforma invalida ativações anteriores, emite um novo token, tenta
  a entrega e retorna `emailSent: true` sem expor o token

##### Scenario: Falha no reenvio
- **WHEN** o novo token é emitido mas a tentativa de entrega falha
- **THEN** a conta permanece `PROVISIONED`, o resultado retorna `emailSent: false`
  e nenhum token aparece na resposta, no log ou na auditoria

##### Scenario: Conta inelegível para reenvio
- **WHEN** o alvo não pertence à instituição, não é de origem local ou não está
  `PROVISIONED`
- **THEN** a plataforma rejeita a solicitação sem emitir novo token

#### Requirement: Conteúdo sensível nunca é registrado em log ou auditoria
Nenhum log de aplicação ou registro de auditoria relacionado ao envio de e-mail
transacional SHALL conter o corpo da mensagem, o token de ativação/recuperação, a
senha ou qualquer segredo de MFA.

##### Scenario: Log de falha de envio
- **WHEN** uma falha de envio é registrada em log de aplicação
- **THEN** o registro contém apenas informação operacional (tipo de erro genérico,
  destinatário), nunca a exceção bruta do cliente SMTP nem o conteúdo da mensagem

##### Scenario: Bootstrap com entrega automática bem-sucedida
- **WHEN** o token inicial é entregue com sucesso pelo canal SMTP configurado
- **THEN** o token não é duplicado no console, log de aplicação ou auditoria

##### Scenario: Bootstrap manual com API executada pelo Rider
- **WHEN** a primeira execução ocorre pelo Rider e o canal SMTP está desabilitado
  ou a entrega do token inicial falha
- **THEN** a API apresenta o token uma única vez no console da Run Configuration,
  sem registrá-lo na auditoria nem nos logs do serviço de e-mail

##### Scenario: Bootstrap manual com API executada em container
- **WHEN** a primeira execução ocorre por Docker Compose e o canal SMTP está
  desabilitado ou a entrega do token inicial falha
- **THEN** a API apresenta o token uma única vez na saída do container, acessível
  pelo procedimento documentado, sem registrá-lo na auditoria nem nos logs do
  serviço de e-mail

##### Scenario: Reinício após emissão do token inicial
- **WHEN** a API reinicia pela IDE ou por container depois que a instalação já foi
  provisionada
- **THEN** o token inicial não é enviado novamente por e-mail nem reapresentado no
  console ou na saída do container

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
