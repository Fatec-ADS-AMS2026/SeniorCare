## Context

`platform-authentication` (arquivada em `stabilize-existing-platform`) já
implementa geração de token de ativação/recuperação e cadastro de MFA por
TOTP — o que falta é a **entrega** desses artefatos até a pessoa certa sem
depender de alguém com acesso a log/banco copiar e colar manualmente. O
próprio `design.md` da mudança anterior já registrou o risco ("Canal de
ativação indisponível em ILPI de baixo orçamento") com um mitigante
operacional (procedimento manual, já documentado em
`infra/deploy/BOOTSTRAP.md`) — esta mudança implementa o mitigante técnico
que aquele risco deixou como trabalho futuro, sem substituir o mitigante
operacional (que continua sendo o contorno pra quem não configurar SMTP).

## Goals / Non-Goals

**Goals:**
- Entregar o token de ativação/recuperação por e-mail quando o ambiente de
  implantação tiver SMTP configurado, sem regressão pra quem não tiver.
- Adicionar QR code no cadastro de MFA, mantendo a chave manual como
  alternativa (nem todo autenticador escaneia bem em tela pequena/baixa
  resolução — não remover o texto).
- Manter as garantias já promovidas pela spec: nenhuma senha, token ou
  segredo MFA em log; resposta pública uniforme em recuperação (não vaza
  se a conta existe).

**Non-Goals:**
- Não é um sistema de notificação genérico — sem fila de mensagens, sem
  template engine sofisticada, sem notificação push/SMS/WhatsApp. Só e-mail
  transacional pros 3 eventos listados no proposal.
- Não prepara infraestrutura para alertas clínicos, lembretes de tarefa ou
  qualquer notificação de domínio assistencial futuro — isso é uma decisão
  de uma mudança própria quando o núcleo assistencial existir.
- Não introduz um provedor de e-mail comercial (SendGrid, SES, Postmark
  etc.) como dependência obrigatória — SMTP puro, pra não forçar toda ILPI
  (público de baixo orçamento, `docs/escopo-do-projeto.md`) a ter conta num
  serviço pago; instituições que já usam um desses providers configuram o
  SMTP relay deles normalmente.

## Decisions

### 1. SMTP puro via MailKit, não um provedor de API comercial

MailKit é a biblioteca .NET madura e amplamente usada pra SMTP (substituto
do `System.Net.Mail` obsoleto). Configuração por variáveis de ambiente,
mesmo padrão `__` já usado em todo o projeto (`ConnectionStrings__DefaultConnection`,
`Bootstrap__*`):

```
Smtp__Host
Smtp__Port
Smtp__Username
Smtp__Password
Smtp__FromAddress
Smtp__FromDisplayName
Smtp__UseStartTls        (bool, default true)
```

Nenhuma dessas é obrigatória — ausentes, o serviço de notificação vira um
no-op consciente (loga "SMTP não configurado, e-mail não enviado" em nível
`Information`, nunca `Error`) e o fluxo que a chamou continua exatamente
como hoje (token só interno, procedimento manual documentado se aplica).

### 2. `INotificationSender` como abstração, SMTP como única implementação hoje

Interface pequena (`Task SendAsync(string to, string subject, string body)`
ou equivalente) — não porque se espera trocar de provedor amanhã, mas
porque já é o padrão do projeto (toda dependência externa é abstraída atrás
de uma interface própria — `IAccountTokenService`, `ISessionService` etc.)
e torna o teste de unidade dos controllers/services que disparam e-mail
trivial (mock, sem SMTP real) — mesmo racional já usado no resto do
backend, não uma decisão nova.

### 3. URL de ativação aponta pro front-end, endereço configurável

O e-mail precisa montar um link tipo `https://<host>/ativar-conta?email=...&token=...`.
Nova variável `Frontend__ActivationBaseUrl` (sem default de produção — se
ausente e SMTP estiver configurado, falha no startup como as outras
variáveis obrigatórias condicionais, mesmo padrão de
`Program.GetMissingConfiguration`). A variável aponta para o Senior Portal,
que é a raiz institucional e hospeda `ActivateAccountPage` em
`/ativar-conta`; care e stock preservam suas rotas durante a transição, mas
não são o destino canônico de novos links.

### 4. Falha de envio não bloqueia a operação que originou o token

Se o SMTP estiver configurado mas o envio falhar (rede, credencial errada,
servidor fora), a conta/token já foram criados com sucesso no banco antes
do envio — a falha de e-mail é capturada, auditada como evento de falha
(sem o conteúdo da mensagem), e a resposta da API ao admin que criou a
conta inclui um aviso (`emailSent: false`) pra ele saber que precisa cair
pro procedimento manual dessa vez, sem a operação inteira falhar. Consistente
com "falha segura" já estabelecido no projeto (§8 do change anterior).

### 5. QR code: biblioteca só no front-end, sem mudança de contrato da API

O endpoint `POST /Auth/mfa/enroll` já devolve `otpAuthUri` — é só isso que
uma lib de QR (ex.: `qrcode`, pura JS, sem dependência nativa) precisa pra
renderizar um `<canvas>`/SVG no `MfaEnrollPage` do Senior Portal, care e
stock. Nenhuma mudança de contrato HTTP, nenhuma dependência nova no backend.
A chave em texto continua visível abaixo do QR — não é substituída, é
complementada.

### 6. Escopo da configuração SMTP e exceção operacional do bootstrap

As variáveis `Smtp__*` configuram a implantação inteira. Esta mudança não
introduz credenciais SMTP persistidas por instituição; isso exigiria modelo,
criptografia de segredo, autorização administrativa e rotação próprios.

O token inicial só pode ser exibido no canal manual documentado quando o SMTP
estiver desabilitado ou quando a entrega falhar. Uma entrega automática
bem-sucedida nunca duplica o token no console/log. Essa exceção operacional é
restrita ao bootstrap; logs e auditorias do serviço de e-mail nunca recebem
token ou corpo da mensagem.

### 7. Auditoria do envio, não do conteúdo

Novo tipo de evento de auditoria (`AuditEventCategory` já existente,
provavelmente `AUTHENTICATION` reaproveitado ou um novo valor) registra
"e-mail de ativação enviado"/"falhou" com o destinatário (já é dado que a
auditoria de outros eventos de identidade já registra) mas nunca o token
nem o corpo da mensagem — mesma regra que já vale pra todo o resto da
auditoria (`platform-authentication`, "Eventos de identidade... são
auditáveis").

## Risks / Trade-offs

- **[Credencial SMTP vazando em log/erro genérico]** → `INotificationSender`
  captura exceções da lib SMTP e loga só uma mensagem genérica
  ("falha ao enviar e-mail"), nunca a exceção bruta (que pode conter a
  senha SMTP em alguns clientes) — mesmo cuidado que `GlobalExceptionHandler`
  já aplica a outras exceções sensíveis.
- **[Ambiente configura SMTP errado e ninguém percebe]** → resposta da
  API já sinaliza `emailSent: false` pro admin que criou a conta; considerar
  (fora do escopo desta mudança, mas registrado aqui) um healthcheck
  opcional de SMTP em `/health/ready` se isso se mostrar necessário depois.
- **[E-mail como único canal ainda exclui ILPI sem e-mail configurado]** →
  não é regressão (hoje NINGUÉM tem entrega automática) — o procedimento
  manual documentado continua existindo e funcionando exatamente como
  antes; esta mudança é estritamente aditiva.

## Migration Plan

1. Adicionar `INotificationSender`/implementação SMTP + testes de unidade
   (mock da lib, sem SMTP real precisando estar disponível em CI).
2. Testes de integração dos 3 pontos de disparo (ativação bootstrap,
   ativação via `AdminUserOverview`, recuperação) com o sender mockado,
   cobrindo sucesso e falha de envio.
3. Adicionar QR code no `MfaEnrollPage` dos três front-ends (mesmo
   componente compartilhado por convenção do projeto — copiar igual, sem
   pacote compartilhado, mesmo padrão de todo o resto do front-end).
4. Documentar as variáveis `Smtp__*`/`Frontend__ActivationBaseUrl` em
   `CONFIGURATION.md`, atualizar `infra/deploy/BOOTSTRAP.md` (a seção
   "Pendências conhecidas" perde o item de e-mail; o de QR code também sai
   quando o front-end entregar) e os dois tutoriais de desenvolvimento.
5. Sem migração de banco necessária além do novo tipo de evento de
   auditoria, se for um enum novo em vez de reaproveitar `AUTHENTICATION`.
