## ADDED Requirements

### Requirement: Envio de e-mail transacional é opcional e configurável por instituição
A plataforma SHALL suportar envio de e-mail transacional via SMTP configurado por
variáveis de ambiente. A ausência de configuração SHALL ser tratada como estado
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

### Requirement: Conteúdo sensível nunca é registrado em log ou auditoria
Nenhum log de aplicação ou registro de auditoria relacionado ao envio de e-mail
transacional SHALL conter o corpo da mensagem, o token de ativação/recuperação, a
senha ou qualquer segredo de MFA.

#### Scenario: Log de falha de envio
- **WHEN** uma falha de envio é registrada em log de aplicação
- **THEN** o registro contém apenas informação operacional (tipo de erro genérico,
  destinatário), nunca a exceção bruta do cliente SMTP nem o conteúdo da mensagem
