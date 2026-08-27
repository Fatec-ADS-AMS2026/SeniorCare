## 1. Capacidade de envio de e-mail (`notification-delivery`)

- [x] 1.1 Adicionar dependência MailKit ao projeto `WebAPI`.
- [x] 1.2 Criar `INotificationSender` (abstração) e `SmtpNotificationSender`
      (implementação), lendo `Smtp__Host`/`Smtp__Port`/`Smtp__Username`/
      `Smtp__Password`/`Smtp__FromAddress`/`Smtp__FromDisplayName`/
      `Smtp__UseStartTls`.
- [x] 1.3 Registrar como no-op consciente (loga `Information`, não falha) quando
      nenhuma variável `Smtp__*` está presente; falhar no startup (mesmo padrão de
      `Program.GetMissingConfiguration`) quando configuração está parcial.
- [x] 1.4 Capturar exceção do cliente SMTP e logar só mensagem genérica — nunca a
      exceção bruta, nunca o corpo da mensagem.
- [x] 1.5 Testes de unidade: SMTP ausente (no-op), SMTP parcial (falha de startup),
      SMTP configurado + envio simulado (mock do cliente), falha de envio simulada
      (não propaga exceção pro chamador).

## 2. Wiring nos pontos de disparo

- [x] 2.1 Bootstrap (`Program.cs`): após criar o token de ativação inicial, chamar
      `INotificationSender` se configurado; exibir o token no canal manual somente
      quando SMTP estiver ausente ou a entrega falhar. Uma entrega automática
      bem-sucedida não pode duplicar o token no console/log.
- [x] 2.2 Criação de usuário administrativo (`AdminUserOverview`/controller
      correspondente): disparar e-mail de ativação; resposta do endpoint passa a
      incluir `emailSent: boolean`.
- [x] 2.3 `POST /Auth/recover`: disparar e-mail de recuperação apenas quando houver
      conta elegível — sem alterar a resposta pública uniforme já exigida pela spec.
- [x] 2.4 Auditoria: novo evento (envio bem-sucedido / falha) nos 3 pontos, sem
      token/senha/segredo MFA/corpo da mensagem.
- [x] 2.5 Testes de integração dos 3 pontos e do reenvio com sender mockado —
      sucesso e falha de envio, confirmando que a operação de origem sempre
      completa, o token anterior é invalidado e o isolamento institucional é
      preservado.
- [x] 2.6 Adicionar `POST /api/v1/AdminUser/{id}/resend-activation`, restrito a
      `AdminUser:write`, conta `LOCAL`/`PROVISIONED` da mesma instituição; invalidar
      ativações pendentes antes de emitir outra, retornar apenas `emailSent` e
      oferecer a ação correspondente em `AdminUserOverview`.

## 3. QR code no cadastro de MFA (front-end)

- [x] 3.1 Adicionar biblioteca de geração de QR code (client-side, sem dependência
      nova no backend) aos três front-ends (Senior Portal, care e stock).
- [x] 3.2 Renderizar o QR code em `MfaEnrollPage` a partir do `otpAuthUri` já
      retornado por `POST /Auth/mfa/enroll` nos três front-ends, mantendo a chave
      em texto visível.
- [x] 3.3 Testes de componente (Vitest) cobrindo a renderização do QR code junto da
      chave em texto.

## 4. Configuração e documentação

- [x] 4.1 Documentar `Smtp__*` e `Frontend__ActivationBaseUrl` em
      `SeniorCareManager-Backend/SeniorCareManager.WebAPI/CONFIGURATION.md`.
- [x] 4.2 Atualizar `infra/deploy/BOOTSTRAP.md` — remover os itens de e-mail e QR
      code da tabela "Pendências conhecidas" (ou marcar como resolvidos), ajustar as
      seções 2/4/5 pra refletir o envio automático quando configurado.
- [x] 4.3 Atualizar `docs/tutorial-desenvolvimento-ides.md` e `docs/tutorial-docker.md`
      — remover os blockquotes de pendência conhecida ou ajustá-los pra citar que o
      envio automático é opcional, explicar que não existe senha inicial padrão e
      separar claramente configuração, ativação e observação do fallback no Rider e
      no Docker Compose.
- [x] 4.4 Adaptar `infra/docker-test/bootstrap-dev-admin.sh` ao token condicional:
      aceitar `--token`/token do log somente no fallback manual e remover a senha
      conhecida `DevSenhaForte!2026` como default, exigindo
      `DEV_ADMIN_PASSWORD` não versionada ou gerando senha efêmera de alta entropia.
- [x] 4.5 Documentar uma matriz única de primeira execução para Rider/WebStorm e
      Docker Compose, deixando explícito que somente a API executa o bootstrap, os
      front-ends apenas conduzem a ativação e reinícios não reemitem token nem
      redefinem credencial.
- [x] 4.6 Documentar que o token bruto não é recuperável do banco: o fallback do
      bootstrap é de saída única e, para contas administrativas posteriores, o
      procedimento seguro é corrigir/configurar SMTP e usar “Reenviar ativação”.

## 5. Aceite

- [x] 5.1 `dotnet test` no backend e `npm test` nos 3 front-ends, 100% verde.
- [x] 5.2 `openspec validate improve-first-access-delivery --strict`.
- [x] 5.3 Confirmar manualmente na stack Docker local com SMTP de teste (ex.:
      Mailhog/Mailpit) que o e-mail de ativação chega, o link funciona fim a fim e
      o token entregue não aparece na saída do container.
- [x] 5.4 Adicionar testes de integração do bootstrap cobrindo banco vazio, conta
      administrativa `PROVISIONED` sem senha, configuração parcial sem criação
      parcial, ativação com senha escolhida pela pessoa e reinício idempotente sem
      novo token ou redefinição de credencial.
- [x] 5.5 Validar manualmente a primeira execução pela IDE: API no Rider,
      frontend no WebStorm, SMTP ausente, token apresentado uma vez no console,
      ativação/MFA concluídos e reinício sem reemissão.
- [x] 5.6 Validar manualmente a primeira execução por Docker Compose nos modos
      SMTP e fallback manual, incluindo leitura condicional do token pelo helper,
      ausência de senha default conhecida e reinício sem duplicação ou reemissão.
