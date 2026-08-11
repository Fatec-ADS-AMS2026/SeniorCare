## MODIFIED Requirements

### Requirement: Ativação e recuperação não distribuem senha conhecida
Administradores SHALL criar a conta e disparar ativação ou recuperação, mas SHALL
NOT visualizar nem definir uma senha permanente conhecida por eles. Ativação e
recuperação SHALL usar token aleatório, armazenado de forma não recuperável, de uso
único e com validade curta. As respostas públicas SHALL ser uniformes para impedir
enumeração de contas. Quando a instituição tiver um canal de e-mail configurado, a
plataforma SHALL enviar o token automaticamente para o identificador da conta; a
ausência de canal configurado SHALL NOT impedir a criação do token nem a ativação
por procedimento manual.

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
- **WHEN** um token de ativação ou recuperação é gerado e a instituição tem canal de
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
- **WHEN** um token de ativação ou recuperação é gerado e a instituição não tem
  canal de e-mail configurado
- **THEN** a plataforma cria o token normalmente e disponibiliza-o apenas por meio
  do procedimento manual documentado, sem tentar nenhum envio

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
