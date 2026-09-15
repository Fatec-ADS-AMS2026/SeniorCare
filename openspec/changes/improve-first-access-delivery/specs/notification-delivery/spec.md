## ADDED Requirements

### Requirement: Envio de e-mail transacional é opcional e configurável por ambiente
A plataforma SHALL suportar envio de e-mail transacional via SMTP configurado por
variáveis de ambiente para a implantação inteira. Esta entrega SHALL NOT persistir
credenciais SMTP por instituição. A ausência de configuração SHALL ser tratada como estado
válido (canal desabilitado), NOT como erro de inicialização, e SHALL preservar o
comportamento de qualquer fluxo que dependa do canal como se o envio nunca fosse
tentado.

#### Scenario: SMTP configurado
- **WHEN** todas as variáveis de configuração de SMTP obrigatórias estão presentes
- **THEN** a plataforma habilita o envio de e-mail para os eventos suportados

#### Scenario: SMTP ausente
- **WHEN** nenhuma variável de configuração de SMTP está presente
- **THEN** a plataforma inicia normalmente com o canal de e-mail desabilitado, sem
  impedir nenhum fluxo que apenas deixa de enviar a notificação

#### Scenario: SMTP parcialmente configurado
- **WHEN** algumas, mas não todas, as variáveis de configuração de SMTP obrigatórias
  estão presentes
- **THEN** a plataforma falha na inicialização com uma mensagem indicando quais
  variáveis estão faltando, no mesmo padrão usado para outras configurações
  obrigatórias condicionais da plataforma

### Requirement: Falha de envio não compromete a operação de origem
Uma falha ao enviar um e-mail transacional SHALL NOT reverter nem impedir a
conclusão da operação que originou o envio. A falha SHALL ser auditada sem incluir
o conteúdo da mensagem, token, senha ou segredo de MFA.

#### Scenario: Envio bem-sucedido
- **WHEN** um e-mail transacional é enviado com sucesso
- **THEN** o evento é auditado com o destinatário e o tipo de notificação, sem o
  conteúdo da mensagem

#### Scenario: Envio malsucedido
- **WHEN** a tentativa de envio de um e-mail transacional falha
- **THEN** a operação que originou o envio permanece concluída, a falha é auditada
  sem o conteúdo da mensagem ou detalhe de erro sensível, e a resposta da operação
  de origem indica que o envio automático não ocorreu

### Requirement: Ativação administrativa pode ser reenviada sem expor token
A plataforma SHALL permitir que um administrador com permissão de escrita em
contas solicite novo envio de ativação para uma conta local `PROVISIONED` da mesma
instituição. O reenvio SHALL invalidar tokens de ativação anteriores, emitir um
novo token de uso único e retornar somente o resultado da entrega (`emailSent`),
nunca o token ou o link bruto.

#### Scenario: Reenvio após corrigir SMTP
- **WHEN** um administrador autorizado solicita o reenvio para uma conta local
  `PROVISIONED` da própria instituição depois de corrigir o canal SMTP
- **THEN** a plataforma invalida ativações anteriores, emite um novo token, tenta
  a entrega e retorna `emailSent: true` sem expor o token

#### Scenario: Falha no reenvio
- **WHEN** o novo token é emitido mas a tentativa de entrega falha
- **THEN** a conta permanece `PROVISIONED`, o resultado retorna `emailSent: false`
  e nenhum token aparece na resposta, no log ou na auditoria

#### Scenario: Conta inelegível para reenvio
- **WHEN** o alvo não pertence à instituição, não é de origem local ou não está
  `PROVISIONED`
- **THEN** a plataforma rejeita a solicitação sem emitir novo token

### Requirement: Conteúdo sensível nunca é registrado em log ou auditoria
Nenhum log de aplicação ou registro de auditoria relacionado ao envio de e-mail
transacional SHALL conter o corpo da mensagem, o token de ativação/recuperação, a
senha ou qualquer segredo de MFA.

#### Scenario: Log de falha de envio
- **WHEN** uma falha de envio é registrada em log de aplicação
- **THEN** o registro contém apenas informação operacional (tipo de erro genérico,
  destinatário), nunca a exceção bruta do cliente SMTP nem o conteúdo da mensagem

#### Scenario: Bootstrap com entrega automática bem-sucedida
- **WHEN** o token inicial é entregue com sucesso pelo canal SMTP configurado
- **THEN** o token não é duplicado no console, log de aplicação ou auditoria

#### Scenario: Bootstrap manual com API executada pelo Rider
- **WHEN** a primeira execução ocorre pelo Rider e o canal SMTP está desabilitado
  ou a entrega do token inicial falha
- **THEN** a API apresenta o token uma única vez no console da Run Configuration,
  sem registrá-lo na auditoria nem nos logs do serviço de e-mail

#### Scenario: Bootstrap manual com API executada em container
- **WHEN** a primeira execução ocorre por Docker Compose e o canal SMTP está
  desabilitado ou a entrega do token inicial falha
- **THEN** a API apresenta o token uma única vez na saída do container, acessível
  pelo procedimento documentado, sem registrá-lo na auditoria nem nos logs do
  serviço de e-mail

#### Scenario: Reinício após emissão do token inicial
- **WHEN** a API reinicia pela IDE ou por container depois que a instalação já foi
  provisionada
- **THEN** o token inicial não é enviado novamente por e-mail nem reapresentado no
  console ou na saída do container
