## Context

`platform-authentication` (arquivada em `stabilize-existing-platform`) já
implementa geração de token de ativação/recuperação e cadastro de MFA por
TOTP — o que falta é a **entrega** desses artefatos até a pessoa certa sem
depender de alguém com acesso a log/banco copiar e colar manualmente. O
próprio `design.md` da mudança anterior já registrou o risco ("Canal de
ativação indisponível em ILPI de baixo orçamento") com um mitigante
operacional (procedimento manual do bootstrap, já documentado em
`infra/deploy/BOOTSTRAP.md`) — esta mudança implementa o mitigante técnico
que aquele risco deixou como trabalho futuro. Para contas posteriores, cujo
token bruto é corretamente irrecuperável, o contorno passa a ser configurar
ou corrigir o SMTP e solicitar um reenvio autenticado.

## Goals / Non-Goals

**Goals:**
- Entregar o token de ativação/recuperação por e-mail quando o ambiente de
  implantação tiver SMTP configurado, sem regressão pra quem não tiver.
- Adicionar QR code no cadastro de MFA, mantendo a chave manual como
  alternativa (nem todo autenticador escaneia bem em tela pequena/baixa
  resolução — não remover o texto).
- Manter as garantias já promovidas pela spec: nenhuma senha ou segredo MFA
  em log, e token apenas na saída única e explícita do fallback manual do
  bootstrap; resposta pública uniforme em recuperação (não vaza se a conta
  existe).
- Garantir comportamento equivalente na primeira execução pela combinação
  Rider/WebStorm e pela stack Docker Compose, sem introduzir senha inicial
  padrão em nenhum dos ambientes.

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
`Information`, nunca `Error`) e o fluxo que a chamou continua concluindo. O
bootstrap usa a saída manual única; contas posteriores usam o reenvio seguro
depois que o canal for configurado.

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
conta inclui um aviso (`emailSent: false`). O administrador corrige o canal e
usa a ação autenticada de reenvio; nenhum token é recuperado do banco ou
exposto. Consistente com "falha segura" já estabelecido no projeto (§8 do
change anterior).

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

### 8. Um único contrato de bootstrap para IDE e containers, sem senha padrão

O bootstrap pertence exclusivamente à API. Rodar o backend pelo Rider ou pelo
container muda apenas o canal operacional em que a saída manual é observada;
WebStorm, Vite e os containers dos front-ends não criam instituição, usuário,
token ou senha. Em ambos os modos, banco vazio mais as três variáveis
`Bootstrap__*` completas produz uma instituição e um administrador
`PROVISIONED` sem `PasswordHash`, seguido da emissão de um token de ativação.

| Execução | Entrada do bootstrap | Entrega automática | Fallback manual |
|---|---|---|---|
| Rider + WebStorm | Run Configuration/ambiente da API | caixa SMTP de teste ou institucional | console da Run Configuration |
| Docker Compose | `.env` local/secret injetado no serviço da API | Mailpit/Mailhog local ou SMTP institucional | saída de `seniorcare-api` consultada por `docker compose logs` |

Se SMTP entregar o token, `Program` não o imprime. Se SMTP estiver ausente ou
falhar, `Program` o imprime uma única vez no canal da linha correspondente. Em
reinícios, `BootstrapService` encontra a instituição já existente, não cria nova
conta e não reemite token.

O helper `infra/docker-test/bootstrap-dev-admin.sh` é automação de ambiente de
desenvolvimento, não uma fonte de credencial da plataforma. O valor conhecido
`DevSenhaForte!2026` deixa de ser default: o helper recebe
`DEV_ADMIN_PASSWORD` explicitamente de fonte não versionada ou gera uma senha
efêmera de alta entropia, exibida somente para a pessoa que executou o comando.
Essa senha é a credencial escolhida durante a ativação, não uma senha criada
pelo bootstrap.

### 9. Reenvio seguro substitui o falso fallback por leitura do banco

O endpoint autenticado `POST /api/v1/AdminUser/{id}/resend-activation`, protegido
pela permissão `AdminUser:write`, aceita apenas contas `LOCAL`, `PROVISIONED` e da
mesma instituição do administrador. Antes de emitir o novo token, o serviço marca
como usados todos os tokens de ativação ainda pendentes daquela conta; assim, um
link antigo não volta a funcionar depois do reenvio.

O endpoint tenta a entrega e responde somente `{ "emailSent": true|false }`. Uma
falha mantém a conta provisionada e pode ser tentada novamente após a correção do
SMTP. Nem a resposta nem os logs/auditoria incluem token ou link. A interface de
administração oferece a ação somente para contas `PROVISIONED`; a API continua
aplicando todas as validações independentemente da interface.

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
  o bootstrap conserva a saída manual única. Para contas posteriores não existe
  recuperação segura do token bruto; a interface informa a falha e permite o
  reenvio assim que a implantação configurar um SMTP, inclusive um relay local.
- **[Helper local perpetua uma senha administrativa conhecida]** → remover o
  default versionado, aceitar senha explícita somente pelo ambiente local ou
  gerar uma senha aleatória por execução; nunca gravá-la em arquivo rastreado.
- **[Documentação diverge entre IDE e containers]** → manter uma matriz única
  de invariantes e validar os dois caminhos com banco vazio e reinício, variando
  apenas a origem da configuração e o canal do fallback manual.

## Migration Plan

1. Adicionar `INotificationSender`/implementação SMTP + testes de unidade
   (mock da lib, sem SMTP real precisando estar disponível em CI).
2. Testes de integração dos 3 pontos de disparo (ativação bootstrap,
   ativação via `AdminUserOverview`, recuperação) e do reenvio administrativo
   com o sender mockado, cobrindo sucesso, falha e invalidação do token anterior.
3. Adicionar QR code no `MfaEnrollPage` dos três front-ends (mesmo
   componente compartilhado por convenção do projeto — copiar igual, sem
   pacote compartilhado, mesmo padrão de todo o resto do front-end).
4. Documentar as variáveis `Smtp__*`/`Frontend__ActivationBaseUrl` em
   `CONFIGURATION.md`, atualizar `infra/deploy/BOOTSTRAP.md` (a seção
   "Pendências conhecidas" perde o item de e-mail; o de QR code também sai
   quando o front-end entregar) e os dois tutoriais de desenvolvimento.
5. Adaptar o helper de desenvolvimento para não possuir senha conhecida e
   validar o primeiro acesso com API no Rider/front-end no WebStorm e com a
   stack Docker Compose, nos modos SMTP e fallback manual.
6. Sem migração de banco necessária além do novo tipo de evento de
   auditoria, se for um enum novo em vez de reaproveitar `AUTHENTICATION`.
