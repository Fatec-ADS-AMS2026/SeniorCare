# Bootstrap da instituição e do administrador inicial

Procedimento operacional fim-a-fim para o primeiro deploy de um cliente novo —
complementa `CONFIGURATION.md` (que documenta as variáveis) com o passo a
passo real. Verificado de ponta a ponta via `infra/docker-test` nesta seção
(§12.8 do change `stabilize-existing-platform`).

A mesma mecânica (variáveis → subir → capturar token do log → ativar → MFA)
vale local, via [`infra/docker-test`](../docker-test/README.md) — só o comando
de subir a stack muda (`docker compose up -d --build` em vez de `./deploy.sh`).

## 1. Antes do primeiro deploy

No `.env` do cliente (`clients/<nome>/.env`, a partir de `clients/exemplo/.env.example`),
defina as três variáveis de bootstrap **juntas** (a validação de startup rejeita
qualquer combinação parcial):

```bash
Bootstrap__InstitutionName=ILPI Exemplo
Bootstrap__AdminEmail=admin@exemplo.com.br
Bootstrap__AdminDisplayName=Administrador Inicial
```

Elas só têm efeito **enquanto nenhuma instituição existir no banco** — em
deploys seguintes (instituição já criada), ficam sem efeito e podem continuar
no `.env` sem risco de recriar nada. Não é necessário removê-las depois.

## 2. Subir e capturar o token de ativação

```bash
./deploy.sh 2026.08.0
```

No primeiro boot, a API cria a instituição e a conta administrativa em estado
`PROVISIONED` e imprime o token de ativação **uma única vez** no log do
processo — ele não é reimpresso nem fica salvo em nenhum outro lugar:

```
Bootstrap: instituição e administrador PROVISIONED criados.
  Token de ativação (uso único, capture agora — não será reimpresso): <token>
```

Capture esse log imediatamente:

```bash
docker logs seniorcare-api 2>&1 | grep -A1 "Token de ativação"
```

Se o token for perdido antes da ativação, não há como recuperá-lo pela API —
a única saída é reprovisionar a conta diretamente no banco (fora do escopo
deste runbook; consulte o time de backend).

## 3. Ativar a conta

Com o token em mãos, ative a conta (define a senha real e libera o login) —
via UI (`RecoverAccountPage`/fluxo de ativação do front-end) ou diretamente:

```bash
curl -X POST https://<host>/api/v1/Auth/activate \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@exemplo.com.br","token":"<token>","newPassword":"<senha-forte>"}'
```

O primeiro login em seguida vai pedir cadastro de MFA (`mfa_enrollment_required`)
— obrigatório para toda conta administrativa, sem exceção pro bootstrap.

## 4. Canal de ativação para contas administrativas seguintes (gap operacional reconhecido)

O procedimento acima cobre só a **primeira** conta (bootstrap via variável de
ambiente, lida do log do processo). Contas administrativas criadas depois
disso pela tela `AdminUserOverview` (§10.6) também nascem `PROVISIONED` com um
token de ativação — mas a plataforma **não tem serviço de e-mail** hoje, e a
tela mostra só uma mensagem pedindo para seguir "o procedimento institucional"
(gap já registrado em `tasks.md`, tarefa 10.6, não resolvido nesta seção).

Até existir um canal técnico de envio, o procedimento operacional recomendado
é:

1. Quem tem acesso ao banco/logs do servidor recupera o token junto ao
   administrador que criou a conta (mesmo mecanismo do passo 2, mas via
   consulta pontual, não log de boot — combine com o time de backend o método
   de consulta pra essa situação específica).
2. A entrega do token/link à pessoa nova acontece por um canal **fora da
   plataforma** já confiável institucionalmente (ramal telefônico conhecido,
   entrega presencial, ou o canal que a ILPI já usa para credenciais
   sensíveis) — nunca por e-mail não criptografado ou mensagem que fique
   registrada em texto plano num sistema de terceiros sem necessidade.
3. A pessoa confirma a própria identidade pelo procedimento institucional já
   em uso (o mesmo usado hoje para qualquer outra credencial sensível) antes
   de receber o token.

Isso não é uma solução técnica — é a orientação operacional mínima enquanto o
gap não é fechado por um canal de envio de verdade (trabalho futuro).

## 5. Backup pré-deploy e rollback

Já implementados em `deploy.sh` — este runbook só aponta pra eles, não duplica
a lógica:

- **Backup**: `pg_backup()` roda um `pg_dump` completo antes de todo deploy
  (`deploy.sh`, função `pg_backup`), salvo em `backups/<versão>-pre-<timestamp>.sql`,
  retendo os `BACKUP_KEEP` mais recentes. Se o `pg_dump` falhar, o deploy é
  abortado antes de tocar em qualquer container (`deploy.sh` nunca sobe sem
  backup confirmado).
- **Pré-validação de migração**: `pre_validate_migration()` roda o SQL da
  migração pendente (gerado em CI, `releases/<versão>-migration.sql`) contra o
  banco de produção real dentro de uma transação sempre revertida — detecta
  incompatibilidade de dado ANTES do deploy real acontecer (ver `deploy.sh` e
  `design.md`, risco "Migração falha por dados atuais inválidos").
- **Rollback**: `./deploy.sh rollback` restaura o release manifest anterior
  registrado (`do_rollback()`) e reaplica o deploy com ele. Migrações do
  projeto são preferencialmente aditivas (nunca removem coluna/tabela) — ver
  `design.md`, seção Rollback — justamente para que voltar o binário pra
  versão anterior continue funcionando mesmo depois de uma migração ter
  rodado. Se a migração alterou dado de forma incompatível com o binário
  anterior, restaure também o backup pré-deploy salvo no passo acima.

## 6. Incompatibilidade entre versões — não faça deploy parcial

O release manifest (`releases/<versão>.env`) fixa a API e os dois front-ends
na MESMA versão, publicados juntos pelo `release.yml`. **Não troque a imagem
de só um dos três serviços manualmente** (ex.: só o `care-web`) — um
front-end mais novo pode esperar um contrato de API que a versão antiga do
backend ainda não tem, e vice-versa. Sempre use `./deploy.sh <versão>` com o
manifest completo; ele já garante que os três sobem juntos.
