## 1. Capacidade de envio de e-mail (`notification-delivery`)

- [ ] 1.1 Adicionar dependência MailKit ao projeto `WebAPI`.
- [ ] 1.2 Criar `INotificationSender` (abstração) e `SmtpNotificationSender`
      (implementação), lendo `Smtp__Host`/`Smtp__Port`/`Smtp__Username`/
      `Smtp__Password`/`Smtp__FromAddress`/`Smtp__FromDisplayName`/
      `Smtp__UseStartTls`.
- [ ] 1.3 Registrar como no-op consciente (loga `Information`, não falha) quando
      nenhuma variável `Smtp__*` está presente; falhar no startup (mesmo padrão de
      `Program.GetMissingConfiguration`) quando configuração está parcial.
- [ ] 1.4 Capturar exceção do cliente SMTP e logar só mensagem genérica — nunca a
      exceção bruta, nunca o corpo da mensagem.
- [ ] 1.5 Testes de unidade: SMTP ausente (no-op), SMTP parcial (falha de startup),
      SMTP configurado + envio simulado (mock do cliente), falha de envio simulada
      (não propaga exceção pro chamador).

## 2. Wiring nos pontos de disparo

- [ ] 2.1 Bootstrap (`Program.cs`): após criar o token de ativação inicial, chamar
      `INotificationSender` se configurado; exibir o token no canal manual somente
      quando SMTP estiver ausente ou a entrega falhar. Uma entrega automática
      bem-sucedida não pode duplicar o token no console/log.
- [ ] 2.2 Criação de usuário administrativo (`AdminUserOverview`/controller
      correspondente): disparar e-mail de ativação; resposta do endpoint passa a
      incluir `emailSent: boolean`.
- [ ] 2.3 `POST /Auth/recover`: disparar e-mail de recuperação apenas quando houver
      conta elegível — sem alterar a resposta pública uniforme já exigida pela spec.
- [ ] 2.4 Auditoria: novo evento (envio bem-sucedido / falha) nos 3 pontos, sem
      token/senha/segredo MFA/corpo da mensagem.
- [ ] 2.5 Testes de integração dos 3 pontos com sender mockado — sucesso e falha de
      envio, confirmando que a operação de origem sempre completa.

## 3. QR code no cadastro de MFA (front-end)

- [ ] 3.1 Adicionar biblioteca de geração de QR code (client-side, sem dependência
      nova no backend) aos três front-ends (Senior Portal, care e stock).
- [ ] 3.2 Renderizar o QR code em `MfaEnrollPage` a partir do `otpAuthUri` já
      retornado por `POST /Auth/mfa/enroll` nos três front-ends, mantendo a chave
      em texto visível.
- [ ] 3.3 Testes de componente (Vitest) cobrindo a renderização do QR code junto da
      chave em texto.

## 4. Configuração e documentação

- [ ] 4.1 Documentar `Smtp__*` e `Frontend__ActivationBaseUrl` em
      `SeniorCareManager-Backend/SeniorCareManager.WebAPI/CONFIGURATION.md`.
- [ ] 4.2 Atualizar `infra/deploy/BOOTSTRAP.md` — remover os itens de e-mail e QR
      code da tabela "Pendências conhecidas" (ou marcar como resolvidos), ajustar as
      seções 2/4/5 pra refletir o envio automático quando configurado.
- [ ] 4.3 Atualizar `docs/tutorial-desenvolvimento-ides.md` e `docs/tutorial-docker.md`
      — remover os blockquotes de pendência conhecida ou ajustá-los pra citar que o
      envio automático é opcional (depende de `Smtp__*` estar configurado no
      ambiente).
- [ ] 4.4 Confirmar que `infra/docker-test/bootstrap-dev-admin.sh` continua
      funcionando sem alteração (ele já lê o token do log/console, que continua
      sendo impresso independentemente do e-mail estar configurado).

## 5. Aceite

- [ ] 5.1 `dotnet test` no backend e `npm test` nos 3 front-ends, 100% verde.
- [ ] 5.2 `openspec validate improve-first-access-delivery --strict`.
- [ ] 5.3 Confirmar manualmente (stack Docker local com SMTP de teste, ex.:
      Mailhog/Mailpit) que o e-mail de ativação chega e o link funciona fim a fim.
