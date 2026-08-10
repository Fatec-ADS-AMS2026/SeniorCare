# infra/deploy — publicação pull-based no servidor

Compose de **produção** que consome imagens prontas do GHCR (não constrói nada).
Fundamentação: [`docs/infra/ci-cd-arquitetura.md`](../../docs/infra/ci-cd-arquitetura.md).

## Componentes

- [`BOOTSTRAP.md`](BOOTSTRAP.md) — procedimento completo de bootstrap da instituição/admin
  inicial, canal de ativação para contas seguintes, backup/rollback e incompatibilidade
  entre versões. **Leia antes do primeiro deploy de um cliente novo.**
- `docker-compose.yml` — serviços com `image: ${*_IMAGE}` (pinadas pelo release manifest).
- `deploy.sh` — orquestra: login → `pg_dump` → pull por digest → up → healthcheck → registro.
- `clients/<nome>/.env` — segredos/paths/CORS por ambiente (fora do git; ver `clients/exemplo/.env.example`).
- `releases/<versão>.env` — manifest gerado pelo `release.yml` (na raiz do repo, `../../releases`).
- `docker-compose.ops.yml` + `ops.sh` + `ops/Caddyfile` — **camada de operação** opcional (ingress/TLS, painel, logs, backup agendado).

## Uso

```bash
export CLIENT=exemplo                   # nome da pasta em clients/
cp clients/exemplo/.env.example clients/exemplo/.env   # e ajuste os valores
# 1ª vez: docker login ghcr.io (ou exporte GHCR_USER/GHCR_TOKEN p/ o deploy.sh logar)

./deploy.sh 2026.08.0     # pull + up + healthcheck-gate
./deploy.sh status        # versão corrente + estado dos containers
./deploy.sh rollback      # volta à versão anterior
```

## Portas (default)

| Serviço | Porta host |
|---|---|
| care-web | 3000 |
| stock-web | 3001 |
| API | 8080 |
| Postgres (loopback) | 5432 |
| pgAdmin (`--profile tools`) | 5050 |

## Camada de operação (`ops.sh` / `--profile ops`)

Ferramentas single-purpose que entregam o que um PaaS daria — UI, logs, ingress/TLS,
backup agendado — **sem regredir** o núcleo pull-based. Rodam como **projeto compose
separado** (`seniorcare-ops`), então o `--remove-orphans` do `deploy.sh` nunca as toca.

```bash
cp .env.ops.example .env.ops     # ajuste domínio/portas/credenciais
./ops.sh up                      # sobe a camada de operação (só com a stack base no ar)
./ops.sh status | logs | down
```

| Serviço | Papel | Porta host (default) |
|---|---|---|
| **Homepage** | **hub de gestão** — atalhos p/ todas as UIs + status por container | **8090** |
| Caddy | ingress + TLS (CA interna) | 8880 / 8443 |
| Portainer CE | painel de gestão/visibilidade | 9443 |
| Dozzle | logs de container ao vivo | 8888 |
| backup (cron + `ops/backup.sh`) | **zip diário**: `pg_dump` (02:00, configurável) | — |

**Notas:** (1) as portas do Caddy vêm fora de 80/443 para não conflitar com os
frontends durante a avaliação — no cutover de produção, libere 80/443 removendo as
publicações diretas de porta dos frontends. (2) Portainer/Dozzle montam o socket do
Docker — restrinja o acesso a essas portas (firewall) ou exponha só em `127.0.0.1`.
(3) Fonte da verdade do deploy segue no git/manifest; estas ferramentas **só
observam**. (4) O **Homepage** é o ponto de entrada — abra `http://<ip>:8090`
(config em `ops/homepage/`, links via `OPS_HOST`).

### Backup diário (`backup` / `ops/backup.sh`)

Gera **um `.zip` por dia** em `OPS_BACKUP_PATH` com o `pg_dump` do banco. Roda por cron
interno no horário `BACKUP_SCHEDULE` (default `0 2 * * *` = 02:00), respeitando `TZ`;
retém `BACKUP_KEEP_DAYS` dias. Complementa (não substitui) o `pg_dump` pré-deploy do
`deploy.sh`.

```bash
# rodar um backup sob demanda (fora do horário):
docker compose -p seniorcare-ops -f docker-compose.ops.yml exec backup /usr/local/bin/backup.sh

# restaurar o banco a partir de um zip:
unzip -p backups/scheduled/seniorcare-backup-AAAAMMDD.zip database-AAAAMMDD.sql \
  | docker exec -i seniorcare-postgres psql -U postgres -d db_seniorcare
```
